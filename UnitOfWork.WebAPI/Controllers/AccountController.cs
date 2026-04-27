using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using UnitOfWork.Core.Models;
using UnitOfWork.Services.Interfaces;
using UnitOfWork.WebAPI.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;

namespace UnitOfWork.WebAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IRepositoryServices _repository;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly IConfiguration _config;

        public AccountController(IRepositoryServices repository, SignInManager<AppUser> signInManager, UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, IConfiguration config)
        {
            _repository = repository;
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
            _config = config;
        }

        #region Code Craft sep 2025

       

        [HttpPost]
        [ActionName("AuthPPortalOffAirCiv")]
        public async Task<IActionResult> AuthPPortalOffAirCiv(User_DTO model)
        {
            var pass = GenHsh(model.PasswordHash);
            model.PasswordHash = GenHsh(pass);
            if (string.IsNullOrEmpty(model.PasswordHash))
                return NoContent();
            var record = await _repository.AuthPPortalOffAirCiv(model);
            if (record.Count() > 0)
            {
                int? id;
                tbl_USERS_DTO user_DTO = new tbl_USERS_DTO();
                user_DTO.username = model.Pakno;
                user_DTO.is_locked = false;
                user_DTO.is_deleted = false;
                user_DTO.is_active = true;
                user_DTO.Action = "INSERT";
                user_DTO.role_id = 2;
                var userExist = await _repository.GET_tbl_USER_BY_NAME(model.Pakno);
                Response_DTO addRes = null;
                if (userExist == null || !userExist.Any())
                {
                    addRes = _repository.AddUser(user_DTO);
                    id = addRes.newId;
                    var userqota = await _repository.GET_tbl_USER_EXIST_QUOTA(Convert.ToString(id));
                    if (!userqota.Any())
                    {
                        var result = await _repository.GET_tbl_PROMPT_LIMIT(user_DTO.role_id);
                        if (result.Any())
                        {
                            tbl_USER_PROMPTS_DTO entity = new tbl_USER_PROMPTS_DTO()
                            {
                                user_id = Convert.ToString(id),
                                prompt_text = result.First().PromptLimit,
                            };
                            var result2 = _repository.CRUD_tbl_USER_PROMPTS_QUOTA(entity);
                        }
                    }
                }
                else
                {
                    id = userExist.First().id;
                }

                // Get profile data from external service
                var user = await _repository.GetPersonalBasicDataCMD(model.PType.ToString(), model.Pakno);

                // If we just created the user, save initial profile into USER_PROFILE table
                if (addRes != null && addRes.status && addRes.newId.HasValue)
                {
                    try
                    {
                        var profileRes = _repository.AddUserProfile(addRes.newId.Value, user);
                        // optionally log profileRes.message / profileRes.status
                    }
                    catch
                    {
                        // swallow or log - do not block authentication on profile insert failure
                    }
                }

                var jwt = GenerateToken(model.Pakno);
                Console.WriteLine($"User Validation Successful For Testing");
                return Ok(new { message = "User Validation Successful", id = id, status = true, jwt = jwt, user = user });
            }
            else
                Console.WriteLine($"User Validation Not Successful For Testing");
            return Unauthorized(new { message = "User Validation failed", status = false });
        }


        [HttpPost]
        [ActionName("AuthPPortalLoginByPassCredt")]
        public async Task<IActionResult> AuthPPortalNEW(User_DTO model)
        {
            var record = await _repository.GetPPortalAuthByUID(model.PasswordHash);

            if (record is null)
            {
                // handle error / no data
                return Unauthorized(new { message = "User Validation failed", status = false });
            }

            // Step 2 – read the “success” flag (optional)

            if (record.success == true)
            {
                model.Pakno = record.response;
                int? id;
                tbl_USERS_DTO user_DTO = new tbl_USERS_DTO();
                user_DTO.username = model.Pakno;
                user_DTO.is_locked = false;
                user_DTO.is_deleted = false;
                user_DTO.is_active = true;
                user_DTO.Action = "INSERT";
                user_DTO.role_id = 2;
                var userExist = await _repository.GET_tbl_USER_BY_NAME(model.Pakno); //get pakno from record
                Response_DTO addRes = null;
                if (userExist == null || !userExist.Any())
                {
                    addRes = _repository.AddUser(user_DTO);
                    id = addRes.newId;
                }
                else
                {
                    id = userExist.First().id;
                }

                model.PType = DeterminePType(record.response);

                var user = await _repository.GetPersonalBasicDataCMD(model.PType.ToString(), model.Pakno); //get pakno from record

                if (addRes != null && addRes.status && addRes.newId.HasValue)
                {
                    try
                    {
                        var profileRes = _repository.AddUserProfile(addRes.newId.Value, user);
                        // optionally inspect profileRes
                    }
                    catch
                    {
                        // ignore/profile log
                    }
                }

                var jwt = GenerateToken(model.Pakno);
                return Ok(new { message = "User Validation Successful", id = id, status = true, jwt = jwt, user = user });
            }
            else
                return Unauthorized(new { message = "User Validation failed", status = false });
        }

        public int DeterminePType(string pakno)
        {
            // Check for alphabetic characters first (highest priority)
            if (System.Text.RegularExpressions.Regex.IsMatch(pakno, "[a-zA-Z]"))
            {
                return 3;
            }

            string trimmedPakno = pakno?.Trim() ?? "";

            // Check length conditions
            if (trimmedPakno.Length < 6)
            {
                return 1;
            }
            else if (trimmedPakno.Length == 6)
            {
                return 2;
            }

            // Default case (if no conditions match, though your logic doesn't specify this)
            return 0; // or you could throw an exception
        }

        #region password encryption

        [NonAction]
        public string GenHsh(string password)
        {
            System.Security.Cryptography.MD5 mD5 = System.Security.Cryptography.MD5.Create();
            var encData = System.Text.Encoding.ASCII.GetBytes(password);
            var hashData = mD5.ComputeHash(encData);
            StringBuilder strA = new StringBuilder();
            foreach (var item in hashData)
            {
                strA.Append(item.ToString("X2"));
            }
            var pwd = strA.ToString();
            return pwd;
        }

        #endregion

        #region JWT Token

        [NonAction]
        public string GenerateToken(string username)
        {
            var user = username;
            var claims = new[] {
            new Claim(JwtRegisteredClaimNames.Sub, user),
            new Claim(JwtRegisteredClaimNames.UniqueName, Guid.NewGuid().ToString())};
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var tokenData = new JwtSecurityToken(
                      issuer: _config["Jwt:Issuer"],
                      audience: _config["Jwt:Audience"],
                      claims: claims,
                      expires: DateTime.Now.AddMinutes(30),
                      signingCredentials: creds);
            var token = new
            {
                token = new JwtSecurityTokenHandler().WriteToken(tokenData),
                expiry = tokenData.ValidTo
            };
            return token.token;
        }

        #endregion

        #endregion 

        #region Old Code Craft Code

        [HttpPost]
        [AllowAnonymous]
        [ActionName("Userlogin")]
        public async Task<IActionResult> Userlogin([FromBody] CreateUser model)
        {
            if (ModelState.IsValid)
            {
                var result = _repository.UserAuthenticate(model);
                if (result.Count() > 0)
                {
                    return Ok(new { result });
                }
                return BadRequest("Plz Enter Valid Credential");
            }
            return BadRequest("Plz Enter Valid Information");
        }

        [HttpPost]
        [AllowAnonymous]
        [ActionName("AddUser")]
        public async Task<IActionResult> AddUser(CreateUser model)
        {
            if (ModelState.IsValid)
            {
                var result = _repository.AddUser(model);
                if (result > 0)
                {

                    return Ok(new { msg = "User Account Created Successfully" });
                }
                else
                {
                    return BadRequest(new { msg = "Error While Creating User Account" });
                }
            }
            // If we got this far, something failed, redisplay form
            return BadRequest(ModelState);
        }

        [HttpPost]
        [ActionName("CreateRole")]
        public async Task<IActionResult> CreateRole(RoleViewModel model)
        {
            try
            {
                var role = new AppRole()
                {
                    Name = model.RoleName
                };
                var result = await _roleManager.CreateAsync(role);
                if (result.Succeeded)
                {
                    return Ok(new { msg = "Role Created Successfully" });
                }
                return BadRequest(new { msg = "Error Occured" });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion
    }
}

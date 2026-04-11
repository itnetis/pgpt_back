using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace UnitOfWork.WebAPI.Models
{
    //public class SignInModel
    //{
    //    [Display(Name = "User Name")]
    //    [Required(ErrorMessage = "{0} is required")]
    //    [StringLength(25, ErrorMessage = "{0}  must be {2} - {1} Character", MinimumLength = 4)]
    //    public string UserName { get; set; }

    //    [Display(Name = "Password")]
    //    [Required(ErrorMessage = "{0} is required")]
    //    [StringLength(25, ErrorMessage = "{0}  must be {2} - {1} Character", MinimumLength = 4)]
    //    public string Password { get; set; }

    //    [Display(Name = "Remember me?")]
    //    public bool RememberMe { get; set; }
    //}


    //[HttpPost]
    //[AllowAnonymous]
    //[ActionName("login")]
    ////[ValidateAntiForgeryToken]
    //public async Task<IActionResult> SignIn([FromBody] SignInModel model)
    //{
    //    if (ModelState.IsValid)
    //    {
    //        var result = await _signInManager.PasswordSignInAsync(
    //            model.UserName,
    //            model.Password,
    //            isPersistent: true,
    //            lockoutOnFailure: false
    //            );
    //        if (result.Succeeded)
    //        {
    //            var user = "SheryarLatif";
    //            var claims = new[] {
    //                new Claim(JwtRegisteredClaimNames.Sub, user),
    //                new Claim(JwtRegisteredClaimNames.UniqueName, Guid.NewGuid().ToString())};
    //            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
    //            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    //            var tokenData = new JwtSecurityToken(
    //                      issuer: _config["Jwt:Issuer"],
    //                      audience: _config["Jwt:Audience"],
    //                      claims: claims,
    //                      expires: DateTime.Now.AddMinutes(30),
    //                      signingCredentials: creds);
    //            var token = new
    //            {
    //                token = new JwtSecurityTokenHandler().WriteToken(tokenData),
    //                expiry = tokenData.ValidTo
    //            };

    //            var users = _repository.GetUserByName(model.UserName);
    //            //var rolesList = await _userManager.GetRolesAsync(users);
    //            //var rolename = rolesList.First();
    //            //var roles = await _roleManager.FindByNameAsync(rolename);
    //            return Ok(new { users, token });
    //        }
    //        else
    //        {
    //            return BadRequest("Plz Enter Valid UserName and Password");
    //        }
    //    }
    //    return BadRequest("Plz Enter Valid Information");

    //}
}

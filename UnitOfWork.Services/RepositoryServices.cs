using Microsoft.AspNetCore.Identity;
using System.Data;
using UnitOfWork.Core.Interfaces;
using UnitOfWork.Core.Models;
using UnitOfWork.Services.Interfaces;

namespace UnitOfWork.Services
{
    public class RepositoryServices : IRepositoryServices
    {
        private readonly IUnitOfWork _unitOfWork;

        public RepositoryServices(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        #region GPT_tbl_USERS
        public async Task<IEnumerable<dynamic>> GET_tbl_USER_PROMPTS_BY_USER(string user_id, int limit)
        {
            return await _unitOfWork.GET_tbl_USER_PROMPTS_BY_USER(user_id, limit);
        }
        public async Task<IEnumerable<dynamic>> GET_tbl_USER_PROMPTS_TOP(int limit)
        {
            return await _unitOfWork.GET_tbl_USER_PROMPTS_TOP(limit);
        }
        public Response_DTO CRUD_tbl_USER_PROMPTS(tbl_USER_PROMPTS_DTO model)
        {
            return  _unitOfWork.CRUD_tbl_USER_PROMPTS(model);
        }
        // Get remaining user quota for today
        public async Task<FeedbackResponse> SubmitFeedback(FeedbackRequest request)
        {
            return await _unitOfWork.SubmitFeedback(request);
        }

        public async Task<dynamic> GET_USER_QUOTA(string user_id)
        {
            return await _unitOfWork.GET_USER_QUOTA(user_id);
        }


        public async Task<List<FeedbackItemDto>> GetFeedbackList(GetFeedbackListRequest request)
        {
            return await _unitOfWork.GetFeedbackList(request);
        }

        public async Task<FeedbackStatsResponse> GetFeedbackStats()
        {
            return await _unitOfWork.GetFeedbackStats();
        }

        public Response_DTO AddUser(tbl_USERS_DTO obj)
        {
            return _unitOfWork.AddUser(obj);
        }

        public async Task<IEnumerable<dynamic>> GET_tbl_MODELS_LIST(int? id)
        {
            return await _unitOfWork.GET_tbl_MODELS_LIST(id);
        }

        public async Task<IEnumerable<dynamic>> GET_tbl_USER_BY_ID(int id)
        {
            return await _unitOfWork.GET_tbl_USER_BY_ID(id);
        } 
        
        public async Task<IEnumerable<dynamic>> GET_tbl_USER_BY_NAME(string username)
        {
            return await _unitOfWork.GET_tbl_USER_BY_NAME(username);
        }
        public async Task<IEnumerable<dynamic>> GET_tbl_USER_BASED_REQUESTS_LIST()
        {
            return await _unitOfWork.GET_tbl_USER_BASED_REQUESTS_LIST();
        } 
        public Response_DTO CRUD_Model(tbl_MODELS_DTO model)
        {
            return _unitOfWork.CRUD_Model(model);
        }
        
        public Response_DTO CRUD_USER_BASED_REQUESTS(UserBasedRequest_DTO request)
        {
            return _unitOfWork.CRUD_USER_BASED_REQUESTS(request);
        }
        public async Task<PromptResponse> CHECK_AND_INSERT_PROMPT(string userId, string model, string prompt)
        {
            return await _unitOfWork.CHECK_AND_INSERT_PROMPT(userId, model, prompt);
        }
        #endregion

        #region CodeCraftCode 2025
        public async Task<IEnumerable<Token>> GetTokenListAsync()
        {
            return await _unitOfWork.GetTokenListAsync();
        }
        
        public async Task<IEnumerable<Prompt>> GetPromptListAsync()
        {
            return await _unitOfWork.GetPromptListAsync();
        }

        public async Task<IEnumerable<Model>> GetModelListAsync()
        {
            return await _unitOfWork.GetModelListAsync();
        }
        
        public async Task<LoginUser_DTO> GetPersonalBasicDataCMD(string ptype, string pakno)
        {
            return await _unitOfWork.GetPersonalBasicDataCMD(ptype, pakno);
        }

        public async Task<ApiResponse> GetPPortalAuthByUID(string uId)
        {
            return await _unitOfWork.GetPPortalAuthByUID(uId);
        }

        public Response_DTO AddUserProfile(int userId, LoginUser_DTO profile)
        {
            return _unitOfWork.AddUserProfile(userId, profile);
        }

        public async Task<IEnumerable<dynamic>> GET_tbl_USER_PROFILE_LIST()
        {
            return await _unitOfWork.GET_tbl_USER_PROFILE_LIST();
        }


        #endregion

        #region Photo
        public DataTable GetAllPhoto(int cat, string pakno)
        {
            return _unitOfWork.GetAllPhoto(cat,pakno);
        }
        #endregion

        #region User Method

        public List<PromptLog> GetPromptLogs(string userName)
        {
            return _unitOfWork.GetPromptLogs(userName);
        }

        public int AddPromptLog(PromptLog promptLog)
        {
            return _unitOfWork.AddPromptLog(promptLog);
        }

        public List<AppUser> GetUserAll()
        {
            return _unitOfWork.GetUserAll();
        }

        public List<CreateUser> GetUsers()
        {
            return _unitOfWork.GetUsers();
        }

        public async Task<AppUser> GetUserById(string id)
        {
            return await _unitOfWork.GetUserById(id);
        }

        public async Task<AppUser> GetUserByName(string name)
        {
            return await _unitOfWork.GetUserByName(name);
        }

        public List<IdentityUserRole<string>> GetUserRoleList()
        {      
            return _unitOfWork.GetUserRoleList();
        }
        public int AddUser(CreateUser model)
        {
            return _unitOfWork.AddUser(model);
        }

        public string UpdateUser(CreateUser user) {

            return  _unitOfWork.UpdateUser(user);
        
        }

        public string DeleteUser(CreateUser user)
        {

            return _unitOfWork.DeleteUser(user);

        }

        public int ChangePassword(CreateUser user) { 
            
            return _unitOfWork.ChangePassword(user);
        }

       public List<CreateUser> GetUser(int id) {

            return _unitOfWork.GetUser(id);
        }

        public PromptHistory GetUserCount(PromptHistory prompt)
        {
            var result = _unitOfWork.GetPromptCountByName(prompt.UserName);
            return result;
        }

        public int UserPromptCount(PromptHistory prompt)
        {
            var result = _unitOfWork.GetPromptCountByName(prompt.UserName);
            var userLimit = _unitOfWork.GetPromptlimitByName(prompt.UserName);
            int result2 ;
            
            if (result == null)
            {
                prompt.Count = 1;
                prompt.TimeStamp = DateTime.Now;
                result2 = _unitOfWork.AddPromptCount(prompt);
            }
            else
            {
                if (result.TimeStamp.Date == DateTime.Now.Date)
                {
                    if (userLimit.prompt.PromptAllowed > result.Count)
                    {
                        result.TimeStamp = DateTime.Now;
                        result.Count = result.Count + 1;
                        result2 = _unitOfWork.EditPromptCount(result);
                    }
                    else
                    {
                        return 2;
                    }
                }
                else
                {
                    prompt.Count = 1;
                    prompt.TimeStamp = DateTime.Now;
                    result2 = _unitOfWork.AddPromptCount(prompt);
                }
            }
            return result2;
        }

        public List<CreateUser> UserAuthenticate(CreateUser model)
        {
            return _unitOfWork.UserAuthenticate(model);
        }

        public CreateUser GetPromptlimitByName(string username)
        {
            return _unitOfWork.GetPromptlimitByName(username);
        }
        public Response_DTO SetUserQuota(string user_id, int dailyLimit)
        {
            return _unitOfWork.SetUserQuota(user_id, dailyLimit);
        }

        #endregion

        #region PPortalAuthentication
        public async Task<IEnumerable<dynamic>> AuthPPortalOffAirCiv(User_DTO model)
        {
            if (model.PType == 1)
            {
                return await _unitOfWork.AuthPPortalOff(model.Pakno, model.PasswordHash);
            }
            else if(model.PType == 2)
            {
                return await _unitOfWork.AuthPPortalAir(model.Pakno, model.PasswordHash);
            }
            else if(model.PType == 3)
            {
                return await _unitOfWork.AuthPPortalCiv(model.Pakno, model.PasswordHash);
            }
            else
            {
                return null;
            }
        }
        #endregion

    }
}
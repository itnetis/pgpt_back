using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitOfWork.Core.Models;

namespace UnitOfWork.Core.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {

        #region CodeCraftCode Sep 2025
        Task<IEnumerable<dynamic>> GET_tbl_USER_PROMPTS_BY_USER(string user_id, int limit);
        Task<IEnumerable<dynamic>> GET_tbl_USER_PROMPTS_TOP(int limit);
        Response_DTO CRUD_tbl_USER_PROMPTS(tbl_USER_PROMPTS_DTO model);
        Task<IEnumerable<Token>> GetTokenListAsync();
        Task<IEnumerable<Prompt>> GetPromptListAsync();
        Task<IEnumerable<Model>> GetModelListAsync();
        Task<LoginUser_DTO> GetPersonalBasicDataCMD(string ptype, string pakno);
        Task<ApiResponse> GetPPortalAuthByUID(string uId);
        Task<FeedbackResponse> SubmitFeedback(FeedbackRequest request);
        Task<List<FeedbackItemDto>> GetFeedbackList(GetFeedbackListRequest request);
        Task<FeedbackStatsResponse> GetFeedbackStats();


        # endregion

        #region GPT_tbl_USERS
        Response_DTO AddUser(tbl_USERS_DTO obj);
        Response_DTO AddUserProfile(int userId, LoginUser_DTO profile);
        Task<IEnumerable<dynamic>> GET_tbl_MODELS_LIST(int? id);
        Response_DTO CRUD_Model(tbl_MODELS_DTO model);
        Response_DTO CRUD_USER_BASED_REQUESTS(UserBasedRequest_DTO request);
        Task<IEnumerable<dynamic>> GET_tbl_USER_BY_ID(int id);
        Task<IEnumerable<dynamic>> GET_tbl_USER_BY_NAME(string username);
        Task<IEnumerable<dynamic>> GET_tbl_USER_BASED_REQUESTS_LIST();
        // New: get all user profiles
        Task<IEnumerable<dynamic>> GET_tbl_USER_PROFILE_LIST();
        #endregion

        #region Photo
        DataTable GetAllPhoto(int cat, string pakno);
        #endregion

        #region User Method
        List<AppUser> GetUserAll();
        List<CreateUser> GetUsers();
        List<CreateUser> GetUser(int id);
        Task<AppUser> GetUserById(string id);
        Task<AppUser> GetUserByName(string name);
        int AddUser(CreateUser model);

        List<PromptLog> GetPromptLogs(string userName);
        int AddPromptCount(PromptHistory prompt);
        int EditPromptCount(PromptHistory prompt);

        int ChangePassword(CreateUser user);

        int AddPromptLog(PromptLog promptLog);

        //List<CreateUser> UpdateUser(CreateUser user);

        string UpdateUser(CreateUser user);
        string DeleteUser(CreateUser user);

        List<CreateUser> GetUserDataByName(String username);
        PromptHistory GetPromptCountByName(string username);
        List<CreateUser> UserAuthenticate(CreateUser model);
        CreateUser GetPromptlimitByName(string username);
        #endregion

        #region Role Method
        #endregion

        #region Identity Method
        int Save();
        Task<int> SaveAsync();
        List<IdentityUserRole<string>> GetUserRoleList();

        #endregion

        #region PPortalAuthentication
        Task<IEnumerable<dynamic>> AuthPPortalCiv(string username, string password);
        Task<IEnumerable<dynamic>> AuthPPortalOff(string username, string password);
        Task<IEnumerable<dynamic>> AuthPPortalAir(string username, string password);
        #endregion
    }
}

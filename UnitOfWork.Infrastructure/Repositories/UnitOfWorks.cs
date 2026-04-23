using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Protocols;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Xml;
using UnitOfWork.Core.Interfaces;
using UnitOfWork.Core.Models;
using UnitOfWork.Infrastructure.DbContextClass;
using System.Dynamic;
using Dapper;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using JsonSerializer = System.Text.Json.JsonSerializer;
using Newtonsoft.Json;
using System.Net.Http.Json;

namespace UnitOfWork.Infrastructure.Repositories
{
    public class UnitOfWorks : IUnitOfWork
    {
        public SqlConnection Conn;
        public SqlCommand cmd;
        public SqlDataAdapter da;

        private readonly CERDBContext _dbContext;
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;
        private readonly string? _connectionDefault;

        public UnitOfWorks(CERDBContext dbContext, IConfiguration config, HttpClient httpClient)
        {
            _dbContext = dbContext;
            _config = config;
            _httpClient = httpClient;
            _connectionDefault = config.GetConnectionString("GPTConnections");
        }


        //added by umair
        public async Task<PromptResponse> CHECK_AND_INSERT_PROMPT(string userId, string model, string prompt)
        {
            using var connection = new SqlConnection(_connectionDefault);

            var parameters = new DynamicParameters();
            parameters.Add("@user_id", userId);
            parameters.Add("@model_name", model);
            parameters.Add("@prompt_text", prompt);
            parameters.Add("@is_allowed", dbType: DbType.Boolean, direction: ParameterDirection.Output);
            parameters.Add("@remaining", dbType: DbType.Int32, direction: ParameterDirection.Output);

            await connection.ExecuteAsync(
                "sp_CHECK_AND_INSERT_PROMPT",
                parameters,
                commandType: CommandType.StoredProcedure);

            return new PromptResponse
            {
                allowed = parameters.Get<bool>("@is_allowed"),
                remaining = parameters.Get<int>("@remaining"),
                success = true
            };
        }

        public async Task<FeedbackResponse> SubmitFeedback(FeedbackRequest request)
        {
            
                using var connection = new SqlConnection(_connectionDefault);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_InsertResponseFeedback", connection)
                {
                    CommandType = System.Data.CommandType.StoredProcedure
                };

            // Add parameters with proper mapping from request to SP
            command.Parameters.AddWithValue("@user_id", request.user_id);
            command.Parameters.AddWithValue("@username", (object)request.username ?? DBNull.Value);
            command.Parameters.AddWithValue("@rank_decode", (object)request.rank_decode ?? DBNull.Value);
            command.Parameters.AddWithValue("@base_decode", (object)request.base_decode ?? DBNull.Value);
            command.Parameters.AddWithValue("@model_name", request.model_name);
            command.Parameters.AddWithValue("@user_query", (object)request.user_query ?? DBNull.Value);
            command.Parameters.AddWithValue("@ai_response_preview", (object)request.ai_response_preview ?? DBNull.Value);
            command.Parameters.AddWithValue("@feedback_type", request.feedback_type);
            command.Parameters.AddWithValue("@feedback_comment", (object)request.feedback_comment ?? DBNull.Value);
            command.Parameters.AddWithValue("@session_id", (object)request.session_id ?? DBNull.Value);

            // Execute and get the new ID
            // Execute and get the new ID
            var result = await command.ExecuteScalarAsync();
                long? longFeedbackId = result as long?;
                int? feedbackId = longFeedbackId.HasValue ? (int)longFeedbackId.Value : null;

                return new FeedbackResponse { Success = true, Id = feedbackId };
        }
        
        public async Task<List<FeedbackItemDto>> GetFeedbackList(GetFeedbackListRequest request)
        {
            
                using var connection = new SqlConnection(_connectionDefault);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_GetResponseFeedback", connection)
                {
                    CommandType = System.Data.CommandType.StoredProcedure
                };

            // Add parameters with proper mapping from request to SP
            command.Parameters.AddWithValue("@model_name", (object)request.ModelName ?? DBNull.Value);
            command.Parameters.AddWithValue("@feedback_type", (object)request.FeedbackType ?? DBNull.Value);

            if (request.FromDate.HasValue)
                command.Parameters.AddWithValue("@from_date", request.FromDate.Value.Date); // Use Date only
            else
                command.Parameters.AddWithValue("@from_date", DBNull.Value);

            if (request.ToDate.HasValue)
                command.Parameters.AddWithValue("@to_date", request.ToDate.Value.Date.AddDays(1).AddSeconds(-1)); // End of day
            else
                command.Parameters.AddWithValue("@to_date", DBNull.Value);

            // Note: We're not using user_id and username filters in this API endpoint
            command.Parameters.AddWithValue("@user_id", DBNull.Value);
            command.Parameters.AddWithValue("@username", DBNull.Value);

            var reader = await command.ExecuteReaderAsync();

            var feedbackList = new List<FeedbackItemDto>();

            while (await reader.ReadAsync())
            {
                feedbackList.Add(new FeedbackItemDto
                {
                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                    UserId = reader.IsDBNull(reader.GetOrdinal("user_id")) ? null : reader.GetString(reader.GetOrdinal("user_id")),
                    Username = reader.IsDBNull(reader.GetOrdinal("username")) ? null : reader.GetString(reader.GetOrdinal("username")),
                    RankDecode = reader.IsDBNull(reader.GetOrdinal("rank_decode")) ? null : reader.GetString(reader.GetOrdinal("rank_decode")),
                    BaseDecode = reader.IsDBNull(reader.GetOrdinal("base_decode")) ? null : reader.GetString(reader.GetOrdinal("base_decode")),
                    ModelName = reader.IsDBNull(reader.GetOrdinal("model_name")) ? null : reader.GetString(reader.GetOrdinal("model_name")),
                    UserQuery = reader.IsDBNull(reader.GetOrdinal("user_query")) ? null : reader.GetString(reader.GetOrdinal("user_query")),
                    AiResponsePreview = reader.IsDBNull(reader.GetOrdinal("ai_response_preview")) ? null : reader.GetString(reader.GetOrdinal("ai_response_preview")),
                    FeedbackType = reader.IsDBNull(reader.GetOrdinal("feedback_type")) ? null : reader.GetString(reader.GetOrdinal("feedback_type")),
                    FeedbackComment = reader.IsDBNull(reader.GetOrdinal("feedback_comment")) ? null : reader.GetString(reader.GetOrdinal("feedback_comment")),
                    SessionId = reader.IsDBNull(reader.GetOrdinal("session_id")) ? null : reader.GetString(reader.GetOrdinal("session_id")),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at"))
                });
            }

            return feedbackList;
        }

        public async Task<FeedbackStatsResponse> GetFeedbackStats()
        {
            using var connection = new SqlConnection(_connectionDefault);
            await connection.OpenAsync();

            using var command = new SqlCommand("sp_GetResponseFeedbackStats", connection)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };

            var reader = await command.ExecuteReaderAsync();
            var response = new FeedbackStatsResponse();

            // Process summary data (first result set)
            if (await reader.ReadAsync())
            {
                response.TotalFeedback = reader.GetInt32(reader.GetOrdinal("total_feedback"));
                response.TotalLikes = reader.GetInt32(reader.GetOrdinal("likes_count"));
                response.TotalDislikes = reader.GetInt32(reader.GetOrdinal("dislikes_count"));
                response.LikeRate = Convert.ToDecimal(reader["like_rate_percentage"]);
            }

            // Process model breakdown (second result set)
            if (!reader.IsClosed && await reader.NextResultAsync())
            {
                while (await reader.ReadAsync())
                {
                    var reportType = reader.GetString(reader.GetOrdinal("report_type"));
                    if (reportType == "ModelBreakdown")
                    {
                        response.FeedbackByModel.Add(new ModelBreakdown
                        {
                            ModelName = reader.IsDBNull(reader.GetOrdinal("model_name")) ? null : reader.GetString(reader.GetOrdinal("model_name")),
                            Likes = reader.GetInt32(reader.GetOrdinal("likes")),
                            Dislikes = reader.GetInt32(reader.GetOrdinal("dislikes"))
                        });
                    }
                }
            }

            // Process recent dislikes (third result set)
            if (!reader.IsClosed && await reader.NextResultAsync())
            {
                while (await reader.ReadAsync())
                {
                    var reportType = reader.GetString(reader.GetOrdinal("report_type"));
                    if (reportType == "RecentDislikes")
                    {
                        response.RecentDislikes.Add(new RecentDislike
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("id")),
                            Username = reader.IsDBNull(reader.GetOrdinal("username")) ? null : reader.GetString(reader.GetOrdinal("username")),
                            ModelName = reader.IsDBNull(reader.GetOrdinal("model_name")) ? null : reader.GetString(reader.GetOrdinal("model_name")),
                            UserQuery = reader.IsDBNull(reader.GetOrdinal("user_query")) ? null : reader.GetString(reader.GetOrdinal("user_query")),
                            FeedbackComment = reader.IsDBNull(reader.GetOrdinal("feedback_comment")) ? null : reader.GetString(reader.GetOrdinal("feedback_comment")),
                            CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at"))
                        });
                    }
                }
            }

            return response;
        }

        #region GPT_tbl_USERS
        public Response_DTO AddUser(tbl_USERS_DTO obj)
        {
            Response_DTO res = new Response_DTO();
            try
            {
                using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("GPTConnections")))
                {
                    // Assuming we're calling a different stored procedure that matches your SP parameters
                    SqlCommand cmd = new SqlCommand("sp_tbl_USERS_CRUD", conn);  // Replace with actual SP name

                    // Add the standard user management parameters from your SP definition
                    cmd.Parameters.Add("@Action", SqlDbType.VarChar, 10).Value = obj.Action;  // Default to INSERT for adding a user
                    cmd.Parameters.Add("@id", SqlDbType.Int);
                    cmd.Parameters.Add("@username", SqlDbType.VarChar, 100).Value = obj.username;  // Assuming UserName maps to username
                    cmd.Parameters.Add("@role_id", SqlDbType.Int).Value = obj.role_id; // You might want to set this from your DTO if available

                    // Optional parameters - only add these if they exist in your DTO
                    if (obj.is_active != null) cmd.Parameters.Add("@is_active", SqlDbType.Bit).Value = obj.is_active;
                    if (obj.is_locked != null) cmd.Parameters.Add("@is_locked", SqlDbType.Bit).Value = obj.is_locked;
                    if (obj.is_deleted != null) cmd.Parameters.Add("@is_deleted", SqlDbType.Bit).Value = obj.is_deleted;

                    // If your DTO has an ID property that should be used
                    if (obj.id != 0) cmd.Parameters["@id"].Value = obj.id;

                    cmd.Parameters.Add("@Status", SqlDbType.Bit).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@Message", SqlDbType.VarChar, 200).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@NewId", SqlDbType.Int).Direction = ParameterDirection.Output;
                    // Add return value parameter
                    //cmd.Parameters.Add("@RetVal", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;
                    cmd.CommandType = CommandType.StoredProcedure;

                    conn.Open();

                    cmd.ExecuteNonQuery();

                    res.status = Convert.ToBoolean(cmd.Parameters["@Status"].Value);
                    res.message = cmd.Parameters["@Message"].Value.ToString();
                    res.newId = cmd.Parameters["@NewId"].Value == DBNull.Value
                                 ? null
                                 : (int)cmd.Parameters["@NewId"].Value;

                    // Get the return value if needed
                    //int returnValue = (int)cmd.Parameters["@RetVal"].Value;

                    conn.Close();
                    return res;
                }
            }
            catch (Exception ex)
            {
                res.status = false;
                res.message = ex.Message;
                return res;
                // Consider logging the exception here in a real application
            }
        }

        public Response_DTO CRUD_Model(tbl_MODELS_DTO model)
        {
            Response_DTO res = new Response_DTO();
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionDefault))
                {
                    SqlCommand cmd = new SqlCommand("sp_tbl_MODELS_CRUD", conn);

                    // Set action to INSERT
                    cmd.Parameters.Add("@Action", SqlDbType.VarChar, 10).Value = model.Action;

                    // Add all parameters with null checks
                    if (model.id.HasValue)
                        cmd.Parameters.Add("@id", SqlDbType.Int).Value = model.id.Value;

                    cmd.Parameters.Add("@name", SqlDbType.VarChar, 100).Value = model.name ?? string.Empty;
                    cmd.Parameters.Add("@type", SqlDbType.VarChar, 20).Value = model.type ?? string.Empty;
                    cmd.Parameters.Add("@description", SqlDbType.Text).Value = model.description ?? string.Empty;

                    if (model.context_window_tokens.HasValue)
                        cmd.Parameters.Add("@context_window_tokens", SqlDbType.Int).Value = model.context_window_tokens.Value;

                    cmd.Parameters.Add("@features", SqlDbType.NVarChar, -1).Value = model.features ?? string.Empty;
                    cmd.Parameters.Add("@is_locked", SqlDbType.Bit).Value = model.is_locked ?? false;
                    cmd.Parameters.Add("@icon_class", SqlDbType.VarChar, 100).Value = model.icon_class ?? string.Empty;
                    cmd.Parameters.Add("@access_level", SqlDbType.VarChar, 20).Value = model.access_level ?? string.Empty;

                    // Default to enabled if not specified
                    cmd.Parameters.Add("@is_enabled", SqlDbType.Bit).Value = model.is_enabled ?? true;

                    // User information (assuming these are IDs)
                    if (model.created_by.HasValue)
                        cmd.Parameters.Add("@created_by", SqlDbType.Int).Value = model.created_by.Value;
                    else
                        cmd.Parameters.Add("@created_by", SqlDbType.Int).Value = DBNull.Value; // Or set default user

                    cmd.Parameters.Add("@Status", SqlDbType.Bit).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@Message", SqlDbType.VarChar, 200).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@NewId", SqlDbType.Int).Direction = ParameterDirection.Output;

                    cmd.CommandType = CommandType.StoredProcedure;

                    conn.Open();

                    cmd.ExecuteNonQuery();

                    res.status = Convert.ToBoolean(cmd.Parameters["@Status"].Value);
                    res.message = cmd.Parameters["@Message"].Value.ToString();
                    res.newId = cmd.Parameters["@NewId"].Value == DBNull.Value
                                 ? null
                                 : (int)cmd.Parameters["@NewId"].Value;

                    conn.Close();

                    // Get the newly inserted ID if needed (would need output parameter in SP)

                    return res;
                }
            }
            catch (Exception ex)
            {
                // Log exception
                res.status = false;
                res.message = ex.Message;
                return res;
            }
        }

        public async Task<dynamic> GET_USER_QUOTA(string user_id)
        {
            try
            {
                using var connection = CreateConnection();
                var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    "sp_GET_USER_QUOTA",
                    new { user_id },
                    commandType: CommandType.StoredProcedure);
                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public Response_DTO CRUD_USER_BASED_REQUESTS(UserBasedRequest_DTO request)
        {
            Response_DTO res = new Response_DTO();
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionDefault))
                {
                    SqlCommand cmd = new SqlCommand("sp_tbl_USER_BASED_REQUESTS_CRUD", conn);

                    // Set action type
                    cmd.Parameters.Add("@Action", SqlDbType.VarChar, 10).Value = request.Action;

                    // Add all existing parameters with null checks
                    if (request.id.HasValue)
                        cmd.Parameters.Add("@id", SqlDbType.Int).Value = request.id.Value;

                    if (request.model_id.HasValue)
                        cmd.Parameters.Add("@model_id", SqlDbType.Int).Value = request.model_id.Value;

                    if (request.user_id.HasValue)
                        cmd.Parameters.Add("@user_id", SqlDbType.Int).Value = request.user_id.Value;

                    cmd.Parameters.Add("@detail_justification", SqlDbType.Text).Value =
                        request.detail_justification ?? string.Empty;

                    cmd.Parameters.Add("@priority", SqlDbType.VarChar, 50).Value =
                        request.priority ?? string.Empty;

                    cmd.Parameters.Add("@tel_ext", SqlDbType.VarChar, 100).Value =
                        request.tel_ext ?? string.Empty;

                    cmd.Parameters.Add("@additional_notes", SqlDbType.Text).Value =
                        request.additional_notes ?? string.Empty;

                    cmd.Parameters.Add("@admin_notes", SqlDbType.Text).Value =
                        request.admin_notes ?? string.Empty;

                    cmd.Parameters.Add("@status", SqlDbType.VarChar, 20).Value =
                        request.status ?? string.Empty;

                    if (request.total_promptperday.HasValue)
                        cmd.Parameters.Add("@total_promptperday", SqlDbType.Int).Value = request.total_promptperday.Value;

                    if (request.reviewed_by.HasValue)
                        cmd.Parameters.Add("@reviewed_by", SqlDbType.Int).Value = request.reviewed_by.Value;

                    // Add new parameters from SQL stored procedure
                    cmd.Parameters.Add("@RankCode", SqlDbType.VarChar, 50).Value =
                        request.RankCode ?? string.Empty;

                    cmd.Parameters.Add("@RankDecode", SqlDbType.VarChar, 300).Value =
                        request.RankDecode ?? string.Empty;

                    cmd.Parameters.Add("@Username", SqlDbType.VarChar, 100).Value =
                        request.Username ?? string.Empty;

                    cmd.Parameters.Add("@PType", SqlDbType.VarChar, 50).Value =
                        request.PType ?? string.Empty;

                    cmd.Parameters.Add("@BaseCode", SqlDbType.VarChar, 50).Value =
                        request.BaseCode ?? string.Empty;

                    cmd.Parameters.Add("@BaseDecode", SqlDbType.VarChar, 300).Value =
                        request.BaseDecode ?? string.Empty;

                    cmd.Parameters.Add("@UnitCode", SqlDbType.VarChar, 50).Value =
                        request.UnitCode ?? string.Empty;

                    cmd.Parameters.Add("@UnitDecode", SqlDbType.VarChar, 300).Value =
                        request.UnitDecode ?? string.Empty;

                    // Output parameters
                    cmd.Parameters.Add("@Status2", SqlDbType.Bit).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@Message", SqlDbType.VarChar, 200).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@NewId", SqlDbType.Int).Direction = ParameterDirection.Output;

                    cmd.CommandType = CommandType.StoredProcedure;

                    conn.Open();
                    cmd.ExecuteNonQuery();

                    res.status = Convert.ToBoolean(cmd.Parameters["@Status2"].Value);
                    res.message = cmd.Parameters["@Message"].Value.ToString();
                    res.newId = cmd.Parameters["@NewId"].Value == DBNull.Value
                                 ? null
                                 : (int?)cmd.Parameters["@NewId"].Value;

                    conn.Close();

                    return res;
                }
            }
            catch (Exception ex)
            {
                // Log exception
                res.status = false;
                res.message = $"Error: {ex.Message}";
                return res;         
            }
        }


        public async Task<IEnumerable<dynamic>> GET_tbl_MODELS_LIST(int? id)
        {
            try
            {
                using var connection = CreateConnection();

                var result = await connection.QueryAsync<dynamic>(
                    "sp_GET_tbl_MODELS_LIST",
                    new
                    {
                        userid = id
                    },
                    commandType: System.Data.CommandType.StoredProcedure
                );

                return result;
            }
            catch
            {
                return null;
            }
        }

        public async Task<IEnumerable<dynamic>> GET_tbl_USER_BY_ID(int id)
        {
            try
            {
                using var connection = CreateConnection();
                var result = await connection.QueryAsync<dynamic>("sp_GET_tbl_USER_BY_ID", new
                {
                    @id = id
                });
                return result;
            }
            catch(Exception ex)
            {
                return null;
            }
        }
        
        public async Task<IEnumerable<dynamic>> GET_tbl_USER_BY_NAME(string username)
        {
            try
            {
                using var connection = CreateConnection();
                var result = await connection.QueryAsync<dynamic>("sp_GET_tbl_USER_BY_NAME", new
                {
                    @username = username
                });
                return result;
            }
            catch(Exception ex)
            {
                return null;
            }
        } 
        
        public async Task<IEnumerable<dynamic>> GET_tbl_USER_BASED_REQUESTS_LIST()
        {
            try
            {
                using var connection = CreateConnection();
                var result = await connection.QueryAsync<dynamic>("sp_GET_tbl_USER_BASED_REQUESTS_LIST");
                return result;
            }
            catch(Exception ex)
            {
                return null;
            }
        }

        public async Task<IEnumerable<dynamic>> GET_tbl_USER_PROFILE_LIST()
        {
            try
            {
                using var connection = CreateConnection();
                var result = await connection.QueryAsync<dynamic>("sp_GET_tbl_USER_PROFILE_LIST");
                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private IDbConnection CreateConnection()
        {
            return new SqlConnection(_connectionDefault);
        }


        #endregion

        #region CodeCraftCode 2025


        public async Task<IEnumerable<Token>> GetTokenListAsync()
        {
            return await _dbContext.TokenTb.ToListAsync();
        }

        public async Task<IEnumerable<Prompt>> GetPromptListAsync()
        {
            return await _dbContext.PromptTb.ToListAsync();
        }

        public async Task<IEnumerable<Model>> GetModelListAsync()
        {
            return await _dbContext.ModelTb.ToListAsync();
        }

        public async Task<ApiResponse> GetPPortalAuthByUID(string uId)
        {
            var requestBody = new { Uid = uId };
            using var httpContent = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json");

            using var response = await _httpClient.PostAsync(
                "https://172.32.3.219/Personal_Intranet_API/api/SoapCheck",
                httpContent);

            response.EnsureSuccessStatusCode();
            string json = await response.Content.ReadAsStringAsync();

            // Deserialize the JSON into ApiResponse
            return JsonSerializer.Deserialize<ApiResponse>(json);
        }


    // No change to your registration – you keep the single client.
    //public async Task<string> GetPPortalAuthByUID(string uId)
    //{
    //    dynamic obj = new ExpandoObject();
    //    obj.Uid = uId;

    //    var jsonContent = JsonSerializer.Serialize(obj);
    //    var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

    //    // Notice the *full* URL here:
    //    var response = await _httpClient.PostAsync(
    //        "https://172.32.3.219/Personal_Intranet_API/api/SoapCheck",
    //        httpContent);

    //    response.EnsureSuccessStatusCode();
    //    var result = await response.Content.ReadAsStringAsync();

    //    //var listdata = JsonConvert.DeserializeObject<List<LoginUser_DTO>>(result);
    //    //return listdata.First();
    //    return result;
    //}


    public async Task<LoginUser_DTO> GetPersonalBasicDataCMD(string ptype, string pakno)
        {
            try
            {
                // Create a new ExpandoObject
                dynamic user = new ExpandoObject();
                // Dynamically add properties
                user.pType = ptype;
                user.pakno = pakno;
                var jsonContent = JsonSerializer.Serialize(user);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("PrismCodesHR/GetBasicDataCmd", httpContent);
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync();
                var listdata = JsonConvert.DeserializeObject<List<LoginUser_DTO>>(result);
                return listdata.First();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Save initial user profile into USER_PROFILE table (via stored procedure).
        /// Uses GPTConnections (_connectionDefault) as other GPT-related operations do.
        /// </summary>
        public Response_DTO AddUserProfile(int userId, LoginUser_DTO profile)
        {
            var res = new Response_DTO();
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionDefault))
                {
                    using (SqlCommand cmd = new SqlCommand("sp_tbl_USER_PROFILE_CRUD", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("@Action", SqlDbType.VarChar, 10).Value = "INSERT";
                        cmd.Parameters.Add("@user_id", SqlDbType.Int).Value = userId;
                        cmd.Parameters.Add("@pakno", SqlDbType.VarChar, 50).Value = profile.pakno ?? string.Empty;
                        cmd.Parameters.Add("@ptype", SqlDbType.Int).Value = profile.peR_TYPE ?? null;
                        cmd.Parameters.Add("@full_name", SqlDbType.VarChar, 200).Value = profile.fulL_NAME ?? string.Empty;
                        cmd.Parameters.Add("@rank_code", SqlDbType.VarChar, 50).Value = profile.currenT_RANK_CODE ?? string.Empty;
                        cmd.Parameters.Add("@rank_decode", SqlDbType.VarChar, 300).Value = profile.currenT_RANK_DECODE ?? string.Empty;

                        if (profile.basE_CODE.HasValue)
                            cmd.Parameters.Add("@base_code", SqlDbType.Int).Value = profile.basE_CODE.Value;
                        else
                            cmd.Parameters.Add("@base_code", SqlDbType.Int).Value = DBNull.Value;

                        cmd.Parameters.Add("@base_decode", SqlDbType.VarChar, 300).Value = profile.basE_DECODE ?? string.Empty;

                        if (profile.uniT_CODE.HasValue)
                            cmd.Parameters.Add("@unit_code", SqlDbType.Int).Value = profile.uniT_CODE.Value;
                        else
                            cmd.Parameters.Add("@unit_code", SqlDbType.Int).Value = DBNull.Value;

                        cmd.Parameters.Add("@unit_decode", SqlDbType.VarChar, 300).Value = profile.uniT_DECODE ?? string.Empty;

                        if (profile.sectioN_CODE.HasValue)
                            cmd.Parameters.Add("@section_code", SqlDbType.Int).Value = profile.sectioN_CODE.Value;
                        else
                            cmd.Parameters.Add("@section_code", SqlDbType.Int).Value = DBNull.Value;

                        cmd.Parameters.Add("@section_decode", SqlDbType.VarChar, 300).Value = profile.sectioN_DECODE ?? string.Empty;

                        cmd.Parameters.Add("@main_branch_code", SqlDbType.VarChar, 50).Value = profile.maiN_BRANCH_CODE ?? string.Empty;

                        cmd.Parameters.Add("@main_branch_decode", SqlDbType.VarChar, 300).Value = profile.maiN_BRANCH_DECODE ?? string.Empty;


                        cmd.Parameters.Add("@Status", SqlDbType.Bit).Direction = ParameterDirection.Output;
                        cmd.Parameters.Add("@Message", SqlDbType.VarChar, 200).Direction = ParameterDirection.Output;
                        cmd.Parameters.Add("@NewId", SqlDbType.Int).Direction = ParameterDirection.Output;

                        conn.Open();
                        cmd.ExecuteNonQuery();

                        res.status = Convert.ToBoolean(cmd.Parameters["@Status"].Value);
                        res.message = cmd.Parameters["@Message"].Value.ToString();
                        res.newId = cmd.Parameters["@NewId"].Value == DBNull.Value
                                     ? null
                                     : (int?)cmd.Parameters["@NewId"].Value;

                        conn.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                res.status = false;
                res.message = ex.Message;
            }

            return res;
        }

        public Response_DTO SetUserQuota(string user_id, int dailyLimit)
        {
            var res = new Response_DTO();
            try
            {
                using (var conn = new SqlConnection(_connectionDefault))
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    // Use MERGE to upsert quota row
                    cmd.CommandText = @"
MERGE dbo.tbl_USER_TOTAL_QUOTA AS target
USING (SELECT @user_id AS user_id, @dailyLimit AS daily_total_limit) AS src
ON target.user_id = src.user_id
WHEN MATCHED THEN
    UPDATE SET daily_total_limit = src.daily_total_limit, created_at = GETDATE()
WHEN NOT MATCHED THEN
    INSERT (user_id, daily_total_limit, created_at) VALUES (src.user_id, src.daily_total_limit, GETDATE());
";
                    cmd.Parameters.Add(new SqlParameter("@user_id", SqlDbType.VarChar, 50) { Value = user_id });
                    cmd.Parameters.Add(new SqlParameter("@dailyLimit", SqlDbType.Int) { Value = dailyLimit });

                    conn.Open();
                    var affected = cmd.ExecuteNonQuery();
                    res.status = true;
                    res.message = "Quota set successfully";
                    res.newId = null;
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                res.status = false;
                res.message = ex.Message;
            }
            return res;
        }

        #endregion

        #region Photo
        public DataTable GetAllPhoto(int cat, string pakno)
        {
            DataTable dt = new DataTable();
            try
            {
                string conStringOffPhoto = _config.GetConnectionString("OffPhoto");
                string conStringAirPhoto = _config.GetConnectionString("AirPhoto");
                string conStringCivPhoto = _config.GetConnectionString("CivPhoto");
                if (cat != 0)
                {
                    if (cat == 1) //for Officer Photo
                    {
                        Conn = new SqlConnection(conStringOffPhoto);
                        cmd = new SqlCommand("Get_photoRecordForERP", Conn);
                    }
                    else if (cat == 2) //for Civ Photo 
                    {
                        Conn = new SqlConnection(conStringAirPhoto);
                        cmd = new SqlCommand("Get_photoRecordAir", Conn);
                    }
                    else if (cat == 3) //for Civ Photo 
                    {
                        Conn = new SqlConnection(conStringCivPhoto);
                        cmd = new SqlCommand("Get_photoRecordCiv", Conn);
                    }
                    else { return dt; }

                    cmd.Parameters.AddWithValue("@relCode", 1);
                    cmd.Parameters.AddWithValue("@pakNo", pakno);
                    cmd.Parameters.AddWithValue("@category", cat);
                    cmd.CommandType = CommandType.StoredProcedure;
                    Conn.Open();
                    da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                    Conn.Close();
                    //var bytearry = dt.Rows[0]["PicBytes"];
                    return dt;
                }
                else
                {
                    return dt;
                }
            }

            catch (Exception ex)
            {
                return dt;
            }
            finally
            {
                dt.Dispose();
                Conn.Close();
                Conn.Dispose();
            }
        }
        #endregion

        #region User Method

        public int AddPromptLog(PromptLog promptLog)
        {
            promptLog.TimeStamp = DateTime.Now;
            _dbContext.PromptLogTb.Add(promptLog);
            return _dbContext.SaveChanges();
        }

        public List<PromptLog> GetPromptLogs(string userName)
        {
            return _dbContext.PromptLogTb.Where(p => p.UserName == userName).OrderByDescending(p => p.TimeStamp).Take(30).ToList();
        }

        public List<AppUser> GetUserAll()
        {
            return _dbContext.Users.ToList();
        }

        public List<CreateUser> GetUsers()
        {
            return _dbContext.CreateUserTb
                .Include(p => p.role)
                .Include(p => p.model)
                .Include(p => p.prompt)
                .ToList();
        }

        public List<CreateUser> GetUser(int id)
        {
            return _dbContext.CreateUserTb
                .Include(p => p.role)
                .Include(p => p.model)
                .Include(p => p.prompt).Where(p => p.Id == id)
                .ToList();
        }

        public List<CreateUser> GetUserDataByName(String username)
        {
            var result = _dbContext.CreateUserTb.Include(p => p.role)
                 .Include(p => p.model)
                 .Include(p => p.prompt).Where(p => p.UserName == username).ToList();

            return result;// _dbContext.SaveChanges();
        }

        public int AddPromptCount(PromptHistory prompt)
        {
            _dbContext.PromptHistoryTb.Add(prompt);
            return _dbContext.SaveChanges();
        }
        public PromptHistory GetPromptCountByName(string username)
        {
            var result = _dbContext.PromptHistoryTb.Where((p) => p.UserName == username && p.TimeStamp.Date == DateTime.Now.Date).ToList();
            var result2 = result.FirstOrDefault();
            return result2;
        }

        public CreateUser GetPromptlimitByName(string username)
        {
            var result = _dbContext.CreateUserTb.Where(p => p.UserName == username)
                .Include(p => p.prompt).ToList();
            return result.FirstOrDefault();

        }
        public int EditPromptCount(PromptHistory prompt)
        {
            _dbContext.PromptHistoryTb.Update(prompt);
            return _dbContext.SaveChanges();
        }

        //public List<CreateUser> UpdateUser(CreateUser user)
        //{

        //    var res = _dbContext.CreateUserTb.Update(user);

        //    _dbContext.SaveChanges();

        //    var result = new List<CreateUser>();
        //    return result;

        //}

        public string UpdateUser(CreateUser user)
        {
            string result = "false";
            var dbUser = _dbContext.CreateUserTb.FirstOrDefault(p => p.Id == user.Id);

            //if (dbUser == null)
            //{
            //    return NotFound();
            //}
            // dbUser.UserName = user.UserName;

            dbUser.FullName = user.FullName;
            dbUser.PhoneNumber = user.PhoneNumber;
            dbUser.Password = user.Password;
            dbUser.role = _dbContext.RoleTb.FirstOrDefault(p => p.Id == user.role.Id);
            dbUser.prompt = _dbContext.PromptTb.FirstOrDefault(p => p.Id == user.prompt.Id);
            dbUser.model = _dbContext.ModelTb.FirstOrDefault(p => p.Id == user.model.Id);
            dbUser.TokenAllowed = user.TokenAllowed;

            if (dbUser != null)
            {

                _dbContext.SaveChanges();

                result = "User Data Updated Successfully";
            }
            else
            {
                result = "false";

            }

            return result;
        }

        public string DeleteUser(CreateUser user)
        {
            var dbUser = _dbContext.CreateUserTb.FirstOrDefault(p => p.Id == user.Id);

            //if (dbUser == null)
            //{
            //    return NotFound();
            //}

            if (dbUser != null)
            {
                _dbContext.CreateUserTb.Remove(dbUser);
                _dbContext.SaveChanges();

                return "User Deleted Succesfully";
            }
            return "User Deleted Succesfully";
        }

        public async Task<AppUser> GetUserById(string id)
        {
            return await _dbContext.Users.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<AppUser> GetUserByName(string name)
        {
            return await _dbContext.Users.FirstOrDefaultAsync(p => p.UserName.ToLower() == name.ToLower());
        }

        public List<CreateUser> UserAuthenticate(CreateUser model)
        {

            List<CreateUser> createUsers;
            var dbUser = _dbContext.CreateUserTb.FirstOrDefault(p => p.UserName == model.UserName);

            if (dbUser != null)
            {
                return _dbContext.CreateUserTb

                    .Where(p => p.UserName == model.UserName && dbUser.Password == model.Password).Include(p => p.role)
                    .Include(p => p.model)
                    .Include(p => p.prompt).ToList();
            }
            else
            {

                createUsers = new List<CreateUser>();

                createUsers.Add(model);
                createUsers.Add(model);
                return createUsers;
            }
        }

        public int ChangePassword(CreateUser user)
        {
            int result = 0;

            var dbUser = _dbContext.CreateUserTb.FirstOrDefault(p => p.Id == user.Id);

            if (dbUser != null)
            {
                dbUser.Password = user.Password;
                _dbContext.SaveChanges();

                result = 1;
            }
            else
            {
                result = 2;
            }
            return result;
        }

        public int AddUser(CreateUser model)
        {
            int result = 0;
            var dbUser = _dbContext.CreateUserTb.FirstOrDefault(p => p.UserName == model.UserName);
            model.role = _dbContext.RoleTb.FirstOrDefault(p => p.Id == model.role.Id);
            model.prompt = _dbContext.PromptTb.FirstOrDefault(p => p.Id == model.prompt.Id);
            model.model = _dbContext.ModelTb.FirstOrDefault(p => p.Id == model.model.Id);
            if (dbUser == null)
            {
                _dbContext.CreateUserTb.Add(model);
                _dbContext.SaveChanges();
                result = 1;
            }
            else if (dbUser.UserName != model.UserName)
            {
                _dbContext.CreateUserTb.Add(model);
                _dbContext.SaveChanges();
                result = 1;
            }
            else
            {
                result = 2;
            }
            return result;
        }

        



        #endregion

        #region Identity Method
        public int Save()
        {
            return _dbContext.SaveChanges();
        }

        public async Task<int> SaveAsync()
        {
            return await _dbContext.SaveChangesAsync();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _dbContext.Dispose();
            }
        }

        public List<IdentityUserRole<string>> GetUserRoleList()
        {
            return _dbContext.UserRoles.ToList();
        }
        #endregion

        #region PPortalAuthentication
        public async Task<IEnumerable<dynamic>> AuthPPortalCiv(string username, string password)
        {
            try {
                using var connection = CreateConnectionPPortalAirCivOff("PPC");
                var result = await connection.QueryAsync<dynamic>("SP_ValidateUserPersonalPortalCivillian", new
                {
                    @UserName = username,
                    @Password = password
                });
                return result;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IEnumerable<dynamic>> AuthPPortalOff(string username, string password)
        {
            using var connection = CreateConnectionPPortalAirCivOff("PPO");
            var result = await connection.QueryAsync<dynamic>("SP_ValidateUserPersonalPortalOfficer", new
            {
                @UserName = username,
                @Password = password
            });
            return result;
        }

        public async Task<IEnumerable<dynamic>> AuthPPortalAir(string username, string password)
        {
            using var connection = CreateConnectionPPortalAirCivOff("PPA");
            var result = await connection.QueryAsync<dynamic>("SP_ValidateUserPersonalPortalAirmen", new
            {
                @UserName = username,
                @Password = password
            });
            return result;
        }
        #endregion

        private IDbConnection CreateConnectionPPortalAirCivOff(string connType)
        {
            if (connType == "PPA") return new SqlConnection(_config.GetConnectionString("DefaultPPortalAirmen"));
            else if (connType == "PPC") return new SqlConnection(_config.GetConnectionString("DefaultPPortalCivilian"));
            else if (connType == "PPO") return new SqlConnection(_config.GetConnectionString("DefaultPPortal"));
            else if (connType == "Default") return new SqlConnection(_config.GetConnectionString("DefaultConnections"));
            else return null;
        }
        // Add these methods into the existing UnitOfWorks class (near other CRUD / GET methods).

        public Response_DTO CRUD_tbl_USER_PROMPTS(tbl_USER_PROMPTS_DTO model)
        {
            var res = new Response_DTO();
            try
            {
                using (var conn = new SqlConnection(_connectionDefault))
                using (var cmd = new SqlCommand("sp_tbl_USER_PROMPTS_CRUD", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@Action", SqlDbType.VarChar, 10).Value = (object)model.Action ?? "INSERT";
                    cmd.Parameters.Add("@user_id", SqlDbType.VarChar, 50).Value = (object)model.user_id ?? DBNull.Value;
                    cmd.Parameters.Add("@username", SqlDbType.NVarChar, 100).Value = (object)model.username ?? DBNull.Value;
                    cmd.Parameters.Add("@rank_decode", SqlDbType.NVarChar, 50).Value = (object)model.rank_decode ?? DBNull.Value;
                    cmd.Parameters.Add("@base_decode", SqlDbType.NVarChar, 100).Value = (object)model.base_decode ?? DBNull.Value;
                    cmd.Parameters.Add("@model_name", SqlDbType.NVarChar, 100).Value = (object)model.model_name ?? DBNull.Value;

                    var promptText = (model.prompt_text ?? string.Empty);
                    if (promptText.Length > 2000) promptText = promptText.Substring(0, 2000);
                    cmd.Parameters.Add("@prompt_text", SqlDbType.NVarChar, 2000).Value = (object)promptText ?? DBNull.Value;

                    cmd.Parameters.Add("@has_attachments", SqlDbType.Bit).Value = (object)model.has_attachments ?? false;
                    cmd.Parameters.Add("@attachment_names", SqlDbType.NVarChar, 500).Value = (object)model.attachment_names ?? DBNull.Value;
                    cmd.Parameters.Add("@session_id", SqlDbType.VarChar, 50).Value = (object)model.session_id ?? DBNull.Value;
                    cmd.Parameters.Add("@timestamp", SqlDbType.DateTime).Value = (object)model.timestamp ?? DBNull.Value;

                    cmd.Parameters.Add("@Status", SqlDbType.Bit).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@Message", SqlDbType.VarChar, 200).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@NewId", SqlDbType.Int).Direction = ParameterDirection.Output;

                    conn.Open();
                    cmd.ExecuteNonQuery();

                    res.status = Convert.ToBoolean(cmd.Parameters["@Status"].Value);
                    res.message = cmd.Parameters["@Message"].Value?.ToString();
                    res.newId = cmd.Parameters["@NewId"].Value == DBNull.Value ? null : (int?)cmd.Parameters["@NewId"].Value;
                }
            }
            catch (Exception ex)
            {
                res.status = false;
                res.message = ex.Message;
            }

            return res;
        }

        public async Task<IEnumerable<dynamic>> GET_tbl_USER_PROMPTS_TOP(int limit = 20)
        {
            if (limit <= 0) limit = 20;
            if (limit > 50) limit = 50;

            try
            {
                using var connection = CreateConnection();
                var result = await connection.QueryAsync<dynamic>(
                    "sp_GET_tbl_USER_PROMPTS_TOP",
                    new { limit },
                    commandType: CommandType.StoredProcedure);
                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<IEnumerable<dynamic>> GET_tbl_USER_PROMPTS_BY_USER(string user_id, int limit = 20)
        {
            if (string.IsNullOrEmpty(user_id)) return Enumerable.Empty<dynamic>();
            if (limit <= 0) limit = 20;
            if (limit > 100) limit = 100;

            try
            {
                using var connection = CreateConnection();
                var result = await connection.QueryAsync<dynamic>(
                    "sp_GET_tbl_USER_PROMPTS_BY_USER",
                    new { user_id, limit },
                    commandType: CommandType.StoredProcedure);
                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }
        
    }
}

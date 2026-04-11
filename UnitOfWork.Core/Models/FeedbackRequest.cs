using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitOfWork.Core.Models
{

    public class FeedbackResponse
    {
        public bool Success { get; set; }
        public int? Id { get; set; } // Null if error occurs
        public string ErrorMessage { get; set; }
    }


    public class FeedbackRequest
    {

        [Required]
        public string Action { get; set; } // "INSERT" (unchanged)

        [Required]
        [MaxLength(50)]
        public string user_id { get; set; }  // Changed from UserId → user_id

        [MaxLength(100)]
        public string username { get; set; }  // Changed from Username → username

        [MaxLength(50)]
        public string rank_decode { get; set; }  // Changed from RankDecode → rank_decode

        [MaxLength(100)]
        public string base_decode { get; set; }  // Changed from BaseDecode → base_decode

        [Required]
        [MaxLength(100)]
        public string model_name { get; set; }  // Changed from ModelName → model_name

        public string user_query { get; set; }   // Changed from UserQuery → user_query (MAX length in DB)

        [MaxLength(500)]
        public string ai_response_preview { get; set; }  // Changed from AiResponsePreview → ai_response_preview

        [Required]
        [RegularExpression("^(like|dislike)$", ErrorMessage = "Feedback type must be 'like' or 'dislike'")]
        public string feedback_type { get; set; }  // Changed from FeedbackType → feedback_type

        [MaxLength(500)]
        public string feedback_comment { get; set; }  // Changed from FeedbackComment → feedback_comment

        [MaxLength(50)]
        public string session_id { get; set; }   // Changed from SessionId → session_id
        //public DateTime timestamp { get; set; }   // Changed from SessionId → session_id
    }

}

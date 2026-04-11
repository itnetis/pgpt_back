using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitOfWork.Core.Models
{
    public class FeedbackItemDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Username { get; set; }
        public string RankDecode { get; set; }
        public string BaseDecode { get; set; }
        public string ModelName { get; set; }
        public string UserQuery { get; set; }
        public string AiResponsePreview { get; set; }
        public string FeedbackType { get; set; }
        public string FeedbackComment { get; set; }
        public string SessionId { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class GetFeedbackListRequest
    {
        public string? ModelName { get; set; }
        public string? FeedbackType { get; set; }

        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}

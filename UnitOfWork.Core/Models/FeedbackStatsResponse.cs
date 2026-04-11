using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitOfWork.Core.Models
{
    // FeedbackStatsResponse.cs (in DTOs folder)
    public class FeedbackStatsResponse
    {
        public int TotalFeedback { get; set; }
        public int TotalLikes { get; set; }
        public int TotalDislikes { get; set; }
        public decimal LikeRate { get; set; }

        public List<ModelBreakdown> FeedbackByModel { get; set; } = new();
        public List<RecentDislike> RecentDislikes { get; set; } = new();
    }

    public class ModelBreakdown
    {
        public string? ModelName { get; set; }
        public int Likes { get; set; }
        public int Dislikes { get; set; }
    }

    public class RecentDislike
    {
        public int Id { get; set; }
        public string? Username { get; set; }
        public string? ModelName { get; set; }
        public string? UserQuery { get; set; }
        public string? FeedbackComment { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}

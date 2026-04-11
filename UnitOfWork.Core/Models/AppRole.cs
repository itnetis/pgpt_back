using Microsoft.AspNetCore.Identity;

namespace UnitOfWork.Core.Models
{
    public class AppRole : IdentityRole
    {
        public string Description { get; set; }
    }
}

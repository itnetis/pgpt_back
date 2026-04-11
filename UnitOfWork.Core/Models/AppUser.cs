using Microsoft.AspNetCore.Identity;

namespace UnitOfWork.Core.Models
{
    public class AppUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public int? Category { get; set; }
        public string? Formation { get; set; }
        public int? PayScale { get; set; }
        public bool? IsDCP { get; set; }
    }
}

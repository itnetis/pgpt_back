using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UnitOfWork.Core.Models;

namespace UnitOfWork.Infrastructure.DbContextClass
{
    public class CERDBContext : IdentityDbContext<AppUser, AppRole, string>
    {
        public CERDBContext(DbContextOptions<CERDBContext> contextOptions) : base(contextOptions)
        {
        }
        public DbSet<Role>  RoleTb { get; set; }
        public DbSet<Token> TokenTb { get; set; }
        public DbSet<CreateUser> CreateUserTb { get; set; } 
        public DbSet<Prompt>  PromptTb { get; set; }
        public DbSet<Model> ModelTb { get; set; }
        public DbSet<PromptHistory> PromptHistoryTb { get; set; }
        public DbSet<PromptLog> PromptLogTb { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}

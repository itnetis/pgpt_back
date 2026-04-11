using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using UnitOfWork.Core.Interfaces;
using UnitOfWork.Core.Models;
using UnitOfWork.Infrastructure.DbContextClass;
using UnitOfWork.Infrastructure.Repositories;
using UnitOfWork.Services;
using UnitOfWork.Services.Interfaces;
using UnitOfWork.WebAPI.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<CERDBContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnections"));
});
builder.Services.AddIdentity<AppUser, AppRole>().AddEntityFrameworkStores<CERDBContext>()
.AddDefaultTokenProviders();
builder.Services.AddScoped<IUnitOfWork, UnitOfWorks>();
builder.Services.AddScoped<IRepositoryServices, RepositoryServices>();
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 3;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
});

var AppSettings = builder.Configuration.GetSection("Jwt").Get<AppSetting>();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = AppSettings.Issuer,
        ValidAudience = AppSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(AppSettings.Key)),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = false,
        ValidateIssuerSigningKey = true
    };
});

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        Policy =>
        {
            Policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
        });
});

builder.Services.Configure<FormOptions>(o =>
{
    o.ValueLengthLimit = Int32.MaxValue;
    o.MultipartBodyLengthLimit = Int64.MaxValue;
    o.MemoryBufferThreshold = Int32.MaxValue;
    o.MultipartBoundaryLengthLimit = Int32.MaxValue;
    o.MultipartHeadersLengthLimit = Int32.MaxValue;

});

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = long.MaxValue;
});

builder.Services.AddHttpClient<IUnitOfWork, UnitOfWorks>(client =>
{
    client.BaseAddress = new Uri("https://172.32.3.46:4445/api/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

var app = builder.Build();

//using (var scope = app.Services.CreateScope())
//{
//    var _dbcontext = (CER_DBContext)scope.ServiceProvider.GetService(typeof(CER_DBContext));
//    var _userManager = (UserManager<AppUser>)scope.ServiceProvider.GetService(typeof(UserManager<AppUser>));
//    var _roleManager = (RoleManager<AppRole>)scope.ServiceProvider.GetService(typeof(RoleManager<AppRole>));
//    DbInitializer.Initialize(_dbcontext, _userManager, _roleManager);
//}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
    {
        //app.UseSwagger();
        //app.UseSwaggerUI();
    }

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors();
app.MapControllers();
app.Run();

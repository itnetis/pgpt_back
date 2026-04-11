using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnitOfWork.Core.Models;
using UnitOfWork.Infrastructure.DbContextClass;

namespace UnitOfWork.WebAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class WeatherForecastController : ControllerBase
    {
        private CERDBContext _context;
        public WeatherForecastController(CERDBContext context)
        {
            _context = context;
        }
    }
}
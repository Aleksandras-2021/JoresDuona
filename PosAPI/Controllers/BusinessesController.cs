using Microsoft.AspNetCore.Mvc;
using PosAPI.Data.DbContext;
using PosShared.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PosAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BusinessesController : ControllerBase
    {
        private ApplicationDbContext _dbContext;


        public BusinessesController(ApplicationDbContext context) => _dbContext = context;


        [HttpGet("{id}")]
        public IActionResult GetBusiness(int id)
        {
            var result = _dbContext.Businesses.Find(id);
            if (result != null)
                return Ok(result);
            else
                return NotFound();
        }

        [HttpPost]
        public JsonResult CreateBusiness(Business business)
        {
            if (business != null)
            {
                business.Type = BusinessType.Catering;
                var result = _dbContext.Add(business);
                return new JsonResult(result);
            }
            else return new JsonResult(NotFound());

        }

    }
}

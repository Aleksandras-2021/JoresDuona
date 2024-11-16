using Microsoft.AspNetCore.Mvc;
using PosAPI.Data.DbContext;
using PosShared.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using PosShared;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;

namespace PosAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class BusinessesController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<BusinessesController> _logger;


        public BusinessesController(ApplicationDbContext context, ILogger<BusinessesController> logger)
        {
            _dbContext = context;
            _logger = logger;
        }

        // GET: api/Businesses
        [HttpGet("")]
        public async Task<IActionResult> GetAllBusinesses()
        {
            string? token = HttpContext.Request.Headers["Authorization"].ToString();
            int? workerId = ExtractUserIdFromBearerToken(token);
            _logger.LogInformation("Token" + token);
            User user = _dbContext.Users.Find(workerId);

            //if worker, list nothing , no permissios
            //if owner, list only his own businesses
            //if SuperAdmin, list everythign


            var businesses = await _dbContext.Businesses.ToListAsync();
            if (businesses != null && businesses.Any())
                return Ok(businesses);
            else
                return NotFound("No businesses found.");
        }

        // GET: api/Businesses/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBusinessById(int id)
        {
            var business = await _dbContext.Businesses.FindAsync(id);
            if (business != null)
                return Ok(business);
            else
                return NotFound($"Business with ID {id} not found.");
        }

        // PUT: api/Businesses/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> EditBusiness(int id, [FromBody] Business updatedBusiness)
        {
            if (updatedBusiness == null || updatedBusiness.Id != id)
            {
                return BadRequest("Invalid business data.");
            }

            var existingBusiness = await _dbContext.Businesses.FindAsync(id);
            if (existingBusiness == null)
            {
                return NotFound($"Business with ID {id} not found.");
            }

            // Update the properties of the existing business
            existingBusiness.Name = updatedBusiness.Name;
            existingBusiness.PhoneNumber = updatedBusiness.PhoneNumber;
            existingBusiness.Email = updatedBusiness.Email;
            existingBusiness.Address = updatedBusiness.Address;
            existingBusiness.VATCode = updatedBusiness.VATCode;
            existingBusiness.Type = updatedBusiness.Type;

            try
            {
                _dbContext.Businesses.Update(existingBusiness);
                await _dbContext.SaveChangesAsync();
                return Ok(existingBusiness);
            }
            catch (DbUpdateException e)
            {
                return StatusCode(500, $"Internal server error: {e.Message}");
            }
        }

        // POST: api/Businesses
        [HttpPost]
        public async Task<IActionResult> CreateBusiness([FromBody] Business business)
        {
            if (business == null)
            {
                return BadRequest("Business data is null.");
            }

            try
            {
                // Optionally set default values for properties if needed
                business.Type = BusinessType.Catering;

                await _dbContext.Businesses.AddAsync(business);
                await _dbContext.SaveChangesAsync();

                return CreatedAtAction(nameof(GetBusinessById), new { id = business.Id }, business);
            }
            catch (DbUpdateException e)
            {
                return StatusCode(500, $"Internal server error: {e.Message}");
            }
        }

        // DELETE: api/Businesses/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBusiness(int id)
        {
            var business = await _dbContext.Businesses.FindAsync(id);
            if (business == null)
            {
                return NotFound($"Business with ID {id} not found.");
            }

            try
            {
                _dbContext.Businesses.Remove(business);
                await _dbContext.SaveChangesAsync();
                return Ok($"Business with ID {id} deleted.");
            }
            catch (DbUpdateException e)
            {
                return StatusCode(500, $"Internal server error: {e.Message}");
            }
        }


        private int? ExtractUserIdFromBearerToken(string bearerToken)
        {
            string authHeader = bearerToken;
            if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return null; // Token not found or invalid format
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);

                // Extract the "UserId" claim (assuming it exists in the token)
                var userIdClaim = jwtToken.Claims.FirstOrDefault(claim => claim.Type == "UserId");
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    return userId;
                }
            }
            catch (Exception ex)
            {
                // Handle any parsing or validation errors (optional logging can be added here)
                Console.WriteLine($"Error extracting user ID from token: {ex.Message}");
            }

            return null; // Return null if extraction fails
        }


    }





}

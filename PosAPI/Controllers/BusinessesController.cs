using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PosShared.Ultilities;
using PosAPI.Repositories;
using PosAPI.Services;
using PosAPI.Services.Interfaces;
using PosShared.Models;

namespace PosAPI.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class BusinessesController : ControllerBase
{
    private readonly IBusinessService _businessService;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<BusinessesController> _logger;


    public BusinessesController(IBusinessService businessService,IUserRepository userRepository, ILogger<BusinessesController> logger)
    {
        _businessService = businessService;
        _userRepository = userRepository;
        _logger = logger;
    }

    // GET: api/Businesses
    [HttpGet("")]
    public async Task<IActionResult> GetAllBusinesses(int pageNumber = 1, int pageSize = 10)
    {
        User? sender = await GetUserFromToken();
        
        try
        {
            var paginatedBusinesses = await _businessService.GetAuthorizedBusinessesAsync
                (sender, pageNumber, pageSize);

            if (paginatedBusinesses.Items.Count > 0)
                return Ok(paginatedBusinesses);
            else
                return NotFound("No businesses found.");
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error {ex.Message}");
        }
    }

    // GET: api/Businesses/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetBusinessById(int id)
    {
        User? sender = await GetUserFromToken();
        
        try
        {
            var business = await _businessService.GetAuthorizedBusinessByIdAsync(id,sender);
            return Ok(business);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error {ex.Message}");
        }
    }

    // PUT: api/Businesses/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> EditBusiness([FromRoute]int id, [FromBody] Business updatedBusiness)
    {
        User? sender = await GetUserFromToken();
        
        try
        {
            Business? existingBusiness = await _businessService.GetAuthorizedBusinessByIdAsync(id,sender);
            // Update the properties of the existing business
            existingBusiness.Name = updatedBusiness.Name;
            existingBusiness.PhoneNumber = updatedBusiness.PhoneNumber;
            existingBusiness.Email = updatedBusiness.Email;
            existingBusiness.Address = updatedBusiness.Address;
            existingBusiness.VATCode = updatedBusiness.VATCode;
            existingBusiness.Type = updatedBusiness.Type;

            await _businessService.UpdateAuthorizedBusinessAsync(existingBusiness, sender);
            
            return Ok(existingBusiness);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            return StatusCode(500, $"Internal server error {ex.Message}");
        }
    }

    // POST: api/Businesses
    [HttpPost]
    public async Task<IActionResult> CreateBusiness([FromBody] Business business)
    {
        User? sender = await GetUserFromToken();
        
        try
        {
            await _businessService.CreateAuthorizedBusinessAsync(business, sender);

            return CreatedAtAction(nameof(GetBusinessById), new { id = business.Id }, business);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error {ex.Message}");
        }
    }

    // DELETE: api/Businesses/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBusiness(int id)
    {
        User? sender = await GetUserFromToken();
        try
        {
            await _businessService.DeleteAuthorizedBusinessAsync(id, sender);
            return Ok($"Business with ID {id} deleted.");
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error {ex.Message}");
        }
    }
    
    #region HelperMethods
    private async Task<User?> GetUserFromToken()
    {
        string token = HttpContext.Request.Headers["Authorization"].ToString();

        if (string.IsNullOrEmpty(token))
        {
            _logger.LogWarning("Authorization token is missing or null.");
            return null;
        }

        int? userId = Ultilities.ExtractUserIdFromToken(token);
        User? user = await _userRepository.GetUserByIdAsync(userId);

        if (user == null)
        {
            _logger.LogWarning($"Failed to find user with {userId} in DB");
            return null;
        }

        return user;

    }
    #endregion


}


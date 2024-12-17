using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PosShared.Utilities;
using PosAPI.Repositories;
using PosAPI.Services;
using PosAPI.Services.Interfaces;
using PosShared.Models;
using PosAPI.Repositories.Interfaces;

namespace PosAPI.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class BusinessesController : ControllerBase
{
    private readonly IBusinessService _businessService;
    private readonly IUserTokenService _userTokenService;
    private readonly ILogger<BusinessesController> _logger;
    private readonly IBusinessRepository _businessRepository;


    public BusinessesController(IBusinessService businessService, IUserTokenService userTokenService, ILogger<BusinessesController> logger, IBusinessRepository businessRepository)
    {
        _businessService = businessService;
        _userTokenService = userTokenService;
        _logger = logger;
        _businessRepository = businessRepository;
    }

    // GET: api/Businesses
    [HttpGet("")]
    public async Task<IActionResult> GetAllBusinesses(int pageNumber = 1, int pageSize = 10)
    {
        User? sender = await _userTokenService.GetUserFromTokenAsync();
        var paginatedBusinesses = await _businessService.GetAuthorizedBusinessesAsync
            (sender, pageNumber, pageSize);

        if (paginatedBusinesses.Items.Count > 0)
            return Ok(paginatedBusinesses);
        else
            return NotFound("No businesses found.");
    }

    // GET: api/Businesses/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetBusinessById(int id)
    {
        User? sender = await _userTokenService.GetUserFromTokenAsync();
        var business = await _businessService.GetAuthorizedBusinessByIdAsync(id, sender);
        return Ok(business);
    }

    // PUT: api/Businesses/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> EditBusiness([FromRoute] int id, [FromBody] Business updatedBusiness)
    {
        User? sender = await _userTokenService.GetUserFromTokenAsync();
        Business? existingBusiness = await _businessService.GetAuthorizedBusinessByIdAsync(id, sender);
        existingBusiness.Name = updatedBusiness.Name;
        existingBusiness.PhoneNumber = updatedBusiness.PhoneNumber;
        existingBusiness.Email = updatedBusiness.Email;
        existingBusiness.Address = updatedBusiness.Address;
        existingBusiness.VATCode = updatedBusiness.VATCode;
        existingBusiness.Type = updatedBusiness.Type;

        await _businessService.UpdateAuthorizedBusinessAsync(existingBusiness, sender);

        return Ok(existingBusiness);
    }

    // POST: api/Businesses
    [HttpPost]
    public async Task<IActionResult> CreateBusiness([FromBody] Business business)
    {
        User? sender = await _userTokenService.GetUserFromTokenAsync();
        await _businessService.CreateAuthorizedBusinessAsync(business, sender);
        return CreatedAtAction(nameof(GetBusinessById), new { id = business.Id }, business);
    }

    // DELETE: api/Businesses/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBusiness(int id)
    {
        User? sender = await _userTokenService.GetUserFromTokenAsync();

        await _businessService.DeleteAuthorizedBusinessAsync(id, sender);
        return Ok($"Business with ID {id} deleted.");
    }

    [HttpGet("list")]
    public async Task<IActionResult> GetBusinesses()
    {
        // Fetch businesses from the database using the repository
        var businesses = await _businessRepository.GetAllAsync();

        var businessDtos = businesses.Select(b => new
        {
            Id = b.Id,
            Name = b.Name
        });

        return Ok(businessDtos);
    }


}

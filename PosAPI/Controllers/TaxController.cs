using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PosAPI.Repositories;
using PosAPI.Services.Interfaces;
using PosShared.DTOs;
using PosShared.Models;
using PosShared.Utilities;
using PosShared.ViewModels;

namespace PosAPI.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class TaxController : ControllerBase
{
    private readonly ILogger<TaxController> _logger;
    private readonly ITaxRepository _taxRepository;
    private readonly IUserRepository _userRepository;
    private readonly ITaxService _taxService;
    public TaxController(ILogger<TaxController> logger, ITaxRepository taxRepository, IUserRepository userRepository, ITaxService taxService)
    {
        _logger = logger;
        _taxRepository = taxRepository;
        _userRepository = userRepository;
        _taxService = taxService;
    }


    // GET: api/Tax
    [HttpGet]
    public async Task<IActionResult> GetAllTaxes()
    {
        User? sender = await GetUserFromToken();
        List<Tax> taxes = await _taxService.GetAuthorizedTaxesAsync(sender);
        return Ok(taxes);
    }
    
    // GET: api/Items/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTaxById(int id)
    {
        User? sender = await GetUserFromToken();
        Tax? tax =  await _taxService.GetAuthorizedTaxByIdAsync(id,sender);
        return Ok(tax);
    }

    // POST: api/Tax
    [HttpPost]
    public async Task<IActionResult> CreateTax([FromBody] TaxDTO tax)
    {
        User? sender = await GetUserFromToken();
        
        Tax newTax = new Tax()
        {
            BusinessId = sender.BusinessId,
            Name = tax.Name,
            IsPercentage = tax.IsPercentage,
            Amount = tax.Amount,
            Category = tax.Category,
        };
        
        await _taxRepository.AddTaxAsync(newTax);

        return CreatedAtAction(nameof(GetTaxById), new { id = newTax.Id }, newTax);
    }


    // PUT: api/Tax/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTax(int id, [FromBody] TaxDTO tax)
    {
        User? sender = await GetUserFromToken();
        Tax? existingTax = await _taxService.GetAuthorizedTaxByIdAsync(id,sender);
        
        existingTax.Name = tax.Name;
        existingTax.IsPercentage = tax.IsPercentage;
        existingTax.Amount = tax.Amount;
        existingTax.Category = tax.Category;
        
        await _taxService.UpdateAuthorizedTaxAsync(existingTax, sender);
        
        return Ok();
    }
    
    // DELETE: api/Tax/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTax(int id)
    {
        User? sender = await GetUserFromToken();
        await _taxService.DeleteAuthorizedTaxAsync(id,sender);
        return NoContent();
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
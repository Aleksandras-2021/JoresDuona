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

        try
        {
            List<Tax> taxes = await _taxService.GetAuthorizedTaxesAsync(sender);

            return Ok(taxes);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError($"Unauthorized access taxes: {ex.Message}");
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving all taxes: {ex.Message}");
            return StatusCode(500, "Internal server error");
        }
    }
    
    // GET: api/Items/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTaxById(int id)
    {
        User? sender = await GetUserFromToken();
        
        try
        {
            Tax? tax =  await _taxService.GetAuthorizedTaxByIdAsync(id,sender);
            return Ok(tax);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogError($"Tax with id  {id} not found. {ex.Message}");
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError($"Unauthorized access to tax with ID {id}. {ex.Message}");
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving user with ID {id}: {ex.Message}");
            return StatusCode(500, "Internal server error");
        }
    }

    // POST: api/Tax
    [HttpPost]
    public async Task<IActionResult> CreateTax([FromBody] TaxDTO tax)
    {
        User? sender = await GetUserFromToken();

        try
        {
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
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError($"Unauthorized access to tax creation from sender with ID {sender.Id}. {ex.Message}");
            return Unauthorized(ex.Message);
        }
        catch (ArgumentNullException ex)
        {
            _logger.LogError($"{ex.Message}");
            return Unauthorized(ex.Message);
        }
        catch (Exception e)
        {
            return StatusCode(500, $"Internal server error: {e.Message}");
        }
    }


    // PUT: api/Tax/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTax(int id, [FromBody] TaxDTO tax)
    {
        User? sender = await GetUserFromToken();

        try
        {
            Tax? existingTax = await _taxService.GetAuthorizedTaxByIdAsync(id,sender);
            existingTax.Name = tax.Name;
            existingTax.IsPercentage = tax.IsPercentage;
            existingTax.Amount = tax.Amount;
            existingTax.Category = tax.Category;

            await _taxService.UpdateAuthorizedTaxAsync(existingTax, sender);
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError($"Unauthorized access to tax update. {ex.Message}");
            return Unauthorized(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning($"Tax with ID {id} not found: {ex.Message}");
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating Tax with ID {id}: {ex.Message}");
            return StatusCode(500, "Internal server error");
        }
    }
    
    // DELETE: api/Tax/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTax(int id)
    {
        User? sender = await GetUserFromToken();
        
        try
        {
            await _taxService.DeleteAuthorizedTaxAsync(id,sender);
            
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError($"Unauthorized access to tax delete from sender with ID {sender.Id}. {ex.Message}");
            return Unauthorized(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning($"Tax with ID {id} not found: {ex.Message}");
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error deleting Tax with ID {id}: {ex.Message}");
            return StatusCode(500, "Internal server error");
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
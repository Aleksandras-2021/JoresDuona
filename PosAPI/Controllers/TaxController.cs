using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PosAPI.Repositories;
using PosShared.DTOs;
using PosShared.Models;
using PosShared.Ultilities;
using PosShared.ViewModels;

namespace PosAPI.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class TaxController : ControllerBase
{
    private readonly ILogger<ItemsController> _logger;
    private readonly ITaxRepository _taxRepository;
    private readonly IUserRepository _userRepository;
    public TaxController(ILogger<ItemsController> logger, ITaxRepository taxRepository, IUserRepository userRepository)
    {
        _logger = logger;
        _taxRepository = taxRepository;
        _userRepository = userRepository;
    }


    // GET: api/Tax
    [HttpGet]
    public async Task<IActionResult> GetAllTaxes()
    {
        User? sender = await GetUserFromToken();

        if (sender == null)
            return Unauthorized();

        try
        {
            List<Tax> taxes;
            if (sender.Role == UserRole.SuperAdmin)
            {
                taxes = await _taxRepository.GetAllTaxesAsync();
            }
            else
            {
                taxes = await _taxRepository.GetAllBusinessTaxesAsync(sender.BusinessId);
            }


            if (taxes == null || taxes.Count == 0)
            {
                return NotFound("No items found.");
            }


            return Ok(taxes);
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
        User? senderUser = await GetUserFromToken();

        if (senderUser == null)
            return Unauthorized();

        try
        {
            Tax? tax;

            if (senderUser.Role == UserRole.SuperAdmin)
            {
                tax = await _taxRepository.GetTaxByIdAsync(id);
            }
            else if (senderUser.Role == UserRole.Manager || senderUser.Role == UserRole.Owner || senderUser.Role == UserRole.Worker)
            {
                tax = await _taxRepository.GetTaxByIdAsync(id);

                if (tax.BusinessId != senderUser.BusinessId)
                {
                    return Unauthorized();
                }
            }
            else
            {
                return Unauthorized();
            }

            if (tax == null)
            {
                return NotFound($"Tax with ID {id} not found.");
            }

            return Ok(tax);
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

        _logger.LogInformation($"{sender.Name} is creating a tax {tax.Name}");

        if (tax == null)
            return BadRequest("Tax data is null.");

        if (sender == null || sender.Role == UserRole.Worker)
            return Unauthorized();

        if (sender.BusinessId <= 0)
            return BadRequest("Invalid BusinessId associated with the user.");

        Tax newTax = new Tax();

        newTax.BusinessId = sender.BusinessId;
        newTax.Name = tax.Name;
        newTax.IsPercentage = tax.IsPercentage;
        newTax.Amount = tax.Amount;
        newTax.Category = tax.Category;


        try
        {
            await _taxRepository.AddTaxAsync(newTax);

            return CreatedAtAction(nameof(GetTaxById), new { id = newTax.Id }, newTax);
        }
        catch (DbUpdateException e)
        {
            return StatusCode(500, $"Internal server error: {e.Message}");
        }
    }


    // PUT: api/Tax/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTax(int id, [FromBody] TaxDTO tax)
    {
        if (tax == null)
        {
            return BadRequest("Invalid tax data.");
        }

        try
        {
            User? sender = await GetUserFromToken();

            Tax? existingTax = await _taxRepository.GetTaxByIdAsync(id);

            if (existingTax == null)
            {
                return NotFound($"Tax with ID {id} not found.");
            }
            if (sender == null || sender.Role == UserRole.Worker || existingTax.BusinessId != sender.BusinessId)
                return Unauthorized();

            existingTax.Name = tax.Name;
            existingTax.IsPercentage = tax.IsPercentage;
            existingTax.Amount = tax.Amount;
            existingTax.Category = tax.Category;


            await _taxRepository.UpdateTaxAsync(existingTax);

            return NoContent();
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

        if (sender == null || sender.Role == UserRole.Worker)
            return Unauthorized();

        try
        {
            Tax? tax = await _taxRepository.GetTaxByIdAsync(id);

            if (tax == null)
            {
                return NotFound($"Tax with ID {id} not found.");
            }

            if (sender.Role == UserRole.SuperAdmin)
            {
                await _taxRepository.DeleteTaxAsync(id);
            }
            else if ((sender.Role == UserRole.Owner || sender.Role == UserRole.Manager) && tax.BusinessId == sender.BusinessId)
            {
                await _taxRepository.DeleteTaxAsync(id);
            }
            else
            {
                return Unauthorized();
            }

            _logger.LogInformation($"User with id {sender.Id} deleted Tax with id {tax.Id} at {DateTime.Now}");

            return NoContent();
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

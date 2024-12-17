using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PosAPI.Repositories.Interfaces;
using PosAPI.Services.Interfaces;
using PosShared.DTOs;
using PosShared.Models;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Json;

namespace PosAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DiscountsController : ControllerBase
{
    private readonly IDiscountService _discountRepository;
    private readonly ILogger<DiscountsController> _logger;
    public DiscountsController(IDiscountService discountRepository, ILogger<DiscountsController> logger)
    {
        _discountRepository = discountRepository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetDiscounts()
    {
        var discounts = await _discountRepository.GetAllAsync();
        var discountDtos = discounts.Select(d => new DiscountDto
        {
            Id = d.Id,
            BusinessId = d.BusinessId,
            Description = d.Description,
            Amount = d.Amount,
            IsPercentage = d.IsPercentage,
            ValidFrom = d.ValidFrom,
            ValidTo = d.ValidTo
        });

        return Ok(discountDtos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetDiscount(int id)
    {
        var discount = await _discountRepository.GetByIdAsync(id);
        if (discount == null) return NotFound();

        var discountDto = new DiscountDto
        {
            Id = discount.Id,
            BusinessId = discount.BusinessId,
            Description = discount.Description,
            Amount = discount.Amount,
            IsPercentage = discount.IsPercentage,
            ValidFrom = discount.ValidFrom,
            ValidTo = discount.ValidTo
        };

        return Ok(discountDto);
    }
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateDiscount([FromBody] DiscountDto discountDto)
    {
        var debugInfo = new List<string>(); // Use a list to store debug messages
        debugInfo.Add("CreateDiscount action started.");

        // Log input values
        debugInfo.Add($"Received DiscountDto: {JsonSerializer.Serialize(discountDto)}");
        debugInfo.Add($"IsPercentage Value: {discountDto.IsPercentage}");

        if (!ModelState.IsValid)
        {
            foreach (var state in ModelState)
            {
                foreach (var error in state.Value.Errors)
                {
                    debugInfo.Add($"Field: {state.Key}, Error: {error.ErrorMessage}");
                }
            }
            return BadRequest(new { message = "Invalid input", debugInfo });
        }

        // Automatically assign BusinessId from claims
        var userBusinessId = GetUserBusinessId();
        if (userBusinessId == 0)
        {
            debugInfo.Add("Failed to retrieve BusinessId from user claims.");
            return Unauthorized(new { message = "Invalid token. BusinessId not found.", debugInfo });
        }

        debugInfo.Add($"Retrieved BusinessId: {userBusinessId}");

        var discount = new Discount
        {
            BusinessId = userBusinessId,
            Description = discountDto.Description,
            Amount = discountDto.Amount,
            IsPercentage = discountDto.IsPercentage, // Explicitly assign IsPercentage
            ValidFrom = DateTime.SpecifyKind(discountDto.ValidFrom, DateTimeKind.Utc),
            ValidTo = DateTime.SpecifyKind(discountDto.ValidTo, DateTimeKind.Utc)
        };

        debugInfo.Add($"Saving Discount: {JsonSerializer.Serialize(discount)}");

        try
        {
            await _discountRepository.AddAsync(discount);
            debugInfo.Add("Discount saved successfully.");
        }
        catch (Exception ex)
        {
            debugInfo.Add($"Error saving discount: {ex.Message}");
            return StatusCode(500, new { message = "Internal Server Error. Could not save discount.", debugInfo });
        }

        // Return debug info along with success response
        return CreatedAtAction(nameof(GetDiscount), new { id = discount.Id }, new
        {
            message = "Discount created successfully.",
            debugInfo,
            discount = new DiscountDto
            {
                Description = discount.Description,
                Amount = discount.Amount,
                IsPercentage = discount.IsPercentage,
                ValidFrom = discount.ValidFrom,
                ValidTo = discount.ValidTo
            }
        });
    }


    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] DiscountDto discountDto)
    {
        if (id != discountDto.Id)
        {
            return BadRequest("Mismatched ID");
        }

        try
        {
            var existingDiscount = await _discountRepository.GetByIdAsync(id);
            if (existingDiscount == null) return NotFound("Discount not found");

            // Update properties
            existingDiscount.Description = discountDto.Description;
            existingDiscount.Amount = discountDto.Amount;
            existingDiscount.IsPercentage = discountDto.IsPercentage;
            existingDiscount.ValidFrom = DateTime.SpecifyKind(discountDto.ValidFrom, DateTimeKind.Utc);
            existingDiscount.ValidTo = DateTime.SpecifyKind(discountDto.ValidTo, DateTimeKind.Utc);

            await _discountRepository.UpdateAsync(existingDiscount);
            return NoContent();
        }
        catch (DbUpdateException dbEx)
        {
            return StatusCode(500, $"Database Error: {dbEx.InnerException?.Message}");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal Server Error: {ex.Message}");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDiscount(int id)
    {
        await _discountRepository.DeleteAsync(id);
        return NoContent();
    }

    // Helper Method to Get User's Business ID
    private int GetUserBusinessId()
    {
        var claim = User.Claims.FirstOrDefault(c => c.Type == "BusinessId");
        if (claim == null) throw new UnauthorizedAccessException("User BusinessId not found in token claims.");
        return int.Parse(claim.Value);
    }
}

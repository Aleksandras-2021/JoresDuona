using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PosAPI.Repositories.Interfaces;
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
    private readonly IDiscountRepository _discountRepository;

    public DiscountsController(IDiscountRepository discountRepository)
    {
        _discountRepository = discountRepository;
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
        if (!ModelState.IsValid)
        {
            foreach (var state in ModelState)
            {
                foreach (var error in state.Value.Errors)
                {
                    Console.WriteLine($"Field: {state.Key}, Error: {error.ErrorMessage}");
                }
            }
            return BadRequest(ModelState);
        }

        // Automatically assign BusinessId based on sender's claims
        var userBusinessId = GetUserBusinessId();

        var discount = new Discount
        {
            BusinessId = userBusinessId,
            Description = discountDto.Description,
            Amount = discountDto.Amount,
            IsPercentage = discountDto.IsPercentage,
            ValidFrom = DateTime.SpecifyKind(discountDto.ValidFrom, DateTimeKind.Utc),
            ValidTo = DateTime.SpecifyKind(discountDto.ValidTo, DateTimeKind.Utc)
        };

        await _discountRepository.AddAsync(discount);

        // Return the created discount (without IDs in DiscountDto)
        var responseDto = new DiscountDto
        {
            Description = discount.Description,
            Amount = discount.Amount,
            IsPercentage = discount.IsPercentage,
            ValidFrom = discount.ValidFrom,
            ValidTo = discount.ValidTo
        };

        return CreatedAtAction(nameof(GetDiscount), new { id = discount.Id }, responseDto);
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

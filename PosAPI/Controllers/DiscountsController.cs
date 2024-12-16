using Microsoft.AspNetCore.Mvc;
using PosAPI.Repositories.Interfaces;
using System.Threading.Tasks;
using System.Collections.Generic;
using PosShared.Models;

namespace PosAPI.Controllers
{
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
            return Ok(discounts);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDiscount(int id)
        {
            var discount = await _discountRepository.GetByIdAsync(id);
            if (discount == null) return NotFound();
            return Ok(discount);
        }

        [HttpPost]
        public async Task<IActionResult> CreateDiscount([FromBody] Discount discount)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            discount.ValidFrom = DateTime.SpecifyKind(discount.ValidFrom, DateTimeKind.Utc);
            discount.ValidTo = DateTime.SpecifyKind(discount.ValidTo, DateTimeKind.Utc);

            await _discountRepository.AddAsync(discount);
            return CreatedAtAction(nameof(GetDiscount), new { id = discount.Id }, discount);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDiscount(int id, [FromBody] Discount discount)
        {
            if (id != discount.Id)
            {
                return BadRequest("ID mismatch.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Ensure DateTime values are UTC
            discount.ValidFrom = DateTime.SpecifyKind(discount.ValidFrom, DateTimeKind.Utc);
            discount.ValidTo = DateTime.SpecifyKind(discount.ValidTo, DateTimeKind.Utc);

            await _discountRepository.UpdateAsync(discount);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDiscount(int id)
        {
            await _discountRepository.DeleteAsync(id);
            return NoContent();
        }
        [HttpGet("activate/{name}")]
        public async Task<IActionResult> ActivateDiscount(string name)
        {
            var discount = await _discountRepository.GetActiveDiscountByNameAsync(name);

            if (discount == null)
            {
                return NotFound($"No active discount found with the name '{name}'.");
            }

            return Ok(discount);
        }

    }
}

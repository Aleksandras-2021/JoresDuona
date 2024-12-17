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
    private readonly IUserTokenService _userTokenService;
    private readonly ITaxService _taxService;
    public TaxController(ILogger<TaxController> logger, ITaxRepository taxRepository, IUserTokenService userTokenService, ITaxService taxService)
    {
        _logger = logger;
        _taxRepository = taxRepository;
        _userTokenService = userTokenService;
        _taxService = taxService;
    }


    // GET: api/Tax
    [HttpGet]
    public async Task<IActionResult> GetAllTaxes()
    {
        User? sender = await _userTokenService.GetUserFromTokenAsync();
        List<Tax> taxes = await _taxService.GetAuthorizedTaxesAsync(sender);
        return Ok(taxes);
    }
    
    // GET: api/Items/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTaxById(int id)
    {
        User? sender = await _userTokenService.GetUserFromTokenAsync();
        Tax? tax =  await _taxService.GetAuthorizedTaxByIdAsync(id,sender);
        return Ok(tax);
    }

    // POST: api/Tax
    [HttpPost]
    public async Task<IActionResult> CreateTax([FromBody] TaxDTO tax)
    {
        User? sender = await _userTokenService.GetUserFromTokenAsync();
        
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
        User? sender = await _userTokenService.GetUserFromTokenAsync();
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
        User? sender = await _userTokenService.GetUserFromTokenAsync();
        await _taxService.DeleteAuthorizedTaxAsync(id,sender);
        return NoContent();
    }
}
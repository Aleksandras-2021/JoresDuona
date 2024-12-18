using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;
using PosAPI.Repositories;
using PosAPI.Services;
using PosAPI.Services.Interfaces;
using PosShared.DTOs;
using PosShared.Models;
using PosShared.Utilities;
using PosShared.ViewModels;

namespace PosAPI.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class ItemsController : ControllerBase
{
    private readonly IUserTokenService _userTokenService;
    private readonly IItemService _itemService;
    public ItemsController(IUserTokenService userTokenService,IItemService itemService)
    {
        _userTokenService = userTokenService;
        _itemService = itemService;
    }
    
    // GET: api/Items
    [HttpGet]
    public async Task<IActionResult> GetAllItems(int pageNumber = 1, int pageSize = 10)
    {
        User? sender = await _userTokenService.GetUserFromTokenAsync();
        
        var paginatedItems = await _itemService.GetAuthorizedItemsAsync(sender, pageNumber, pageSize);

        return Ok(paginatedItems);
    }
    
    // GET: api/Items/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetItemById(int id)
    {
        User? sender = await _userTokenService.GetUserFromTokenAsync();
        Item? item = await _itemService.GetAuthorizedItemByIdAsync(id, sender);
        return Ok(item);
    }

    // POST: api/Items
    [HttpPost]
    public async Task<IActionResult> CreateItem([FromBody] ItemViewModel item)
    {
        User? sender = await _userTokenService.GetUserFromTokenAsync();
        var newItem = await _itemService.CreateAuthorizedItemAsync(item,sender);
        return CreatedAtAction(nameof(GetItemById), new { id = newItem.Id }, newItem);
    }
    
    // PUT: api/Items/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateItem(int id, [FromBody] ItemViewModel item)
    {
        User? sender = await _userTokenService.GetUserFromTokenAsync();
        await _itemService.UpdateAuthorizedItemAsync(id,item, sender);
        return Ok();
    }
    
    // DELETE: api/Items/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteItem(int id)
    {
        User? sender = await _userTokenService.GetUserFromTokenAsync();
        await _itemService.DeleteAuthorizedItemAsync(id,sender);
        return Ok();
    }
}

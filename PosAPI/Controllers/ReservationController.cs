using Microsoft.AspNetCore.Mvc;
using PosAPI.Services.Interfaces;
using PosShared.DTOs;
using PosShared.Models;

[Route("api/[controller]")]
[ApiController]
public class ReservationController : ControllerBase
{
    private readonly IReservationService _reservationService;
    private readonly IUserTokenService _userTokenService;

    public ReservationController(IReservationService reservationService, IUserTokenService userTokenService)
    {
        _reservationService = reservationService;
        _userTokenService = userTokenService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        User? sender = await _userTokenService.GetUserFromTokenAsync();
        var reservations = await _reservationService.GetAuthorizedReservationsAsync(sender);
        return Ok(reservations);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        User? sender = await _userTokenService.GetUserFromTokenAsync();
        var reservations = await _reservationService.GetAuthorizedReservationAsync(id, sender);
        return Ok(reservations);
    }
    
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ReservationCreateDTO dto)
    {
        User? sender = await _userTokenService.GetUserFromTokenAsync();
        await _reservationService.CreateAuthorizedReservationAsync(dto, sender);
        return Ok();
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id,[FromBody] ReservationCreateDTO dto)
    {
        User? sender = await _userTokenService.GetUserFromTokenAsync();
        await _reservationService.UpdateAuthorizedReservationAsync(id,dto, sender);
        return Ok();
    }
    
    [HttpDelete("{reservationId}")]
    public async Task<IActionResult> Delete(int reservationId)
    {
        User? sender = await _userTokenService.GetUserFromTokenAsync();
        await _reservationService.DeleteAuthorizedReservationAsync(reservationId,sender);
        return Ok();
    }
}
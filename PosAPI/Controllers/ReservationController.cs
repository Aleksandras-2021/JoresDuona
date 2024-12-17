using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PosAPI.Repositories;
using PosAPI.Services.Interfaces;
using PosShared.DTOs;
using PosShared.Models;
using PosShared.Utilities;
using PosShared.ViewModels;

[Route("api/[controller]")]
[ApiController]
public class ReservationController : ControllerBase
{
    private readonly IServiceService _serviceService;
    private readonly IReservationRepository _reservationRepository;
    private readonly IUserTokenService _userTokenService;

    public ReservationController(IServiceService serviceService, IReservationRepository reservationRepository,
        IUserTokenService userTokenService)
    {
        _serviceService = serviceService;
        _reservationRepository = reservationRepository;
        _userTokenService = userTokenService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        User? sender = await _userTokenService.GetUserFromTokenAsync();
        var reservations = await _reservationRepository.GetAllReservationsAsync();
        return Ok(reservations);
    }
    
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ReservationCreateDTO dto)
    {
        User? sender = await _userTokenService.GetUserFromTokenAsync();
        await _serviceService.CreateAuthorizedReservation(dto, sender);
        return Ok();
    }
    
    [HttpDelete("{reservationId}")]
    public async Task<IActionResult> Delete(int reservationId)
    {
        User? sender = await _userTokenService.GetUserFromTokenAsync();
        await _serviceService.DeleteAuthorizedReservationAsync(reservationId,sender);
        return Ok();
    }
}
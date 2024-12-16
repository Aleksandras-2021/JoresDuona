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
    private readonly IUserRepository _userRepository;
    private readonly ILogger<ReservationController> _logger;

    public ReservationController(IServiceService serviceService, IReservationRepository reservationRepository,
        IUserRepository userRepository, ILogger<ReservationController> logger)
    {
        _serviceService = serviceService;
        _reservationRepository = reservationRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        User? sender = await GetUserFromToken();
        var reservations = await _reservationRepository.GetAllReservationsAsync();
        return Ok(reservations);
    }

    [HttpGet("services/{serviceId}/available-slots")]//Unimplemented
    public async Task<IActionResult> GetAvailableSlots(int serviceId, DateTime date)
    {
        User? sender = await GetUserFromToken();
        var slots = await _serviceService.GetAvailableTimeSlots(serviceId);
        return Ok(slots);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ReservationCreateDTO dto)
    {
        User? sender = await GetUserFromToken();
        await _serviceService.CreateAuthorizedReservation(dto, sender);
        return Ok();
    }
    
    [HttpDelete("{reservationId}")]
    public async Task<IActionResult> Delete(int reservationId)
    {
        User? sender = await GetUserFromToken();
        await _serviceService.DeleteAuthorizedReservationAsync(reservationId,sender);
        return Ok();
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
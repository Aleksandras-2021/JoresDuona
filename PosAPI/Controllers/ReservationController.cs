using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PosAPI.Repositories;
using PosShared.Models;
using PosShared.Ultilities;
using PosShared.ViewModels;

[Route("api/[controller]")]
[ApiController]
public class ReservationController : ControllerBase
{
    private readonly IServiceBusinessLogic _serviceBusinessLogic;
    private readonly IReservationRepository _reservationRepository;
    private readonly IServiceRepository _serviceRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<ReservationController> _logger;

    public ReservationController(
        IServiceBusinessLogic serviceBusinessLogic,
        IReservationRepository reservationRepository,
        IServiceRepository serviceRepository,
        IUserRepository userRepository,
        ILogger<ReservationController> logger)
    {
        _serviceBusinessLogic = serviceBusinessLogic;
        _reservationRepository = reservationRepository;
        _serviceRepository = serviceRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        User? sender = await GetUserFromToken();
        if (sender == null)
            return Unauthorized();

        try 
        {
            var reservations = await _reservationRepository.GetAllReservationsAsync();
            return Ok(reservations);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting reservations: {ex.Message}");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("services/{serviceId}/available-slots")]
    public async Task<IActionResult> GetAvailableSlots(int serviceId, DateTime date)
    {
        User? sender = await GetUserFromToken();
        if (sender == null)
            return Unauthorized();

        try 
        {
            var slots = await _serviceBusinessLogic.GetAvailableTimeSlots(serviceId, date);
            return Ok(slots);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting available slots: {ex.Message}");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ReservationViewModel model)
    {
        User? sender = await GetUserFromToken();
        if (sender == null)
            return Unauthorized();

        try
        {
            // reservation logic

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error creating reservation: {ex.Message}");
            return StatusCode(500, "Internal server error");
        }
    }

    // Add other reservation-related endpoints: modify, cancel, etc.

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
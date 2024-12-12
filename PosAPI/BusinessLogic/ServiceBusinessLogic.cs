using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PosAPI.Repositories;
using PosShared.Models;

public class ServiceBusinessLogic : IServiceBusinessLogic
{
    private readonly IServiceRepository _serviceRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<ServiceBusinessLogic> _logger;

    public ServiceBusinessLogic(
        IServiceRepository serviceRepository,
        IUserRepository userRepository,
        ILogger<ServiceBusinessLogic> logger)
    {
        _serviceRepository = serviceRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<bool> CheckAvailability(int serviceId, DateTime requestedTime, int? employeeId = null)
    {
        try
        {
            var service = await _serviceRepository.GetServiceByIdAsync(serviceId);
            if (service == null)
            {
                _logger.LogWarning($"Service with ID {serviceId} not found");
                return false;
            }

            // Check if requested time is in business hours
            if (!IsWithinBusinessHours(requestedTime))
            {
                _logger.LogInformation($"Requested time {requestedTime} is outside business hours");
                return false;
            }

            // Check if time slot is free
            if (!await IsTimeSlotFree(serviceId, requestedTime, service.DurationInMinutes))
            {
                _logger.LogInformation($"Time slot {requestedTime} is not available");
                return false;
            }

            // Check employee availability if specified (not sure if we want to select employee for each reservation)
            if (employeeId.HasValue)
            {
                if (!await IsEmployeeAvailable(employeeId.Value, requestedTime, service.DurationInMinutes))
                {
                    _logger.LogInformation($"Employee {employeeId} is not available at {requestedTime}");
                    return false;
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error checking availability: {ex.Message}");
            return false;
        }
    }

    private bool IsWithinBusinessHours(DateTime time)
    {
        // Implement business hours check
        return time.Hour >= 9 && time.Hour < 17;
    }

    private async Task<bool> IsTimeSlotFree(int serviceId, DateTime requestedTime, int duration, int? employeeId = null)
    {
        try
        {
            DateTime serviceEndTime = requestedTime.AddMinutes(duration);

            // Build the base query for overlapping appointments
            var query = _context.Reservations
                .Where(r => r.Status != ReservationStatus.Cancelled &&
                        ((r.ReservationTime <= requestedTime && 
                            r.ReservationTime.AddMinutes(r.Service.DurationInMinutes) > requestedTime) ||
                            (r.ReservationTime < serviceEndTime && 
                            r.ReservationTime >= requestedTime)));

            // If employee is specified, check only their appointments
            if (employeeId.HasValue)
            {
                query = query.Where(r => r.EmployeeId == employeeId.Value);
            }

            var hasOverlappingAppointment = await query.AnyAsync();

            if (hasOverlappingAppointment)
            {
                _logger.LogInformation($"Found overlapping appointment at {requestedTime}");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error checking time slot availability: {ex.Message}");
            return false;
        }
    }

    private async Task<bool> IsEmployeeAvailable(int employeeId, DateTime time, int duration)
    {
        try
        {
            // Check if employee exists and is active
            var employee = await _userRepository.GetUserByIdAsync(employeeId);
            if (employee == null || employee.EmploymentStatus != EmploymentStatus.Active)
            {
                _logger.LogWarning($"Employee {employeeId} not found or not active");
                return false;
            }

            // Check for time off requests
            var timeOffs = await _context.TimeOffs
                .Where(t => t.UserId == employeeId &&
                        t.StartDate <= requestedTime &&
                        t.EndDate >= requestedTime)
                .AnyAsync();

            if (timeOffs)
            {
                _logger.LogInformation($"Employee {employeeId} has time off during {requestedTime}");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error checking employee availability: {ex.Message}");
            return false;
        }    
    }
    public Task<bool> ConfirmReservation(int serviceId, DateTime time, int customerId, int? employeeId = null)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ModifyReservation(int reservationId, DateTime newTime)
    {
        throw new NotImplementedException();
    }

    public Task<bool> CancelReservation(int reservationId)
    {
        throw new NotImplementedException();
    }

    public Task<List<DateTime>> GetAvailableTimeSlots(int serviceId, DateTime date)
    {
        throw new NotImplementedException();
    }
}
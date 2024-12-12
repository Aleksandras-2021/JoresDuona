using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PosAPI.Repositories;
using PosShared.Models;

public class ServiceBusinessLogic : IServiceBusinessLogic
{
    private readonly IServiceRepository _serviceRepository;
    private readonly IUserRepository _userRepository;
    private readonly IReservationRepository _reservationRepository;
    private readonly ILogger<ServiceBusinessLogic> _logger;

    public ServiceBusinessLogic(
        IServiceRepository serviceRepository,
        IUserRepository userRepository,
        IReservationRepository reservationRepository,
        ILogger<ServiceBusinessLogic> logger)
    {
        _serviceRepository = serviceRepository;
        _userRepository = userRepository;
        _reservationRepository = reservationRepository;
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
            
            // Use the repository method instead of direct query
            bool hasOverlapping = await _userRepository.HasOverlappingReservationsAsync(
                requestedTime, 
                serviceEndTime, 
                employeeId);

            if (hasOverlapping)
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
            var hasTimeOff = await _userRepository.HasTimeOffAsync(employeeId, time);
            if (hasTimeOff)
            {
                _logger.LogInformation($"Employee {employeeId} has time off during {time}");
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

    public async Task<List<TimeSlot>> GetAvailableTimeSlots(int serviceId, DateTime date)
    {
        var availableSlots = new List<TimeSlot>();
        
        // Get business hours (for now hardcoded, later from configuration)
        var startHour = 9; 
        var endHour = 17;  
        var intervalMinutes = 30; 

        for (int hour = startHour; hour < endHour; hour++)
        {
            for (int minute = 0; minute < 60; minute += intervalMinutes)
            {
                var timeSlot = new DateTime(date.Year, date.Month, date.Day, hour, minute, 0);
                if (await IsTimeSlotFree(serviceId, timeSlot, intervalMinutes, null))
                {
                    availableSlots.Add(new TimeSlot(0, timeSlot, timeSlot.AddMinutes(intervalMinutes), true));
                }
            }
        }

        return availableSlots;
    }
}
using PosAPI.Middlewares;
using PosAPI.Repositories;
using PosAPI.Services.Interfaces;
using PosShared;
using PosShared.DTOs;
using PosShared.Models;

namespace PosAPI.Services;

public class ServiceService: IServiceService
{
    private readonly IUserRepository _userRepository;
    private readonly IServiceRepository _serviceRepository;
    private readonly IScheduleRepository _scheduleRepository;
    private readonly IReservationRepository _reservationRepository;
    private readonly IOrderService _orderService;

    public ServiceService(IUserRepository userRepository, IServiceRepository serviceRepository,
        IScheduleRepository scheduleRepository,IOrderService orderService, IReservationRepository reservationRepository)
    {
        _userRepository = userRepository;
        _serviceRepository = serviceRepository;
        _scheduleRepository = scheduleRepository;
        _reservationRepository = reservationRepository;
        _orderService = orderService;
    }

    
    
    public async Task<List<DateTime>> GetAvailableTimeSlots(int serviceId)
    {
        // Ensure week is converted to UTC
       // week = DateTime.SpecifyKind(week, DateTimeKind.Utc);
    
        DateTime today = DateTime.UtcNow;
        DateTime endTime = DateTime.UtcNow.AddDays(7);

        var service = await _serviceRepository.GetServiceByIdAsync(serviceId);
        var employee = await _userRepository.GetUserByIdAsync(service.EmployeeId);
    
        if (employee == null)
        {
            throw new Exception($"Employee with ID {service.EmployeeId} not found.");
        }
        
        var schedules = await _scheduleRepository.GetSchedulesByUserIdAsync(employee.Id, today, endTime);

        // Duration of the service in minutes
        int serviceDurationMinutes = service.DurationInMinutes;

        // List to store the resulting time slots
        var availableTimeSlots = new List<DateTime>();

        // Process each schedule and divide it into intervals
        foreach (var schedule in schedules)
        {
            DateTime currentTime = schedule.StartTime; // Start of the schedule
            while (currentTime.AddMinutes(serviceDurationMinutes) <= schedule.EndTime)
            {
                availableTimeSlots.Add(currentTime);
                currentTime = currentTime.AddMinutes(serviceDurationMinutes); // Move to the next interval
            }
        }

        return availableTimeSlots;
    }

    public async Task CreateReservation(ReservationCreateDTO reservation, User? sender)
    {
        AuthorizationHelper.Authorize("Reservation", "Create", sender);

        var service = await _serviceRepository.GetServiceByIdAsync(reservation.ServiceId);
        if (service == null)
            throw new Exception($"Service with ID {reservation.ServiceId} not found.");

        AuthorizationHelper.ValidateOwnershipOrRole(sender, service.BusinessId, sender.BusinessId, "Create");

        int orderId = await _orderService.CreateAuthorizedOrder(sender);

        // Check if customer exists or create a new one
        Customer? customer = await _reservationRepository.FindCustomerByPhone(reservation.CustomerPhone);
        if (customer == null)
        {
            customer = new Customer
            {
                Name = reservation.CustomerName,
                Phone = reservation.CustomerPhone,
                Email = string.Empty,
            };

            await _reservationRepository.AddCustomer(customer);
            customer = await _reservationRepository.FindCustomerByPhone(reservation.CustomerPhone);
        }

        if (customer == null)
            throw new Exception("Failed to create or retrieve customer.");

        // Create new reservation
        Reservation newReservation = new Reservation
        {
            ReservationTime = reservation.ReservationTime,
            CustomerPhone = reservation.CustomerPhone,
            CustomerId = customer.Id,
            BookedAt = DateTime.UtcNow.AddHours(2),
            ServiceId = reservation.ServiceId,
            CustomerName = reservation.CustomerName,
            EmployeeId = service.EmployeeId,
            OrderId = orderId
        };

        await _reservationRepository.AddReservationAsync(newReservation);
    }

    
    
}
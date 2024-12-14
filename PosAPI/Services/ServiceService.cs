using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
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
    private readonly ITaxRepository _taxRepository;
    private readonly IOrderService _orderService;
    private readonly IOrderRepository _orderRepository;

    public ServiceService(IUserRepository userRepository, IServiceRepository serviceRepository,
        IScheduleRepository scheduleRepository,IOrderService orderService, 
        IReservationRepository reservationRepository,ITaxRepository taxRepository,IOrderRepository orderRepository)
    {
        _userRepository = userRepository;
        _serviceRepository = serviceRepository;
        _scheduleRepository = scheduleRepository;
        _reservationRepository = reservationRepository;
        _orderService = orderService;
        _taxRepository = taxRepository;
        _orderRepository = orderRepository;
    }

    
    
    public async Task<List<DateTime>> GetAvailableTimeSlots(int serviceId)
    {
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

    public async Task CreateAuthorizedReservation(ReservationCreateDTO reservation, User? sender)
    {
        AuthorizationHelper.Authorize("Reservation", "Create", sender);

        var service = await _serviceRepository.GetServiceByIdAsync(reservation.ServiceId);

        AuthorizationHelper.ValidateOwnershipOrRole(sender, service.BusinessId, sender.BusinessId, "Create");

        
        //1. Create an order for reservation
        var orderId = await _orderService.CreateAuthorizedOrder(sender);
        if (orderId <= 0) 
        {
            throw new Exception("Failed to create an order.");
        }

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
        
        // 2. Calculate the end time for the reservation
        var startTime = reservation.ReservationTime;
        var endTime = reservation.ReservationTime.AddMinutes(service.DurationInMinutes);
        

        // Check for overlapping reservations
        var isOverlapping = await _reservationRepository.IsReservationOverlappingAsync(reservation.ServiceId, startTime, endTime);
        
        if (isOverlapping)
            throw new Exception($"The selected time slot from {startTime} to {endTime} overlaps with an existing reservation.");
        
        //3. Create new reservation
        Reservation newReservation = new Reservation
        {
            ReservationTime = reservation.ReservationTime,
            ReservationEndTime = reservation.ReservationTime.AddMinutes(service.DurationInMinutes),
            CustomerPhone = reservation.CustomerPhone,
            CustomerId = customer.Id,
            BookedAt = DateTime.Now,
            ServiceId = reservation.ServiceId,
            CustomerName = reservation.CustomerName,
            EmployeeId = service.EmployeeId,
            OrderId = orderId
        };
        await _reservationRepository.AddReservationAsync(newReservation);

        //Make OrderService 
        Tax? tax = await _taxRepository.GetTaxByCategoryAsync(service.Category, sender.BusinessId);

        decimal serviceTax;
        if (tax != null)
        {
            if (tax.IsPercentage)
                serviceTax = service.BasePrice * tax.Amount / 100;
            else
                serviceTax = tax.Amount;
        }
        else
        {
            serviceTax = 0;
        }

        // Must specify namespace, because it clashes with another class
        var orderService = new PosShared.Models.OrderService()
        {
            ServiceId = service.Id,
            DurationInMinutes = service.DurationInMinutes,
            Charge = service.BasePrice,
            OrderId = orderId,
            Tax = serviceTax,
            Total = service.BasePrice + serviceTax
        };
        await _orderRepository.AddOrderServiceAsync(orderService);
        await _orderService.RecalculateOrderCharge(orderId);
    }

    public async Task DeleteAuthorizedReservationAsync(int reservationId, User? sender)
    {
        AuthorizationHelper.Authorize("Reservation", "Delete", sender);
        Reservation? reservation = await _reservationRepository.GetReservationByIdAsync(reservationId);
        Service? service = await _serviceRepository.GetServiceByIdAsync(reservation.ServiceId);
        AuthorizationHelper.ValidateOwnershipOrRole(sender,service.BusinessId ,sender.BusinessId, "Delete");
        
        await _reservationRepository.DeleteReservationAsync(reservationId);
    }
    
}
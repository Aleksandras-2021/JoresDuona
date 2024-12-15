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
    private readonly ITaxService _taxService;
    private readonly IOrderService _orderService;
    private readonly IOrderRepository _orderRepository;

    public ServiceService(IUserRepository userRepository, IServiceRepository serviceRepository,
        IScheduleRepository scheduleRepository,IOrderService orderService, 
        IReservationRepository reservationRepository,ITaxService taxService,IOrderRepository orderRepository)
    {
        _userRepository = userRepository;
        _serviceRepository = serviceRepository;
        _scheduleRepository = scheduleRepository;
        _reservationRepository = reservationRepository;
        _orderService = orderService;
        _taxService = taxService;
        _orderRepository = orderRepository;
    }

    public async Task<List<Service>> GetAuthorizedServices(User? sender)
    {
        AuthorizationHelper.Authorize("Service", "List", sender);
        List<Service> services;

        if (sender.Role == UserRole.SuperAdmin)
        {
            services = await _serviceRepository.GetAllServicesAsync();
        }
        else
        {
            services = await _serviceRepository.GetAllBusinessServicesAsync(sender.BusinessId);
        }

        if (!services.Any())
            throw new KeyNotFoundException("No services found");

        return services;
    }
    
    public async Task<Service> GetAuthorizedService(int id,User? sender)
    {
        AuthorizationHelper.Authorize("Service", "Read", sender);

        var service = await _serviceRepository.GetServiceByIdAsync(id);
        AuthorizationHelper.ValidateOwnershipOrRole(sender, service.BusinessId, sender.BusinessId, "Read");

        return service;
    }
    
    public async Task<Service> CreateAuthorizedService(ServiceCreateDTO service,User? sender)
    {
        AuthorizationHelper.Authorize("Service", "Create", sender);

        Service newService = new Service()
        {

            BusinessId = sender.BusinessId,
            Name = service.Name,
            Description = service.Description,
            EmployeeId = service.EmployeeId,
            BasePrice = service.BasePrice,
            DurationInMinutes = service.DurationInMinutes,
            Category = service.Category
        };
        
        await _serviceRepository.AddServiceAsync(newService);

        
        return newService;
    }
    
    public async Task UpdateAuthorizedService(int id,ServiceCreateDTO service,User? sender)
    {
        AuthorizationHelper.Authorize("Service", "Create", sender);
        Service? existingService = await _serviceRepository.GetServiceByIdAsync(id);
        AuthorizationHelper.ValidateOwnershipOrRole(sender, existingService.BusinessId, sender.BusinessId, "Update");

        existingService.Name = service.Name;
        existingService.Description = service.Description;
        existingService.EmployeeId = service.EmployeeId;
        existingService.BasePrice = service.BasePrice;
        existingService.DurationInMinutes = service.DurationInMinutes;
        existingService.Category = service.Category;
        
        await _serviceRepository.UpdateServiceAsync(existingService);
    }
    
    public async Task DeleteAuthorizedService(int id,User? sender)
    {
        AuthorizationHelper.Authorize("Service", "Delete", sender);
        Service? existingService = await _serviceRepository.GetServiceByIdAsync(id);
        AuthorizationHelper.ValidateOwnershipOrRole(sender, existingService.BusinessId, sender.BusinessId, "Delete");

        await _serviceRepository.DeleteServiceAsync(id);
    }
    
    //Reservations
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

        var order = await _orderRepository.GetOrderByIdAsync(orderId);

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
        

        // Check for overlapping reservations (it Does not check user schedule)
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
        decimal serviceTax = await _taxService.CalculateTaxByCategory(service.BasePrice,1,service.Category,service.BusinessId);

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

        order.ChargeAmount += orderService.Charge;
        order.TaxAmount += orderService.Tax;
        
        await _orderRepository.UpdateOrderAsync(order);
    }

    public async Task DeleteAuthorizedReservationAsync(int reservationId, User? sender)
    {
        AuthorizationHelper.Authorize("Reservation", "Delete", sender);
        Reservation? reservation = await _reservationRepository.GetReservationByIdAsync(reservationId);
        Service? service = await _serviceRepository.GetServiceByIdAsync(reservation.ServiceId);
        AuthorizationHelper.ValidateOwnershipOrRole(sender,service.BusinessId ,sender.BusinessId, "Delete");
        Order? order = await _orderRepository.GetOrderByIdAsync(reservation.OrderId);

        if (order.Status is not OrderStatus.Closed or OrderStatus.Paid)
        {
            order.ChargeAmount -= service.BasePrice;
            order.TaxAmount -=
                await _taxService.CalculateTaxByCategory(service.BasePrice, 1, service.Category, service.BusinessId);
        }

        await _reservationRepository.DeleteReservationAsync(reservationId);
    }
    
}
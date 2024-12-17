using System.Transactions;
using PosAPI.Middlewares;
using PosAPI.Repositories;
using PosAPI.Services.Interfaces;
using PosShared;
using PosShared.DTOs;
using PosShared.Models;

namespace PosAPI.Services;

public class ReservationService: IReservationService
{

    private readonly IReservationRepository _reservationRepository;
    private readonly IOrderService _orderService;
    private readonly IOrderRepository _orderRepository;
    private readonly IServiceService _serviceService;
    private readonly ITaxService _taxService;
    private readonly IDefaultShiftPatternService _defaultShiftPatternService;
    
    public ReservationService(IReservationRepository reservationRepository, 
        IOrderService orderService, IServiceService serviceService,ITaxService taxService,
        IOrderRepository orderRepository, IDefaultShiftPatternService defaultShiftPatternService)
    {
        _reservationRepository = reservationRepository;
        _serviceService = serviceService;
        _taxService = taxService;
        _orderService = orderService;
        _orderRepository = orderRepository;
        _defaultShiftPatternService = defaultShiftPatternService;
    }

    public async Task<PaginatedResult<Reservation>> GetAuthorizedReservationsAsync(User? sender, int pageNumber = 1, int pageSize = 10)
    {
        AuthorizationHelper.Authorize("Reservation", "List", sender);
        
        PaginatedResult<Reservation> reservations = null; 
        
        if (sender.Role is UserRole.SuperAdmin)
        {

             reservations = await _reservationRepository.GetAllReservationsAsync(pageNumber,pageSize);
        }
        else
        {
             reservations = await _reservationRepository.GetAllBusinessReservationsAsync(sender.BusinessId,pageNumber,pageSize);
        }

        return reservations;
    }

    public async Task<Reservation> GetAuthorizedReservationAsync(int id,User? sender)
    {
        AuthorizationHelper.Authorize("Reservation", "Read", sender);
        var reservation = await _reservationRepository.GetReservationByIdAsync(id);
        var service = await _serviceService.GetAuthorizedService(reservation.ServiceId, sender);

        return reservation;
    }
    
public async Task CreateAuthorizedReservationAsync(ReservationCreateDTO reservation, User? sender)
{
    AuthorizationHelper.Authorize("Reservation", "Create", sender);
    var service = await _serviceService.GetAuthorizedService(reservation.ServiceId, sender);

    using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
    {
            // 1. Create an order for reservation
            var order = await _orderService.CreateAuthorizedOrder(sender);

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

            // 2. Calculate the end time for the reservation
            var startTime = reservation.ReservationTime.ToUniversalTime();
            var endTime = reservation.ReservationTime.AddMinutes(service.DurationInMinutes).ToUniversalTime();

            // 3. Check for overlapping reservations
            var isOverlapping = await _reservationRepository.IsReservationOverlappingAsync(reservation.ServiceId, startTime, endTime);
            var isEmployeeAvailable = await IsValidShiftForReservationAsync(sender.Id, sender, startTime);

            if (!isEmployeeAvailable || isOverlapping || startTime < DateTime.Today)
                throw new BusinessRuleViolationException($"The selected time slot from {startTime} to {endTime} are invalid. Is employee available: {isEmployeeAvailable}");

            // 4. Create new reservation
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
                OrderId = order.Id
            };
            await _reservationRepository.AddReservationAsync(newReservation);

            // 5. Make OrderService 
            decimal serviceTax = await _taxService.CalculateTaxByCategory(service.BasePrice, 1, service.Category, service.BusinessId);

            var orderService = new PosShared.Models.OrderService()
            {
                ServiceId = service.Id,
                DurationInMinutes = service.DurationInMinutes,
                Charge = service.BasePrice,
                OrderId = order.Id,
                Tax = serviceTax,
                Total = service.BasePrice + serviceTax
            };
            await _orderRepository.AddOrderServiceAsync(orderService);

            order.ChargeAmount += orderService.Charge;
            order.TaxAmount += orderService.Tax;

            await _orderRepository.UpdateOrderAsync(order);

            // Commit the transaction
            transaction.Complete();
        }
    }

    
    private async Task<bool> IsValidShiftForReservationAsync(int userId,User? sender ,DateTime startTime)
    {
        startTime = startTime.ToUniversalTime();

        var patterns = await _defaultShiftPatternService.GetAuthorizedPatternsByUserAsync(userId,sender);

        DayOfWeek dayOfWeek = startTime.DayOfWeek;

        foreach (var pattern in patterns)
        {
            // We only care about the patterns that match the day of the week
            if (pattern.DayOfWeek == dayOfWeek)
            {
                // Extract the times from the StartDate and EndDate
                var patternStartTime = pattern.StartDate.TimeOfDay;
                var patternEndTime = pattern.EndDate.TimeOfDay;

                // Check if the reservation time falls within this shift pattern's time range
                if (startTime.TimeOfDay >= patternStartTime && startTime.TimeOfDay <= patternEndTime)
                {
                    return true;
                }
            }
        }

        // No matching shift pattern found for the given reservation time
        return false;
    }



    public async Task DeleteAuthorizedReservationAsync(int reservationId, User? sender)
    {
        AuthorizationHelper.Authorize("Reservation", "Delete", sender);
        Reservation? reservation = await _reservationRepository.GetReservationByIdAsync(reservationId);
        Service? service = await _serviceService.GetAuthorizedService(reservation.ServiceId,sender);
   
        
        Order? order = await _orderRepository.GetOrderByIdAsync(reservation.OrderId);

        if (order.Status is not OrderStatus.Closed or OrderStatus.Paid)
        {
            order.ChargeAmount -= service.BasePrice;
            order.TaxAmount -=
                await _taxService.CalculateTaxByCategory(service.BasePrice, 1, service.Category, service.BusinessId);
        }

        await _reservationRepository.DeleteReservationAsync(reservationId);
    }

    public async Task UpdateAuthorizedReservationAsync(int reservationId, ReservationCreateDTO reservation, User? sender)
    {
        AuthorizationHelper.Authorize("Reservation", "Update", sender);
        var existingReservation = await  GetAuthorizedReservationAsync(reservationId, sender);
        var service = await _serviceService.GetAuthorizedService(existingReservation.ServiceId, sender);
        
        var startTime = reservation.ReservationTime.ToUniversalTime();
        var endTime = reservation.ReservationTime.AddMinutes(service.DurationInMinutes).ToUniversalTime();

        var isOverlapping = await _reservationRepository.IsReservationOverlappingAsync(reservation.ServiceId, startTime, endTime);
        var isEmployeeAvailable = await IsValidShiftForReservationAsync(sender.Id, sender, startTime);

        if (!isEmployeeAvailable || isOverlapping || startTime < DateTime.Today)
            throw new BusinessRuleViolationException($"The selected time slot from {startTime} to {endTime} are invalid. Is employee available: {isEmployeeAvailable}");

        existingReservation.ReservationTime = startTime;
        existingReservation.ReservationEndTime = endTime;
        existingReservation.CustomerName = reservation.CustomerName;
        existingReservation.CustomerPhone = reservation.CustomerPhone;

        await _reservationRepository.UpdateReservationAsync(existingReservation);
    }
}
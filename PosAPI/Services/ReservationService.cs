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
    private readonly IScheduleRepository _scheduleRepository;
    
    
    public ReservationService(IReservationRepository reservationRepository, 
        IOrderService orderService, IServiceService serviceService,ITaxService taxService,
        IOrderRepository orderRepository, IScheduleRepository scheduleRepository)
    {
        _reservationRepository = reservationRepository;
        _serviceService = serviceService;
        _taxService = taxService;
        _orderService = orderService;
        _orderRepository = orderRepository;
        _scheduleRepository = scheduleRepository;
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
            var startTime = reservation.ReservationTime;
            var endTime = reservation.ReservationTime.AddMinutes(service.DurationInMinutes);

            // 3. Check for overlapping reservations
            var isOverlapping = await _reservationRepository.IsReservationOverlappingAsync(reservation.ServiceId, startTime, endTime);
            bool isEmployeeAvailable = await IsEmployeeAvailableAsync(service.EmployeeId, startTime, endTime);
            
            if (!isEmployeeAvailable || isOverlapping || startTime < DateTime.Now.ToUniversalTime())
                throw new ReservationRuleViolationException(isEmployeeAvailable,isOverlapping,startTime < DateTime.Now.ToUniversalTime());
            
            DateTime now = DateTime.Now.ToUniversalTime();
            
            // 4. Create new reservation
            Reservation newReservation = new Reservation
            {
                ReservationTime = startTime,
                ReservationEndTime = endTime,
                CustomerPhone = reservation.CustomerPhone,
                CustomerId = customer.Id,
                BookedAt = now,
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
        var existingReservation = await  _reservationRepository.GetReservationByIdAsync(reservationId);
        var service = await _serviceService.GetAuthorizedService(existingReservation.ServiceId, sender);

        var startTime = reservation.ReservationTime;
        var endTime = reservation.ReservationTime.AddMinutes(service.DurationInMinutes);

        var isOverlapping = await _reservationRepository.IsReservationOverlappingAsync(service.Id, startTime, endTime,existingReservation.Id);
        bool isEmployeeAvailable = await IsEmployeeAvailableAsync(service.EmployeeId, startTime, endTime); //Checks Employee schedule

        if (!isEmployeeAvailable || isOverlapping || startTime < DateTime.Now.ToUniversalTime())
            throw new ReservationRuleViolationException(isEmployeeAvailable,isOverlapping,startTime < DateTime.Now.ToUniversalTime());

        existingReservation.ServiceId = reservation.ServiceId;
        existingReservation.ReservationTime = startTime;
        existingReservation.ReservationEndTime = endTime;
        existingReservation.CustomerName = reservation.CustomerName;
        existingReservation.CustomerPhone = reservation.CustomerPhone;
        
        await _reservationRepository.UpdateReservationAsync(existingReservation);
    }
    
    public async Task<bool> IsEmployeeAvailableAsync(int employeeId, DateTime reservationStartTime, DateTime reservationEndTime)
    {
        // Fetch the employee's schedules that include the reservation date
        var schedules = await _scheduleRepository.GetSchedulesForDateRangeAsync(reservationStartTime.Date, reservationEndTime.Date, employeeId);

        // Check if any schedule overlaps with the reservation time
        bool isWithinSchedule = schedules.Any(schedule =>
            reservationStartTime >= schedule.StartTime && reservationEndTime <= schedule.EndTime
        );

        return isWithinSchedule;
    }
}
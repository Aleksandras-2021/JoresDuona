using PosShared.Models;

public interface IServiceBusinessLogic
{
    Task<bool> CheckAvailability(int serviceId, DateTime requestedTime, int? employeeId = null);

    Task<bool> ConfirmReservation(int serviceId, DateTime time, int customerId, int? employeeId = null);

    Task<bool> ModifyReservation(int reservationId, DateTime newTime);

    Task<bool> CancelReservation(int reservationId);

    Task<List<TimeSlot>> GetAvailableTimeSlots(int serviceId, DateTime date);
}
using PosShared.DTOs;
using PosShared.Models;

namespace PosAPI.Services.Interfaces;

public interface IServiceService
{
     Task<List<DateTime>> GetAvailableTimeSlots(int serviceId);
     Task CreateAuthorizedReservation(ReservationCreateDTO reservation, User? sender);
     Task DeleteAuthorizedReservationAsync(int reservationId, User? sender);

}
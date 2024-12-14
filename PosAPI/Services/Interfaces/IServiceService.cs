using PosShared.DTOs;
using PosShared.Models;

namespace PosAPI.Services.Interfaces;

public interface IServiceService
{
     Task<List<DateTime>> GetAvailableTimeSlots(int serviceId);
     Task CreateReservation(ReservationCreateDTO reservation, User? sender);

}
using PosShared;
using PosShared.DTOs;
using PosShared.Models;

namespace PosAPI.Services.Interfaces;

public interface IReservationService
{

    Task<PaginatedResult<Reservation>> GetAuthorizedReservationsAsync(User? sender, int pageNumber = 1,
        int pageSize = 10);
    Task<Reservation> GetAuthorizedReservationAsync(int id, User? sender);
    
    Task CreateAuthorizedReservationAsync(ReservationCreateDTO reservation, User? sender);
    Task DeleteAuthorizedReservationAsync(int reservationId, User? sender);
    Task UpdateAuthorizedReservationAsync(int reservationId, ReservationCreateDTO reservation,User? sender);
}
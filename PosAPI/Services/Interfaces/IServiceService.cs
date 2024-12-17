using PosShared.DTOs;
using PosShared.Models;

namespace PosAPI.Services.Interfaces;

public interface IServiceService
{
     Task<List<Service>> GetAuthorizedServices(User? sender);
     Task<Service> GetAuthorizedService(int id, User? sender);
     Task<Service> CreateAuthorizedService(ServiceCreateDTO service, User? sender);
     Task UpdateAuthorizedService(int id, ServiceCreateDTO service, User? sender);
     Task DeleteAuthorizedService(int id, User? sender);
     Task CreateAuthorizedReservation(ReservationCreateDTO reservation, User? sender);
     Task DeleteAuthorizedReservationAsync(int reservationId, User? sender);

}
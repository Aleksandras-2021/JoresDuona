//repository for reservation interface
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PosShared;
using PosShared.DTOs;
using PosShared.Models;

namespace PosAPI.Repositories
{
    public interface IReservationRepository
    {
        Task AddReservationAsync(Reservation reservation);
        Task<Reservation> GetReservationByIdAsync(int id);
        Task<PaginatedResult<Reservation>> GetAllReservationsAsync(int pageNumber, int pageSize);
        Task<PaginatedResult<Reservation>> GetAllBusinessReservationsAsync(int businessId,int pageNumber, int pageSize);
        Task UpdateReservationAsync(Reservation reservation);
        Task AddCustomer(Customer customer);
        Task<Customer?> FindCustomerByPhone(string phone);
        Task<bool> IsReservationOverlappingAsync(int serviceId, DateTime startTime, DateTime endTime);
        Task DeleteReservationAsync(int id);
        
    }
}
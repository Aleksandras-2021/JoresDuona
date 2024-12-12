//repository for reservation interface
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PosShared.DTOs;
using PosShared.Models;

namespace PosAPI.Repositories
{
    public interface IReservationRepository
    {
        Task AddReservationAsync(Reservation reservation);
        Task<Reservation> GetReservationByIdAsync(int id);
        Task<List<Reservation>> GetAllReservationsAsync();
        Task<List<Reservation>> GetAllBusinessReservationsAsync(int businessId);
        Task UpdateReservationAsync(Reservation reservation);
        Task DeleteReservationAsync(int id);
        //method to check if the reservation is available
        Task<bool> CheckAvailability(int serviceId, DateTime requestedTime);
        //method to check if employee is available
        Task<bool> IsEmployeeAvailable(int employeeId, DateTime requestedTime, int duration);

    }
}
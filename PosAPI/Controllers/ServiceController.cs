using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using PosAPI.Repositories;
using PosShared.DTOs;
using PosShared.Models;
using PosShared.Ultilities;
using PosShared.ViewModels;

namespace PosAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceController : ControllerBase
    {
        private readonly ILogger<ServiceController> _logger;
        private readonly IServiceRepository _serviceRepository;
        private readonly IUserRepository _userRepository;
        public ServiceController(ILogger<ServiceController> logger, IServiceRepository serviceRepository, IUserRepository userRepository)
        {
            _logger = logger;
            _serviceRepository = serviceRepository;
            _userRepository = userRepository;
        }

        // GET: api/Service
        [HttpGet]
        public async Task<IActionResult> GetAllServices()
        {
            User? sender = await GetUserFromToken();

            if (sender == null)
                return Unauthorized();

            try
            {
                List<Service> services;
                if (sender.Role == UserRole.SuperAdmin)
                {
                    services = await _serviceRepository.GetAllServicesAsync();
                }
                else
                {
                    services = await _serviceRepository.GetAllBusinessServicesAsync(sender.BusinessId);
                }

                if (services == null || services.Count == 0)
                {
                    return NotFound("No items found.");
                }

                return Ok(services);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving all services: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: api/Service/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetServiceById(int id)
        {
            User? senderUser = await GetUserFromToken();

            if (senderUser == null)
                return Unauthorized();

            try
            {
                Service? service;

                if (senderUser.Role == UserRole.SuperAdmin)
                {
                    service = await _serviceRepository.GetServiceByIdAsync(id);
                }
                else if (senderUser.Role == UserRole.Manager || senderUser.Role == UserRole.Owner || senderUser.Role == UserRole.Worker)
                {
                    service = await _serviceRepository.GetServiceByIdAsync(id);

                    if (service.BusinessId != senderUser.BusinessId)
                    {
                        return Unauthorized();
                    }
                }
                else
                {
                    return Unauthorized();
                }

                if (service == null)
                {
                    return NotFound($"Service with ID {id} not found.");
                }

                return Ok(service);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving service with ID {id}: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        // POST: api/Service
        [HttpPost]
        public async Task<IActionResult> CreateService([FromBody] ServiceDTO serviceDTO)
        {
            User? senderUser = await GetUserFromToken();

            _logger.LogInformation($"User {senderUser?.Id} is creating a service {serviceDTO.Name}");

            if (serviceDTO == null)
                return BadRequest("Service is null");

            if (senderUser == null || senderUser.Role == UserRole.Worker)
                return Unauthorized();

            if (senderUser.BusinessId <= 0)
                return BadRequest("Invalid BusinessId associated with the user.");

            Service newService = new Service();

            newService.BusinessId = senderUser.BusinessId;
            newService.Name = serviceDTO.Name;
            newService.Description = serviceDTO.Description;
            newService.BasePrice = serviceDTO.BasePrice;
            newService.DurationInMinutes = serviceDTO.DurationInMinutes;

            try
            {
                await _serviceRepository.AddServiceAsync(newService);

                return CreatedAtAction(nameof(GetServiceById), new { id = newService.Id }, newService);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating service: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        // PUT: api/Service/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateService(int id, [FromBody] ServiceDTO serviceDTO)
        {

            if (serviceDTO == null)
                return BadRequest("Invalid service data.");

            try
            {
                User? senderUser = await GetUserFromToken();

                Service? existingService = await _serviceRepository.GetServiceByIdAsync(id);

                if (existingService == null)
                    return NotFound($"Service with ID {id} not found.");
                
                if (senderUser == null || senderUser.Role == UserRole.Worker)
                    return Unauthorized();

                existingService.Name = serviceDTO.Name;
                existingService.Description = serviceDTO.Description;
                existingService.BasePrice = serviceDTO.BasePrice;
                existingService.DurationInMinutes = serviceDTO.DurationInMinutes;

                await _serviceRepository.UpdateServiceAsync(existingService);

                return NoContent();
                
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError($"Error updating service with ID {id}: {ex.Message}");
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating service with ID {id}: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        // DELETE: api/Service/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteService(int id)
        {
            User? senderUser = await GetUserFromToken();

            if (senderUser == null || senderUser.Role == UserRole.Worker)
                return Unauthorized();

            try
            {
                Service? service = await _serviceRepository.GetServiceByIdAsync(id);

                if (service == null)
                    return NotFound($"Service with ID {id} not found.");

                if (senderUser.Role == UserRole.SuperAdmin)
                {
                    await _serviceRepository.DeleteServiceAsync(id);
                }
                else if ((senderUser.Role == UserRole.Manager || senderUser.Role == UserRole.Owner) && service.BusinessId == senderUser.BusinessId)
                {
                    await _serviceRepository.DeleteServiceAsync(id);
                }
                else
                {
                    return Unauthorized();
                }
                
                _logger.LogInformation($"User with id {senderUser.Id} deleted service with id {id} at {DateTime.Now}");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting service with ID {id}: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }



        #region HelperMethods
        private async Task<User?> GetUserFromToken()
        {
            string token = HttpContext.Request.Headers["Authorization"].ToString();

            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Authorization token is missing or null.");
                return null;
            }

            int? userId = Ultilities.ExtractUserIdFromToken(token);
            User? user = await _userRepository.GetUserByIdAsync(userId);

            if (user == null)
            {
                _logger.LogWarning($"Failed to find user with {userId} in DB");
                return null;
            }

            return user;

        }
        #endregion
    }
}
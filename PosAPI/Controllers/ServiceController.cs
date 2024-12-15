using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using PosAPI.Repositories;
using PosAPI.Services.Interfaces;
using PosShared.DTOs;
using PosShared.Models;
using PosShared.Utilities;
using PosShared.ViewModels;

namespace PosAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceController : ControllerBase
    {
        private readonly ILogger<ServiceController> _logger;
        private readonly IServiceService _serviceService;
        private readonly IUserRepository _userRepository;
        public ServiceController(ILogger<ServiceController> logger, IServiceService serviceService, IUserRepository userRepository)
        {
            _logger = logger;
            _serviceService = serviceService;
            _userRepository = userRepository;
        }

        // GET: api/Service
        [HttpGet]
        public async Task<IActionResult> GetAllServices()
        {
            User? sender = await GetUserFromToken();
            try
            {
                
                List<Service> services = await _serviceService.GetAuthorizedServices(sender);
                return Ok(services);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning($"{ex.Message}");
                return Forbid(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning($"{ex.Message}");
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Internal server error {ex.Message}");
                return StatusCode(500, $"Internal server error {ex.Message}");
            }
        }

        // GET: api/Service/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetServiceById(int id)
        {
            User? sender = await GetUserFromToken();

            try
            {
                Service? service = await _serviceService.GetAuthorizedService(id, sender);
                return Ok(service);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning($"{ex.Message}");
                return Forbid(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning($"{ex.Message}");
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Internal server error {ex.Message}");
                return StatusCode(500, $"Internal server error {ex.Message}");
            }

        }

        // POST: api/Service
        [HttpPost]
        public async Task<IActionResult> CreateService([FromBody] ServiceCreateDTO service)
        {
            User? sender = await GetUserFromToken();
            
            try
            {
                var newService = _serviceService.CreateAuthorizedService(service,sender);

                return CreatedAtAction(nameof(GetServiceById), new { id = newService.Id }, newService);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning($"{ex.Message}");
                return Forbid(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning($"{ex.Message}");
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Internal server error {ex.Message}");
                return StatusCode(500, $"Internal server error {ex.Message}");
            }
        }

        // PUT: api/Service/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateService(int id, [FromBody] ServiceCreateDTO service)
        {
            User? sender = await GetUserFromToken();

            try
            {
                await _serviceService.UpdateAuthorizedService(id, service, sender);
                return Ok();
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning($"{ex.Message}");
                return Forbid(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning($"{ex.Message}");
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
            User? sender = await GetUserFromToken();

            try
            {
                 await _serviceService.DeleteAuthorizedService(id,sender);
                 
                return Ok();
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning($"{ex.Message}");
                return Forbid(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning($"{ex.Message}");
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Internal server error {ex.Message}");
                return StatusCode(500, $"Internal server error {ex.Message}");
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
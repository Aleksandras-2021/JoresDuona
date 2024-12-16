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
            List<Service> services = await _serviceService.GetAuthorizedServices(sender);
            return Ok(services);
        }

        // GET: api/Service/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetServiceById(int id)
        {
            User? sender = await GetUserFromToken();
            Service? service = await _serviceService.GetAuthorizedService(id, sender);
            return Ok(service);
        }

        // POST: api/Service
        [HttpPost]
        public async Task<IActionResult> CreateService([FromBody] ServiceCreateDTO service)
        {
            User? sender = await GetUserFromToken();
            var newService = await  _serviceService.CreateAuthorizedService(service,sender);
            return CreatedAtAction(nameof(GetServiceById), new { id = newService.Id }, newService);
        }

        // PUT: api/Service/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateService(int id, [FromBody] ServiceCreateDTO service)
        {
            User? sender = await GetUserFromToken();
            await _serviceService.UpdateAuthorizedService(id, service, sender);
            return Ok();
        }

        // DELETE: api/Service/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteService(int id)
        {
            User? sender = await GetUserFromToken();
            await _serviceService.DeleteAuthorizedService(id,sender);
            return Ok();
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
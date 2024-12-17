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

namespace PosAPI.Controllers;

    [Route("api/[controller]")]
    [ApiController]
    public class ServiceController : ControllerBase
    {
        private readonly ILogger<ServiceController> _logger;
        private readonly IServiceService _serviceService;
        private readonly IUserTokenService _userTokenService;

        public ServiceController(ILogger<ServiceController> logger, IServiceService serviceService,
            IUserTokenService userTokenService)
        {
            _logger = logger;
            _serviceService = serviceService;
            _userTokenService = userTokenService;
        }

        // GET: api/Service
        [HttpGet]
        public async Task<IActionResult> GetAllServices()
        {
            User? sender = await _userTokenService.GetUserFromTokenAsync();
            List<Service> services = await _serviceService.GetAuthorizedServices(sender);
            return Ok(services);
        }

        // GET: api/Service/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetServiceById(int id)
        {
            User? sender = await _userTokenService.GetUserFromTokenAsync();
            Service? service = await _serviceService.GetAuthorizedService(id, sender);
            return Ok(service);
        }

        // POST: api/Service
        [HttpPost]
        public async Task<IActionResult> CreateService([FromBody] ServiceCreateDTO service)
        {
            User? sender = await _userTokenService.GetUserFromTokenAsync();
            var newService = await _serviceService.CreateAuthorizedService(service, sender);
            return CreatedAtAction(nameof(GetServiceById), new { id = newService.Id }, newService);
        }

        // PUT: api/Service/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateService(int id, [FromBody] ServiceCreateDTO service)
        {
            User? sender = await _userTokenService.GetUserFromTokenAsync();
            await _serviceService.UpdateAuthorizedService(id, service, sender);
            return Ok();
        }

        // DELETE: api/Service/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteService(int id)
        {
            User? sender = await _userTokenService.GetUserFromTokenAsync();
            await _serviceService.DeleteAuthorizedService(id, sender);
            return Ok();
        }

    }
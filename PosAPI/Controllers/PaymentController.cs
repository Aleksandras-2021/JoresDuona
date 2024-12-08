using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PosAPI.Repositories;
using PosShared.Models;
using PosShared.Ultilities;

namespace PosAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {

        private readonly IOrderRepository _orderRepository;
        private readonly IUserRepository _userRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly ITaxRepository _taxRepository;
        private readonly ILogger<OrderController> _logger;
        public PaymentController(IOrderRepository orderRepository, IUserRepository userRepository, IPaymentRepository paymentRepository, ITaxRepository taxRepository, ILogger<OrderController> logger)
        {
            _orderRepository = orderRepository;
            _userRepository = userRepository;
            _paymentRepository = paymentRepository;
            _taxRepository = taxRepository;
            _logger = logger;
        }


        // GET: api/Items
        [HttpGet]
        public async Task<IActionResult> GetAllPayments()
        {
            User? sender = await GetUserFromToken();

            if (sender == null)
                return Unauthorized();

            try
            {
                List<Item> items;
                if (sender.Role == UserRole.SuperAdmin)
                {
                    items = await _itemRepository.GetAllItemsAsync();
                }
                else
                {
                    items = await _itemRepository.GetAllBusinessItemsAsync(sender.BusinessId);
                }


                if (items == null || items.Count == 0)
                {
                    return NotFound("No items found.");
                }


                return Ok(items);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving all items: {ex.Message}");
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

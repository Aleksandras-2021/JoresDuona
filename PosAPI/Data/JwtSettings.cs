using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PosAPI.Data
{
    [Route("api/[controller]")]
    [ApiController]
    public class JwtSettings
    {
        public string Secret { get; set; }
        public double ExpirationInMinutes { get; set; }
    }
}

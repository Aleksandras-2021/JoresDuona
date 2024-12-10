using System.ComponentModel.DataAnnotations;

namespace PosShared.DTOs
{
    public class ServiceDTO
    {
        [Required]
        public string Name { get; set; }
        
        public string? Description { get; set; }  

        [Required]
        public decimal BasePrice { get; set; }

        [Required]
        public int DurationInMinutes { get; set; }
    }
}
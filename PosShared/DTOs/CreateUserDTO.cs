using System.ComponentModel.DataAnnotations;
using PosShared.Models;

namespace PosShared.DTOs;

public class CreateUserDTO
{
    public int BusinessId { get; set; }
    [Required]
    public string Username { get; set; }

    [Required]
    public string Password { get; set; }
    
    [Required]
    public string Name { get; set; }

    [Required]
    public string Email { get; set; }

    [Required]
    public string Phone { get; set; }

    [Required]
    public string Address { get; set; }

    [Required]
    public UserRole Role { get; set; }

    [Required]
    public EmploymentStatus EmploymentStatus { get; set; }
}
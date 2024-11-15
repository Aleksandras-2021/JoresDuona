using PosShared.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PosShared.DTOs;

public class UserDTO
{
    [Required] public int Id { get; set; }
    [Required] public string Username { get; init; } = string.Empty;
    [Required] public string Name { get; init; } = string.Empty;
    [Required] public string Password { get; init; } = string.Empty;
    [Required] public int BusinessId { get; set; }

    public string Email { get; set; }

    public string Phone { get; set; }

    public string Address { get; set; }

    public UserRole Role { get; set; }

    public EmploymentStatus EmploymentStatus { get; set; }
}

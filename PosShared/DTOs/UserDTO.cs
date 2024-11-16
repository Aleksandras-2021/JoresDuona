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
    public string Username { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public int BusinessId { get; set; }

    public string Email { get; set; }

    public string Phone { get; set; }

    public string Address { get; set; }

    public UserRole Role { get; set; }

    public EmploymentStatus EmploymentStatus { get; set; }
}

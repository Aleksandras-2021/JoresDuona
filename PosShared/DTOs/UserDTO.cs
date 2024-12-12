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
    public int Id { get; set; }
    public int BusinessId { get; set; }
    public string Username{ get; set; }
    public string Name{ get; set; }
    public string Email{ get; set; }
    public string Phone{ get; set; }
    public string Address{ get; set; }
    public UserRole Role{ get; set; }
    public EmploymentStatus EmploymentStatus{ get; set; }
}

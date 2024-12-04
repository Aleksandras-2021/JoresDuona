using PosShared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PosShared;

[Serializable]
public class UserProfileSessionData
{
    public int Id { get; set; }

    public string Email { get; set; }

    public UserRole Role { get; set; }
}

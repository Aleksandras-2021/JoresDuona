using PosShared.Models;

namespace PosShared.ViewModels
{
    public class UserCreateViewModel
    {
        public int Id { get; set; }
        public int BusinessId { get; set; }
        public string Username { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public UserRole Role { get; set; }
        public EmploymentStatus EmploymentStatus { get; set; }

        public string Password { get; set; }
    }
}

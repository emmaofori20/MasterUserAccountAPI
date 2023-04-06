using System;
using System.Collections.Generic;

namespace MasterUserAccountAPI.Models
{
    public partial class User
    {
        public User()
        {
            UserApplications = new HashSet<UserApplication>();
        }

        public int UserId { get; set; }
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }

        public virtual ICollection<UserApplication> UserApplications { get; set; }
    }
}

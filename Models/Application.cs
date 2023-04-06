using System;
using System.Collections.Generic;

namespace MasterUserAccountAPI.Models
{
    public partial class Application
    {
        public Application()
        {
            UserApplications = new HashSet<UserApplication>();
        }

        public int ApplicationId { get; set; }
        public string Name { get; set; } = null!;

        public virtual ICollection<UserApplication> UserApplications { get; set; }
    }
}

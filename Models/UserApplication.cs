using System;
using System.Collections.Generic;

namespace MasterUserAccountAPI.Models
{
    public partial class UserApplication
    {
        public int UserApplicationId { get; set; }
        public int UserId { get; set; }
        public int ApplicationId { get; set; }
        public string UserCredentials { get; set; } = null!;

        public virtual Application Application { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}

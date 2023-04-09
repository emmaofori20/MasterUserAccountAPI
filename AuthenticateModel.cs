namespace MasterUserAccountAPI
{
    public class AuthenticateModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public int ApplicationId { get; set; }
    }
    
    public class LoginModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}

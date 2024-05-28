using Newtonsoft.Json.Linq;

namespace OTPManager.Models
{
    public class UserVerificationRequest
    {
        public string? Identifier { get; set; }
        public string UserName { get; set; }
    }
    public class OTPVerificationRequest 
    {
        public string OTP { get; set; }
    }

    public class UserValidationRequest
    {
        public string? token { get; set; }
        public string smsOrEmail { get; set; }
        public int userId { get; set; }
    }

    public class UserRegistrationRequest
    {
        public string host { get; set; }
        public string smsOrEmail { get; set; }
        public int userId { get; set; }
    }
}

namespace OneTimeCodeApi.Models
{
    public class UserVerificationRequest
    {
        public required string PhoneNumber { get; set; }
        public required string UserName { get; set; }
    }
    public class OTPVerificationRequest : UserVerificationRequest
    {
        public required string OTP { get; set; }
    }
}

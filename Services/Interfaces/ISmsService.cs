namespace OTPManager.Services.Interfaces
{
    public interface ISmsService
    {
        public void SendSmsAsync(string phoneNumber, string message);
    }
}

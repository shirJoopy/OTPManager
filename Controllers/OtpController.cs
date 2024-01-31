using Microsoft.AspNetCore.Mvc;
using OneTimeCodeApi.Models;
using OneTimeCodeApi.Services;
using OTPManager.Services.Interfaces;


namespace OneTimeCodeApi.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class OtpController : ControllerBase
    {
        private readonly IOTPService _otpService;

        public OtpController(IOTPService otpService)
        {
            _otpService = otpService;
        }

        [HttpPost("generate")]
        public ActionResult<string> GenerateOtp([FromBody] string secretKey)
        {
            var otp = _otpService.GenerateTotp(secretKey);
            return Ok(otp);
        }

        [HttpPost("validate")]
        public ActionResult<bool> ValidateOtp([FromBody] OtpRequest request)
        {
            var isValid = _otpService.ValidateTotp(request.Otp, request.SecretKey);
            return Ok(isValid);
        }
    };

    public class OtpRequest
    {
        public required string Otp { get; set; }
        public required string SecretKey { get; set; }
    }
}
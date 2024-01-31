using Microsoft.AspNetCore.Mvc;
using OneTimeCodeApi.Models;
using OTPManager.Services.Interfaces;
using System.Text;

namespace OneTimeCodeApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class VerificationController : ControllerBase
    {
        private readonly IVerificationService _verificationService;
        private readonly IOTPService _otpService;

        public VerificationController(IVerificationService verificationService, IOTPService otpService)
        {
            _verificationService = verificationService;
            _otpService = otpService; // Assign the OTP service dependency
        }

        [HttpPost("request-code")]
        public IActionResult RequestOneTimeCode([FromBody] UserVerificationRequest request)
        {
            _verificationService.CleanSecrets(request.UserName);
            string secretKey = _verificationService.GetUserSecret(request.UserName, request.PhoneNumber);
            if (secretKey == null)
            {
                return BadRequest("Invalid user ID or phone number.");
            }


            var oneTimeCode = _otpService.GenerateTotp(secretKey);
            return Ok($"{{ \"status\": \"Ok\", \"data\" : \"{oneTimeCode}\" }} ");
        }
        [HttpPost("validate-code")]
        public IActionResult ValidateOneTimeCode([FromBody] OTPVerificationRequest request)
        {
            if (_verificationService != null)
            {
                string secretKey = _verificationService.GetUserSecret(request.UserName, request.PhoneNumber);
                if (secretKey == null)
                {
                    return BadRequest("Invalid user ID or phone number.");
                }



                var result = _otpService.ValidateTotp(request.OTP, secretKey);
                return Ok($"{{ \"status\": \"Ok\", \"data\" : \"{(result ? "true" : "false")}\" }} ");

            }


            return Ok($"{{ \"status\": \"Ok\", \"data\" : \"no verifaication service injected\" }} ");

        }



    }
}

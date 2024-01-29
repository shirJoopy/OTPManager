using Microsoft.AspNetCore.Mvc;
using OneTimeCodeApi.Models;
using OneTimeCodeApi.Services;

namespace OneTimeCodeApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class VerificationController : ControllerBase
    {
        private readonly IVerificationService _verificationService;

        public VerificationController(IVerificationService verificationService)
        {
            _verificationService = verificationService;
        }

        [HttpPost("request-code")]
        public IActionResult RequestOneTimeCode([FromBody] UserVerificationRequest request)
        {
            if (!_verificationService.ValidateUser(request.UserName, request.PhoneNumber))
            {
                return BadRequest("Invalid user ID or phone number.");
            }

            var oneTimeCode = _verificationService.GenerateAndSendCode(request.PhoneNumber);
            return Ok($"Code sent to {request.PhoneNumber}");
        }
    }
}

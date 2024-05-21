using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using OTPManager.Filters;
using OTPManager.Models;
using OTPManager.Services;
using OTPManager.Services.Interfaces;
using OTPManager.Utilities;
using System.Text;
using Vonage.Common.Monads;
using Vonage.Users;

namespace OneTimeCodeApi.Controllers
{
    public class TotpResult 
    {
        public string Status { get; set; }
        public Object Data { get; set; }

      
    }

    [Authorize]
    [ApiController]
    [Route("verification")]
    public class VerificationController : ControllerBase
    {
        private readonly IVerificationService _verificationService;
        private readonly IOTPService _otpService;
        private readonly ISmsService _smsService;
        private readonly IConfiguration _configuration;
        private readonly IEMailService _emailService;

        public VerificationController(IVerificationService verificationService, 
            IOTPService otpService, 
            ISmsService smsService,
            IConfiguration configuration,
            IEMailService eMailService
            )
        {
            _verificationService = verificationService;
            _otpService = otpService; // Assign the OTP service dependency
            _smsService = smsService;
            _configuration = configuration;
            _emailService = eMailService;
        }


        [HttpGet]
        [AllowAnonymous]
        [Route("check")]
        public IActionResult CheckHealth()
        {
            try
            {
                // Perform necessary health checks, e.g., check database connectivity, API dependencies, etc.
                if (StaticDataStore.Languages.Count > 0 && _verificationService.CheckDbConnection())
                {
                    return Ok("Service is up and running.");
                }
                else
                {
                    return StatusCode(500, "DB is not connected to service");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Health check failed: " + ex.Message);
            }
        }

        [HttpPost("register")]
        [Authorize(AuthenticationSchemes = "TOTP")]
        public IActionResult Register(int userId, string smsOrEmail,string host)
        {

            var user = _verificationService.GetUser(userId);
            string verificationToken = _verificationService.GetRegistrationToken(Int32.Parse(user["TENANT_ID"]),user["USERNAME"], smsOrEmail);
            var encodedToken = System.Web.HttpUtility.UrlEncode(verificationToken);
            var verificationLink = $"{host}/verify/{user["USER_ID"]}/{encodedToken}/{smsOrEmail}";
            if (smsOrEmail == "sms")
            {
                var message = $"Please verify your phone by visiting: {verificationLink}";
                _smsService.SendSmsAsync(user["PHONE_NUMBER"], message);
            } else
            {
                var body = $"Please verify your email by clicking on the link: {verificationLink}";
                _emailService.SendEmailAsync(user["EMAIL"], "Verify your email", body);

            }
            // Lookup the token in your database
            // If found, mark the email/phone number as verified
            // Respond accordingly
            return Ok("{ \"Status\": \"Ok\" }");
        }

        [HttpPost("verify")]
        [AllowAnonymous]
        public IActionResult Verify([FromBody] UserValidationRequest body) 
        {
            try
            {
                var user = _verificationService.GetUser(body.userId);

                string secret = _verificationService.GetUserSecret(Int32.Parse(user["TENANT_ID"]), user["USERNAME"], user["TOTP_IDENTIFIER"], body.smsOrEmail);

                if (secret == body.token)
                {

                    _verificationService.ValidateUser(body.userId, body.smsOrEmail);
                    return Ok("{ \"Status\": \"Ok\" }");

                }

                return NotFound("{ \"Status\": \"Error, Wrong token\" }");


            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
            // Lookup the token in your database
            // If found, mark the email/phone number as verified
            // Respond accordingly
        


        [HttpPost("request-totp")]
        [ServiceFilter(typeof(SendEmailTotp))] // Use ServiceFilter to support DI in the filter
        [ServiceFilter(typeof(SendSMSTotp))] // Use ServiceFilter to support DI in the filter
        [Authorize(AuthenticationSchemes = "TOTP")]
        public IActionResult RequestOneTimeCode(string smsOrEmail)
        {
            var username = HttpContext.Items["username"]?.ToString();
            var identifier = HttpContext.Items["identifier"]?.ToString();
            Dictionary<string, string> details = (Dictionary<string, string>)HttpContext.Items["userDetails"];
            var tenantId = Int32.Parse(details["TENANT_ID"]);
            _verificationService.CleanSecrets(username, smsOrEmail);

            
            var secret = _verificationService.GetUserSecret(tenantId, username, identifier,smsOrEmail);

            if (secret == string.Empty)
            {
                secret = _verificationService.GenerateAndSaveSecret(tenantId, username, smsOrEmail);
                if (secret == null)
                {
                    return Unauthorized($"{{ \"Status\": \"Error\" ,\"Data\": \"User did not validate {smsOrEmail}.\" }}");
                }
                
            }

            var oneTimeCode = _otpService.GenerateTotp(secret);
            HttpContext.Items.Add("TOTP", oneTimeCode);
            string field = smsOrEmail == "sms" ? "PHONE_NUMBER" : "EMAIL";
            if (smsOrEmail == "sms")
            {
                HttpContext.Items.Add("SEND_SMS", true);
            }
            else
            {
                HttpContext.Items.Add("SEND_EMAIL", true);
            }

            return Ok($"{{ \"Status\": \"Ok\", \"Data\":\"The TOTP was sent to {details[field]}\" }} ");
        }



        [HttpPost("request-jwt")]
        [AllowAnonymous]
        public IActionResult GetJWT([FromBody] UserVerificationRequest request)
        {
            try
            {


                if (request.Identifier == null)
                {
                    request.Identifier = request.UserName;
                }
                var user = _verificationService.GetUser(request.UserName, request.Identifier);
                string token = JWTGenerator.GenerateJwtToken(request.UserName, request.Identifier, request.TenantId, Int32.Parse(user["USER_ID"]), user["PHONE_NUMBER"], user["EMAIL"], _configuration);


                return Ok($"{{ \"Status\": \"Ok\", \"Data\" : \"{token}\" }} ");
            }
            catch (Exception ex)
            {
                if (ex.Message == "User not found.")
                {
                    return Unauthorized($" {{ \"Status\" : \"Error\", " +
                                            $"\"Data\": \"{ex.Message}\" " +
                                            $"}} ");
                }
                else
                {
                    return ValidationProblem($"{{ \"Status\" : \"Error\", " +
                                                 $"\"Data\": \"Contact System admin to check Configuration.\" " +
                                                 $"}} ");
                }

            }
        }

        [HttpGet("validate-totp/{totp}")]
        [Authorize(AuthenticationSchemes = "TOTP")]

        public IActionResult ValidateOneTimeCode(string totp,string smsOrEmail,string tokenType="")
        {

            if (_verificationService != null)
            {
                var username = HttpContext.Items["username"]?.ToString();
                var identifier = HttpContext.Items["identifier"]?.ToString();
                var userId = Int32.Parse(HttpContext.Items["userId"]?.ToString() ?? "");
                var tenantId = Int32.Parse(HttpContext.Items["tenantId"]?.ToString());
                Dictionary<string, string> details = (Dictionary<string, string>)HttpContext.Items["userDetails"];

                string secret;
                try
                {
                    secret = _verificationService.GetUserSecret(tenantId, username, identifier, smsOrEmail);

                    if (secret == "")
                    {
                        return Ok($"{{ \"Status\" : \"Error\", \"Data\": \"Invalid username or identifier\" }}");
                    }

                }
                catch (Exception)
                {
                    return Ok($"{{ \"Status\" : \"Error\", \"Data\":Somthing went wrong when getting user Secret\" }}");
                }


                var result = _otpService.ValidateTotp(totp, secret);
                if (result)
                {
                    string jwtToken = JWTGenerator.GenerateAccessJwtToken(details, _configuration);
                    var token = _verificationService.SetAndGetUserJoopyToken(userId, tokenType);
                    Response.Headers.Add("Authorization", "Bearer " + jwtToken);
                    return Ok($"{{ \"Status\": \"Ok\", \"Data\": \"{token}\" }}");
                }
                return Ok($"{{ \"Status\": \"Error\", \"Data\": \"Oops looks like wrong OTP\" }}");
                
            }

            return Ok($"{{ \"Status\": \"Error\", \"Data\" : \"No verifaication service injected\" }} ");

        }
    }
}

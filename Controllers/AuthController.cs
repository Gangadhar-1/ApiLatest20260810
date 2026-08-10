using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;
using Twilio.Types;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using static System.Net.WebRequestMethods;
using Microsoft.Identity.Client;
using System.Collections.Specialized;
using System.Web;
using System.Text;
using OtpAuthServices.AzureService;
using OtpAuthServices.Model;
using Twilio.TwiML.Messaging;
using Microsoft.AspNetCore.RateLimiting;



namespace OtpAuthServices.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {

        private readonly bhashsms _bhashsms;
        private readonly IConfiguration _configuration;
        private readonly TwilioSettings _twilioSettings;
        private readonly IMemoryCache _memoryCache;
        private string? LSSPHM;


        private readonly ICosmosDbService<OtpCache> _cosmosDbService;



        public AuthController(
           IMemoryCache memoryCache,
           ICosmosDbService<OtpCache> cosmosDbService,
           IOptions<bhashsms> bhashsms)
        {
            _memoryCache = memoryCache;
            _cosmosDbService = cosmosDbService;
            _bhashsms = bhashsms.Value;
        }



        [EnableRateLimiting("ratepolicy")]

        [HttpPost("bhashsmssendotp")]
        public async Task<IActionResult> bhashsmssendotp([FromBody] OptRequest request)
        {
            string dynmaicotp = GenerateRandomOtp();

            if (request.Type.ToLower() == "sms")
            {
                try
                {
                    string mobile = request.SenderValue?.Trim();

                    if (string.IsNullOrWhiteSpace(mobile))
                        return BadRequest("Mobile number is required.");

                    string message = $"Use Verification code {dynmaicotp} for HandyMan Authentication\r\n \r\nThanks\r\nHandy Man Service Providers\r\n https://handymanserviceproviders.com/";

                    string encodedMessage = Uri.EscapeDataString(message);

                    string user = _bhashsms.User;
                    string pass = _bhashsms.Password;
                    string sender = _bhashsms.Sender;
                    string priority = _bhashsms.Priority;
                    string stype = _bhashsms.Stype;

                    string apiUrl = $"http://bhashsms.com/api/sendmsg.php?user={user}&pass={pass}&sender={sender}&phone={mobile}&text={encodedMessage}&priority={priority}&stype={stype}";

                    using (var httpClient = new HttpClient())
                    {
                        var response = await httpClient.GetAsync(apiUrl);

                        if (response.IsSuccessStatusCode)
                        {
                            _memoryCache.Set(mobile, dynmaicotp, TimeSpan.FromSeconds(90));

                            var otpData = new OtpCache
                            {
                                id = mobile,
                                senderValue = mobile,
                                otp = dynmaicotp,
                                expiryTime = DateTime.UtcNow.AddSeconds(90)
                            };

                            await _cosmosDbService.UpsertItemAsync(otpData);

                            return Ok(new { Message = "OTP SMS sent successfully."});
                        }
                        else
                        {
                            return StatusCode((int)response.StatusCode, "SMS sending failed");
                        }
                    }
                }
                catch (Exception ex)
                {
                    return StatusCode(500, ex.Message);
                }
            }

            return BadRequest("Invalid request type");
        }

        //[HttpPost("bhashsmssendotp")]
        //public async Task<IActionResult> bhashsmssendotp([FromBody] OptRequest request)
        //{


        //    string dynmaicotp = GenerateRandomOtp();

        //    if (request.Type.ToLower() == "sms")
        //    {
        //        try
        //        {
        //            string mobile = request.SenderValue?.Trim();
        //            if (string.IsNullOrWhiteSpace(mobile))
        //            {
        //                return BadRequest("Mobile number is required.");
        //            }
        //            string message = $"Use Verification code {dynmaicotp} for HandyMan Authentication\r\n \r\nThanks\r\nHandy Man Service Providers\r\n https://handymanserviceproviders.com/";
        //            //string message = $"Use Verification code {dynmaicotp} for HandyMan Authentication. Thanks Handy Man Service Providers https://handymanserviceproviders.com/";
        //            string encodedMessage = Uri.EscapeDataString(message);

        //            string user = _bhashsms.User;
        //            string pass = _bhashsms.Password;
        //            string sender = _bhashsms.Sender;
        //            string priority = _bhashsms.Priority;
        //            string stype = _bhashsms.Stype;

        //            string apiUrl = $"http://bhashsms.com/api/sendmsg.php?user={user}&pass={pass}&sender={sender}&phone={mobile}&text={encodedMessage}&priority={priority}&stype={stype}";

        //            using (var httpClient = new HttpClient())
        //            {
        //                var response = await httpClient.GetAsync(apiUrl);
        //                var result = await response.Content.ReadAsStringAsync();

        //                if (response.IsSuccessStatusCode)
        //                {
        //                    _memoryCache.Set(mobile, dynmaicotp, TimeSpan.FromSeconds(90));
        //                    return Ok(new { Message = "OTP SMS sent successfully.OTP  " + dynmaicotp });
        //                }
        //                else
        //                {
        //                    return StatusCode((int)response.StatusCode, "Failed to send SMS: " + result);
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            return StatusCode(500, "Internal server error (sms): " + ex.Message);
        //        }
        //    }
        //    else
        //    {
        //        return BadRequest("Invalid request type. Only 'email' or 'sms' are supported.");
        //    }
        //}





        [HttpPost("sendpromosms")]
        public async Task<IActionResult> SendPromoSms([FromBody] PromoRequest request)
        {
            if (request == null)
                return BadRequest("Invalid request.");

            string mobile = request.mobile?.Trim();
            if (string.IsNullOrWhiteSpace(mobile))
                return BadRequest("Mobile number is required.");

            string name = request.name?.Trim();
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest("Name is required.");

            try
            {

                string messageTemplate =
                    "Hello , I'm {#var#} using the Handy Man Service Providers & Lakshmi Mart for all home services (plumbing, electrical, AC, painting, etc.) and groceries at the best prices . Download the APP now : https://play.google.com/store/apps/details?id=com.ShMJqnAZKkEl.natively LAKSHMI SAI SERVICE PROVIDER";

                string finalMessage = messageTemplate.Replace("{#var#}", name);

                string encodedMessage = Uri.EscapeDataString(finalMessage);

                string user = _bhashsms.User;
                string pass = _bhashsms.Password;
                string sender = _bhashsms.Sender;
                string priority = _bhashsms.Priority;
                string stype = _bhashsms.Stype;

                string apiUrl =
                    $"http://bhashsms.com/api/sendmsg.php?user={user}&pass={pass}&sender={sender}&phone={mobile}&text={encodedMessage}&priority={priority}&stype={stype}";

                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.GetAsync(apiUrl);
                    var result = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        return Ok(new
                        {
                            success = true,
                            message = "SMS sent successfully.",
                            providerResponse = result
                        });
                    }
                    else
                    {
                        return StatusCode((int)response.StatusCode,
                            new
                            {
                                success = false,
                                message = "Failed to send SMS.",
                                providerResponse = result
                            });
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error (sms): " + ex.Message);
            }
        }

        public class PromoRequest
        {
            public string mobile { get; set; }
            public string name { get; set; }
        }






        [HttpPost("sendLmartsms")]
        public async Task<IActionResult> SendLmartsms([FromBody] Lmart lmart)
        {
            if (lmart == null)
                return BadRequest("Invalid request.");

            string ticketId = lmart.TicketId?.Trim();
            if (string.IsNullOrWhiteSpace(ticketId))
                return BadRequest("TicketId is required.");

            string name = lmart.Name?.Trim();
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest("Name is required.");

            string phoneNumber = lmart.PhoneNumber?.Trim();
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return BadRequest("PhoneNumber is required.");

            string address = lmart.Address?.Trim();
            if (string.IsNullOrWhiteSpace(address))
                return BadRequest("Address is required.");

            try
            {
                // Hardcoded multiple mobile numbers (comma separated)
                string mobile = "6281198953";

                // SMS message with dynamic placeholders
                string messageTemplate =
                    "Dear Customer Care, a new order has been raised by {Name}. Ticket ID: {TicketId}, Phone Number: {PhoneNumber}, Address: {Address}. Please contact the customer at the earliest. LAKSHMI SAI SERVICE PROVIDER";

                string finalMessage = messageTemplate
                    .Replace("{Name}", name)
                    .Replace("{TicketId}", ticketId)
                    .Replace("{PhoneNumber}", phoneNumber)
                    .Replace("{Address}", address);

                string encodedMessage = Uri.EscapeDataString(finalMessage);

                string user = _bhashsms.User;
                string pass = _bhashsms.Password;
                string sender = _bhashsms.Sender;
                string priority = _bhashsms.Priority;
                string stype = _bhashsms.Stype;

                string apiUrl =
                    $"http://bhashsms.com/api/sendmsg.php?user={user}&pass={pass}&sender={sender}&phone={mobile}&text={encodedMessage}&priority={priority}&stype={stype}";

                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.GetAsync(apiUrl);
                    var result = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        return Ok(new
                        {
                            success = true,
                            message = "SMS sent successfully to multiple numbers.",
                            providerResponse = result
                        });
                    }
                    else
                    {
                        return StatusCode((int)response.StatusCode, new
                        {
                            success = false,
                            message = "Failed to send SMS.",
                            providerResponse = result
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error (sms): " + ex.Message);
            }
        }

        public class Lmart
        {
            public string Name { get; set; }
            public string TicketId { get; set; }
            public string PhoneNumber { get; set; }
            public string Address { get; set; }
        }




        [HttpPost("sendotp")]
        public async Task<IActionResult> SendOtp([FromBody] OptRequest request)
        {


            string dynmaicotp = GenerateRandomOtp();

            if (request.Type == "email")
            {
                if (string.IsNullOrEmpty(request.SenderValue))
                {
                    return Ok("Email and OTP are required.");
                }

                var smtpSettings = _configuration.GetSection("SmtpSettings");
                var fromEmail = smtpSettings["FromEmail"];
                var host = smtpSettings["Host"];
                var port = int.Parse(smtpSettings["Port"]);
                var username = smtpSettings["UserName"];
                var password = smtpSettings["Password"];

                try
                {
                    var smtpClient = new SmtpClient(host)
                    {
                        Port = 587,
                        Credentials = new NetworkCredential(username, password),
                        EnableSsl = true,
                        UseDefaultCredentials = false
                    };

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(fromEmail),
                        Subject = "Your single-use code",
                        Body = $"Hi {request.SenderValue},\n\nYour OTP is: {dynmaicotp}. It's valid for 3 minutes.",
                        IsBodyHtml = false,
                    };
                    mailMessage.To.Add(request.SenderValue);

                    await smtpClient.SendMailAsync(mailMessage);

                    // Cache OTP with expiration (e.g., 3 minutes)
                    _memoryCache.Set(request.SenderValue, dynmaicotp, TimeSpan.FromMinutes(3));

                    return Ok(new { Message = "OTP email sent successfully." });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, "Internal server error: " + ex.Message);
                }
            }
            else if (request.Type == "sms")
            {
                try
                {
                    string result;
                    string apiKey = "NTgzNDZjNzY3MjQ5NDI0YTMxNTE0ZjRlNjQ2MjY0NDU=";
                    string numbers = request.SenderValue;
                    //string dynamicOtp = GenerateRandomOtp();
                    string message = $"Use Verification code {dynmaicotp} for HandyMan Authentication\r\n \r\nThanks\r\nHandy Man Service Providers\r\n https://handymanserviceproviders.com/";
                    string sender = "LSSPHM";

                    // URL encode the message
                    string encodedMessage = WebUtility.UrlEncode(message);

                    string postData = $"apikey={apiKey}&numbers={numbers}&message={encodedMessage}&sender={sender}";

                    // Create the HTTP request
                    HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create("https://api.textlocal.in/send/");
                    objRequest.Method = "POST";
                    objRequest.ContentType = "application/x-www-form-urlencoded";
                    objRequest.ContentLength = Encoding.UTF8.GetByteCount(postData);


                    // Write the post data to the request stream
                    using (StreamWriter writer = new StreamWriter(objRequest.GetRequestStream()))
                    {
                        writer.Write(postData);
                    }

                    // Get the response from the server
                    HttpWebResponse objResponse = (HttpWebResponse)objRequest.GetResponse();
                    using (StreamReader reader = new StreamReader(objResponse.GetResponseStream()))
                    {
                        result = reader.ReadToEnd();
                    }
                    _memoryCache.Set(request.SenderValue, dynmaicotp, TimeSpan.FromMinutes(3));

                    return Ok(new { Message = "OTP Mobile sent successfully." });

                }
                catch (Exception ex)
                {
                    return StatusCode(500, "Internal server error: " + ex.Message);
                }
            }

            return Ok("Invalid OTP request.");
        }



        [HttpPost("verifyuserexist")]
        public async Task<IActionResult> VerifyUserExist([FromBody] VerifyUserExist request)
        {
            try
            {
                var user = await _cosmosDbService.GetUserByUserIdAsync(request.UserName);

                if (user != null)
                {
                    // User already exists, return a message
                    return Ok(new { message = "Username already exists, choose another username." });
                }

                // User does not exist, return no content
                return NoContent();
            }
            catch (Exception ex)
            {
                // Log the error (using your preferred logging approach)
                Console.WriteLine($"Error in VerifyUserExist: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while verifying the user." });
            }
        }

        [HttpPost("sendotpbefore")]
        public async Task<IActionResult> sendotpbefore([FromBody] OptRequest request)
        {
            var user = await _cosmosDbService.GetUserByEmailOrMobileAsync(request.SenderValue);

            if (user != null)
            {
                return Ok(new { message = "Email or Mobile Number already exists, choose another email or mobile." });

            }

            string dynmaicotp = GenerateRandomOtp();

            if (request.Type == "email")
            {
                if (string.IsNullOrEmpty(request.SenderValue))
                {
                    return Ok(new { Message = "Email and OTP are required." });
                }

                var smtpSettings = _configuration.GetSection("SmtpSettings");
                var fromEmail = smtpSettings["FromEmail"];
                var host = smtpSettings["Host"];
                var port = int.Parse(smtpSettings["Port"]);
                var username = smtpSettings["UserName"];
                var password = smtpSettings["Password"];

                try
                {
                    var smtpClient = new SmtpClient(host)
                    {
                        Port = 587,
                        Credentials = new NetworkCredential(username, password),
                        EnableSsl = true,
                        UseDefaultCredentials = false
                    };

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(fromEmail),
                        Subject = "Your single-use code",
                        Body = $"Hi {request.SenderValue},\n\nYour OTP is: {dynmaicotp}. It's valid for 3 minutes.",
                        IsBodyHtml = false,
                    };
                    mailMessage.To.Add(request.SenderValue);

                    await smtpClient.SendMailAsync(mailMessage);

                    // Cache OTP with expiration (e.g., 3 minutes)
                    _memoryCache.Set(request.SenderValue, dynmaicotp, TimeSpan.FromMinutes(3));

                    return Ok(new { Message = "OTP email sent successfully." });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, "Internal server error: " + ex.Message);
                }
            }
            else if (request.Type == "sms")
            {
                try
                {
                    //TwilioClient.Init(_twilioSettings.AccountSid, _twilioSettings.AuthToken);

                    //var message = MessageResource.Create(
                    //    body: $"Dear Applicant, your OTP is {dynmaicotp}. It's valid for 3 minutes.",
                    //    from: new PhoneNumber(_twilioSettings.FromPhoneNumber),
                    //    to: new PhoneNumber(request.SenderValue)
                    //);

                    //if (message.ErrorCode == null)
                    //{
                    //    // Cache OTP with expiration (e.g., 3 minutes)
                    //    _memoryCache.Set(request.SenderValue, dynmaicotp, TimeSpan.FromMinutes(3));
                    //    return Ok(new { Message = "OTP SMS sent successfully." });
                    //}
                    //else
                    //{
                    //    return Ok(new { success = false, error = message.ErrorMessage });
                    //}


                    //string dynamicOtp = GenerateRandomOtp(); // Assuming this function generates your OTP
                    //string message = HttpUtility.UrlEncode($"Use Verification code {dynmaicotp} for HandyMan Authentication\n\nThanks\nHandy Man Service Providers\nhttps://handymanserviceproviders.com/");
                    // string dynamicOtp = GenerateRandomOtp(); // Assuming this function generates your OTP
                    string result;
                    string apiKey = "NTgzNDZjNzY3MjQ5NDI0YTMxNTE0ZjRlNjQ2MjY0NDU=";
                    string numbers = request.SenderValue;
                    //string dynamicOtp = GenerateRandomOtp();
                    string message = $"Use Verification code {dynmaicotp} for HandyMan Authentication\r\n \r\nThanks\r\nHandy Man Service Providers\r\n https://handymanserviceproviders.com/";
                    string sender = "LSSPHM";

                    // URL encode the message
                    string encodedMessage = WebUtility.UrlEncode(message);

                    string postData = $"apikey={apiKey}&numbers={numbers}&message={encodedMessage}&sender={sender}";

                    // Create the HTTP request
                    HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create("https://api.textlocal.in/send/");
                    objRequest.Method = "POST";
                    objRequest.ContentType = "application/x-www-form-urlencoded";
                    objRequest.ContentLength = Encoding.UTF8.GetByteCount(postData);


                    // Write the post data to the request stream
                    using (StreamWriter writer = new StreamWriter(objRequest.GetRequestStream()))
                    {
                        writer.Write(postData);
                    }

                    // Get the response from the server
                    HttpWebResponse objResponse = (HttpWebResponse)objRequest.GetResponse();
                    using (StreamReader reader = new StreamReader(objResponse.GetResponseStream()))
                    {
                        result = reader.ReadToEnd();
                    }
                    _memoryCache.Set(request.SenderValue, dynmaicotp, TimeSpan.FromMinutes(3));

                    return Ok(new { Message = "OTP SMS sent successfully." });

                }
                catch (Exception ex)
                {
                    return StatusCode(500, "Internal server error: " + ex.Message);
                }
            }

            return Ok("Invalid OTP request.");
        }

        [HttpPost("validateotp")]
        public async Task<IActionResult> ValidateOtp([FromBody] OtpValidationRequest request)
        {
            if (string.IsNullOrEmpty(request.SenderValue) || string.IsNullOrEmpty(request.Otp))
            {
                return Ok(new { Message = "Sender value and OTP are required." });
            }

            // 1️⃣ Check MemoryCache first
            if (_memoryCache.TryGetValue(request.SenderValue, out string cachedOtp))
            {
                if (cachedOtp == request.Otp)
                {
                    _memoryCache.Remove(request.SenderValue);
                    return Ok(new { Message = "OTP validated successfully." });
                }
                else
                {
                    return NotFound(new { Message = "Invalid OTP." });
                }
            }

            // 2️⃣ If not found in cache → check Cosmos DB
            try
            {
                var otpRecord = await _cosmosDbService.GetItemAsync(request.SenderValue);

                if (otpRecord == null)
                    return NotFound(new { Message = "OTP expired or not found." });

                if (otpRecord.expiryTime < DateTime.UtcNow)
                    return NotFound(new { Message = "OTP expired." });

                if (otpRecord.otp != request.Otp)
                    return NotFound(new { Message = "Invalid OTP." });

                await _cosmosDbService.DeleteItemAsync(request.SenderValue);

                return Ok(new { Message = "OTP validated successfully." });
            }
            catch
            {
                return NotFound(new { Message = "OTP expired or not found." });
            }
        }


        //[HttpPost("validateotp")]
        //public async Task<IActionResult> ValidateOtp([FromBody] OtpValidationRequest request)
        //{
        //    if (string.IsNullOrEmpty(request.SenderValue) || string.IsNullOrEmpty(request.Otp))
        //    {
        //        return Ok(new { Message = "Sender value and OTP are required." });
        //    }



        //    // Otherwise, check OTP from cache
        //    if (_memoryCache.TryGetValue(request.SenderValue, out string cachedOtp))
        //    {
        //        if (cachedOtp == request.Otp)
        //        {
        //            _memoryCache.Remove(request.SenderValue); // Optionally remove OTP after successful validation
        //            return Ok(new { Message = "OTP validated successfully." });
        //        }
        //        else
        //        {
        //            return NotFound(new { Message = "Invalid OTP." });
        //        }
        //    }
        //    else
        //    {
        //        return NotFound(new { Message = "OTP expired or not found." });
        //    }
        //}



        [HttpPost("validateFirstOtp")]
        public async Task<IActionResult> validateFirstOtp([FromBody] OtpValidationRequest request)
        {
            if (string.IsNullOrEmpty(request.SenderValue) || string.IsNullOrEmpty(request.Otp))
            {
                return Ok("Sender value and OTP are required.");
            }

            // Check OTP from cache
            if (_memoryCache.TryGetValue(request.SenderValue, out string cachedOtp))
            {
                if (cachedOtp == request.Otp)
                {
                    // OTP is valid, you can now proceed with further logic
                    _memoryCache.Remove(request.SenderValue); // Optionally remove OTP after successful validation


                    //else
                    //{
                    return Ok(new { Message = "OTP validated successfully." });
                    //}
                }
                else
                {
                    return NotFound("Invalid OTP.");
                }
            }
            else
            {
                return NotFound("OTP expired or not found.");
            }
        }

        private string GenerateRandomOtp()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }
    }

    public class OtpValidationRequest
    {
        public string SenderValue { get; set; } // email or mobile
        public string Otp { get; set; }
    }
    public class OtpRequest
    {
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
    }

    public class OtpValidateUser
    {
        public string UserId { get; set; }
    }

    public class OtpValidateRequest
    {
        public string PhoneNumber { get; set; }
    }

    

    public class bhashsms
    {
        public string User { get; set; }

        public string Password { get; set; }

        public string Sender { get; set; }

        public string Priority { get; set; }

        public string Stype { get; set; }

    }
}



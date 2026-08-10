
namespace OtpAuthServices.Model;

public class OtpRequest
{
    public required string PhoneNumber { get; set; }
    public string Email { get; set; }   // Optional
    //public required string Otp { get; set; }     // OTP field (string)
}

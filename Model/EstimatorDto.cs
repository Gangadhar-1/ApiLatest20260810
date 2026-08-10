
namespace OtpAuthServices.Model;

public class Estimator
{

    public Guid EstimatorId { get; set; }

    public  string id { get; set; } 
    public required string EstimatorName { get; set; }
    
    public required string EstimatorFirmName { get; set; }
    public required string EstimatorFirmRegistrationNumber { get; set; }
    public required string PANNumber { get; set; }
    public string AadharNumber { get; set; }
    public string Address { get; set; }
    public required string State { get; set; }

    public required string StateId { get; set; }
    public required string District { get; set; }

    public required string DistrictId { get; set; }
    public required string ZipCode { get; set; }
    public required string PhoneNumber { get; set; }
    public string AlternativeMobileNumber { get; set; } = string.Empty;
    public required string NameOfTheOwnerShip { get; set; }
    public required string PhoneVerificationCode { get; set; }
    public required string EmailAddress { get; set; }
    public required string EmailVerificationCode { get; set; }
    public required string Category { get; set; }


   // public  string EstimatorPhotoUrl { get; set; }

    public  string EstimatorPhotoId { get; set; }
    public Guid UserId { get; set; }
    public bool IsApproved { get; set; }         // Whether the dealer is approved
    public bool IsRejected { get; set; }
    public bool IsPending { get; set; }   // Whether the dealer is pending

    public string Status { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    //public required string UserPassword { get; set; }  
}

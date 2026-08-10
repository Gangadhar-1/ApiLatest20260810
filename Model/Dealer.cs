namespace  OtpAuthServices.Model;
public class Dealer
{

    public  Guid DealerId { get; set; } 
    
    public string id { get; set; }  
    public required string DealerFirmName { get; set; }
    public required string OwnershipName { get; set; }
    public required string Category { get; set; }
    public required string PANNumber { get; set; }
    public required string Address { get; set; }
    public required string State { get; set; }

    public required string StateId { get; set; }
    public required string District { get; set; }

    public required string DistrictId { get; set; }
    public required string ZipCode { get; set; }
    public string LandMark { get; set; } = string.Empty;    
    public required string PhoneNumber { get; set; }
    public required string PhoneVerificationCode { get; set; }
    public required string EmailAddress { get; set; }
    public required string EmailVerificationCode { get; set; }
    public string AlternativeMobile { get; set; } = string.Empty;

    public   string GSTNUMBER { get; set; }     
    public string GSTDocumentId {  get; set; }

    public  string  FirmRegistrationNumber { get; set; }

    public  required  string FirmRegistrationDocumentId {  get; set; }

    public  string DealerPhotoId { get; set; }=string.Empty;
 
    public bool IsApproved { get; set; }         // Whether the dealer is approved
    public bool IsRejected { get; set; }
    public bool IsPending { get; set; }   // Whether the dealer is pending

    public string Status { get; set; } = string.Empty;

    public bool IsActive { get; set; }


    public Guid UserId { get; set; }       
    
    
}

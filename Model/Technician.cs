namespace OtpAuthServices.Model;

// Models/Technician.cs
public class Technician
{


   public Guid TechnicianId { get; set; }   

    public string id { get; set; }

    public DateTime Date { get; set; }
    public required string TechnicianFullName { get; set; }
    public  string ? PANNumber { get; set; }
    public int  NumberOfTechnicians  { get; set; }  
    public required string AadharNumber { get; set; }
    public required string Address { get; set; }
    public required string State { get; set; }

    public required string StateId { get; set; }
    public required string District { get; set; }

    public required string DistrictId { get; set; }
    public required string ZipCode { get; set; }
   
    public required string PhoneNumber { get; set; }
    public string AlternativePhoneNumber { get; set; } = string.Empty;
    public required string PhoneVerificationCode { get; set; }
    public  string ? EmailAddress { get; set; }
    public  string ? EmailVerificationCode { get; set; }
    public required string Category { get; set; }
    //public string Status { get; set; }
    //public  string TechnicianPhotoUrl { get; set; }
    public string TechnicianPhotoId { get; set; }
    public Guid UserId { get; set; }
    public bool IsApproved { get; set; }         // Whether the dealer is approved
    public bool IsRejected { get; set; }
    public bool IsPending { get; set; }   // Whether the dealer is pending

    public string Status { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    //public required string UserPassword { get; set; }
}


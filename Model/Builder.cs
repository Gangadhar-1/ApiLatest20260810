public class Builder
{


    public Guid BuilderId { get; set; } 

    public  string id { get; set; }
    public required string BuilderFirmName { get; set;}
    public required string BuilderName     { get; set; }
    public required string PanNumber       { get; set; }
    public required string AadharNumber    { get; set; }
    public required string GSTNumber       { get; set; }
    public required string Address      { get; set; }
    public     string Landmark  { get; set; }=string.Empty;
    public required string State { get; set; }

    public required string StateId { get; set; }
    public required string District { get; set; }

    public required string DistrictId { get; set; }
    public string AlternativeMobileNumber { get; set; } =string.Empty;  
    public required string ZipCode                 { get; set; }
    public required string PhoneNumber             { get; set; }
    public required string PhoneVerificationCode   { get; set;}
    public required string EmailAddress            { get; set; }
    public required string EmailVerificationCode { get; set; }
    public required string Category { get; set; }

    //public  string BuilderPhotoUrl { get; set; }

    public string BuilderPhotoId { get; set; } = string.Empty;
    public required string BuilderRegistrationDocumentUrl { get; set; }

    public  string BuilderRegistrationId { get; set; }  

    public Guid UserId { get; set; }
    public bool IsApproved { get; set; }         // Whether the dealer is approved
    public bool IsRejected { get; set; }
    public bool IsPending { get; set; }   // Whether the dealer is pending

    public string Status { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    //public required string UserPassword { get; set; }
}


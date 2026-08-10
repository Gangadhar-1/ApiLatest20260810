using System.Xml;

namespace OtpAuthServices.Model
{
    public class Customer
    {
        public string CustomerId { get; set; }

        public string id { get; set; }                                                                 
        
        public string Date { get; set;}      
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string MobileNumber { get; set; }
        public required string MobileVerificationCode { get; set; }
        public  string? EmailAddress { get; set; }
        public  string? EmailVerificationCode { get; set; }

        // Optional fields should not have [required] annotations or the 'required' keyword
        public string AlternativeMobileNumber { get; set; } = string.Empty;
        public string GSTNumber { get; set; } = string.Empty;

        public required string Address { get; set; }
        public string Landmark { get; set; } = string.Empty;
        public required string State { get; set; }

        public required string StateId { get; set; }
        public required string District { get; set; }

        public required string DistrictId { get; set; }
        public required string ZipCode { get; set; }

       // public string CustomerphotoUrl { get; set; }

        public  string CustomerPhotoId { get; set; }=string.Empty;

        public Guid UserId { get; set; } // Unique identifier

        public bool IsApproved { get; set; }         // Whether the dealer is approved
        //public bool IsRejected { get; set; }
        //public bool IsPending { get; set; }   // Whether the dealer is pending

        public string Status { get; set; } = string.Empty;



        public string WalletAmount { get; set; }

        
    }
}

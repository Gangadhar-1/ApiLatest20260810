namespace OtpAuthServices.Model
{
    public class UpdateDocumentRequest
    {

        public string Id { get; set; }

        // Dealer identifier, just as an additional field (if needed for reference in other logic)
        public string UserId { get; set; }

        // Status to be updated in the document
        public string Status { get; set; }

        // Profile type (could be Dealer, Admin, etc.)
        public string Profiletype { get; set; }

        // Who approved the profile (e.g., Admin)
        public string ProfileApprovedby { get; set; }

        // Who requested the profile update (e.g., dealer)
        public string ProfileRequestedby { get; set; }

        // The date when the profile was created
        public DateTime CreatedDate { get; set; }

        // The date when the profile was last modified
        public DateTime ModifiedDate { get; set; }

        // Any comments related to the document update
        public string Comments { get; set; }
    }

    }

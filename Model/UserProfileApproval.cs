namespace OtpAuthServices.Model
{
    public class UserProfileApproval
    {

        public string id { get; set; }
        public string Userid { get; set; }

        public string Status { get; set; }

        public DateTime RequestedDate { get; set; }

        public string RequestedBy { get; set; }

        public DateTime ApprovedDate { get; set; }


        public string ApprovedBy { get; set; }

        public string Comments { get; set; }
    }
}

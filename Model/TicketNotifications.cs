namespace OtpAuthServices.Model
{
    public class TicketNotifications
    {

        public string id { get; set; }

        public string TicketNotificationId { get; set; }

        public string TicketId { get; set; }

        public string MobileNumber { get; set; }

        public string EmailId { get; set; }


        public bool IsSMSDelivered { get; set; }

        public bool IsMobileDelivered { get; set; }

        public string Smslog { get; set; }

        public string MobileLog { get; set; }

        public string SmsDispatchedDate { get; set; }

        public string EmailDispatchedDate { get; set; }

        public string TechnicianId { get; set; }


        public string DealerId { get; set; }


    }
}


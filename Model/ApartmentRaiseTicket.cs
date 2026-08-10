namespace OtpAuthServices.Model
{
    public class ApartmentRaiseTicket
    {

        public string id { get; set; }

        public string ApartmentRaiseTicketId { get; set; }

        public string UserId { get; set; }


        public DateTime Date { get; set; }
        public string Subject { get; set; }

        public string Details { get; set; }

        public string State { get; set; }


        public string Category { get; set; }

        public string AssignedTo { get; set; }
        public string District { get; set; }
        public string ApartmentName { get; set; }

        public string phoneNumber { get; set; }

        public string NumberOfFlats { get; set; }

        public string TotalAmount { get; set; }

        public string ConsentPersonName { get; set; }

        public string ApartmentAddress { get; set; }

        public string Pincode { get; set; }

        public List<string> Attachments { get; set; } = new List<string>();

        public string PaymentId { get; set; }

        public string PaidAmount { get; set; }

        public string IsSubscription { get; set; }

        public string Status { get; set; }


    }
}

namespace OtpAuthServices.Model
{
    public class Payment
    {
        public string id { get; set; }


        public string RaiseTicketId { get; set; }
        public string PaymentId { get; set; }

        public string PaymentMode { get; set; }

        public string ApprovedAmount { get; set; }


        public string PaidAmount { get; set; }


        public string BalancedAmount { get; set; }


        public string PaymentDataTime { get; set; }

        public string TechnicianAmount { get; set; }


        public string DealerAmont { get; set; }


        public string CustomerCareAmount { get; set; }

        public string UTRTransactionNumber { get; set; }


        public string TechnicianConfirmationCode { get; set; }
    }
}


namespace OtpAuthServices.Model
{
    public class 
        PaymentRequest
    {

        public string id { get; set; }
        public string OrederId { get; set; }

        public string OrderDate { get; set; }

        public string PaidAmount { get; set; }

        public string TransactionStatus { get; set; }

        public string TransactionType { get; set; }

        public string UTRNumber { get; set; }   

        public string InvoiceId { get; set; }


        public string InvoiceURL { get; set; }
    }
}

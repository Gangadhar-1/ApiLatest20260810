namespace OtpAuthServices.Model
{
    public class GroceryPaymentRequest
    {

        public string id {  get; set; }

        public string UTRTransactionNumber { get; set; }

        public string TransactionNumber { get; set; }   

        public string TransactionStatus { get; set; }

       public string  TransactionType { get; set; }

        public string PaidAmount { get; set; }

        public string Status { get; set; }
    }
}

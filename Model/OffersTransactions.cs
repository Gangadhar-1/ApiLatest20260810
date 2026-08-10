namespace OtpAuthServices.Model
{

    public class OffersTransactions
    {

        public string id { get; set; }

        public string UserId { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime UpdatedDate { get; set; }

        //public string TicketId { get; set; }

        public string TotalWalletAmount { get; set; }

        public string AvailedAmount { get; set; }


        public string RemainingAmount { get; set; }


    }
}

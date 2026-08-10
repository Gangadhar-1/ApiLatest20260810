namespace OtpAuthServices.Model
{
    public class RaiseAQuote
    {
        public string id { get; set; }

        public DateTime QuotedDate { get; set; }

        public string RaiseAQuoteId { get; set; }

        public string RaiseTicketId { get; set; }
        public string CustomerId { get; set; }

        public string TicketId { get; set; }

        public string TechnicianId { get; set; }

        public string EnterQuoteAmount { get; set; }

        public string Discount { get; set; }

        public string Othercharges { get; set; }


        public string ServiceCharges { get; set; }

        public string GST { get; set; }

        // public string TotalQuotedAmount { get; set; }

        //public string LowestBidder { get;set; }

        public string TotalAmount { get; set; }

        public string fixedQuote { get; set; }

        public string fixedDiscount { get; set; }
        public string fixedOtherCharge
        { get; set; }




        public string fixedServiceCharge { get; set; }

        public string fixedGST { get; set; }

        public  List<MaterialQuotation> materialQuotation { get; set; }
        public List<AddRemark> AddRemarks { get; set; }

        public List<Materials> Materials { get; set; }   


    }


    public class AddRemark
    {
        public DateTime RequestedDate { get; set; }

        public string Remarks { get; set; }
    }

    public class Materials
    {
        public string material { get; set; } 
        public string Quantity { get; set; }
       public string  Price { get; set; }

        public string Total { get; set; }

    }

    public class MaterialQuotation
    {
        public string discount { get; set; }
        public string deliverycharges { get; set; }
        public string servicecharges { get; set; }
        public string gst { get; set; }
        public string grandtotal  { get; set; }

        public  string fixedDiscount { get; set; }

        public string fixedDeliveryChargs { get; set; } 

        public string fixedServicecharges {  get; set; }

        public string fixedGST { get; set; }

        public MaterialQuotation()
        {
            discount = "0";
            deliverycharges = "0";
            servicecharges = "0";
            gst = "0";
            grandtotal = "0";
        }
    }


}

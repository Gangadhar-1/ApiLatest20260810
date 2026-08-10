namespace OtpAuthServices.Model
{
    public class RaiseAQuoteByDealer
    {

        public string id { get; set; }
        public string TicketId { get; set; }
        public string CustomerId { get; set; }
        public string DealerId { get; set; }
        public string RaiseTicketId { get; set; }
        
        public DateTime RaiseAQuoteDate { get; set; }
        public string RaiseAQuoteByDealerId { get; set; }
        
        public string TotalAmount { get; set; } 
        //public string Subject { get; set; }

        //public string Status { get; set; }

        //public string internalStatus { get; set; }
        //public string AssignedTo { get; set; }
        //public string Details { get; set; }

        //public string Category { get; set; }

        //public string State { get; set; }

        //public string District { get; set; }
        public List<MaterialQuotation> materialQuotation { get; set; }
        public List<AddRemark> AddrRmarks { get; set; }

        public List<Materials> Materials { get; set; }
    }
}


public class Materials
{
    public string material { get; set; }
    public string Quantity { get; set; }
    public string Price { get; set; }

    public string Total { get; set; }

    

 

}

public class MaterialQuotations
{
    public string discount { get; set; }
    public string deliverycharges { get; set; }
    public string servicecharges { get; set; }
    public string gst { get; set; }
    public string grandtotal { get; set; }

    public string fixedDiscount { get; set; }

    public string fixedDeliveryChargs { get; set; }

    public string fixedServicecharges { get; set; }

    public string fixedGST { get; set; }
}

public class AddDealerRemark
{
    public DateTime RequestedDate { get; set; }

    public string DealerRemarks { get; set; }
}


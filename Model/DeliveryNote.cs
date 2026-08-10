namespace OtpAuthServices.Model
{
    public class DeliveryNote
    {
        public string id { get; set; }

        public string TicketId { get; set; }

        public string DeliveryNoteId { get; set; }

         public string   InvoiceNumber { get; set; }    
       public string  InvoiceDate { get; set; }     
        public List<string> UploadInvoice { get; set; } = new List<string>();


        public string Option1Day { get; set; }
        public string Option1Time { get; set; }
        public string Option2Day { get; set; }
        public string Option2Time { get; set; }

        public string DeliveryTime { get; set; }

        public string DeliveryInvoiceId { get; set; }

        public string InternalStatus { get; set; }

        public string TechnicianStatus { get; set; }

        public string DealerStatus { get; set; }




        public List<TechnicianAcceptance> TechnicianAcceptance { get; set; }


        public List<DealerAcceptance> DealerAcceptance { get; set; }


        public List<MaterialCollection> MaterialCollection { get; set; }


        public string AssignedTo { get; set; }
    }

    public class MaterialCollection
    {
        public string Material { get; set; }

        public string Quantity { get; set; }

        public string ReceivedQuantity { get; set; }

        public string RemainingQuantity { get; set; }
    }


    public class TechnicianAcceptance
    {
        public string Type { get; set; }

        public string TechnicianRemarks { get; set; }

    }

    public class DealerAcceptance
    {
        public string Type { get; set; }

        public string DealerRemarks { get; set; }

    }
}
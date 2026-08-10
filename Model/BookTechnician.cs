namespace OtpAuthServices.Model
{
    public class BookTechnician
    {

        public string id { get; set; }
        public string BookTechnicianId { get; set; }

        public DateTime Date { get; set; }
        public string CustomerName { get; set; }

        public string CustomerEmail { get; set; }
        public string Address { get; set; }


        public string Category { get; set; }

        public string status { get; set; }

        public string NoOfQuantity { get; set; }

        public string TotalAmount { get; set; }


        public string AssignedTo { get; set; }

             

        public string TechnicianFullName { get; set; }
       
        public string CustomerId { get; set; }

        public string State { get; set; }

        public int StatusCode { get; set; }
        public string District { get; set; }
        public string ZipCode { get; set; }
        public string phoneNumber { get; set; }

        public string Remarks { get; set; }


        public string Rate { get; set; }
        public string Discount { get; set; }

        public string MoreInfo { get; set; }

        public string AfterDiscount { get; set; }

        public string JobDescription { get; set; }

        public string PaymentMode { get; set; }

        public string ApprovedAmount { get; set; }
        public string UTRTransactionNumber { get; set; }


        public string TechnicianConfirmationCode { get; set; }


        public string OrderId { get; set; }

        public string OrderDate { get; set; }

        public string PaidAmount { get; set; }

        public string TransactionStatus { get; set; }

        public string TransactionType { get; set; }

        public string InvoiceId { get; set; }


        public string InvoiceURL { get; set; }

        public string TechnicianPincode { get; set; }


        public List<string> TechnicianName { get; set; } = new List<string>();
  }
}

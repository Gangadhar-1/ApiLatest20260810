namespace OtpAuthServices.Model
{
    public class Collections
    {

        public string id { get; set; }
        public string Date { get; set; }
        public string LakshmiCollectionId { get; set; }

        public string CustomerId { get; set; }

        public bool IsDelivered { get; set; }
        public double latitude { get; set; }

        public double longitude { get; set; }
        public string AssignedTo { get; set; }

        public string DeliveryPartnerUserId { get; set; }
        public string CustomerName { get; set; }

        public string CustomerPhonenumber { get; set; }



        public string TotalItemsSelected { get; set; }

        public string Address { get; set; }

        public string State { get; set; }


        public string District { get; set; }

        public string ZipCode { get; set; }

        public string Status { get; set; }
        public string PaymentMode { get; set; }


        public string UTRTransactionNumber { get; set; }

        public string TransactionNumber { get; set; }

        public string TransactionStatus { get; set; }


        public string TransactionType { get; set; }
        public string PaidAmount { get; set; }
        public string GrandTotal { get; set; }

        public List<Categories> categoriess { get; set; } = new List<Categories>();
    }

    public class Categories
    {
        public string CategoryName { get; set; }
       
        public string ProductName { get; set; }
        public int NoOfQuantity { get; set; }
        public string ProductImage { get; set; }
        public decimal MRP { get; set; }
        public decimal Discount { get; set; }

        public string size { get; set; }

        public string colour { get; set;}
        public string StockLeft { get; set; }
        public string code { get; set; }
        public decimal AfterDiscountPrice { get; set; }
    


}
}

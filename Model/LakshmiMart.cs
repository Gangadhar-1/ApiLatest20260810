
namespace OtpAuthServices.Model
{
    public class LakshmiMart
    {
        public string id { get; set; }

        public string customerId { get; set; }

        public bool IsPickUp { get; set; }

        public bool IsDelivered { get; set; }

        public string TotalWalletAmount { get; set; }

        public string AvailedAmount { get; set; }


        public string RemainingAmount { get; set; }

        public double latitude { get; set; }

        public double longitude { get; set; }

        public string CustomerName { get; set; }

        public string AssignedTo { get; set; }

        public string DeliveryPartnerUserId { get; set; }

        public string CustomerPhoneNumber { get; set; }
        public string MartId { get; set; }
        public string Date { get; set; }


        public string DeliveryAssignedTime { get; set; }

        public string DeliverySubmitTime { get; set; }

        public string GrandTotal { get; set; }

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

        public string WalletAmount { get; set; }

       
        public string SlotTime { get; set; }

        //public string Location { get; set; }

        public List<Category> Categories { get; set; } = new List<Category>();

        //public static implicit operator LakshmiMart(LakshmiMart_v1 v)
        //{
        //    throw new NotImplementedException();
        //}
    }

    public class Category
    {
        public string CategoryName { get; set; }
        public int NumberOfItemsSelected { get; set; }
        public decimal TotalAmount { get; set; }

        public List<Products> Products { get; set; } = new List<Products>();
    }


    public class Products
    {
        public string ProductName { get; set; }
        public int NoOfQuantity { get; set; }
        public string ProductImage { get; set; }
        public decimal MRP { get; set; }
        public decimal Discount { get; set; }

        public string StockLeft { get; set; }

        public string code { get; set; }
        public string Units { get; set; }

        public decimal AfterDiscountPrice { get; set; }
    }
}

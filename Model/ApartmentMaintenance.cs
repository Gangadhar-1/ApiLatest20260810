namespace OtpAuthServices.Model
{
    public class ApartmentMaintenance
    {
        public string id { get; set; }

        public string UserId { get; set; }

        public string ApartmentMaintenanceId { get; set; }

        public string Date { get; set; }

        public string ApartmentName { get; set; }

        public string ApartmentAddress { get; set; }


        public string State { get; set; }


        public string District { get; set; }
        public string PinCode { get; set; }

        public string ConsentPersonName { get; set; }

        public string MobileNumber { get; set; }

        public string NumberOfFlats { get; set; }

        public string TotalAmount { get; set; }

        public string PaymentId { get; set; }

        public string IsSubscription
        {
            get; set;
        }

        public string SubscriptionDate { get; set; }

        public string Status { get; set; }
        public string PaidAmount { get; set; }



    }
}

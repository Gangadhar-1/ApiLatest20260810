using Newtonsoft.Json;

namespace OtpAuthServices.Model
{
    public class CustomerDTO
    {
        [JsonProperty("CustomerId")]
        public string CustomerId { get; set; }

        [JsonProperty("FirstName")]
        public string FirstName { get; set; }

        [JsonProperty("LastName")]
        public string LastName { get; set; }

        [JsonProperty("MobileNumber")]
        public string MobileNumber { get; set; }

        [JsonProperty("EmailAddress")]
        public string EmailAddress { get; set; }

        [JsonProperty("Address")]
        public string Address { get; set; }

        [JsonProperty("State")]
        public string State { get; set; }

        [JsonProperty("District")]
        public string District { get; set; }

        [JsonProperty("ZipCode")]
        public string ZipCode { get; set; }

        [JsonProperty("CustomerPhotoId")]
        public string CustomerPhotoId { get; set; }

        [JsonProperty("Status")]
        public string Status { get; set; }


        [JsonProperty("StateId")]
        public string StateId { get; set; }

        [JsonProperty("DistrictId")]
        public  string DistrictId { get; set; }
    }
}

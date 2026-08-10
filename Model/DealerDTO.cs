using Newtonsoft.Json;

namespace OtpAuthServices.Model
{
    public class DealerDTO
    {
        /// <summary>
        /// Unique identifier for the dealer.
        /// </summary>
        [JsonProperty("dealerId")]
        public Guid DealerId { get; set; }

        [JsonProperty("dealerFirmName")]
        public string DealerFirmName { get; set; }

        [JsonProperty("phoneNumber")]
        public string PhoneNumber { get; set; }

        [JsonProperty("emailAddress")]
        public string EmailAddress { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("district")]
        public string District { get; set; }

        [JsonProperty("zipCode")]
        public string ZipCode { get; set; }

        [JsonProperty("dealerPhotoId")]
        public string DealerPhotoId { get; set; }

        [JsonProperty("Status")]
        public string Status { get; set; }

        [JsonProperty("IsActive")]
        public bool IsActive { get; set; }

        [JsonProperty("StateId")]
        public string StateId { get; set; }

        [JsonProperty("DistrictId")]
        public string DistrictId { get; set; }


    }

}

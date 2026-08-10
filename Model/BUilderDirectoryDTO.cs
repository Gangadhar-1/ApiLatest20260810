using Newtonsoft.Json;

namespace OtpAuthServices.Model
{
    public class BUilderDirectoryDTO
    {
        [JsonProperty("builderId")]
        public string BuilderId { get; set; }

        [JsonProperty("builderName")]
        public string BuilderName { get; set; }

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

        [JsonProperty("ZipCode")]
        public string ZipCode { get; set; }

        [JsonProperty("builderPhotoId")]
        public string BuilderPhotoId { get; set; }

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

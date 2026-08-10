namespace OtpAuthServices.Model
{
    public class UploadFileResponse
    {
        public required string FileReturnUrl { get; set; }

        public required string UserId { get; set; }

        public ProfileType Type { get; set; }


        public enum ProfileType
        {
            Customer = 0,
            Dealer  = 1,
            Estimator =2,
            Builder= 3,
            Technician = 4



        }
            






    }
}

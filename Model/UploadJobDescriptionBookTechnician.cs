namespace OtpAuthServices.Model
{
    public class UploadJobDescriptionBookTechnician
    {
        public string id { get; set; }
        public DateTime? CreatedAt { get; set; }

        public string uploadBookTechnicianId { get; set; }
        public string Category { get; set; }
        public List<SelectedJob> SelectedJobs { get; set; } = new List<SelectedJob>();
       
    }

    public class SelectedJob
    {
        public string JobDescription { get; set; }
        public string Rate { get; set; }
        public string Discount { get; set; }
        public string AfterDiscount { get; set; }

        public string Remarks { get; set; }
        public string MoreInfo { get; set; }
    }

}

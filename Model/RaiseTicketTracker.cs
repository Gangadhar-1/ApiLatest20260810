namespace OtpAuthServices.Model
{
    public class RaiseTicketTracker
    {
        public int Total   { get; set; }
        public int Open    { get; set; }
        public int Pending { get; set; }
        public int NotAssigned { get; set; }
        public int Closed { get; set; }

        public int State { get; set; }

        public int District { get; set; }

        public int ZipCode { get; set; }

        public void CalculateTotalCount()
        {
            Total = Open + Pending + NotAssigned + Closed ;
        }

    }
}

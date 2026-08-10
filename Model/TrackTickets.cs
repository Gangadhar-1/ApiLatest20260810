namespace OtpAuthServices.Model
{
    public class TrackTickets
    {
        //public string TicketId { get; set; }
        //public string Status { get; set; } // e.g., "To do", "In Progress", "Done"
        //public string Name { get; set; }  // e.g.
        //public string TicketPhotoId { get; set; } = string.Empty;
        //public required string Subject { get; set; } = "Uncategorized";
        //public required string Category { get; set; } = "Uncategorized";
        //public required string AssignedTo { get; set; } = "Uncategorized";

        //public required string Details { get; set; } = "Uncategorized";

        //public required string Address { get; set; } = "Uncategorized";
        public string CustomerId { get; set; }
        public string TicketId { get; set; }
        public DateTime Date { get; set; }
        public string Address { get; set; }
        public string Subject { get; set; }
        public string Category { get; set; }
        public string AssignedTo { get; set; }
        public string Id { get; set; }
        //public string CustomerId { get; set; }
        public List<string> Attachments { get; set; }




    }
}

namespace EventManagementSystem.DTOs
{
    public class PayTicketDto
    {
        public int RegistrationId { get; set; }
        public int PaidByUserId { get; set; } // Owner, Admin, or SuperAdmin
    }

    public class TicketResponseDto
    {
        public int TicketId { get; set; }
        public int RegistrationId { get; set; }
        public int UserId { get; set; } // Exposed in response
        public string TicketCode { get; set; } = string.Empty;
        public string TicketType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // "Unpaid", "Paid", "Cancelled"
        public DateTime IssuedAt { get; set; }
    }
}
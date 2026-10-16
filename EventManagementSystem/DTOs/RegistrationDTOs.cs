using System.ComponentModel.DataAnnotations;

namespace EventManagementSystem.DTOs
{
    public class CreateRegistrationDto
    {
        public int EventId { get; set; }
        public int UserId { get; set; }

        [Required]
        [RegularExpression("^(Online|WalkIn|Walk-In)$", ErrorMessage = "RegistrationType must be 'Online' or 'WalkIn'.")]
        public string RegistrationType { get; set; } = "Online";

        [Required]
        [RegularExpression("^(Standard|VIP)$", ErrorMessage = "TicketType must be 'Standard' or 'VIP'.")]
        public string TicketType { get; set; } = "Standard";

        [Range(1, 10, ErrorMessage = "You can purchase between 1 and 10 tickets.")]
        public int Quantity { get; set; } = 1;
    }

    public class RegistrationResponseDto
    {
        public int RegistrationId { get; set; }
        public int EventId { get; set; }
        public int UserId { get; set; }
        public string RegistrationType { get; set; } = string.Empty;
        public string TicketType { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime RegistrationDate { get; set; }
    }
}
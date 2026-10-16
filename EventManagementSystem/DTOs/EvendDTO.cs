using System.ComponentModel.DataAnnotations;

namespace EventManagementSystem.DTOs
{
    public class CreateEventDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        [Required]
        public string Venue { get; set; } = string.Empty;
        [Range(1, 100000, ErrorMessage = "Capacity must be at least 1.")]
        public int Capacity { get; set; }
        [Range(0, 999999.99, ErrorMessage = "Price cannot be negative.")]
        public decimal Price { get; set; } = 0.00m;

        public DateTime RegistrationStart { get; set; }
        public DateTime RegistrationEnd { get; set; }
        public DateTime EventStart { get; set; }
        public DateTime EventEnd { get; set; }
        public int CreatedBy { get; set; }
    }

    public class UpdateEventDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        [Required]
        public string Venue { get; set; } = string.Empty;

        [Range(1, 100000, ErrorMessage = "Capacity must be at least 1.")]
        public int Capacity { get; set; }

        [Range(0, 999999.99, ErrorMessage = "Price cannot be negative.")]
        public decimal Price { get; set; }

        public DateTime RegistrationStart { get; set; }
        public DateTime RegistrationEnd { get; set; }
        public DateTime EventStart { get; set; }
        public DateTime EventEnd { get; set; }
        public int UpdatedBy { get; set; }
        public string? Reason { get; set; }
    }

    public class EventResponseDto
    {
        public int EventId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Venue { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public int AvailableSlots { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime EventStart { get; set; }
        public DateTime EventEnd { get; set; }
    }
}
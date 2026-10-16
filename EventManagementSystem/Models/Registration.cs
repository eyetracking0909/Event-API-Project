using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EventManagementSystem.Models;

public partial class Registration
{
    [Key]
    public int RegistrationId { get; set; }

    public int EventId { get; set; }

    public int UserId { get; set; }

    public DateTime RegistrationDate { get; set; }

    [StringLength(20)]
    public string TicketType { get; set; } = "Standard";
    public int Quantity { get; set; } = 1;
    public decimal TotalAmount { get; set; }
    public string RegistrationType { get; set; } = null!;

    [StringLength(20)]
    public string Status { get; set; } = null!;

    public DateTime? CancelledAt { get; set; }

    [ForeignKey("EventId")]
    [InverseProperty("Registrations")]
    public virtual Event Event { get; set; } = null!;

    [InverseProperty("Registration")]
    public virtual Ticket? Ticket { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("Registrations")]
    public virtual User User { get; set; } = null!;
}

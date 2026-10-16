using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EventManagementSystem.Models;

[Index("TicketCode", Name = "UQ__Tickets__598CF7A32B6851B8", IsUnique = true)]
[Index("RegistrationId", Name = "UQ__Tickets__6EF58811AEFB11BA", IsUnique = true)]
public partial class Ticket
{
    [Key]
    public int TicketId { get; set; }

    public int RegistrationId { get; set; }

    [StringLength(50)]
    public string TicketCode { get; set; } = null!;

    [StringLength(30)]
    public string TicketType { get; set; } = null!;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Price { get; set; }

    public DateTime IssuedAt { get; set; }

    [StringLength(20)]
    public string Status { get; set; } = null!;

    [ForeignKey("RegistrationId")]
    [InverseProperty("Ticket")]
    public virtual Registration? Registration { get; set; } = null!;
}

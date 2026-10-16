using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EventManagementSystem.Models;

public partial class Event
{
    [Key]
    public int EventId { get; set; }

    [StringLength(150)]
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    [StringLength(150)]
    public string Venue { get; set; } = null!;

    public int Capacity { get; set; }

    public int AvailableSlots { get; set; }
    public decimal Price { get; set; }
    public DateTime RegistrationStart { get; set; }

    public DateTime RegistrationEnd { get; set; }

    public DateTime EventStart { get; set; }

    public DateTime EventEnd { get; set; }

    [StringLength(30)]
    public string Status { get; set; } = null!;

    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    [ForeignKey("CreatedBy")]
    [InverseProperty("Events")]
    public virtual User CreatedByNavigation { get; set; } = null!;

    [InverseProperty("Event")]
    public virtual ICollection<EventOverride> EventOverrides { get; set; } = new List<EventOverride>();

    [InverseProperty("Event")]
    public virtual ICollection<Registration> Registrations { get; set; } = new List<Registration>();
}

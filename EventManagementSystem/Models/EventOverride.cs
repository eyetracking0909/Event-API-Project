using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventManagementSystem.Models;

public partial class EventOverride
{
    [Key]
    public int EventOverrideId { get; set; }

    public int EventId { get; set; }

    public int SuperAdminId { get; set; }

    [StringLength(50)]
    public string OverrideType { get; set; } = null!;

    [StringLength(255)]
    public string PreviousValue { get; set; } = null!;

    [StringLength(255)]
    public string NewValue { get; set; } = null!;

    public string Reason { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    [ForeignKey("EventId")]
    [InverseProperty("EventOverrides")]
    public virtual Event Event { get; set; } = null!;

    [ForeignKey("SuperAdminId")]
    [InverseProperty("EventOverrides")]
    public virtual User SuperAdmin { get; set; } = null!;
}

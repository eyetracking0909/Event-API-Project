using AutoMapper;
using EventManagementSystem.Data;
using EventManagementSystem.DTOs;
using EventManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegistrationsController : ControllerBase
    {
        private readonly EventManagementDbContext _context;
        private readonly IMapper _mapper;

        public RegistrationsController(EventManagementDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/Registrations
        // READ: Get all registrations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RegistrationResponseDto>>> GetRegistrations()
        {
            var registrations = await _context.Registrations.ToListAsync();
            return Ok(_mapper.Map<IEnumerable<RegistrationResponseDto>>(registrations));
        }

        // GET: api/Registrations/5
        // READ: Get registration details by ID
        [HttpGet("{id}")]
        public async Task<ActionResult<RegistrationResponseDto>> GetRegistration(int id)
        {
            var registration = await _context.Registrations.FindAsync(id);
            if (registration == null)
            {
                return NotFound(new { message = $"Registration with ID {id} was not found." });
            }

            return Ok(_mapper.Map<RegistrationResponseDto>(registration));
        }

        // POST: api/Registrations
        // CREATE: Register a user for an event (Online or Walk-In)
        [HttpPost]
        public async Task<IActionResult> RegisterUser([FromBody] CreateRegistrationDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var @event = await _context.Events.FindAsync(dto.EventId);
            if (@event == null)
            {
                return NotFound(new { message = $"Event with ID {dto.EventId} was not found." });
            }
            // Business Rule Check: Block registration if event is cancelled
            if (@event.Status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { message = $"Registration failed. Event '{@event.Title}' is cancelled." });
            }
            // Business Rule Check: Verify sufficient available capacity
            if (@event.AvailableSlots < dto.Quantity)
            {
                return BadRequest(new { message = $"Registration failed. Only {@event.AvailableSlots} slots remaining." });
            }
            // Calculate dynamic pricing
            decimal basePrice = @event.Price;
            decimal multiplier = dto.TicketType.Equals("VIP", StringComparison.OrdinalIgnoreCase) ? 3.0m : 1.0m;
            decimal totalAmount = (basePrice * multiplier) * dto.Quantity;
            // Deduct slots
            @event.AvailableSlots -= dto.Quantity;

            var registration = new Registration
            {
                EventId = dto.EventId,
                UserId = dto.UserId,
                RegistrationType = dto.RegistrationType,
                TicketType = dto.TicketType,
                Quantity = dto.Quantity,
                TotalAmount = totalAmount,
                RegistrationDate = DateTime.UtcNow,
                Status = "Confirmed"
            };
            _context.Registrations.Add(registration);
            await _context.SaveChangesAsync();
            // Generate single ticket record linked to this registration
            var ticket = new Ticket
            {
                RegistrationId = registration.RegistrationId,
                TicketCode = $"TKT-{Guid.NewGuid().ToString()[..8].ToUpper()}",
                TicketType = dto.TicketType,
                Status = "Unpaid",
                IssuedAt = DateTime.UtcNow
            };

            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Registration successful!",
                registrationId = registration.RegistrationId,
                ticketCode = ticket.TicketCode,
                ticketType = dto.TicketType,
                quantity = dto.Quantity,
                totalAmount = totalAmount,
                paymentStatus = ticket.Status
            });
        }

        // PUT: api/Registrations/5/cancel
        // Allowed: The owner who registered, Admin, or SuperAdmin
        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelRegistration(int id, [FromQuery] int cancelledByUserId)
        {
            var requester = await _context.Users.FindAsync(cancelledByUserId);
            if (requester == null)
            {
                return NotFound(new { message = $"User with ID {cancelledByUserId} was not found." });
            }

            var registration = await _context.Registrations.FindAsync(id);
            if (registration == null)
            {
                return NotFound(new { message = $"Registration with ID {id} was not found." });
            }
            // Permission Check: Owner OR Admin OR SuperAdmin
            bool isOwner = registration.UserId == cancelledByUserId;
            bool isAdminOrSuper = requester.Role == "Admin" || requester.Role == "SuperAdmin";

            if (!isOwner && !isAdminOrSuper)
            {
                return Unauthorized(new { message = "Access Denied: You can only cancel your own registrations unless you are an Admin/SuperAdmin." });
            }

            if (registration.Status == "Cancelled")
            {
                return BadRequest(new { message = "Registration is already cancelled." });
            }

            registration.Status = "Cancelled";
            // Restore event capacity
            var @event = await _context.Events.FindAsync(registration.EventId);
            if (@event != null)
            {
                @event.AvailableSlots += registration.Quantity;
            }
            // Update associated ticket status
            var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.RegistrationId == id);
            if (ticket != null)
            {
                ticket.Status = "Cancelled";
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = $"Registration ID {id} cancelled successfully and {registration.Quantity} slot(s) restored.",
                cancelledByRole = requester.Role
            });
        }
    }
}
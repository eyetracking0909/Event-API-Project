using AutoMapper;
using EventManagementSystem.Data;
using EventManagementSystem.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketsController : ControllerBase
    {
        private readonly EventManagementDbContext _context;
        private readonly IMapper _mapper;

        public TicketsController(EventManagementDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TicketResponseDto>>> GetTickets()
        {
            var tickets = await _context.Tickets
                .Include(t => t.Registration)
                .ToListAsync();

            return Ok(_mapper.Map<IEnumerable<TicketResponseDto>>(tickets));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TicketResponseDto>> GetTicket(int id)
        {
            var ticket = await _context.Tickets
                .Include(t => t.Registration)
                .FirstOrDefaultAsync(t => t.TicketId == id);

            if (ticket == null) return NotFound(new { message = $"Ticket {id} not found." });

            return Ok(_mapper.Map<TicketResponseDto>(ticket));
        }

        [HttpGet("code/{ticketCode}")]
        public async Task<ActionResult<TicketResponseDto>> GetTicketByCode(string ticketCode)
        {
            var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.TicketCode == ticketCode);
            if (ticket == null)
            {
                return NotFound(new { message = $"Ticket code '{ticketCode}' not found." });
            }

            return Ok(_mapper.Map<TicketResponseDto>(ticket));
        }

        // POST: api/Tickets/pay
        // Permission Check: Owner OR Admin OR SuperAdmin
        [HttpPost("pay")]
        public async Task<IActionResult> PayTicket([FromBody] PayTicketDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var ticket = await _context.Tickets
                .FirstOrDefaultAsync(t => t.RegistrationId == dto.RegistrationId);

            if (ticket == null)
            {
                return NotFound(new { message = $"No ticket found for Registration ID {dto.RegistrationId}." });
            }

            var registration = await _context.Registrations.FindAsync(dto.RegistrationId);
            if (registration == null)
            {
                return NotFound(new { message = $"Associated registration was not found." });
            }

            var payer = await _context.Users.FindAsync(dto.PaidByUserId);
            if (payer == null)
            {
                return NotFound(new { message = $"User with ID {dto.PaidByUserId} was not found." });
            }

            // Permission Check: Owner OR Admin OR SuperAdmin
            bool isOwner = registration.UserId == dto.PaidByUserId;
            bool isAdminOrSuper = payer.Role == "Admin" || payer.Role == "SuperAdmin";

            if (!isOwner && !isAdminOrSuper)
            {
                return Unauthorized(new { message = "Access Denied: You can only pay for your own tickets unless you are an Admin/SuperAdmin." });
            }

            if (ticket.Status == "Paid")
            {
                return BadRequest(new { message = "This ticket has already been paid." });
            }

            if (ticket.Status == "Cancelled")
            {
                return BadRequest(new { message = "Cannot pay for a cancelled ticket." });
            }

            ticket.Status = "Paid";
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Payment completed successfully!",
                ticketId = ticket.TicketId,
                registrationId = ticket.RegistrationId,
                userId = registration.UserId, // Linked UserId exposed
                ticketCode = ticket.TicketCode,
                ticketType = ticket.TicketType,
                status = ticket.Status,
                processedByRole = payer.Role
            });
        }

        // PUT: api/Tickets/5/cancel
        // Allowed: Owner, Admin, or SuperAdmin
        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelTicket(int id, [FromQuery] int cancelledByUserId)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null) return NotFound(new { message = "Ticket not found." });

            var registration = await _context.Registrations.FindAsync(ticket.RegistrationId);
            if (registration == null) return NotFound(new { message = "Associated registration not found." });

            var requester = await _context.Users.FindAsync(cancelledByUserId);
            if (requester == null) return NotFound(new { message = $"User with ID {cancelledByUserId} was not found." });

            // Permission Check: Owner OR Admin OR SuperAdmin
            bool isOwner = registration.UserId == cancelledByUserId;
            bool isAdminOrSuper = requester.Role == "Admin" || requester.Role == "SuperAdmin";

            if (!isOwner && !isAdminOrSuper)
            {
                return Unauthorized(new { message = "Access Denied: You can only cancel your own tickets unless you are an Admin/SuperAdmin." });
            }

            if (ticket.Status == "Cancelled")
            {
                return BadRequest(new { message = "Ticket is already cancelled." });
            }

            ticket.Status = "Cancelled";
            registration.Status = "Cancelled";

            // Restore available slots
            var @event = await _context.Events.FindAsync(registration.EventId);
            if (@event != null)
            {
                @event.AvailableSlots += registration.Quantity;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = $"Ticket ID {id} cancelled successfully and {registration.Quantity} slot(s) restored.",
                userId = registration.UserId
            });
        }
    }
}
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
    public class EventsController : ControllerBase
    {
        private readonly EventManagementDbContext _context;
        private readonly IMapper _mapper;
        public EventsController(EventManagementDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/Events
        // READ: Get all active events
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EventResponseDto>>> GetEvents()
        {
            var events = await _context.Events.ToListAsync();
            return Ok(_mapper.Map<IEnumerable<EventResponseDto>>(events));
        }

        // GET: api/Events/5
        // READ: Get specific event by ID
        [HttpGet("{id}")]
        public async Task<ActionResult<EventResponseDto>> GetEvent(int id)
        {
            var @event = await _context.Events.FindAsync(id);
            if (@event == null)
            {
                return NotFound(new { message = $"Event with ID {id} was not found." });
            }

            return Ok(_mapper.Map<EventResponseDto>(@event));
        }

        // POST: api/Events
        // CREATE: Only Admin and SuperAdmin can create events
        [HttpPost]
        public async Task<ActionResult<EventResponseDto>> CreateEvent([FromBody] CreateEventDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var creator = await _context.Users.FindAsync(dto.CreatedBy);
            if (creator == null || (creator.Role != "Admin" && creator.Role != "SuperAdmin"))
            {
                return Unauthorized(new { message = "Access Denied: Only Admins or SuperAdmins can create events." });
            }

            var @event = _mapper.Map<Event>(dto);
            @event.AvailableSlots = dto.Capacity;

            _context.Events.Add(@event);
            await _context.SaveChangesAsync();

            var response = _mapper.Map<EventResponseDto>(@event);
            return CreatedAtAction(nameof(GetEvent), new { id = @event.EventId }, response);
        }

        // PUT: api/Events/5
        // UPDATE & CAPACITY ADJUSTMENT: Admin and SuperAdmin update event details and modify capacity
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvent(int id, [FromBody] UpdateEventDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            // 1. Verify Event Exists
            var @event = await _context.Events.FindAsync(id);
            if (@event == null)
            {
                return NotFound(new { message = $"Event with ID {id} was not found." });
            }
            // 2. Business Rule Check: Verify Updating User is Admin or SuperAdmin
            var user = await _context.Users.FindAsync(dto.UpdatedBy);
            if (user == null || (user.Role != "Admin" && user.Role != "SuperAdmin"))
            {
                return Unauthorized(new { message = "Access Denied: Only Admins or SuperAdmins can modify events." });
            }
            int previousCapacity = @event.Capacity;
            // 3. Handle Capacity Changes (Increase or Decrease)
            if (dto.Capacity != previousCapacity)
            {
                // Sum total tickets booked from active registrations for this event
                int totalTicketsBooked = await _context.Registrations
                    .Where(r => r.EventId == id && r.Status == "Confirmed")
                    .SumAsync(r => r.Quantity);
                // Business Rule: Cannot reduce capacity below current registered tickets
                if (dto.Capacity < totalTicketsBooked)
                {
                    return BadRequest(new
                    {
                        message = $"Cannot reduce capacity to {dto.Capacity}. There are already {totalTicketsBooked} tickets registered for this event."
                    });
                }
                // Adjust AvailableSlots dynamically
                @event.AvailableSlots = dto.Capacity - totalTicketsBooked;
            }
            // 4. Update Event Properties
            @event.Title = dto.Title;
            @event.Description = dto.Description;
            @event.Venue = dto.Venue;
            @event.Capacity = dto.Capacity;
            @event.Price = dto.Price;
            @event.RegistrationStart = dto.RegistrationStart;
            @event.RegistrationEnd = dto.RegistrationEnd;
            @event.EventStart = dto.EventStart;
            @event.EventEnd = dto.EventEnd;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = $"Event '{@event.Title}' updated successfully.",
                eventDetails = _mapper.Map<EventResponseDto>(@event)
            });
        }

        // DELETE: api/Events/5
        // DELETE: Cancel or soft-delete an event
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id, [FromQuery] int requestedByUserId)
        {
            var requester = await _context.Users.FindAsync(requestedByUserId);
            if (requester == null || requester.Role != "SuperAdmin")
            {
                return Unauthorized(new { message = "Access Denied: Only SuperAdmins can delete events." });
            }

            var @event = await _context.Events.FindAsync(id);
            if (@event == null)
            {
                return NotFound(new { message = $"Event with ID {id} was not found." });
            }
            // Perform Soft Delete to preserve foreign key constraints and audit trail
            @event.Status = "Cancelled";
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Event '{@event.Title}' was marked as cancelled." });
        }
    }
}
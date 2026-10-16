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
    public class UsersController : ControllerBase
    {
        private readonly EventManagementDbContext _context;
        private readonly IMapper _mapper;

        public UsersController(EventManagementDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        // GET: api/Users
        // RETRIEVE ALL: Returns a list of all registered users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetUsers()
        {
            var users = await _context.Users.ToListAsync();
            var response = _mapper.Map<IEnumerable<UserResponseDto>>(users);

            return Ok(response);
        }
        // GET: api/Users/ID
        // RETRIEVE ALL: Returns a specific registered user
        [HttpGet("{id}")]
        public async Task<ActionResult<UserResponseDto>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { message = $"User with ID {id} was not found." });
            }

            return Ok(_mapper.Map<UserResponseDto>(user));
        }

        [HttpPost]
        public async Task<ActionResult<UserResponseDto>> CreateUser([FromBody] UserRegisterDto dto)
        {
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (existingUser != null)
            {
                return BadRequest(new { message = "Email is already registered." });
            }

            var user = _mapper.Map<User>(dto);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var response = _mapper.Map<UserResponseDto>(user);
            return CreatedAtAction(nameof(GetUser), new { id = user.UserId }, response);
        }
        // DELETE: api/Users/5
        // Restrict deletion strictly to SuperAdmin
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id, [FromQuery] int requestedByUserId)
        {
            var requester = await _context.Users.FindAsync(requestedByUserId);
            if (requester == null || requester.Role != "SuperAdmin")
            {
                return Unauthorized(new { message = "Access Denied: Only SuperAdmins can delete accounts." });
            }

            var userToDelete = await _context.Users.FindAsync(id);
            if (userToDelete == null)
            {
                return NotFound(new { message = $"User with ID {id} was not found." });
            }

            _context.Users.Remove(userToDelete);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"User '{userToDelete.FirstName} {userToDelete.LastName}' was permanently deleted." });
        }
    }
}
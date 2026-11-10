using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.Filters;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class UsersController : ControllerBase
{
    private readonly RaceDayProg6212Context _context;

    public UsersController(RaceDayProg6212Context context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
    {
        return await _context.Users
            .OrderBy(u => u.UserId)
            .Select(u => new UserDto
            {
                UserId = u.UserId,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                Role = u.Role,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUser(int id)
    {
        var user = await _context.Users
            .Where(u => u.UserId == id)
            .Select(u => new UserDto
            {
                UserId = u.UserId,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                Role = u.Role,
                CreatedAt = u.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (user == null)
        {
            return NotFound();
        }

        return user;
    }

    /// <summary>
    /// Register a new user. Password is hashed on the server.
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<UserDto>> PostUser(UserCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email))
        {
            return BadRequest("Email is required.");
        }

        if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
        {
            return BadRequest("A user with this email already exists.");
        }

        var user = new User
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = string.IsNullOrWhiteSpace(dto.Role) ? "Runner" : dto.Role,
            CreatedAt = DateTime.Now
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var result = new UserDto
        {
            UserId = user.UserId,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role,
            CreatedAt = user.CreatedAt
        };

        return CreatedAtAction(nameof(GetUser), new { id = user.UserId }, result);
    }

    /// <summary>
    /// Update a user. Provide `Password` to change password.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> PutUser(int id, UserCreateDto dto)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.Email = dto.Email;
        // Only update password if provided
        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        }
        user.Role = string.IsNullOrWhiteSpace(dto.Role) ? user.Role : dto.Role;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Delete a user (Admin only).
    /// </summary>
    [HttpDelete("{id}")]
    [RequireRole("Admin")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}


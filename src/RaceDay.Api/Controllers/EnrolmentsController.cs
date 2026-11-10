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
[Authorize]
public class EnrolmentsController : ControllerBase
{
    private readonly RaceDayProg6212Context _context;

    public EnrolmentsController(RaceDayProg6212Context context)
    {
        _context = context;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<EnrolmentDto>>> GetEnrolments()
    {
        return await _context.Enrolments
            .OrderBy(e => e.EnrolmentId)
            .Select(e => new EnrolmentDto
            {
                EnrolmentId = e.EnrolmentId,
                UserId = e.UserId,
                CategoryId = e.CategoryId,
                EnrolmentDate = e.EnrolmentDate,
                Status = e.Status
            })
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EnrolmentDto>> GetEnrolment(int id)
    {
        var enrolment = await _context.Enrolments
            .Where(e => e.EnrolmentId == id)
            .Select(e => new EnrolmentDto
            {
                EnrolmentId = e.EnrolmentId,
                UserId = e.UserId,
                CategoryId = e.CategoryId,
                EnrolmentDate = e.EnrolmentDate,
                Status = e.Status
            })
            .FirstOrDefaultAsync();

        if (enrolment == null)
        {
            return NotFound();
        }

        return enrolment;
    }

    [HttpPost]
    public async Task<ActionResult<EnrolmentDto>> PostEnrolment(EnrolmentCreateDto dto)
    {
        var userExists = await _context.Users.AnyAsync(u => u.UserId == dto.UserId);
        if (!userExists)
        {
            return BadRequest("User does not exist.");
        }

        var categoryExists = await _context.Categories.AnyAsync(c => c.CategoryId == dto.CategoryId);
        if (!categoryExists)
        {
            return BadRequest("Category does not exist.");
        }

        if (await _context.Enrolments.AnyAsync(e => e.UserId == dto.UserId && e.CategoryId == dto.CategoryId))
        {
            return BadRequest("This user is already enrolled in this category.");
        }

        var enrolment = new Enrolment
        {
            UserId = dto.UserId,
            CategoryId = dto.CategoryId,
            EnrolmentDate = DateTime.Now,
            Status = string.IsNullOrWhiteSpace(dto.Status) ? "Registered" : dto.Status
        };

        _context.Enrolments.Add(enrolment);
        await _context.SaveChangesAsync();

        var result = new EnrolmentDto
        {
            EnrolmentId = enrolment.EnrolmentId,
            UserId = enrolment.UserId,
            CategoryId = enrolment.CategoryId,
            EnrolmentDate = enrolment.EnrolmentDate,
            Status = enrolment.Status
        };

        return CreatedAtAction(nameof(GetEnrolment), new { id = enrolment.EnrolmentId }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutEnrolment(int id, EnrolmentCreateDto dto)
    {
        var enrolment = await _context.Enrolments.FindAsync(id);
        if (enrolment == null)
        {
            return NotFound();
        }

        enrolment.UserId = dto.UserId;
        enrolment.CategoryId = dto.CategoryId;
        enrolment.Status = string.IsNullOrWhiteSpace(dto.Status) ? enrolment.Status : dto.Status;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [RequireRole("Admin")]
    public async Task<IActionResult> DeleteEnrolment(int id)
    {
        var enrolment = await _context.Enrolments.FindAsync(id);
        if (enrolment == null)
        {
            return NotFound();
        }

        _context.Enrolments.Remove(enrolment);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

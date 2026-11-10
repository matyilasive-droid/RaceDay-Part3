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
    public class EventsController : ControllerBase
{
    private readonly RaceDayProg6212Context _context;

    public EventsController(RaceDayProg6212Context context)
    {
        _context = context;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<EventDto>>> GetEvents()
    {
        return await _context.Events
            .OrderBy(e => e.EventId)
            .Select(e => new EventDto
            {
                EventId = e.EventId,
                Organised = e.Organised,
                EventName = e.EventName,
                EventType = e.EventType,
                EventDate = e.EventDate,
                Location = e.Location,
                Province = e.Province,
                Status = e.Status,
                CreatedAt = e.CreatedAt
            })
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EventDto>> GetEvent(int id)
    {
        var eventItem = await _context.Events
            .Where(e => e.EventId == id)
            .Select(e => new EventDto
            {
                EventId = e.EventId,
                Organised = e.Organised,
                EventName = e.EventName,
                EventType = e.EventType,
                EventDate = e.EventDate,
                Location = e.Location,
                Province = e.Province,
                Status = e.Status,
                CreatedAt = e.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (eventItem == null)
        {
            return NotFound();
        }

        return eventItem;
    }

    [HttpPost]
    public async Task<ActionResult<EventDto>> PostEvent(EventCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.EventName))
        {
            return BadRequest("Event name is required.");
        }

        var organiserExists = await _context.Users.AnyAsync(u => u.UserId == dto.Organised);
        if (!organiserExists)
        {
            return BadRequest("Organiser user does not exist.");
        }

        var eventItem = new Event
        {
            Organised = dto.Organised,
            EventName = dto.EventName,
            EventType = dto.EventType,
            EventDate = dto.EventDate,
            Location = dto.Location,
            Province = dto.Province,
            Status = string.IsNullOrWhiteSpace(dto.Status) ? "Upcoming" : dto.Status,
            CreatedAt = DateTime.Now
        };

        _context.Events.Add(eventItem);
        await _context.SaveChangesAsync();

        var result = new EventDto
        {
            EventId = eventItem.EventId,
            Organised = eventItem.Organised,
            EventName = eventItem.EventName,
            EventType = eventItem.EventType,
            EventDate = eventItem.EventDate,
            Location = eventItem.Location,
            Province = eventItem.Province,
            Status = eventItem.Status,
            CreatedAt = eventItem.CreatedAt
        };

        return CreatedAtAction(nameof(GetEvent), new { id = eventItem.EventId }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutEvent(int id, EventCreateDto dto)
    {
        var eventItem = await _context.Events.FindAsync(id);
        if (eventItem == null)
        {
            return NotFound();
        }

        eventItem.Organised = dto.Organised;
        eventItem.EventName = dto.EventName;
        eventItem.EventType = dto.EventType;
        eventItem.EventDate = dto.EventDate;
        eventItem.Location = dto.Location;
        eventItem.Province = dto.Province;
        eventItem.Status = string.IsNullOrWhiteSpace(dto.Status) ? eventItem.Status : dto.Status;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    [RequireRole("Admin")]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var eventItem = await _context.Events.FindAsync(id);
        if (eventItem == null)
        {
            return NotFound();
        }

        _context.Events.Remove(eventItem);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

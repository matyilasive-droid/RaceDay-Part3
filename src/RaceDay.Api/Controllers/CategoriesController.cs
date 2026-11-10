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
public class CategoriesController : ControllerBase
{
    private readonly RaceDayProg6212Context _context;

    public CategoriesController(RaceDayProg6212Context context)
    {
        _context = context;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
    {
        return await _context.Categories
            .OrderBy(c => c.CategoryId)
            .Select(c => new CategoryDto
            {
                CategoryId = c.CategoryId,
                EventId = c.EventId,
                Name = c.Name,
                DistanceKm = c.DistanceKm,
                MaxParticipant = c.MaxParticipant,
                EntryFee = c.EntryFee
            })
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryDto>> GetCategory(int id)
    {
        var category = await _context.Categories
            .Where(c => c.CategoryId == id)
            .Select(c => new CategoryDto
            {
                CategoryId = c.CategoryId,
                EventId = c.EventId,
                Name = c.Name,
                DistanceKm = c.DistanceKm,
                MaxParticipant = c.MaxParticipant,
                EntryFee = c.EntryFee
            })
            .FirstOrDefaultAsync();

        if (category == null)
        {
            return NotFound();
        }

        return category;
    }

    [HttpPost]
    public async Task<ActionResult<CategoryDto>> PostCategory(CategoryCreateDto dto)
    {
        var eventExists = await _context.Events.AnyAsync(e => e.EventId == dto.EventId);
        if (!eventExists)
        {
            return BadRequest("Event does not exist.");
        }

        var category = new Category
        {
            EventId = dto.EventId,
            Name = dto.Name,
            DistanceKm = dto.DistanceKm,
            MaxParticipant = dto.MaxParticipant,
            EntryFee = dto.EntryFee
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var result = new CategoryDto
        {
            CategoryId = category.CategoryId,
            EventId = category.EventId,
            Name = category.Name,
            DistanceKm = category.DistanceKm,
            MaxParticipant = category.MaxParticipant,
            EntryFee = category.EntryFee
        };

        return CreatedAtAction(nameof(GetCategory), new { id = category.CategoryId }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutCategory(int id, CategoryCreateDto dto)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null)
        {
            return NotFound();
        }

        category.EventId = dto.EventId;
        category.Name = dto.Name;
        category.DistanceKm = dto.DistanceKm;
        category.MaxParticipant = dto.MaxParticipant;
        category.EntryFee = dto.EntryFee;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [RequireRole("Admin")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null)
        {
            return NotFound();
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

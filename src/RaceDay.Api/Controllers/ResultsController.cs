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
public class ResultsController : ControllerBase
{
    private readonly RaceDayProg6212Context _context;

    public ResultsController(RaceDayProg6212Context context)
    {
        _context = context;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<ResultDto>>> GetResults()
    {
        var results = await _context.Results
            .OrderBy(r => r.ResultId)
            .Select(r => new ResultDto
            {
                ResultId = r.ResultId,
                Enrolment = r.Enrolment,
                FinishTime = r.FinishTime,
                Position = r.Position,
                RecordedAt = r.RecordedAt
            })
            .ToListAsync();

        return results;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ResultDto>> GetResult(int id)
    {
        var result = await _context.Results
            .Where(r => r.ResultId == id)
            .Select(r => new ResultDto
            {
                ResultId = r.ResultId,
                Enrolment = r.Enrolment,
                FinishTime = r.FinishTime,
                Position = r.Position,
                RecordedAt = r.RecordedAt
            })
            .FirstOrDefaultAsync();

        if (result == null)
        {
            return NotFound();
        }

        return result;
    }

    [HttpPost]
    [RequireRole("Admin")]
    public async Task<ActionResult<ResultDto>> PostResult(ResultCreateDto dto)
    {
        var enrolmentExists = await _context.Enrolments.AnyAsync(e => e.EnrolmentId == dto.Enrolment);
        if (!enrolmentExists)
        {
            return BadRequest("Enrolment does not exist.");
        }

        if (await _context.Results.AnyAsync(r => r.Enrolment == dto.Enrolment))
        {
            return BadRequest("A result already exists for this enrolment.");
        }

        var result = new Result
        {
            Enrolment = dto.Enrolment,
            FinishTime = dto.FinishTime,
            Position = dto.Position,
            RecordedAt = DateTime.Now
        };

        _context.Results.Add(result);
        await _context.SaveChangesAsync();

        var response = new ResultDto
        {
            ResultId = result.ResultId,
            Enrolment = result.Enrolment,
            FinishTime = result.FinishTime,
            Position = result.Position,
            RecordedAt = result.RecordedAt
        };

        return CreatedAtAction(nameof(GetResult), new { id = result.ResultId }, response);
    }
}

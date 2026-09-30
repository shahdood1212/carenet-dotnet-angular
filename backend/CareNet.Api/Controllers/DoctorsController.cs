using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace CareNet.Api.Controllers;

[ApiController, Route("api/doctors")]
public class DoctorsController(AppDb db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await db.Doctors.AsNoTracking().OrderBy(d => d.Name).ToListAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id) =>
        await db.Doctors.FindAsync(id) is { } d ? Ok(d) : NotFound();

    [HttpGet("{id:int}/slots")]
    public async Task<IActionResult> Slots(int id, DateOnly date)
    {
        if (!await db.Doctors.AnyAsync(x => x.Id == id)) return NotFound();
        var start = date.ToDateTime(TimeOnly.MinValue);
        var taken = await db.Appointments
            .Where(a => a.DoctorId == id && a.StartsAt >= start && a.StartsAt < start.AddDays(1))
            .Select(a => a.StartsAt).ToListAsync();
        var slots = Enumerable.Range(9, 8).Select(h => start.AddHours(h))
            .Where(s => s > DateTime.Now && !taken.Contains(s));
        return Ok(slots);
    }

    [HttpPost, Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(DoctorDto d)
    {
        var doc = new Doctor { Name = d.Name, Specialty = d.Specialty, Bio = d.Bio };
        db.Doctors.Add(doc);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = doc.Id }, doc);
    }

    [HttpPut("{id:int}"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, DoctorDto d)
    {
        var doc = await db.Doctors.FindAsync(id);
        if (doc is null) return NotFound();
        (doc.Name, doc.Specialty, doc.Bio) = (d.Name, d.Specialty, d.Bio);
        await db.SaveChangesAsync();
        return Ok(doc);
    }

    [HttpDelete("{id:int}"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var doc = await db.Doctors.FindAsync(id);
        if (doc is null) return NotFound();
        db.Doctors.Remove(doc);
        await db.SaveChangesAsync();
        return NoContent();
    }
}

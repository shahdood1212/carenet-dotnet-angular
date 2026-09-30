using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace CareNet.Api.Controllers;

[ApiController, Route("api/appointments"), Authorize]
public class AppointmentsController(AppDb db) : ControllerBase
{
    private int UserId => int.Parse(User.FindFirst("sub")!.Value);

    [HttpPost]
    public async Task<IActionResult> Book(BookDto d)
    {
        if (!await db.Doctors.AnyAsync(x => x.Id == d.DoctorId)) return NotFound(new { message = "Doctor not found" });
        var t = d.StartsAt;
        if (t <= DateTime.Now || t.Minute != 0 || t.Second != 0 || t.Hour < 9 || t.Hour > 16)
            return BadRequest(new { message = "Invalid time slot" });
        db.Appointments.Add(new Appointment { DoctorId = d.DoctorId, PatientId = UserId, StartsAt = t });
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException) { return Conflict(new { message = "This slot was just booked by someone else" }); }
        return Ok();
    }

    [HttpGet("mine")]
    public async Task<IActionResult> Mine() => Ok(await db.Appointments
        .Where(a => a.PatientId == UserId).OrderBy(a => a.StartsAt)
        .Select(a => new { a.Id, a.StartsAt, DoctorName = a.Doctor!.Name }).ToListAsync());

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Cancel(int id)
    {
        var a = await db.Appointments.FirstOrDefaultAsync(x => x.Id == id && x.PatientId == UserId);
        if (a is null) return NotFound();
        db.Appointments.Remove(a);
        await db.SaveChangesAsync();
        return NoContent();
    }
}

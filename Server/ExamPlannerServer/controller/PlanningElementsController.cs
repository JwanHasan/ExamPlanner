using DatabaseConnection.DBContext;
using ExamPlannerServer.Auth;
using ExamPlannerServer.Contracts;
using ExamPlannerServer.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExamPlannerServer.controller;

/// <summary>
/// The imported planning elements and the teacher note on their exam.
/// Teachers and planners can read them and edit or delete the note; only planners can delete an element.
/// A note lives on the element's exam (AssessmentEvent). Nothing here creates exams: an element without
/// one has no note to edit.
/// </summary>
[ApiController]
[Route("api/v1/planning-elements")]
[Authorize(Roles = ApiRoles.Staff)]
public class PlanningElementsController(AppDbContext db) : ControllerBase
{
    private const int MaxNoteLength = 2000;

    // GET /api/v1/planning-elements
    [HttpGet]
    public async Task<ActionResult<List<PlanningElementDto>>> GetAll() =>
        await db.PlanningEducationElement
            .AsNoTracking()
            .OrderBy(e => e.Nickmane)
            .Select(e => new PlanningElementDto(
                e.Id, e.Name, e.Nickmane, e.CourseId, e.Course.SueCode, e.Course.Name, e.Campus.Name,
                e.ECTS, e.StartDate, e.EndDate,
                e.AssessmentEvent != null,
                e.AssessmentEvent != null ? e.AssessmentEvent.Note : null))
            .ToListAsync();

    // PUT /api/v1/planning-elements/{id}/note  { "note": "..." }
    [HttpPut("{id:guid}/note")]
    public async Task<IActionResult> SetNote(Guid id, [FromBody] UpdateNoteDto dto)
    {
        var note = dto.Note.Trim();
        if (note.Length == 0 || note.Length > MaxNoteLength)
            return BadRequest(new ApiError(ErrorCodes.InvalidRequest,
                $"A note needs 1 to {MaxNoteLength} characters. To remove a note, delete it."));

        return await SaveNoteAsync(id, note);
    }

    // DELETE /api/v1/planning-elements/{id}/note  -> empties the note; the exam itself stays
    [HttpDelete("{id:guid}/note")]
    public Task<IActionResult> DeleteNote(Guid id) => SaveNoteAsync(id, "");

    // DELETE /api/v1/planning-elements/{id}  (planners only). Its enrollments and exam go with it (database
    // cascade), and so does its class, unless another element of the course uses the same class code.
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = ApiRoles.Admin)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var element = await db.PlanningEducationElement
            .Where(e => e.Id == id)
            .Select(e => new { e.CourseId, e.Nickmane })
            .SingleOrDefaultAsync();
        if (element is null)
            return NotFound(UnknownElement(id));

        await using var transaction = await db.Database.BeginTransactionAsync();

        await db.PlanningEducationElement.Where(e => e.Id == id).ExecuteDeleteAsync();

        var classStillUsed = await db.PlanningEducationElement
            .AnyAsync(e => e.CourseId == element.CourseId && e.Nickmane == element.Nickmane);
        if (!classStillUsed)
            await db.Class.Where(c => c.CourseId == element.CourseId && c.ClassCode == element.Nickmane).ExecuteDeleteAsync();

        await transaction.CommitAsync();
        return NoContent();
    }

    private async Task<IActionResult> SaveNoteAsync(Guid id, string note)
    {
        var changed = await db.AssessmentEvent
            .Where(a => a.PlanningElementId == id)
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.Note, note));
        if (changed > 0)
            return NoContent();

        // Nothing was changed: say whether the element is unknown, or just has no exam yet.
        return await db.PlanningEducationElement.AnyAsync(e => e.Id == id)
            ? NotFound(new ApiError(ErrorCodes.ExamNotFound,
                "This planning element has no exam yet, so there is nothing to put a note on."))
            : NotFound(UnknownElement(id));
    }

    private static ApiError UnknownElement(Guid id) =>
        new(ErrorCodes.PlanningElementNotFound, $"Planning element {id} does not exist.");
}

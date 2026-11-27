namespace ExamPlannerServer.Dto;

// One planning element as staff see it. HasExam is false when there is no exam yet, and then there is no note to edit.
public record PlanningElementDto(
    Guid Id,
    string Name,
    string Nickname,
    int CourseId,
    string CourseCode,
    string CourseName,
    string Campus,
    int Ects,
    DateOnly StartDate,
    DateOnly EndDate,
    bool HasExam,
    string? Note);

// Body of PUT /api/v1/planning-elements/{id}/note
public class UpdateNoteDto
{
    public required string Note { get; set; }
}

// A student as staff see them (API_CONTRACT.md 8: id, viaId, name).
public record StudentDto(int Id, string ViaId, string Name, int AttemptNo);

// What a student sees about themselves.
public record MyStudentDto(int Id, string ViaId, string Name, int AttemptNo, List<MyEnrollmentDto> Enrollments);

public record MyEnrollmentDto(
    Guid PlanningElementId,
    string Name,
    string Nickname,
    string CourseCode,
    string CourseName,
    string Campus,
    string Hold,
    int Ects,
    DateOnly StartDate,
    DateOnly EndDate);

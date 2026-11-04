using DatabaseConnection.DBContext;
using DatabaseConnection.model;
using ExamPlannerServer.Auth;
using ExamPlannerServer.Contracts;
using ExamPlannerServer.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExamPlannerServer.controller;

[ApiController]
[Route("api/v1/planning-elements")]
[Authorize]
public class PlanningEducationElementsController(
    AppDbContext db
) : ControllerBase
{
    [HttpGet]
    public async Task<
        ActionResult<List<PlanningEducationElementDto>>
    > GetAll()
    {
        var elements =
            await db.PlanningEducationElement
                .AsNoTracking()
                .OrderBy(
                    element =>
                        element.Prefix
                )
                .ThenBy(
                    element =>
                        element.Nickmane
                )
                .ThenBy(
                    element =>
                        element.Name
                )
                .ToListAsync();

        return elements
            .Select(ToDto)
            .ToList();
    }

    [HttpGet("{id:guid}")]
    public async Task<
        ActionResult<PlanningEducationElementDto>
    > GetById(
        Guid id
    )
    {
        var element =
            await db.PlanningEducationElement
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    element =>
                        element.Id == id
                );

        if (element is null)
        {
            return NotFound(
                new ApiError(
                    "PLANNING_ELEMENT_NOT_FOUND",
                    $"Planning education element {id} does not exist."
                )
            );
        }

        return ToDto(element);
    }

    [HttpPatch("{id:guid}")]
    [Authorize(
        Roles = AdminSettings.AdminRole
    )]
    public async Task<
        ActionResult<PlanningEducationElementDto>
    > Update(
        Guid id,
        [FromBody]
        UpdatePlanningEducationElementDto dto
    )
    {
        var validationError =
            Validate(dto);

        if (validationError is not null)
        {
            return BadRequest(
                new ApiError(
                    ErrorCodes.InvalidRequest,
                    validationError
                )
            );
        }

        if (
            !Enum.TryParse<
                PlanningEducationElement.Assessment
            >(
                dto.AssessmentType,
                ignoreCase: true,
                out var assessmentType
            )
        )
        {
            return BadRequest(
                new ApiError(
                    ErrorCodes.InvalidRequest,
                    "AssessmentType must be Internal or External."
                )
            );
        }

        var element =
            await db.PlanningEducationElement
                .SingleOrDefaultAsync(
                    element =>
                        element.Id == id
                );

        if (element is null)
        {
            return NotFound(
                new ApiError(
                    "PLANNING_ELEMENT_NOT_FOUND",
                    $"Planning education element {id} does not exist."
                )
            );
        }

        /*
         * API DTO uses clean names.
         * Existing database model currently
         * contains several misspelled properties.
         */

        element.Name =
            dto.Name.Trim();

        element.Nickmane =
            dto.Nickname.Trim();

        element.ECTS =
            dto.Ects;

        element.ExecutionElementCount =
            dto.ExecutionElementCount;

        element.StartDate =
            dto.StartDate;

        element.EndDate =
            dto.EndDate;

        element.ActivityOffering =
            dto.ActivityOffering.Trim();

        element.Prefix =
            dto.Prefix.Trim();

        element.AssessmenType =
            assessmentType;

        element.CompletionAssesment =
            dto.CompletionAssessment;

        element.GradingScale =
            dto.GradingScale.Trim();

        element.CombinedWrittenOral =
            dto.CombinedWrittenOral;

        element.Oral =
            dto.Oral;

        element.PracticalExam =
            dto.PracticalExam;

        element.Project =
            dto.Project;

        element.Written =
            dto.Written;


        await db.SaveChangesAsync();


        return ToDto(element);
    }

    private static string? Validate(
        UpdatePlanningEducationElementDto dto
    )
    {
        if (
            string.IsNullOrWhiteSpace(
                dto.Name
            )
        )
        {
            return "Name must not be empty.";
        }

        if (
            string.IsNullOrWhiteSpace(
                dto.Nickname
            )
        )
        {
            return "Nickname must not be empty.";
        }

        if (dto.Ects < 0)
        {
            return "ECTS must be zero or greater.";
        }

        if (
            dto.ExecutionElementCount < 0
        )
        {
            return "ExecutionElementCount must be zero or greater.";
        }

        if (
            dto.EndDate <
            dto.StartDate
        )
        {
            return "EndDate must be on or after StartDate.";
        }

        if (
            string.IsNullOrWhiteSpace(
                dto.ActivityOffering
            )
        )
        {
            return "ActivityOffering must not be empty.";
        }

        if (
            string.IsNullOrWhiteSpace(
                dto.GradingScale
            )
        )
        {
            return "GradingScale must not be empty.";
        }

        return null;
    }

    private static PlanningEducationElementDto ToDto(
        PlanningEducationElement element
    )
    {
        return new PlanningEducationElementDto(
            element.Id,
            element.SourceChecksum,
            element.SourceModifiedAt,
            element.Name,
            element.Nickmane,
            element.ECTS,
            element.ExecutionElementCount,
            element.StartDate,
            element.EndDate,
            element.ActivityOffering,
            element.Prefix,
            element.AssessmenType.ToString(),
            element.CompletionAssesment,
            element.GradingScale,
            element.CombinedWrittenOral,
            element.Oral,
            element.PracticalExam,
            element.Project,
            element.Written
        );
    }
}
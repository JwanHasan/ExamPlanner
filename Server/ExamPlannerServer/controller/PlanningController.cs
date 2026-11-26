using DatabaseConnection.DBContext;
using DatabaseConnection.model;
using ExamPlannerServer.Auth;
using ExamPlannerServer.Contracts;
using ExamPlannerServer.Planning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ExamPlannerServer.controller;

[ApiController]
[Authorize(Roles = AdminSettings.AdminRole)]
[Route("api/v1/planning")]
public sealed class PlanningController(
    PlanningService service,
    AppDbContext db
) : ControllerBase
{
    [HttpGet("planning-elements")]
    public async Task<IActionResult> PlanningElements(
        CancellationToken ct
    )
    {
        var elements = await db.PlanningEducationElement
            .AsNoTracking()
            .Select(x => new
            {
                x.Id,
                nickname = x.Nickname,
                x.Name,
                x.CampusId,
                x.Written,
                x.Oral,
                x.Project,
                x.CombinedWrittenOral,
                x.PracticalExam,
                hasEvent = x.AssessmentEvent != null
            })
            .ToListAsync(ct);

        return Ok(elements);
    }

    [HttpGet("resources")]
    public async Task<IActionResult> Resources(
        CancellationToken ct
    )
    {
        return Ok(new
        {
            campuses = await db.Campuses
                .AsNoTracking()
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.IsVirtual
                })
                .ToListAsync(ct),

            rooms = await db.Room
                .AsNoTracking()
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.CampusId,
                    x.Capacity
                })
                .ToListAsync(ct)
        });
    }

    [HttpGet("events/{id:int}/facts")]
    public Task<IActionResult> Facts(
        int id,
        CancellationToken ct
    )
    {
        return Run(async () =>
        {
            if (!await db.AssessmentEvent.AnyAsync(
                    x => x.Id == id,
                    ct
                ))
            {
                throw new PlanningException(
                    "Event not found.",
                    404
                );
            }

            var row = await db.PlanningEventFacts
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    x => x.AssessmentEventId == id,
                    ct
                );

            return new FactsRequest(
                row?.CoTaught,
                row?.SmallSemester,
                row?.GraduatingStudents,
                row?.LargeClass,

                JsonSerializer.Deserialize<string[]>(
                    row?.HoldsJson ?? "[]"
                ) ?? [],

                JsonSerializer.Deserialize<DayOfWeek[]>(
                    row?.AvoidWeekdaysJson ?? "[]"
                ) ?? [],

                JsonSerializer.Deserialize<DateOnly[]>(
                    row?.AvoidDatesJson ?? "[]"
                ) ?? []
            );
        });
    }

    [HttpGet("events")]
    public async Task<IActionResult> Events(
        CancellationToken ct
    )
    {
        var events = await db.AssessmentEvent
            .AsNoTracking()
            .Select(e => new
            {
                eventId = e.Id,
                e.PlanningElementId,

                nickname =
                    e.PlanningEducationElement.Nickname,

                name =
                    e.PlanningEducationElement.Name,

                e.PlanningEducationElement.CampusId,
                e.ExamType,
                e.RequiredExamDays
            })
            .ToListAsync(ct);

        return Ok(events);
    }

    [HttpPost("events")]
    public Task<IActionResult> CreateEvent(
        EventCreateRequest request,
        CancellationToken ct
    )
    {
        return Run(async () =>
        {
            if (!Enum.IsDefined(request.ExamType) ||
                request.RequiredExamDays < 1)
            {
                throw new PlanningException(
                    "Valid exam type and positive required exam days are required."
                );
            }

            if (!await db.PlanningEducationElement.AnyAsync(
                    x => x.Id == request.PlanningElementId,
                    ct
                ))
            {
                throw new PlanningException(
                    "Planning education element not found.",
                    404
                );
            }

            if (await db.AssessmentEvent.AnyAsync(
                    x => x.PlanningElementId ==
                         request.PlanningElementId,
                    ct
                ))
            {
                throw new PlanningException(
                    "This planning element already has an assessment event.",
                    409
                );
            }

            var e = new AssessmentEvent
            {
                PlanningElementId = request.PlanningElementId,
                ExamType = request.ExamType,
                RequiredExamDays = request.RequiredExamDays,
                Note = request.Note?.Trim() ?? ""
            };

            db.AssessmentEvent.Add(e);

            await db.SaveChangesAsync(ct);

            return new
            {
                eventId = e.Id
            };
        });
    }

    [HttpGet("schedules")]
    public async Task<IActionResult> Schedules(
        CancellationToken ct
    )
    {
        var schedules = await db.Schedule
            .AsNoTracking()
            .Select(x => new
            {
                x.Id,
                x.Version,
                x.Approved
            })
            .ToListAsync(ct);

        return Ok(schedules);
    }

    [HttpPost("schedules")]
    public Task<IActionResult> CreateSchedule(
        ScheduleCreateRequest request,
        CancellationToken ct
    )
    {
        return Run(async () =>
        {
            if (string.IsNullOrWhiteSpace(request.Version) ||
                request.Days is null ||
                request.Days.Length == 0 ||

                request.Days.Any(x =>
                    x.Date == default ||
                    !Enum.IsDefined(x.Type)
                ) ||

                request.Days
                    .Select(x => x.Date)
                    .Distinct()
                    .Count() != request.Days.Length)
            {
                throw new PlanningException(
                    "Supply a version and distinct valid exam days."
                );
            }

            var schedule = new Schedule
            {
                Version = request.Version.Trim(),

                ExamDays = request.Days
                    .Select(x => new ExamDay
                    {
                        Date = x.Date.ToDateTime(
                            TimeOnly.MinValue,
                            DateTimeKind.Utc
                        ),

                        ExamDayType = x.Type,

                        Usable =
                            x.Usable &&
                            x.Date.DayOfWeek is not
                                (DayOfWeek.Saturday or DayOfWeek.Sunday)
                    })
                    .ToList()
            };

            db.Schedule.Add(schedule);

            await db.SaveChangesAsync(ct);

            return new
            {
                scheduleId = schedule.Id
            };
        });
    }

    [HttpGet("schedules/{scheduleId:int}/conflicts")]
    public Task<IActionResult> Check(
        int scheduleId,
        [FromQuery] int[] eventIds,
        CancellationToken ct
    )
    {
        return Run(() =>
            service.CheckAsync(
                scheduleId,
                eventIds ?? [],
                ct
            )
        );
    }

    [HttpPost("schedules/{scheduleId:int}/preview")]
    public Task<IActionResult> Preview(
        int scheduleId,
        EditRequest request,
        CancellationToken ct
    )
    {
        return Run(() =>
            service.EditAsync(
                scheduleId,
                request,
                0,
                false,
                ct
            )
        );
    }

    [HttpPost("schedules/{scheduleId:int}/commit")]
    public Task<IActionResult> Commit(
        int scheduleId,
        EditRequest request,
        CancellationToken ct
    )
    {
        return Run(() =>
            service.EditAsync(
                scheduleId,
                request,
                UserId(),
                true,
                ct
            )
        );
    }

    [HttpPost("schedules/{scheduleId:int}/acknowledge")]
    public Task<IActionResult> Acknowledge(
        int scheduleId,
        AcknowledgeRequest request,
        CancellationToken ct
    )
    {
        return Run(() =>
            service.AcknowledgeAsync(
                scheduleId,
                request,
                UserId(),
                ct
            )
        );
    }

    [HttpPost("schedules/{scheduleId:int}/priority")]
    public Task<IActionResult> Priority(
        int scheduleId,
        PriorityRequest request,
        CancellationToken ct
    )
    {
        return Run(() =>
            service.RankAsync(
                scheduleId,
                request.EventIds,
                ct
            )
        );
    }

    [HttpGet("rules")]
    public async Task<IActionResult> Rules(
        CancellationToken ct
    )
    {
        var settings = await db.RuleSettings
            .AsNoTracking()
            .Where(x => x.RuleKey != null)
            .ToDictionaryAsync(
                x => x.RuleKey!,
                ct
            );

        return Ok(
            ConflictService.Defaults.Select(x => new
            {
                ruleKey = x.Key,

                enabled =
                    !settings.TryGetValue(x.Key, out var s) ||
                    s.Enabled,

                severity = s?.Severity ?? "Default",

                defaultSeverity = x.Value
            })
        );
    }

    [HttpPut("rules/{key}")]
    public Task<IActionResult> SetRule(
        string key,
        RuleChange request,
        CancellationToken ct
    )
    {
        return Run(async () =>
        {
            var row = await service.SetRuleAsync(
                key,
                request,
                ct
            );

            return new
            {
                row.RuleKey,
                row.Enabled,
                row.Severity
            };
        });
    }

    [HttpPut("events/{id:int}/facts")]
    public Task<IActionResult> SetFacts(
        int id,
        FactsRequest request,
        CancellationToken ct
    )
    {
        return Run(async () =>
        {
            await service.SetFactsAsync(
                id,
                request,
                ct
            );

            return new
            {
                eventId = id
            };
        });
    }

    private int UserId()
    {
        if (int.TryParse(
                User.FindFirst(AuthClaims.UserId)?.Value,
                out var id
            ) &&
            id > 0)
        {
            return id;
        }

        throw new PlanningException(
            "Authenticated user ID is missing.",
            401
        );
    }

    private async Task<IActionResult> Run<T>(
        Func<Task<T>> action
    )
    {
        try
        {
            return Ok(await action());
        }
        catch (PlanningException e)
        {
            return StatusCode(
                e.Status,

                new ApiError(
                    e.Status == 409
                        ? "STALE_PLANNING_DATA"
                        : ErrorCodes.InvalidRequest,

                    e.Message
                )
            );
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict(
                new ApiError(
                    "STALE_PLANNING_DATA",
                    "The data changed. Reload and retry."
                )
            );
        }
        catch (DbUpdateException e)
            when (e.InnerException is
                Npgsql.PostgresException
                {
                    SqlState: "23505" or "23503"
                })
        {
            return Conflict(
                new ApiError(
                    "PLANNING_DATA_CHANGED",
                    "A referenced record or unique value changed. Reload and retry."
                )
            );
        }
    }
}
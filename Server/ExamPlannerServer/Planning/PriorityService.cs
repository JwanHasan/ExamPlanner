using DatabaseConnection.model;

namespace ExamPlannerServer.Planning;

public sealed class PriorityService(
    ClashMapCache? cache = null
)
{
    private readonly ClashMapCache maps =
        cache ?? new ClashMapCache();

    public PriorityItem[] Rank(
        PlanningSnapshot snapshot
    )
    {
        var map = maps.Get(snapshot.Events);

        return snapshot.Events
            .Select(e =>
            {
                var (tier, reason) = Tier(e);
                var warnings = new List<string>();

                if (e.StudentIds.Length == 0)
                {
                    warnings.Add(
                        "No student registrations; counts and clashes may be incomplete."
                    );
                }

                if (e.TeacherIds.Length == 0)
                {
                    warnings.Add(
                        "No teachers recorded; teacher clashes may be incomplete."
                    );
                }

                if (tier >= 5 && e.CoTaught is null)
                {
                    warnings.Add(
                        "Co-teaching not confirmed."
                    );
                }

                if (tier >= 6 &&
                    IsItSw(e) &&
                    e.SmallSemester is null)
                {
                    warnings.Add(
                        "Small-semester status not confirmed."
                    );
                }

                if (tier >= 7 &&
                    e.GraduatingStudents is null)
                {
                    warnings.Add(
                        "Graduating-student status not confirmed."
                    );
                }

                if (tier >= 8 &&
                    e.LargeClass is null)
                {
                    warnings.Add(
                        "Large-class status not confirmed."
                    );
                }

                var related = map.Related(e.Id);

                return new PriorityItem(
                    e.Id,
                    e.PlanningElementId,
                    e.Nickname,
                    e.Code,
                    e.Name,
                    tier,
                    reason,
                    related.Length,
                    e.StudentIds.Distinct().Count(),
                    related,
                    warnings.ToArray()
                );
            })
            .OrderBy(x => x.PriorityTier)
            .ThenByDescending(x => x.ClashCount)
            .ThenByDescending(x => x.StudentCount)
            .ThenBy(x => x.EventId)
            .ToArray();
    }

    private static bool IsItSw(EventData e)
    {
        return e.Prefix
            .Trim()
            .ToUpperInvariant() is "IT" or "SW";
    }

    private static (int, string) Tier(EventData e)
    {
        var gbt = e.Prefix
            .Trim()
            .ToUpperInvariant() is "XA" or "XH" or "GBT";

        var code = e.Code
            .Trim()
            .ToUpperInvariant();

        // Priority 1: Written exams.
        if (e.Type == ExamType.Written)
        {
            return (1, "Written exam");
        }

        // Priority 2: GBT BPR1.
        if (gbt && code == "BPR1")
        {
            return (2, "GBT BPR1");
        }

        // Priority 3: IT/SW BPR2.
        if (IsItSw(e) && code == "BPR2")
        {
            return (3, "IT/SW BPR2");
        }

        // Priority 4: GBT SEP.
        if (gbt && e.Type == ExamType.Sep)
        {
            return (4, "GBT SEP");
        }

        // Priority 5: Co-taught courses.
        if (e.CoTaught == true)
        {
            return (5, "Co-taught");
        }

        // Priority 6: IT/SW small semesters.
        if (IsItSw(e) && e.SmallSemester == true)
        {
            return (6, "IT/SW small semester");
        }

        // Priority 7: Graduating students.
        if (e.GraduatingStudents == true)
        {
            return (7, "Graduating students");
        }

        // Priority 8: Large classes.
        if (e.LargeClass == true)
        {
            return (8, "Large class");
        }

        // Priority 9: Remaining exams.
        return (9, "Remaining / unconfirmed priority");
    }
}
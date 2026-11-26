using System.Text.Json;

namespace ExamPlannerServer.Planning;

public sealed class ClashMap
{
    private readonly Dictionary<(int, int), HashSet<int>>
        _students = new();

    private readonly Dictionary<int, HashSet<int>>
        _related = new();

    public ClashMap(IEnumerable<EventData> events)
    {
        var items = events.ToArray();

        foreach (var item in items)
        {
            _related[item.Id] = new HashSet<int>();
        }

        foreach (var group in items
                     .SelectMany(e => e.StudentIds
                         .Distinct()
                         .Select(s => (
                             Resource: s,
                             Event: e.Id
                         )))
                     .GroupBy(x => x.Resource))
        {
            AddPairs(
                group.Select(x => x.Event),
                group.Key
            );
        }

        foreach (var group in items
                     .SelectMany(e => e.TeacherIds
                         .Distinct()
                         .Select(t => (
                             Resource: t,
                             Event: e.Id
                         )))
                     .GroupBy(x => x.Resource))
        {
            AddPairs(
                group.Select(x => x.Event),
                null
            );
        }
    }

    private void AddPairs(
        IEnumerable<int> ids,
        int? student
    )
    {
        var events = ids
            .Distinct()
            .Order()
            .ToArray();

        for (var i = 0; i < events.Length; i++)
        {
            for (var j = i + 1; j < events.Length; j++)
            {
                var first = events[i];
                var second = events[j];

                _related[first].Add(second);
                _related[second].Add(first);

                if (student is not int studentId)
                {
                    continue;
                }

                var key = (first, second);

                if (!_students.TryGetValue(
                        key,
                        out var shared
                    ))
                {
                    shared = new HashSet<int>();
                    _students[key] = shared;
                }

                shared.Add(studentId);
            }
        }
    }

    public int[] Related(int id)
    {
        return _related.TryGetValue(id, out var related)
            ? related.Order().ToArray()
            : [];
    }

    public int[] Students(int first, int second)
    {
        var key = (
            Math.Min(first, second),
            Math.Max(first, second)
        );

        return _students.TryGetValue(key, out var students)
            ? students.Order().ToArray()
            : [];
    }
}

public sealed class ClashMapCache
{
    private readonly object gate = new();

    private readonly Dictionary<string, ClashMap>
        maps = new();

    private readonly Queue<string>
        order = new();

    public ClashMap Get(EventData[] events)
    {
        var key = JsonSerializer.Serialize(
            events
                .OrderBy(e => e.Id)
                .Select(e => new
                {
                    e.Id,

                    Students = e.StudentIds
                        .Distinct()
                        .Order(),

                    Teachers = e.TeacherIds
                        .Distinct()
                        .Order()
                })
        );

        lock (gate)
        {
            if (maps.TryGetValue(
                    key,
                    out var existing
                ))
            {
                return existing;
            }

            var map = new ClashMap(events);

            maps[key] = map;
            order.Enqueue(key);

            if (order.Count > 8)
            {
                maps.Remove(order.Dequeue());
            }
            return map;
        }
    }
}
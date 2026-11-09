using DatabaseConnection.repo;
using ExamPlannerServer.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExamPlannerServer.controller;

[ApiController]
[Route("api/v1/rooms")]
[Authorize]
public class RoomsController(IRoomRepo rooms, IExamSessionRepo examSessions ) : ControllerBase
{
    [HttpGet("available")]
    public async Task<
        ActionResult<List<AvailableRoomDto>>
    > GetAvailableRooms(
        [FromQuery] DateOnly? date,
        [FromQuery] string? campus,
        [FromQuery] string? search
    )
    {
        var roomList = await rooms.GetAllAsync();

        var occupiedRoomIds =
            new HashSet<int>();
        
        if (date.HasValue)
        {
            var sessions = await examSessions.GetAllAsync();

            occupiedRoomIds =
                sessions
                    .Where(
                        session =>
                            session.Date ==
                            date.Value
                    )
                    .Select(
                        session =>
                            session.RoomId
                    )
                    .ToHashSet();
        }
        
        var query = roomList.AsEnumerable();

        if (date.HasValue)
        {
            query =
                query.Where(
                    room =>
                        !occupiedRoomIds.Contains(
                            room.Id
                        )
                );
        }

        if (!string.IsNullOrWhiteSpace(campus))
        {
            var wantedCampus =
                campus.Trim();

            query =
                query.Where(
                    room =>
                        string.Equals(
                            room.Campus,
                            wantedCampus,
                            StringComparison.OrdinalIgnoreCase
                        )
                );
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();

            query =
                query.Where(
                    room =>
                        room.Name.Contains(
                            term,
                            StringComparison.OrdinalIgnoreCase
                        )
                        ||
                        room.Campus.Contains(
                            term,
                            StringComparison.OrdinalIgnoreCase
                        )
                );
        }
        
        var result =
            query
                .OrderBy(
                    room =>
                        room.Campus
                )
                .ThenBy(
                    room =>
                        room.Name
                )
                .Select(
                    room =>
                        new AvailableRoomDto(
                            room.Id,
                            room.Name,
                            room.Campus,
                            room.Capacity
                        )
                )
                .ToList();
        
        return Ok(result);
    }
}
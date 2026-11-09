namespace ExamPlannerServer.Dto;

public sealed record AvailableRoomDto(
    int Id,
    string Name,
    string Campus,
    int Capacity
);
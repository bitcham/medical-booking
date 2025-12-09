namespace Core.Application.Dtos.Requests;

public record GenerateTimeSlotsRequest(
    DateOnly Date,
    TimeSpan? Offset = null
);

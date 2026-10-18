namespace Web.Models;

public sealed class HourlyReportResult
{
    public bool IsOutsideOperatingHours =>
        RequestedHour < 7 || RequestedHour >= 23;
    public int RequestedHour { get; init; }

    public HourlyReports? SourceReports { get; init; }

    public int? SourceHour => SourceReports?.Hour;

    public ElevatorStatus Status =>
        SourceReports?.Status ?? ElevatorStatus.NoData;

    public bool IsCarriedForward =>
        SourceHour is int sourceHour &&
        sourceHour < RequestedHour;

    public int YesCount => SourceReports?.YesCount ?? 0;

    public int NoCount => SourceReports?.NoCount ?? 0;

    public int TotalCount => YesCount + NoCount;
}
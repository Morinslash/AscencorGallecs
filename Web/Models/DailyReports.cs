namespace Web.Models;

using System;
using System.Linq;

public sealed class DailyReports
{
    public DateOnly Date { get; set; }

    public HourlyReports[] Hours { get; set; } =
        Array.Empty<HourlyReports>();

    public HourlyReportResult GetReportForHour(int hour)
    {
        if (hour < 0 || hour > 23)
        {
            throw new ArgumentOutOfRangeException(nameof(hour));
        }

        var sourceReports = Hours
            .Where(report =>
                report.Hour >= 7 &&
                report.Hour < 23 &&
                report.Hour <= hour &&
                report.TotalCount > 0)
            .OrderByDescending(report => report.Hour)
            .FirstOrDefault();

        return new HourlyReportResult
        {
            RequestedHour = hour,
            SourceReports = sourceReports
        };
    }
}
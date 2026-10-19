namespace API.Models

open System

type HourlyReports =
    {
        Hour: int
        YesCount: int
        NoCount: int
    }

type DailyReports =
    {
        Date: DateOnly
        Hours: HourlyReports array
    }

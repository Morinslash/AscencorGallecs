namespace API.Controllers

open System
open Microsoft.AspNetCore.Mvc
open API.Data
open API.Models
open API.Configuration
open Microsoft.AspNetCore.RateLimiting

[<ApiController>]
[<Route("api/reports")>]
type ReportsController(store: ReportStore) =
    inherit ControllerBase()

    let elevatorTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Madrid")

    [<HttpGet("today")>]
    member _.GetToday() : DailyReports =
        let now = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, elevatorTimeZone)

        let today = DateOnly.FromDateTime(now.DateTime)

        store.GetDailyReports(today)

    [<HttpPost>]
    [<EnableRateLimiting(RateLimiting.SubmissionPolicy)>]
    member this.Submit([<FromBody>] answer: string) : IActionResult =
        match answer with
        | "yes"
        | "no" ->
            let now = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, elevatorTimeZone)

            if now.Hour < 7 || now.Hour >= 23 then
                this.BadRequest("Reports are accepted only between 07:00 and 23:00.") :> IActionResult
            else
                let today = DateOnly.FromDateTime(now.DateTime)

                let isWorking = answer = "yes"

                store.AddReport(today, now.Hour, isWorking)

                this.Ok(store.GetDailyReports(today)) :> IActionResult

        | _ -> this.BadRequest() :> IActionResult

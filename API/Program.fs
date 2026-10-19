namespace API

open System.IO
open API.Data
open API.Configuration
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection

module Program =

    [<EntryPoint>]
    let main args =
        let builder = WebApplication.CreateBuilder(args)

        let databasePath =
            Path.Combine(builder.Environment.ContentRootPath, "App_Data", "ascensorgallecs.db")

        builder.Services.AddControllers() |> ignore

        builder.Services.AddSingleton<ReportStore>(fun _ -> ReportStore(databasePath))
        |> ignore

        RateLimiting.configure builder.Services
        
        let app = builder.Build()

        Database.initialize databasePath
        
        app.UseStaticFiles() |> ignore
        app.UseRouting() |> ignore
        app.UseRateLimiter() |> ignore
        
        app.MapControllers() |> ignore

        app.MapStaticAssets() |> ignore

        app.MapFallbackToFile("index.html") |> ignore

        app.Run()

        0

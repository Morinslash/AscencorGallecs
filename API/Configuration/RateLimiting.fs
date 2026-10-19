namespace API.Configuration

open System
open System.Globalization
open System.Threading
open System.Threading.Tasks
open System.Threading.RateLimiting
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.RateLimiting
open Microsoft.Extensions.DependencyInjection

module RateLimiting =

    [<Literal>]
    let SubmissionPolicy = "report-submissions"

    let configure (services: IServiceCollection) =
        services.AddRateLimiter(fun options ->
            options.RejectionStatusCode <-
                StatusCodes.Status429TooManyRequests

            options.AddPolicy<string>(
                SubmissionPolicy,
                Func<HttpContext, RateLimitPartition<string>>(fun context ->
                    let address = context.Connection.RemoteIpAddress

                    let key =
                        if isNull address then
                            "unknown"
                        else
                            address.MapToIPv6().ToString()

                    RateLimitPartition.GetTokenBucketLimiter(
                        key,
                        Func<string, TokenBucketRateLimiterOptions>(fun _ ->
                            TokenBucketRateLimiterOptions(
                                TokenLimit = 3,
                                TokensPerPeriod = 1,
                                ReplenishmentPeriod = TimeSpan.FromSeconds(60.0),
                                AutoReplenishment = true,
                                QueueLimit = 0,
                                QueueProcessingOrder =
                                    QueueProcessingOrder.OldestFirst
                            )
                        )
                    )
                )
            )
            |> ignore

            options.OnRejected <-
                Func<OnRejectedContext, CancellationToken, ValueTask>(
                    fun context cancellationToken ->
                        let response = context.HttpContext.Response

                        response.StatusCode <-
                            StatusCodes.Status429TooManyRequests

                        let mutable retryAfter = TimeSpan.Zero

                        if context.Lease.TryGetMetadata(
                            MetadataName.RetryAfter,
                            &retryAfter
                        ) then
                            let seconds =
                                Math.Max(
                                    1.0,
                                    Math.Ceiling(retryAfter.TotalSeconds)
                                )

                            response.Headers["Retry-After"] <-
                                Microsoft.Extensions.Primitives.StringValues(
                                    seconds.ToString(
                                        "0",
                                        CultureInfo.InvariantCulture
                                    )
                                )

                        response.ContentType <- "text/plain; charset=utf-8"

                        ValueTask(
                            response.WriteAsync(
                                "Demasiados avisos. Espera unos segundos antes de volver a intentarlo.",
                                cancellationToken
                            )
                        )
                )
        )
        |> ignore

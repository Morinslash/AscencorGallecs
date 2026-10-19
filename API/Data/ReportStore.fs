namespace API.Data

open System
open System.Globalization
open Microsoft.Data.Sqlite
open API.Models

type ReportStore(databasePath: string) =

    let connectionString =
        SqliteConnectionStringBuilder(
            DataSource = databasePath,
            Mode = SqliteOpenMode.ReadWrite
        ).ToString()

    member _.GetDailyReports(date: DateOnly) : DailyReports =
        use connection = new SqliteConnection(connectionString)
        connection.Open()

        use command = connection.CreateCommand()

        command.CommandText <-
            """
            SELECT hour, yes_count, no_count
            FROM hourly_reports
            WHERE report_date = $date
            ORDER BY hour;
            """

        command.Parameters.AddWithValue(
            "$date",
            date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
        )
        |> ignore

        use reader = command.ExecuteReader()

        let hours = ResizeArray<HourlyReports>()

        while reader.Read() do
            hours.Add(
                {
                    Hour = reader.GetInt32(0)
                    YesCount = reader.GetInt32(1)
                    NoCount = reader.GetInt32(2)
                }
            )

        {
            Date = date
            Hours = hours.ToArray()
        }

    member _.AddReport(
        date: DateOnly,
        hour: int,
        isWorking: bool
    ) : unit =
        let yesIncrement = if isWorking then 1 else 0
        let noIncrement = if isWorking then 0 else 1

        use connection = new SqliteConnection(connectionString)
        connection.Open()

        use command = connection.CreateCommand()

        command.CommandText <-
            """
            INSERT INTO hourly_reports (
                report_date,
                hour,
                yes_count,
                no_count
            )
            VALUES ($date, $hour, $yes, $no)
            ON CONFLICT (report_date, hour)
            DO UPDATE SET
                yes_count = hourly_reports.yes_count + excluded.yes_count,
                no_count = hourly_reports.no_count + excluded.no_count;
            """

        command.Parameters.AddWithValue(
            "$date",
            date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
        )
        |> ignore

        command.Parameters.AddWithValue("$hour", hour) |> ignore
        command.Parameters.AddWithValue("$yes", yesIncrement) |> ignore
        command.Parameters.AddWithValue("$no", noIncrement) |> ignore
        
        command.ExecuteNonQuery() |> ignore

namespace API.Data

open System.IO
open Microsoft.Data.Sqlite

module Database =

    let initialize (databasePath: string) =
        let directory = Path.GetDirectoryName(databasePath)

        if not (System.String.IsNullOrEmpty(directory)) then
            Directory.CreateDirectory(directory) |> ignore

        let connectionString =
            SqliteConnectionStringBuilder(
                DataSource = databasePath,
                Mode = SqliteOpenMode.ReadWriteCreate
            ).ToString()

        use connection = new SqliteConnection(connectionString)
        connection.Open()

        use command = connection.CreateCommand()

        command.CommandText <-
            """
            CREATE TABLE IF NOT EXISTS hourly_reports (
                report_date TEXT NOT NULL,
                hour INTEGER NOT NULL
                    CHECK (hour >= 7 AND hour < 23),
                yes_count INTEGER NOT NULL DEFAULT 0
                    CHECK (yes_count >= 0),
                no_count INTEGER NOT NULL DEFAULT 0
                    CHECK (no_count >= 0),

                PRIMARY KEY (report_date, hour)
            );
            """

        command.ExecuteNonQuery() |> ignore


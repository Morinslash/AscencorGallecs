namespace Web.Models;

public sealed class HourlyReports
{
    public int Hour { get; set; }

    public int YesCount { get; set; }

    public int NoCount { get; set; }

    public int TotalCount => YesCount + NoCount;

    public ElevatorStatus Status
    {
        get
        {
            if (TotalCount == 0)
            {
                return ElevatorStatus.NoData;
            }

            if (YesCount > NoCount)
            {
                return ElevatorStatus.Working;
            }

            if (NoCount > YesCount)
            {
                return ElevatorStatus.NotWorking;
            }

            return ElevatorStatus.NoMajority;
        }
    }
}

using SQLite;

[Table("RunRecords")]
public sealed class RunRecord
{
    [PrimaryKey, AutoIncrement]
    public int RunNumber { get; set; }

    public double SurvivedSeconds { get; set; }
    public int FiredProjectileCount { get; set; }
}

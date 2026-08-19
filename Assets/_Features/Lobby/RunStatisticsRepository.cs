using System.Collections.Generic;
using System.IO;
using SQLite;
using UnityEngine;

public sealed class RunStatisticsRepository
{
    private const string DatabaseFileName = "RunStatistics.db3";

    private readonly string databasePath;

    public RunStatisticsRepository()
        : this(Path.Combine(Application.persistentDataPath, DatabaseFileName))
    { }

    private RunStatisticsRepository(string databasePath)
    {
        this.databasePath = databasePath;

        using SQLiteConnection connection = OpenConnection();
        connection.CreateTable<RunRecord>();
    }

    public RunRecord AddRun(double survivedSeconds, int firedProjectileCount)
    {
        RunRecord record = new()
        {
            SurvivedSeconds = survivedSeconds,
            FiredProjectileCount = firedProjectileCount
        };

        using SQLiteConnection connection = OpenConnection();
        connection.Insert(record);
        return record;
    }

    public IReadOnlyList<RunRecord> GetAllRuns()
    {
        using SQLiteConnection connection = OpenConnection();
        return connection.Table<RunRecord>()
            .OrderBy(record => record.RunNumber)
            .ToList();
    }

    private SQLiteConnection OpenConnection() => new(databasePath);
}

using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public sealed class RunStatisticsTextDisplay : BaseBehaviour
{
    private TMP_Text text;

    private RunStatisticsRepository repository;

    private void Awake()
    {
        text = GetComponent<TMP_Text>();
        repository = new RunStatisticsRepository();
    }

    private void OnEnable() => Refresh();

    public void Refresh()
    {
        IReadOnlyList<RunRecord> records = repository.GetAllRuns();
        StringBuilder builder = new();

        for (int i = 0; i < records.Count; i++)
            AppendRecord(builder, records[i]);

        text.text = builder.ToString();
    }

    private static void AppendRecord(StringBuilder builder, RunRecord record)
    {
        long totalSeconds = (long)Math.Floor(record.SurvivedSeconds);
        long hours = totalSeconds / 3600;
        long minutes = totalSeconds / 60 % 60;
        long seconds = totalSeconds % 60;

        builder
            .Append($"<mspace=0.6em>#{record.RunNumber}</mspace>")
            .Append($"<mspace=1em> | 버틴 시간: </mspace><mspace=0.6em>{hours:00}:{minutes:00}:{seconds:00}</mspace>")
            .Append($"<mspace=1em> | 발사한 투사체: </mspace><mspace=0.6em>{record.FiredProjectileCount}</mspace>개")
            .AppendLine();
    }
}

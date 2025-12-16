// Storage layout (under Application.persistentDataPath/<rootFolderName>/):
//   Usage/
//     2025/                (directory per year)
//       01.csv             (file per month)
//       02.csv
//
// Month file CSV format (headers + rows):
//   day,seconds
//   1,120
//   2,0
//   14,3560

using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public sealed class DailyUsageTracker
{
    public struct DailyUsage
    {
        public DateTime Date;   // UTC date (00:00:00)
        public int Minutes;    // minutes watched that day
    }

    private float _pendingSecondsFloat;
    private float _flushTimer;

    // flush every 30sec
    private const float FlushEvery = 30f;

    private readonly string _rootDir;

    public DailyUsageTracker()
    {
        _rootDir = Application.persistentDataPath + "/../" + "Stats";
        InitStorage();
    }

    private void InitStorage()
    {
        try
        {
            Directory.CreateDirectory(_rootDir);
        }
        catch (Exception e)
        {
            Debug.LogError($"[DailyUsageTracker] Failed to create root dir '{_rootDir}': {e}");
        }
    }

    // --------------------------------------------------
    // Runtime tracking
    // --------------------------------------------------

    public void Tick(float deltaTime)
    {
        if (deltaTime <= 0f) return;

        _pendingSecondsFloat += deltaTime;
        _flushTimer += deltaTime;

        if (_flushTimer >= FlushEvery)
        {
            Flush();
            _flushTimer = 0f;
        }
    }

    // --------------------------------------------------
    // Queries
    // --------------------------------------------------

    public List<int> GetYearsWithData()
    {
        var years = new List<int>();
        if (!Directory.Exists(_rootDir)) return years;

        try
        {
            foreach (var dir in Directory.GetDirectories(_rootDir))
            {
                var name = Path.GetFileName(dir);
                if (int.TryParse(name, out int y))
                    years.Add(y);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[DailyUsageTracker] Failed reading years from '{_rootDir}': {e}");
        }

        years.Sort();
        return years;
    }

    public List<int> GetMonthsWithData(int year)
    {
        var months = new List<int>();
        string yearDir = GetYearDir(year);
        if (!Directory.Exists(yearDir)) return months;

        try
        {
            foreach (var file in Directory.GetFiles(yearDir, "*.csv"))
            {
                string name = Path.GetFileNameWithoutExtension(file); // "01"
                if (int.TryParse(name, out int m) && m >= 1 && m <= 12)
                    months.Add(m);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[DailyUsageTracker] Failed reading months from '{yearDir}': {e}");
        }

        months.Sort();
        return months;
    }

    public List<DailyUsage> GetMonthlyUsage(int year, int month)
    {
        int daysInMonth = DateTime.DaysInMonth(year, month);
        var result = new List<DailyUsage>(daysInMonth);

        MonthData monthData = null;
        if (TryLoadMonthData(year, month, out var loaded))
        {
            monthData = loaded;
        }

        for (int d = 1; d <= daysInMonth; d++)
        {
            int secs = monthData != null ? monthData.GetDaySeconds(d) : 0;
            int minutes = secs / 60;

            result.Add(new DailyUsage
            {
                Date = new DateTime(year, month, d, 0, 0, 0, DateTimeKind.Utc),
                Minutes = minutes
            });
        }

        return result;
    }

    // --------------------------------------------------
    // Storage model
    // --------------------------------------------------

    private sealed class MonthData
    {
        public int Year { get; }
        public int Month { get; }
        private readonly Dictionary<int, int> _dayToSeconds = new Dictionary<int, int>(31);

        public MonthData(int year, int month)
        {
            Year = year;
            Month = month;
        }

        public int GetDaySeconds(int day)
        {
            if (day < 1 || day > 31)
            {
                return 0;
            }
            return _dayToSeconds.TryGetValue(day, out int s) ? Mathf.Max(0, s) : 0;
        }

        public void SetDaySeconds(int day, int seconds)
        {
            if (day < 1 || day > 31)
            {
                return;
            }
            _dayToSeconds[day] = Mathf.Max(0, seconds);
        }

        public IEnumerable<KeyValuePair<int, int>> Days => _dayToSeconds;
    }

    // --------------------------------------------------
    // File paths
    // --------------------------------------------------

    private string GetYearDir(int year) => Path.Combine(_rootDir, year.ToString("D4"));
    private string GetMonthFile(int year, int month) => Path.Combine(GetYearDir(year), month.ToString("D2") + ".csv");

    // --------------------------------------------------
    // Month file load/save (CSV)
    // --------------------------------------------------

    private bool TryLoadMonthData(int year, int month, out MonthData data)
    {
        data = null;

        string path = GetMonthFile(year, month);
        if (!File.Exists(path))
        {
            return false;
        }

        string text;
        try
        {
            text = File.ReadAllText(path);
        }
        catch (Exception e)
        {
            Debug.LogError($"[DailyUsageTracker] Failed to read month file '{path}': {e}");
            return false;
        }

        try
        {
            data = ParseMonthCsv(text, year, month);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[DailyUsageTracker] Failed to parse month file '{path}': {e}");
            data = null;
            return false;
        }
    }

    private void TrySaveMonthDataAtomic(MonthData data)
    {
        string yearDir = GetYearDir(data.Year);
        string finalPath = GetMonthFile(data.Year, data.Month);
        string tmpPath = finalPath + ".tmp";

        try
        {
            Directory.CreateDirectory(yearDir);
        }
        catch (Exception e)
        {
            Debug.LogError($"[DailyUsageTracker] Failed to create year dir '{yearDir}': {e}");
            return;
        }

        string csv;
        try
        {
            csv = SerializeMonthCsv(data);
        }
        catch (Exception e)
        {
            Debug.LogError($"[DailyUsageTracker] Failed to serialize month data {data.Year}-{data.Month:D2}: {e}");
            return;
        }

        try
        {
            File.WriteAllText(tmpPath, csv, Encoding.UTF8);

            if (File.Exists(finalPath))
            {
                File.Delete(finalPath);
                File.Move(tmpPath, finalPath);
            }
            else
            {
                File.Move(tmpPath, finalPath);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[DailyUsageTracker] Failed to write month file '{finalPath}': {e}");
            try { if (File.Exists(tmpPath)) File.Delete(tmpPath); } catch { }
        }
    }

    private void Flush()
    {
        int delta = Mathf.FloorToInt(_pendingSecondsFloat);
        if (delta <= 0)
        {
            return;
        }
        _pendingSecondsFloat -= delta; // keep remainder fraction

        DateTime now = DateTime.UtcNow;
        int year = now.Year;
        int month = now.Month;
        int day = now.Day;

        if (!TryLoadMonthData(year, month, out var monthData))
        {
            monthData = new MonthData(year, month);
        }

        int current = monthData.GetDaySeconds(day);
        monthData.SetDaySeconds(day, current + delta);

        TrySaveMonthDataAtomic(monthData);
    }
    private static string SerializeMonthCsv(MonthData data)
    {
        var sb = new StringBuilder(512);
        sb.Append("day,seconds\n");

        var days = new List<int>();
        foreach (var kv in data.Days) days.Add(kv.Key);
        days.Sort();

        foreach (var day in days)
        {
            int secs = data.GetDaySeconds(day);
            sb.Append(day).Append(',').Append(secs).Append('\n');
        }

        return sb.ToString();
    }

    private static MonthData ParseMonthCsv(string text, int year, int month)
    {
        var data = new MonthData(year, month);

        using var sr = new StringReader(text);
        string line;
        bool firstLine = true;

        while ((line = sr.ReadLine()) != null)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }
            line = System.Text.RegularExpressions.Regex.Replace(
                line,
                @"[^a-zA-Z0-9,;\r\n]",
                string.Empty
            );

            // Skip header if present
            if (firstLine)
            {
                firstLine = false;
                var hdr = line.Trim().ToLowerInvariant();
                if (hdr == "day,seconds" || hdr == "day;seconds")
                {
                    continue;
                }
            }

            // Support comma or semicolon
            string trimmed = line.Trim();
            char sep = trimmed.Contains(",") ? ',' : (trimmed.Contains(";") ? ';' : '\0');
            if (sep == '\0')
            {
                continue;
            }

            var parts = trimmed.Split(sep);
            if (parts.Length < 2)
            {
                continue;
            }

            if (!int.TryParse(parts[0].Trim(), out int day))
            {
                continue;
            }
            if (!int.TryParse(parts[1].Trim(), out int secs))
            {
                continue;
            }

            if (day < 1 || day > 31)
            {
                continue;
            }
            if (secs < 0)
            {
                secs = 0;
            }

            data.SetDaySeconds(day, secs);
        }

        return data;
    }
}

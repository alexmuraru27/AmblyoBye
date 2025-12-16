using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using XCharts.Runtime;
using System.Globalization;

public class StatsGraph : MonoBehaviour
{
    [Header("UI (Data source)")]
    [SerializeField] private TMP_Dropdown yearDropdown;
    [SerializeField] private TMP_Dropdown monthDropdown;

    [Header("Chart")]
    [SerializeField] private BarChart barChart;


    private DailyUsageTracker tracker;

    private List<DailyUsageTracker.DailyUsage> usage = new();
    private List<int> years = new();
    private List<int> months = new();
    private bool suppressDropdownEvents;

    private void Awake()
    {
        tracker = new DailyUsageTracker();
        gameObject.SetActive(false);
    }

    private void Start()
    {
        if (!ValidateRefs())
        {
            return;
        }

        barChart.ClearData();
        WireDropdowns();
        PopulateYearDropdown();
    }

    private bool ValidateRefs()
    {
        if (!yearDropdown || !monthDropdown || !barChart)
        {
            Debug.LogError("StatsGraph: Missing references.");
            return false;
        }
        return true;
    }

    private void WireDropdowns()
    {
        yearDropdown.onValueChanged.AddListener(OnYearChanged);
        monthDropdown.onValueChanged.AddListener(OnMonthChanged);
    }

    // ----------------------------
    // Dropdowns (tracker getters only)
    // ----------------------------

    public void PopulateYearDropdown()
    {
        suppressDropdownEvents = true;

        years = tracker.GetYearsWithData();
        yearDropdown.ClearOptions();

        if (years.Count == 0)
        {
            years.Add(DateTime.UtcNow.Year);
        }

        years.Sort();

        var opts = new List<TMP_Dropdown.OptionData>();
        foreach (var y in years)
        {
            opts.Add(new TMP_Dropdown.OptionData(y.ToString()));
        }

        yearDropdown.AddOptions(opts);

        // select most recent year (last)
        yearDropdown.value = years.Count - 1;
        yearDropdown.RefreshShownValue();

        suppressDropdownEvents = false;

        PopulateMonthDropdown(GetSelectedYear());
    }

    public void PopulateMonthDropdown(int year)
    {
        suppressDropdownEvents = true;

        months = tracker.GetMonthsWithData(year);
        monthDropdown.ClearOptions();

        if (months.Count == 0)
        {
            months.Add(DateTime.UtcNow.Month);
        }

        months.Sort();

        var opts = new List<TMP_Dropdown.OptionData>();
        var monthNames = CultureInfo.InvariantCulture.DateTimeFormat.AbbreviatedMonthNames;

        foreach (var m in months)
        {
            // m is 1–12 → monthNames is 0–11
            string label = monthNames[m - 1];
            opts.Add(new TMP_Dropdown.OptionData(label));
        }

        monthDropdown.AddOptions(opts);

        // select most recent month (last)
        monthDropdown.value = months.Count - 1;
        monthDropdown.RefreshShownValue();

        suppressDropdownEvents = false;

        LoadSelectedMonthAndRebuild();
    }


    private void OnYearChanged(int _)
    {
        if (!suppressDropdownEvents)
        {
            PopulateMonthDropdown(GetSelectedYear());
        }

    }

    private void OnMonthChanged(int _)
    {
        if (!suppressDropdownEvents)
        {
            LoadSelectedMonthAndRebuild();
        }
    }

    private int GetSelectedYear() =>
        years[Mathf.Clamp(yearDropdown.value, 0, years.Count - 1)];

    private int GetSelectedMonth() =>
        months[Mathf.Clamp(monthDropdown.value, 0, months.Count - 1)];

    // ----------------------------
    // Chart
    // ----------------------------


    public void LoadSelectedMonthAndRebuild()
    {
        usage = tracker.GetMonthlyUsage(GetSelectedYear(), GetSelectedMonth());
        Rebuild();
    }

    public void Rebuild()
    {
        barChart.ClearData();
        if (usage == null || usage.Count == 0)
        {
            return;

        }

        for (int i = 0; i < usage.Count; i++)
        {
            barChart.AddXAxisData(usage[i].Date.Day.ToString());
            barChart.AddData(0, usage[i].Minutes);
        }

        barChart.RefreshChart();
    }

}

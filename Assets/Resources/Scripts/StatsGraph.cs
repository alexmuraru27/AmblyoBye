using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using XCharts.Runtime;

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
        if (!ValidateRefs()) return;

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
            years.Add(DateTime.UtcNow.Year);

        var opts = new List<TMP_Dropdown.OptionData>();
        foreach (var y in years)
            opts.Add(new TMP_Dropdown.OptionData(y.ToString()));

        yearDropdown.AddOptions(opts);
        yearDropdown.value = 0;
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
            months.Add(DateTime.UtcNow.Month);

        var opts = new List<TMP_Dropdown.OptionData>();
        foreach (var m in months)
            opts.Add(new TMP_Dropdown.OptionData(m.ToString("D2")));

        monthDropdown.AddOptions(opts);
        monthDropdown.value = 0;
        monthDropdown.RefreshShownValue();

        suppressDropdownEvents = false;
        LoadSelectedMonthAndRebuild();
    }

    private void OnYearChanged(int _)
    {
        if (!suppressDropdownEvents)
            PopulateMonthDropdown(GetSelectedYear());
    }

    private void OnMonthChanged(int _)
    {
        if (!suppressDropdownEvents)
            LoadSelectedMonthAndRebuild();
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
            return;

        for (int i = 0; i < usage.Count; i++)
        {
            barChart.AddXAxisData(usage[i].Day.Day.ToString());
            barChart.AddData(0, usage[i].Minutes);
        }

        barChart.RefreshChart();
    }

}

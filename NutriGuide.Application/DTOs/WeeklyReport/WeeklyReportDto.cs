namespace NutriGuide.Application.DTOs.WeeklyReport;

public class WeeklyReportDto
{
    public DateOnly WeekStart { get; set; }
    public DateOnly WeekEnd { get; set; }
    public int DaysLogged { get; set; }

    public decimal AvgCalories { get; set; }
    public decimal AvgProtein_g { get; set; }
    public decimal AvgCarbs_g { get; set; }
    public decimal AvgFat_g { get; set; }
    public decimal AvgFiber_g { get; set; }

    public int TargetCalories { get; set; }
    public decimal CalorieAdherence_pct { get; set; }

    public decimal? StartingWeight_kg { get; set; }
    public decimal? ThisWeekWeight_kg { get; set; }
    public decimal? WeightChange_kg { get; set; }
    public string Goal { get; set; } = string.Empty;
    public bool? OnTrack { get; set; }
    public bool CanLogWeight { get; set; }
}
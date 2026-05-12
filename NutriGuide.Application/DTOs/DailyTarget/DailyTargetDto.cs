namespace NutriGuide.Application.DTOs.DailyTarget;

public class DailyTargetDto
{
    public Guid Id { get; set; }
    public DateOnly TargetDate { get; set; }
    public int Calories { get; set; }
    public decimal Protein_g { get; set; }
    public decimal Carbs_g { get; set; }
    public decimal Fat_g { get; set; }
    public decimal Fiber_g { get; set; }
    public bool IsAiGenerated { get; set; }
}
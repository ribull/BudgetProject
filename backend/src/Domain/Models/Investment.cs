namespace Domain.Models;

public record Investment
{
    public int InvestmentId { get; set; }

    public string Description { get; set; } = "";

    public double CurrentAmount { get; set; }

    public double? YearlyGrowthRate { get; set; }

    public DateTime LastUpdated { get; set; }
}

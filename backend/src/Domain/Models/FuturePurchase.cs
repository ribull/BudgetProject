namespace Domain.Models;

#nullable disable

public record FuturePurchase
{
    public int FuturePurchaseId { get; set; }

    public DateTime Date { get; set; }

    public string Description { get; set; }

    public double Amount { get; set; }

    public string Category { get; set; } 
}

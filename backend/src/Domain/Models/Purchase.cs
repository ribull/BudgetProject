namespace Domain.Models;

public record Purchase
{
    public int PurchaseId { get; init; }

    public DateTime Date { get; init; }

    public string Description { get; init; } = "";

    public double Amount { get; init; }
    
    public string? Category { get; init; }
}

namespace Domain.Models;

#nullable disable

public record Saved
{
    public int SavedId { get; set; }

    public DateTime Date { get; set; }

    public string Description { get; set; }

    public double Amount { get; set; }
}

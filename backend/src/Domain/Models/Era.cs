namespace Domain.Models;

public record Era
{
    public int EraId { get; set; }

    public string Name { get; set; } = "";

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }
}

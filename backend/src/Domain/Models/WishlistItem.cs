namespace Domain.Models;

public record WishlistItem
{
    public int WishlistItemId { get; set; }

    public string Description { get; set; } = "";

    public double? Amount { get; set; }

    public string Notes { get; set; } = "";
}

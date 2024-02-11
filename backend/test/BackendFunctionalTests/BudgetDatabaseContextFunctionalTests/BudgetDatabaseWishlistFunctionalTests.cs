using NUnit.Framework;
using Domain.Models;

namespace BackendFunctionalTests.BudgetDatabaseContextFunctionalTests;

public class BudgetDatabaseWishlistFunctionalTests : BaseBudgetDatabaseContextFunctionalTests
{
    [Test]
    public async Task GetWishlistItemsTest()
    {
        // Arrange
        List<WishlistItem> wishlistItems = new()
        {
            new WishlistItem
            {
                WishlistItemId = 1,
                Description = "Test Description",
                Amount = 99.12,
                Notes = "hello"

            },
            new WishlistItem
            {
                WishlistItemId = 9,
                Description = "Test Description 2",
                Amount = 14.21,
                Notes = "nah"
            },
            new WishlistItem
            {
                WishlistItemId = 1000,
                Description = "Test Description  3",
                Amount = 123.45,
                Notes = ""
            }
        };

        foreach (WishlistItem wishlistItem in wishlistItems)
        {
            await InsertWishlistItem(wishlistItem);
        }

        // Act
        IEnumerable<WishlistItem> result = await BudgetDatabaseContext.GetWishlistAsync();

        // Assert
        Assert.That(result, Is.EquivalentTo(wishlistItems));
    }

    [Test]
    public async Task AddWishlistItemTest()
    {
        // Arrange
        WishlistItem wishlistItem = new WishlistItem
        {
            Description = "Test Description",
            Amount = 123.45,
            Notes = "hello"
        };

        // Act
        await BudgetDatabaseContext.AddWishlistItemAsync(wishlistItem);

        // Assert
        Assert.That(await SqlHelper.ExistsAsync(BudgetDatabaseName,
@$"SELECT 1
FROM Wishlist
WHERE
    Description = '{wishlistItem.Description}'
    AND Amount = {wishlistItem.Amount}
    AND Notes = '{wishlistItem.Notes}'"));
    }

    [Test]
    public async Task UpdateWishlistItemTest()
    {
        // Arrange
        WishlistItem originalWishlistItem = new WishlistItem
        {
            WishlistItemId = 199,
            Description = "Test Description",
            Amount = 123.45,
            Notes = "hello"
        };

        await InsertWishlistItem(originalWishlistItem);

        // Sanity check
        Assert.That(await SqlHelper.ExistsAsync(BudgetDatabaseName,
@$"SELECT 1
FROM Wishlist
WHERE
    WishlistItemId = {originalWishlistItem.WishlistItemId}
    AND Description = '{originalWishlistItem.Description}'
    AND Amount = {originalWishlistItem.Amount}
    AND Notes = '{originalWishlistItem.Notes}'"));

        WishlistItem newWishlistItem = new WishlistItem
        {
            WishlistItemId = originalWishlistItem.WishlistItemId,
            Description = "Test Description New",
            Amount = 99.81,
            Notes = "i dont want"
        };

        // Act
        await BudgetDatabaseContext.UpdateWishlistItemAsync(newWishlistItem);

        // Assert
        Assert.That(await SqlHelper.ExistsAsync(BudgetDatabaseName,
@$"SELECT 1
FROM Wishlist
WHERE
    WishlistItemId = {originalWishlistItem.WishlistItemId}
    AND Description = '{newWishlistItem.Description}'
    AND Amount = {newWishlistItem.Amount}
    AND Notes = '{newWishlistItem.Notes}'"));
    }

    [Test]
    public void UpdateWishlistItemDoesNotExist()
    {
        Assert.ThrowsAsync<ArgumentException>(async () => await BudgetDatabaseContext.UpdateWishlistItemAsync(new WishlistItem
        {
            WishlistItemId = 100,
            Description = "Test Description New",
            Amount = 1,
            Notes = "",
        }));
    }

    [Test]
    public async Task DeleteWishlistItem()
    {
        // Arrange
        WishlistItem wishlistItem = new WishlistItem
        {
            WishlistItemId = 199,
            Description = "Test Description",
            Amount = 123.45,
            Notes = "hello"
        };

        await InsertWishlistItem(wishlistItem);

        // Sanity check
        Assert.That(await SqlHelper.ExistsAsync(BudgetDatabaseName,
@$"SELECT 1
FROM Wishlist
WHERE
    WishlistItemId = {wishlistItem.WishlistItemId}
    AND Description = '{wishlistItem.Description}'
    AND Amount = {wishlistItem.Amount}
    AND Notes = '{wishlistItem.Notes}'"));

        // Act
        await BudgetDatabaseContext.DeleteWishlistItemAsync(wishlistItem.WishlistItemId);

        // Assert
        Assert.That(await SqlHelper.ExistsAsync(BudgetDatabaseName,
@$"SELECT 1
FROM Wishlist
WHERE
    WishlistItemId = {wishlistItem.WishlistItemId}
    AND Description = '{wishlistItem.Description}'
    AND Amount = {wishlistItem.Amount}
    AND Notes = '{wishlistItem.Notes}'"), Is.False);
    }

    [Test]
    public void DeleteWishlistItemDoesNotExist()
    {
        Assert.ThrowsAsync<ArgumentException>(async () => await BudgetDatabaseContext.DeleteWishlistItemAsync(100));
    }

    private async Task InsertWishlistItem(WishlistItem wishlistItem)
    {
        await SqlHelper.ExecuteAsync(BudgetDatabaseName,
@$"INSERT INTO Wishlist
(
    WishlistItemId,
    Description,
    Amount,
    Notes
)
VALUES
(
    {wishlistItem.WishlistItemId},
    '{wishlistItem.Description}',
    {wishlistItem.Amount},
    '{wishlistItem.Notes}'
)");
    }
}

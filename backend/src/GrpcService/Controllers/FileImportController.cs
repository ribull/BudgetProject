using System.Globalization;
using Backend.Interfaces;
using CsvHelper;
using CsvHelper.Configuration;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FileImportController : ControllerBase
{
    private readonly IBudgetDatabaseContext _budgetDatabaseContext;

    public FileImportController(IBudgetDatabaseContext budgetDatabaseContext)
    {
        _budgetDatabaseContext = budgetDatabaseContext;
    }

    [HttpPost]
    public async Task<ActionResult> ImportPurchasesFromCsv(IFormFile csvFile)
    {
        try
        {
            using (StreamReader reader = new(csvFile.OpenReadStream()))
            using (CsvReader csvReader = new(reader, new CsvConfiguration(CultureInfo.InvariantCulture) { HeaderValidated = null, MissingFieldFound = null }))
            {
                IAsyncEnumerable<Purchase> purchases = csvReader.GetRecordsAsync<Purchase>();
                await _budgetDatabaseContext.AddPurchasesAsync(purchases);

                return Ok();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return Problem($"An error occurred while attempting to add purchases: {e}");
        }
    }
}

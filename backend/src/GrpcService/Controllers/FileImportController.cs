using System.Formats.Asn1;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using Backend.Interfaces;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using static System.Net.Mime.MediaTypeNames;

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

    [HttpPost]
    public async Task<ActionResult> ImportPayHistoryFromWorkdayCsv(IFormFile csvFile)
    {
        try
        {
            using (StreamReader reader = new(csvFile.OpenReadStream()))
            using (CsvReader csvReader = new(reader, CultureInfo.InvariantCulture))
            {
                csvReader.Context.RegisterClassMap<WorkdayPayHistoryMap>();
                IAsyncEnumerable<PayHistory> payHistories = csvReader.GetRecordsAsync<PayHistory>();
                await _budgetDatabaseContext.AddPayHistoriesAsync(payHistories);

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

internal static class DateTimeExtractor
{
    public static IEnumerable<DateTime> GetDateTimesFromString(string s)
    { 
        List<DateTime> dateTimes = new();
        foreach (string potentialDate in s.Split(' '))
        {
            if (DateTime.TryParse(potentialDate, out DateTime result))
            {
                dateTimes.Add(result);
            }
        }

        return dateTimes;
    }
}

internal class PaymentPeriodStartDateConverter : DefaultTypeConverter
{
    public override object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
    {
        if (text is null)
        {
            return null;
        }

        return DateTimeExtractor.GetDateTimesFromString(text).Min();
    }
}

internal class PaymentPeriodEndDateConverter : DefaultTypeConverter
{
    public override object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
    {
        if (text is null)
        {
            return null;
        }

        return DateTimeExtractor.GetDateTimesFromString(text).Max();
    }
}

internal class DollarAmountConverter : DefaultTypeConverter
{
    public override object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
    {
        if (text is null)
        {
            return null;
        }

        if (double.TryParse(text.Trim(), NumberStyles.Currency, null, out double result))
        {
            return result;
        }

        return null;
    }
}

internal class WorkdayPayHistoryMap : ClassMap<PayHistory>
{
    public WorkdayPayHistoryMap()
    {
        Map(m => m.PayPeriodStartDate).Name("Period").TypeConverter<PaymentPeriodStartDateConverter>();
        Map(m => m.PayPeriodEndDate).Name("Period").TypeConverter<PaymentPeriodEndDateConverter>();
        Map(m => m.Earnings).Name("Earnings").TypeConverter<DollarAmountConverter>();
        Map(m => m.PreTaxDeductions).Name("Pre Tax Deductions").TypeConverter<DollarAmountConverter>();
        Map(m => m.Taxes).Name("Employee Taxes").TypeConverter<DollarAmountConverter>();
        Map(m => m.PostTaxDeductions).Name("Post Tax Deductions").TypeConverter<DollarAmountConverter>();
    }
}
namespace Domain.Models;

#nullable disable

public record PayHistory
{
    public int PayHistoryId { get; init; }

    public DateTime PayPeriodStartDate { get; init; }

    public DateTime PayPeriodEndDate { get; init; }

    public double Earnings { get; init; }

    public double PreTaxDeductions { get; init; }

    public double Taxes { get; init; }

    public double PostTaxDeductions { get; init; }

    public double NetPay { get => Earnings - PreTaxDeductions - Taxes - PostTaxDeductions; }

    public bool IsAdditionalCompensation { get => NetPay == 0.0; }
}

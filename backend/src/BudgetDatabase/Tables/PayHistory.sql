CREATE TABLE [dbo].[PayHistory]
(
	[PayHistoryId] INT IDENTITY(1, 1) NOT NULL PRIMARY KEY, 
    [PayPeriodStartDate] DATETIME NOT NULL,
    [PayPeriodEndDate] DATETIME NOT NULL,
    [Earnings] FLOAT NOT NULL,
    [PreTaxDeductions] FLOAT NOT NULL,
    [Taxes] FLOAT NOT NULL,
    [PostTaxDeductions] FLOAT NOT NULL
)

CREATE TABLE [dbo].[Purchase]
(
	[PurchaseId] INT IDENTITY(1, 1) NOT NULL PRIMARY KEY, 
    [Date] DATETIME NOT NULL, 
    [Description] NVARCHAR(200) NOT NULL, 
    [Amount] FLOAT NOT NULL, 
    [CategoryId] INT NULL, 
    CONSTRAINT [FK_CategoryPurchase] FOREIGN KEY ([CategoryId]) REFERENCES [Category]([CategoryId]) ON DELETE SET NULL
)

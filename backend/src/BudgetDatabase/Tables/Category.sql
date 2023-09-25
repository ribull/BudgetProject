CREATE TABLE [dbo].[Category]
(
	[CategoryId] INT IDENTITY(1, 1) NOT NULL PRIMARY KEY, 
    [Category] NVARCHAR(50) NOT NULL UNIQUE
)

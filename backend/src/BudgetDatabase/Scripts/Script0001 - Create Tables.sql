CREATE TABLE Category
(
	CategoryId INT GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY, 
    Category VARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE PayHistory
(
	PayHistoryId INT GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY, 
    PayPeriodStartDate DATE NOT NULL,
    PayPeriodEndDate DATE NOT NULL,
    Earnings FLOAT NOT NULL,
    PreTaxDeductions FLOAT NOT NULL,
    Taxes FLOAT NOT NULL,
    PostTaxDeductions FLOAT NOT NULL
);

CREATE TABLE Purchase
(
	PurchaseId INT GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY, 
    Date DATE NOT NULL, 
    Description VARCHAR(200) NOT NULL, 
    Amount FLOAT NOT NULL, 
    CategoryId INT NULL, 
    CONSTRAINT FK_CategoryPurchase FOREIGN KEY (CategoryId) REFERENCES Category(CategoryId) ON DELETE SET NULL
);

CREATE TABLE Era
(
    EraId INT GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    Name VARCHAR(200),
    StartDate DATE NOT NULL,
    EndDate DATE NULL
);

CREATE TABLE FuturePurchase
(
    FuturePurchaseId INT GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    Date DATE NOT NULL, 
    Description VARCHAR(200) NOT NULL,
    Amount FLOAT NOT NULL,
    CategoryId INT NULL,
    CONSTRAINT FK_CategoryFuturePurchase FOREIGN KEY (CategoryId) REFERENCES Category(CategoryId) ON DELETE SET NULL
);

CREATE TABLE Wishlist
(
    WishlistItemId INT GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    Description VARCHAR(200) NOT NULL,
    Amount FLOAT NULL,
    Notes VARCHAR(200) NOT NULL
);

CREATE TABLE Saved
(
    SavedId INT GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    Date DATE NOT NULL, 
    Description VARCHAR(200) NOT NULL,
    Amount FLOAT NOT NULL
);

CREATE TABLE Investment
(
    InvestmentId INT GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    Description VARCHAR(200) NOT NULL,
    CurrentAmount FLOAT NOT NULL,
    YearlyGrowthRate FLOAT NULL,
    LastUpdated Date NOT NULL
);
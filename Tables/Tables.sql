-- 
-- Library Management System
-- Database Creation Script
-- 


-- Create the database
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'LibrarySystemDb')
BEGIN
    CREATE DATABASE LibrarySystemDb;
END
GO

USE LibrarySystemDb;
GO

-- 
-- Create Tables
--

-- Genre
CREATE TABLE Genre (
    GenreID     INT             IDENTITY(1,1) PRIMARY KEY,
    GenreName   NVARCHAR(100)   NOT NULL
);

-- Author
CREATE TABLE Author (
    AuthorID    INT             IDENTITY(1,1) PRIMARY KEY,
    FirstName   NVARCHAR(100)   NOT NULL,
    LastName    NVARCHAR(100)   NOT NULL,
    Bio         NVARCHAR(2000)  NULL
);

-- BookTitle
CREATE TABLE BookTitle (
    BookTitleID     INT             IDENTITY(1,1) PRIMARY KEY,
    ISBN            NVARCHAR(20)    NOT NULL UNIQUE,
    AuthorID        INT             NOT NULL,
    GenreID         INT             NOT NULL,
    BookTitleName   NVARCHAR(300)   NOT NULL,

    CONSTRAINT FK_BookTitle_Author FOREIGN KEY (AuthorID)
        REFERENCES Author (AuthorID),
    CONSTRAINT FK_BookTitle_Genre  FOREIGN KEY (GenreID)
        REFERENCES Genre (GenreID)
);

-- BookCopy
CREATE TABLE BookCopy (
    BookCopyID      INT   IDENTITY(1,1) PRIMARY KEY,
    BookTitleID     INT   NOT NULL,
    PurchasedDate   DATE  NOT NULL,
    RetiredDate     DATE  NULL,

    CONSTRAINT FK_BookCopy_BookTitle FOREIGN KEY (BookTitleID)
        REFERENCES BookTitle (BookTitleID)
);

-- Condition
CREATE TABLE Condition (
    ConditionID     INT             IDENTITY(1,1) PRIMARY KEY,
    ConditionName   NVARCHAR(50)    NOT NULL UNIQUE
);

-- Members
CREATE TABLE Members (
    MemberID    INT             IDENTITY(1,1) PRIMARY KEY,
    Email       NVARCHAR(256)   NOT NULL UNIQUE,
    FirstName   NVARCHAR(100)   NOT NULL,
    LastName    NVARCHAR(100)   NOT NULL,
    PasswordHash    NVARCHAR(500)   NULL
);

-- Checkout
CREATE TABLE Checkout (
    CheckoutID  INT     IDENTITY(1,1) PRIMARY KEY,
    BookCopyID  INT     NOT NULL,
    MemberID    INT     NOT NULL,
    ConditionID INT     NOT NULL,
    CheckoutDate DATE   NOT NULL,
    DueDate      DATE   NOT NULL,
    ReturnDate   DATE   NULL,

    CONSTRAINT FK_Checkout_BookCopy  FOREIGN KEY (BookCopyID)
        REFERENCES BookCopy (BookCopyID),
    CONSTRAINT FK_Checkout_Member    FOREIGN KEY (MemberID)
        REFERENCES Members (MemberID),
    CONSTRAINT FK_Checkout_Condition FOREIGN KEY (ConditionID)
        REFERENCES Condition (ConditionID)
);

-- ===============================================
-- Expense Tracker Database Schema
-- SQLite
--================================================

PRAGMA foreign_keys = ON;

-- ===============================================
-- CATEGORIES TABLE
--================================================
CREATE TABLE Categories (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    MonthlyBudget NUMERIC NULL
);
-- ===============================================
-- EXPENSES TABLE
--================================================
CREATE TABLE Expenses (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Amount NUMERIC NOT NULL,
    Date TEXT NOT NULL,
    Description TEXT NULL,
    CategoryId INTEGER NOT NULL,

    FOREIGN KEY (CategoryId) 
        REFERENCES Categories(Id) 
        ON DELETE CASCADE
);
-- ===============================================
-- BUDGET TABLE
--================================================
CREATE TABLE Budget (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    CategoryId INTEGER NOT NULL,
    Month INTEGER NOT NULL,
    Year INTEGER NOT NULL,

    FOREIGN KEY (CategoryId) 
        REFERENCES Categories(Id) 
        ON DELETE CASCADE
    CHECK (Month >= 1 AND Month <= 12),
);
-- ===============================================
-- INCOME TABLE
--================================================
CREATE TABLE Income (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Amount NUMERIC NOT NULL,
    Source TEXT NOT NULL,
    Date TEXT NULL
);




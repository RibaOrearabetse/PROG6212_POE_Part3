-- Contract Monthly Claim System (CMCS) Database Script
-- NOTE: This script is OPTIONAL - the application will automatically create tables on startup
-- Use this only if you want to manually create the tables in SQL Server Management Studio

-- =============================================
-- Create Roles table
-- =============================================
CREATE TABLE Roles (
    RoleID INT PRIMARY KEY IDENTITY(1,1),
    RoleName NVARCHAR(50) NOT NULL
);

-- =============================================
-- Create Users table
-- =============================================
CREATE TABLE Users (
    UserID INT PRIMARY KEY IDENTITY(1,1),
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    Email NVARCHAR(100) NOT NULL,
    PasswordHash NVARCHAR(256) NOT NULL,
    ContactNumber NVARCHAR(20),
    RoleID INT NOT NULL,
    CONSTRAINT FK_Users_Roles FOREIGN KEY (RoleID) REFERENCES Roles(RoleID)
);

-- Create unique index on Email
CREATE UNIQUE INDEX IX_Users_Email ON Users(Email);

-- =============================================
-- Create Claims table
-- =============================================
CREATE TABLE Claims (
    ClaimID INT PRIMARY KEY IDENTITY(1,1),
    ClaimDate DATETIME2 NOT NULL,
    ClaimStatus NVARCHAR(50) NOT NULL DEFAULT 'Pending',
    HoursWorked DECIMAL(18,2) NOT NULL,
    HourlyRate DECIMAL(18,2) NOT NULL,
    TotalAmount DECIMAL(18,2) NOT NULL,
    SubmissionDate DATETIME2 NOT NULL,
    LastUpdated DATETIME2 NULL,
    StatusNotes NVARCHAR(MAX) NULL,
    Notes NVARCHAR(MAX) NULL,
    UserID INT NOT NULL,
    CONSTRAINT FK_Claims_Users FOREIGN KEY (UserID) REFERENCES Users(UserID)
);

-- =============================================
-- Create Approvals table
-- =============================================
CREATE TABLE Approvals (
    ApprovalID INT PRIMARY KEY IDENTITY(1,1),
    ApprovalDate DATETIME2 NOT NULL,
    Comments NVARCHAR(MAX) NOT NULL,
    ClaimID INT NOT NULL,
    ApproverID INT NOT NULL,
    CONSTRAINT FK_Approvals_Claims FOREIGN KEY (ClaimID) REFERENCES Claims(ClaimID),
    CONSTRAINT FK_Approvals_Users FOREIGN KEY (ApproverID) REFERENCES Users(UserID)
);

-- =============================================
-- Create SupportingDocuments table
-- =============================================
CREATE TABLE SupportingDocuments (
    DocumentID INT PRIMARY KEY IDENTITY(1,1),
    FileName NVARCHAR(255) NOT NULL,
    FilePath NVARCHAR(500) NOT NULL,
    FileSize BIGINT NOT NULL,
    ContentType NVARCHAR(100) NOT NULL,
    UploadDate DATETIME2 NOT NULL,
    ClaimID INT NOT NULL DEFAULT 0,
    CONSTRAINT FK_SupportingDocuments_Claims FOREIGN KEY (ClaimID) REFERENCES Claims(ClaimID)
);

-- =============================================
-- Insert sample Roles
-- =============================================
INSERT INTO Roles (RoleName)
VALUES
    ('Lecturer'),
    ('Coordinator'),
    ('Manager'),
    ('Administrator');

-- =============================================
-- Insert sample Users
-- =============================================
INSERT INTO Users (FirstName, LastName, Email, PasswordHash, ContactNumber, RoleID)
VALUES
    ('Ore', 'Roberts', 'Riba@gmail.com', 'default_password_hash', '0828761643', 2),
    ('Khumo', 'Thato', 'khumo.t@university.edu', 'default_password_hash', '098-765-4321', 2),
    ('Mike', 'Johnson', 'mike.johnson@university.edu', 'default_password_hash', '555-123-4567', 1),
    ('Sarah', 'Williams', 'sarah.williams@university.edu', 'default_password_hash', '444-987-6543', 3),
    ('User', '5', 'User5@gmail.com', 'default_password_hash', '0778889999', 4);

-- =============================================
-- Insert sample Claims
-- =============================================
INSERT INTO Claims (ClaimDate, ClaimStatus, HoursWorked, HourlyRate, TotalAmount, SubmissionDate, LastUpdated, StatusNotes, Notes, UserID)
VALUES
    ('2025-11-21', 'Approved', 4, 100.00, 400.00, GETDATE(), GETDATE(), 'Approved by coordinator/manager', NULL, 1),
    ('2025-11-16', 'Pending', 20, 500.00, 10000.00, GETDATE(), GETDATE(), 'Approved by coordinator/manager', NULL, 1),
    ('2025-12-25', 'Approved', 5, 6000.00, 30000.00, GETDATE(), GETDATE(), NULL, NULL, 1),
    ('2025-11-17', 'Approved', 10, 2000.00, 20000.00, GETDATE(), GETDATE(), 'Approved by coordinator/manager', NULL, 1),
    ('2025-11-17', 'Approved', 20, 5000.00, 100000.00, GETDATE(), GETDATE(), 'Approved by coordinator/manager', NULL, 1),
    ('2025-11-17', 'Rejected', 30, 500.00, 15000.00, GETDATE(), GETDATE(), 'Rejected by coordinator/manager', NULL, 1),
    ('2025-11-17', 'Rejected', 50, 200.00, 10000.00, GETDATE(), GETDATE(), 'Rejected by coordinator/manager', NULL, 1),
    ('2025-11-17', 'Approved', 60, 450.00, 27000.00, GETDATE(), GETDATE(), NULL, NULL, 1),
    ('2025-01-18', 'Processing', 50, 450.00, 22500.00, GETDATE(), GETDATE(), NULL, NULL, 1);

-- =============================================
-- Insert sample Approvals
-- =============================================
INSERT INTO Approvals (ApprovalDate, Comments, ClaimID, ApproverID)
VALUES
    (GETDATE(), 'Approved by coordinator/manager', 2, 1),
    (GETDATE(), 'Approved by coordinator/manager', 5, 1),
    (GETDATE(), 'Approved by coordinator/manager', 4, 1),
    (GETDATE(), 'Rejected by coordinator/manager', 6, 1),
    (GETDATE(), 'Rejected by coordinator/manager', 7, 1),
    (GETDATE(), 'Not approved', 8, 1);

-- =============================================
-- Display all tables
-- =============================================
SELECT * FROM Roles;
SELECT * FROM Users;
SELECT * FROM Claims;
SELECT * FROM Approvals;
SELECT * FROM SupportingDocuments;


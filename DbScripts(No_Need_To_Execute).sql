-- All SQL Scripts are mentioned (it has been updated throughout the course of developing the application)----
--SERVER NAME : 'NIVEDHITHA\MSSQLSERVER1'---
--DB Login : Health_Heroes, PW:Phase3----
-- Used HealthHero DB file in SQLite--

CREATE TABLE Donor_Details (
    SSN VARCHAR(11) PRIMARY KEY,               -- Unique identifier for the donor
    Name VARCHAR(255),                 -- Donor’s full name
    Age INT,                           -- Donor’s age
	Email_Address VARCHAR(255),         -- Email Address
    Blood_Type VARCHAR(3),             -- Blood group (A+, B-, etc.)
    PhoneNumber VARCHAR(15),           -- Donor's phone number
    City VARCHAR(100),                 -- Donor's city
    Illness VARCHAR(255),              -- Description of the medical history
    Height FLOAT,                      -- Height of the donor
    Weight FLOAT,                      -- Weight of the donor
    Gender CHAR(1)                     -- M or F for gender
);
ALTER TABLE Donor_Details
ADD [Email_Address] VARCHAR(255);

ALTER TABLE Donor_Details
ALTER COLUMN SSN VARCHAR(11);
DROP Table Donor_Details



CREATE TABLE [User] (
    UserId INT IDENTITY(1,1) PRIMARY KEY,  -- Auto-incremented unique identifier for the user
    Name VARCHAR(255) NOT NULL,            -- User’s full name
    Email VARCHAR(255) NOT NULL UNIQUE,    -- User’s email address, must be unique
    PasswordHash VARCHAR(255) NOT NULL     -- Password hash for security
);
-- Insert for John Doe
INSERT INTO Donor_Details (SSN, Name, Age, Blood_Type, City, Gender, PhoneNumber, Email_Address, Illness, Height, Weight)
VALUES ('123-45-6789', 'John Doe', 29, 'O+', 'Baltimore', 'M', '555-123-4567', 'john.doe@email.com', 'None', '5''10"', 75);

-- Insert for Emily Smith
INSERT INTO Donor_Details (SSN, Name, Age, Blood_Type, City, Gender, PhoneNumber, Email_Address, Illness, Height, Weight)
VALUES ('987-65-4321', 'Emily Smith', 34, 'A-', 'Rockville', 'F', '555-234-5678', 'emily.smith@email.com', 'Asthma', '5''6"', 50);

-- Insert for Michael Brown
INSERT INTO Donor_Details (SSN, Name, Age, Blood_Type, City, Gender, PhoneNumber, Email_Address, Illness, Height, Weight)
VALUES ('111-22-3333', 'Michael Brown', 42, 'B+', 'Annapolis', 'M', '555-345-6789', 'michael.brown@email.com', 'None', '6''0"', 90);

-- Insert for Sarah Johnson
INSERT INTO Donor_Details (SSN, Name, Age, Blood_Type, City, Gender, PhoneNumber, Email_Address, Illness, Height, Weight)
VALUES ('555-44-6666', 'Sarah Johnson', 27, 'AB+', 'Frederick', 'F', '555-456-7890', 'sarah.johnson@email.com', 'Diabetes', '5''4"', 65);

-- Insert for David Wilson
INSERT INTO Donor_Details (SSN, Name, Age, Blood_Type, City, Gender, PhoneNumber, Email_Address, Illness, Height, Weight)
VALUES ('222-33-4444', 'David Wilson', 38, 'O-', 'Gaithersburg', 'M', '555-567-8901', 'david.wilson@email.com', 'None', '5''11"', 72);


Select *from [User]
EXEC sp_rename 'User.PasswordHash', 'Password', 'COLUMN';
INSERT INTO [User] (Name, Email, Password)
VALUES 
('John Doe', 'john.doe@email.com', 'Jd@2024!xYz'),
('Emily Smith', 'emily.smith@email.com', 'Em!s1234#Ab');

Select *From Donor_Details
INSERT INTO Donor_Details (SSN, Name, Age, Blood_Type, City, Gender, PhoneNumber, Email_Address, Illness, Height, Weight)
VALUES ('987-65-4321', 'Emily Smith', 34, 'A-', 'Rockville', 'F', '555-234-5678', 'emily.smith@email.com', 'Asthma', '5.6', 50);

-- Insert for Michael Brown
INSERT INTO Donor_Details (SSN, Name, Age, Blood_Type, City, Gender, PhoneNumber, Email_Address, Illness, Height, Weight)
VALUES ('111-22-3333', 'Michael Brown', 42, 'B+', 'Annapolis', 'M', '555-345-6789', 'michael.brown@email.com', 'None', '6.0', 90);

-- Insert for Sarah Johnson
INSERT INTO Donor_Details (SSN, Name, Age, Blood_Type, City, Gender, PhoneNumber, Email_Address, Illness, Height, Weight)
VALUES ('555-44-6666', 'Sarah Johnson', 27, 'AB+', 'Frederick', 'F', '555-456-7890', 'sarah.johnson@email.com', 'Diabetes', '5.4', 65);

-- Insert for David Wilson
INSERT INTO Donor_Details (SSN, Name, Age, Blood_Type, City, Gender, PhoneNumber, Email_Address, Illness, Height, Weight)
VALUES ('222-33-4444', 'David Wilson', 38, 'O-', 'Gaithersburg', 'M', '555-567-8901', 'david.wilson@email.com', 'None', '5.11', 72);

INSERT INTO Donor_Details (SSN, Name, Age, Blood_Type, City, Gender, PhoneNumber, Email_Address, Illness, Height, Weight)
VALUES 
('333-22-1111', 'Robert King', 45, 'B-', 'Laurel', 'M', '555-789-0123', 'robert.king@email.com', 'None', '7.0', 85);

INSERT INTO Donor_Details (SSN, Name, Age, Blood_Type, City, Gender, PhoneNumber, Email_Address, Illness, Height, Weight)
VALUES 
('444-55-6666', 'Olivia Harris', 29, 'O+', 'College Park', 'F', '555-890-1234', 'olivia.harris@email.com', 'Thyroid', '6.3', 60);

INSERT INTO Donor_Details (SSN, Name, Age, Blood_Type, City, Gender, PhoneNumber, Email_Address, Illness, Height, Weight)
VALUES 
('999-88-7777', 'Liam Martinez', 50, 'AB-', 'Annapolis', 'M', '555-901-2345', 'liam.martinez@email.com', 'None', '6.9', 80);

ALTER TABLE Donor_Details
ALTER COLUMN Name VARCHAR(255) NOT NULL;

ALTER TABLE Donor_Details
ALTER COLUMN Age INT NOT NULL;

ALTER TABLE Donor_Details
ALTER COLUMN Email_Address VARCHAR(255) NOT NULL;

ALTER TABLE Donor_Details
ALTER COLUMN Blood_Type VARCHAR(3) NOT NULL;

ALTER TABLE Donor_Details
ALTER COLUMN PhoneNumber VARCHAR(15) NOT NULL;

ALTER TABLE Donor_Details
ALTER COLUMN City VARCHAR(100) NOT NULL;


ALTER TABLE Donor_Details
ALTER COLUMN Height FLOAT NOT NULL;

ALTER TABLE Donor_Details
ALTER COLUMN Weight FLOAT NOT NULL;

ALTER TABLE Donor_Details
ALTER COLUMN Gender CHAR(1) NOT NULL;

ALTER TABLE Donor_Details DROP CONSTRAINT PK__Donor_De__CA1E8E3DA52A8D04;  -- Drop the primary key constraint
ALTER TABLE Donor_Details ADD CONSTRAINT PK_Donor_Details_Email PRIMARY KEY (Email_Address);

SELECT 
    tc.CONSTRAINT_NAME
FROM 
    INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS tc
WHERE 
    tc.TABLE_NAME = 'Donor_Details'
    AND tc.CONSTRAINT_TYPE = 'PRIMARY KEY';


-- Step 1: Drop the UserId column
ALTER TABLE [User]
DROP COLUMN UserId;

-- Step 2: Rename Email to Email_Address
EXEC sp_rename 'User.Email', 'Email_Address', 'COLUMN';

-- Step 3: Make Email_Address the primary key
ALTER TABLE [User]
ADD CONSTRAINT PK_User_Email PRIMARY KEY (Email_Address);

-- Step 4: Ensure all columns are NOT NULL (if they aren't already)
ALTER TABLE [User]
ALTER COLUMN Name VARCHAR(255) NOT NULL;

ALTER TABLE [User]
ALTER COLUMN Email_Address VARCHAR(255) NOT NULL;

ALTER TABLE [User]
ALTER COLUMN Password VARCHAR(255) NOT NULL;


SELECT 
    OBJECT_NAME(constraint_object_id) AS ConstraintName,
    OBJECT_NAME(parent_object_id) AS TableName
FROM 
    sys.foreign_key_columns
WHERE 
    parent_column_id = (
        SELECT column_id 
        FROM sys.columns 
        WHERE name = 'UserId' 
        AND object_id = OBJECT_ID('User')
    );




	SELECT 
    i.name AS IndexName,
    c.name AS ColumnName
FROM 
    sys.indexes i
INNER JOIN 
    sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
INNER JOIN 
    sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
WHERE 
    i.object_id = OBJECT_ID('User') AND c.name = 'UserId';


DROP INDEX [PK__User__1788CC4C40D75117] ON [User];



-- Step 1: Drop the primary key constraint
ALTER TABLE [User] DROP CONSTRAINT [PK__User__1788CC4C4C40D75117];

-- Step 2: Drop the UserId column
ALTER TABLE [User] DROP COLUMN UserId;

-- Step 3: Rename Email to Email_Address
EXEC sp_rename 'User.Email', 'Email_Address', 'COLUMN';

-- Step 4: Set Email_Address as the primary key
ALTER TABLE [User]
ADD CONSTRAINT PK_User_Email PRIMARY KEY (Email_Address);

-- Step 5: Ensure all columns are NOT NULL
ALTER TABLE [User]
ALTER COLUMN Name VARCHAR(255) NOT NULL;

ALTER TABLE [User]
ALTER COLUMN Email_Address VARCHAR(255) NOT NULL;

ALTER TABLE [User]
ALTER COLUMN Password VARCHAR(255) NOT NULL;

DROP TABLE IF EXISTS [User];

CREATE TABLE [User] (
    Email_Address VARCHAR(255) NOT NULL PRIMARY KEY,
    Name VARCHAR(255) NOT NULL,
    Password VARCHAR(255) NOT NULL
);
INSERT INTO [User] (Name, Email_Address, Password)
VALUES 
('John Doe', 'john.doe@email.com', 'Jd@2024!xYz'),
('Emily Smith', 'emily.smith@email.com', 'Em!s1234#Ab');

-- Ensure SSN, Name, and Email_Address remain NOT NULL
ALTER TABLE Donor_Details
ALTER COLUMN SSN VARCHAR(11) NOT NULL;

ALTER TABLE Donor_Details
ALTER COLUMN Name VARCHAR(255) NOT NULL;

ALTER TABLE Donor_Details
ALTER COLUMN Email_Address VARCHAR(255) NOT NULL;

-- Set the remaining columns to allow NULL values
ALTER TABLE Donor_Details
ALTER COLUMN Age INT NULL;

ALTER TABLE Donor_Details
ALTER COLUMN Blood_Type VARCHAR(3) NULL;

ALTER TABLE Donor_Details
ALTER COLUMN PhoneNumber VARCHAR(15) NULL;

ALTER TABLE Donor_Details
ALTER COLUMN City VARCHAR(100) NULL;

ALTER TABLE Donor_Details
ALTER COLUMN Illness VARCHAR(255) NULL;

ALTER TABLE Donor_Details
ALTER COLUMN Height FLOAT NULL;

ALTER TABLE Donor_Details
ALTER COLUMN Weight FLOAT NULL;

ALTER TABLE Donor_Details
ALTER COLUMN Gender CHAR(1) NULL;

Select *from [User]
Select *from Donor_Details




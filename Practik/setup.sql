-- Create Database
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'workshop_db')
BEGIN
    CREATE DATABASE workshop_db;
END;
GO

USE workshop_db;
GO

-- Create Tables

-- UserRoles Table
CREATE TABLE UserRoles (
    RoleId INT PRIMARY KEY IDENTITY,
    Code NVARCHAR(50) NOT NULL UNIQUE,
    Name NVARCHAR(100) NOT NULL
);

-- Users Table
CREATE TABLE Users (
    UserId INT PRIMARY KEY IDENTITY,
    Username NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    RoleId INT NOT NULL,
    DateCreated DATETIME NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (RoleId) REFERENCES UserRoles(RoleId)
);

-- Clients Table
CREATE TABLE Clients (
    ClientId INT PRIMARY KEY IDENTITY,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    PhoneNumber NVARCHAR(50) NOT NULL,
    Email NVARCHAR(100)
);

-- DeviceTypes Table
CREATE TABLE DeviceTypes (
    DeviceTypeId INT PRIMARY KEY IDENTITY,
    Name NVARCHAR(100) NOT NULL UNIQUE
);

-- Devices Table
CREATE TABLE Devices (
    DeviceId INT PRIMARY KEY IDENTITY,
    ClientId INT NOT NULL,
    DeviceTypeId INT NOT NULL,
    Manufacturer NVARCHAR(100) NOT NULL,
    Model NVARCHAR(100) NOT NULL,
    SerialNumber NVARCHAR(100) NOT NULL,
    FOREIGN KEY (ClientId) REFERENCES Clients(ClientId),
    FOREIGN KEY (DeviceTypeId) REFERENCES DeviceTypes(DeviceTypeId)
);

-- RepairOrderStatuses Table
CREATE TABLE RepairOrderStatuses (
    StatusId INT PRIMARY KEY IDENTITY,
    Code NVARCHAR(50) NOT NULL UNIQUE,
    Name NVARCHAR(100) NOT NULL
);

-- RepairOrders Table
CREATE TABLE RepairOrders (
    OrderId INT PRIMARY KEY IDENTITY,
    DeviceId INT NOT NULL,
    CreatedByUserId INT NOT NULL,
    CurrentStatusId INT NOT NULL,
    ProblemDescription NVARCHAR(MAX) NOT NULL,
    DateCreated DATETIME NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (DeviceId) REFERENCES Devices(DeviceId),
    FOREIGN KEY (CreatedByUserId) REFERENCES Users(UserId),
    FOREIGN KEY (CurrentStatusId) REFERENCES RepairOrderStatuses(StatusId)
);

-- RepairOrderAssignments Table
CREATE TABLE RepairOrderAssignments (
    AssignmentId INT PRIMARY KEY IDENTITY,
    OrderId INT NOT NULL UNIQUE, -- Assuming one technician per order
    UserId INT NOT NULL,
    DateAssigned DATETIME NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (OrderId) REFERENCES RepairOrders(OrderId),
    FOREIGN KEY (UserId) REFERENCES Users(UserId)
);

-- RepairOrderStatusHistory Table
CREATE TABLE RepairOrderStatusHistory (
    HistoryId INT PRIMARY KEY IDENTITY,
    OrderId INT NOT NULL,
    StatusId INT NOT NULL,
    UserId INT NOT NULL,
    DateChanged DATETIME NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (OrderId) REFERENCES RepairOrders(OrderId),
    FOREIGN KEY (StatusId) REFERENCES RepairOrderStatuses(StatusId),
    FOREIGN KEY (UserId) REFERENCES Users(UserId)
);

-- WorkActs Table
CREATE TABLE WorkActs (
    ActId INT PRIMARY KEY IDENTITY,
    OrderId INT NOT NULL,
    Description NVARCHAR(MAX) NOT NULL,
    DateCreated DATETIME NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (OrderId) REFERENCES RepairOrders(OrderId)
);

-- AuditLog Table
CREATE TABLE AuditLog (
    LogId INT PRIMARY KEY IDENTITY,
    UserId INT NOT NULL,
    ActionType NVARCHAR(100) NOT NULL,
    Details NVARCHAR(MAX),
    DateCreated DATETIME NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(UserId)
);

-- Seed Data

-- Seed UserRoles
INSERT INTO UserRoles (Code, Name) VALUES
('ADMIN', 'Administrator'),
('MASTER', 'Master'),
('RECEPTIONIST', 'Receptionist');

-- Seed RepairOrderStatuses
INSERT INTO RepairOrderStatuses (Code, Name) VALUES
('NEW', 'New'),
('IN_PROGRESS', 'In Progress'),
('COMPLETED', 'Completed'),
('CANCELLED', 'Cancelled');

-- Seed a default admin user (password: admin)
INSERT INTO Users (Username, PasswordHash, FirstName, LastName, RoleId)
VALUES ('admin', '8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918', 'Admin', 'User', (SELECT RoleId FROM UserRoles WHERE Code = 'ADMIN'));

GO

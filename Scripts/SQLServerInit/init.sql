USE master;
GO
    
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'AppDb')
BEGIN
    CREATE DATABASE AppDb;
END
GO

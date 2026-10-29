-- Crea UsuariosDb, la tabla Usuarios y el usuario admin si todavía no existen.
-- Password guarda el hash BCrypt de Admin123!.

IF DB_ID(N'UsuariosDb') IS NULL
BEGIN
    CREATE DATABASE UsuariosDb;
END
GO

USE UsuariosDb;
GO

IF OBJECT_ID(N'dbo.Usuarios', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Usuarios (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Usuarios PRIMARY KEY,
        Username NVARCHAR(50) NOT NULL,
        Email NVARCHAR(150) NOT NULL,
        Password NVARCHAR(255) NOT NULL,
        Estado BIT NOT NULL CONSTRAINT DF_Usuarios_Estado DEFAULT (1),
        FechaCreacion DATETIME2 NOT NULL CONSTRAINT DF_Usuarios_FechaCreacion DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT UQ_Usuarios_Username UNIQUE (Username),
        CONSTRAINT UQ_Usuarios_Email UNIQUE (Email)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Usuarios WHERE Username = N'admin')
BEGIN
    INSERT INTO dbo.Usuarios (Username, Email, Password, Estado, FechaCreacion)
    VALUES (
        N'admin',
        N'admin@demo.local',
        N'$2b$11$t.yQEcD0njME56zriYN56eYXSrGEe4p6CaLSwXtBEjG.DkJoJp6tm',
        1,
        SYSUTCDATETIME()
    );
END
GO

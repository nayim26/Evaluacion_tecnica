-- SQLite (desarrollo local). La API lo aplica al arrancar.
-- Usuario demo: admin / Admin123!

CREATE TABLE IF NOT EXISTS Usuarios (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Username TEXT NOT NULL UNIQUE,
    Email TEXT NOT NULL UNIQUE,
    Password TEXT NOT NULL,
    Estado INTEGER NOT NULL DEFAULT 1,
    FechaCreacion TEXT NOT NULL
);

INSERT INTO Usuarios (Username, Email, Password, Estado, FechaCreacion)
SELECT
    'admin',
    'admin@demo.local',
    '$2b$11$t.yQEcD0njME56zriYN56eYXSrGEe4p6CaLSwXtBEjG.DkJoJp6tm',
    1,
    strftime('%Y-%m-%dT%H:%M:%SZ', 'now')
WHERE NOT EXISTS (
    SELECT 1 FROM Usuarios WHERE lower(Username) = 'admin'
);

-- ================================
-- Create User-defined Table Type
-- ================================
USE Indigo
GO

-- Create the data type
CREATE TYPE dbo.TTCotizar AS TABLE 
(
	IdPersona int NOT NULL, 
	IdProducto int NOT NULL
    PRIMARY KEY CLUSTERED 
(
	IdPersona, IdProducto ASC
)WITH (IGNORE_DUP_KEY = OFF)
)
GO

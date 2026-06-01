-- Create one database per microservice (SRS 4.1 data ownership)
-- Run against master after SQL Server is ready.

IF DB_ID(N'Nexus_User') IS NULL
    CREATE DATABASE [Nexus_User];
GO

IF DB_ID(N'Nexus_Catalog') IS NULL
    CREATE DATABASE [Nexus_Catalog];
GO

IF DB_ID(N'Nexus_Commerce') IS NULL
    CREATE DATABASE [Nexus_Commerce];
GO

IF DB_ID(N'Nexus_Auction') IS NULL
    CREATE DATABASE [Nexus_Auction];
GO

IF DB_ID(N'Nexus_Fulfillment') IS NULL
    CREATE DATABASE [Nexus_Fulfillment];
GO

IF DB_ID(N'Nexus_Notification') IS NULL
    CREATE DATABASE [Nexus_Notification];
GO

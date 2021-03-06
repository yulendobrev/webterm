IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'__MigrationHistory')
BEGIN
	CREATE TABLE __MigrationHistory
	(
		MigrationId nvarchar(150) NOT NULL,
		ContextKey nvarchar(300) NOT NULL,
		Model varbinary(max) NOT NULL,
		ProductVersion nvarchar(32) NOT NULL,
		CONSTRAINT [PK_dbo.__MigrationHistory] PRIMARY KEY (MigrationId, ContextKey)
	)
END

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'AspNetUsers')
BEGIN
	CREATE TABLE AspNetUsers
	(
		Id nvarchar(128) NOT NULL,
		Email nvarchar(256) NULL,
		EmailConfirmed bit NOT NULL,
		PasswordHash nvarchar(max) NULL,
		SecurityStamp nvarchar(max) NULL,
		PhoneNumber nvarchar(max) NULL,
		PhoneNumberConfirmed bit NOT NULL,
		TwoFactorEnabled bit NOT NULL,
		LockoutEndDateUtc datetime NULL,
		LockoutEnabled bit NOT NULL,
		AccessFailedCount int NOT NULL,
		UserName nvarchar(256) NOT NULL,
		CONSTRAINT [PK_dbo.AspNetUsers] PRIMARY KEY (Id)
	)
END

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'AspNetRoles')
BEGIN
	CREATE TABLE AspNetRoles
	(
		Id nvarchar(128) NOT NULL,
		Name nvarchar(256) NOT NULL,
		CONSTRAINT [PK_dbo.AspNetRoles] PRIMARY KEY (Id)
	)
END

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'AspNetUserClaims')
BEGIN
	CREATE TABLE AspNetUserClaims
	(
		Id int IDENTITY(1,1) NOT NULL,
		UserId nvarchar(128) NOT NULL,
		ClaimType nvarchar(max) NULL,
		ClaimValue nvarchar(max) NULL,
		CONSTRAINT [PK_dbo.AspNetUserClaims] PRIMARY KEY (Id),
		CONSTRAINT [FK_dbo.AspNetUserClaims_dbo.AspNetUsers_UserId] FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE
	)
END

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'AspNetUserLogins')
BEGIN
	CREATE TABLE AspNetUserLogins
	(
		LoginProvider nvarchar(128) NOT NULL,
		ProviderKey nvarchar(128) NOT NULL,
		UserId nvarchar(128) NOT NULL,
		CONSTRAINT [PK_dbo.AspNetUserLogins] PRIMARY KEY (LoginProvider, ProviderKey, UserId),
		CONSTRAINT [FK_dbo.AspNetUserLogins_dbo.AspNetUsers_UserId] FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE
	)
END

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'AspNetUserRoles')
BEGIN
	CREATE TABLE AspNetUserRoles
	(
		UserId nvarchar(128) NOT NULL,
		RoleId nvarchar(128) NOT NULL,
		CONSTRAINT [PK_dbo.AspNetUserRoles] PRIMARY KEY (UserId, RoleId),
		CONSTRAINT [FK_dbo.AspNetUserRoles_dbo.AspNetRoles_RoleId] FOREIGN KEY (RoleId) REFERENCES AspNetRoles(Id) ON DELETE CASCADE,
		CONSTRAINT [FK_dbo.AspNetUserRoles_dbo.AspNetUsers_UserId] FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE
	)
END

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'ExecutionMachineLoads')
BEGIN
	CREATE TABLE ExecutionMachineLoads
	(
		[Address] nvarchar(128) NOT NULL,
		VirtualMachineCount int NOT NULL,
		CONSTRAINT [PK_dbo.ExecutionMachineLoads] PRIMARY KEY ([Address])
	)
END

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'VmNodeEntries')
BEGIN
	CREATE TABLE VmNodeEntries
	(
		Id int IDENTITY(1, 1) NOT NULL,
		Name nvarchar(max) NULL,
		[User] nvarchar(max) NOT NULL,
		HostMachine nvarchar(max) NOT NULL,
		VmNodeId int NOT NULL,
		StartedOn datetime NOT NULL,
		CONSTRAINT [PK_dbo.VmNodeEntries] PRIMARY KEY (Id)
	)
END


IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'AspNetRoles') AND name = N'RoleNameIndex')
CREATE UNIQUE NONCLUSTERED INDEX RoleNameIndex ON AspNetRoles
(
	Name ASC
)

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'AspNetUserClaims') AND name = N'IX_UserId')
CREATE NONCLUSTERED INDEX IX_UserId ON AspNetUserClaims
(
	UserId ASC
)

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'AspNetUserLogins') AND name = N'IX_UserId')
CREATE NONCLUSTERED INDEX IX_UserId ON AspNetUserLogins
(
	UserId ASC
)

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'AspNetUserRoles') AND name = N'IX_RoleId')
CREATE NONCLUSTERED INDEX IX_RoleId ON AspNetUserRoles
(
	RoleId ASC
)

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'AspNetUserRoles') AND name = N'IX_UserId')
CREATE NONCLUSTERED INDEX IX_UserId ON AspNetUserRoles
(
	UserId ASC
)

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'AspNetUsers') AND name = N'UserNameIndex')
CREATE UNIQUE NONCLUSTERED INDEX UserNameIndex ON AspNetUsers
(
	UserName ASC
)


CREATE TABLE [dbo].[Owner]
(
	[OwnerId] CHAR(36) NOT NULL PRIMARY KEY,
	[Name] NVARCHAR(60),
	DateOfBirth Date,
	Address NVARCHAR(100)
)

GO

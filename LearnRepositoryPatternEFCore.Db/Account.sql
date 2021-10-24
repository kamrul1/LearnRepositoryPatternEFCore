CREATE TABLE [dbo].[Account]
(
	[AccountId] CHAR(36) NOT NULL PRIMARY KEY,
	DateCreated Date,
	AccountType NVARCHAR(45),
	[OwnerId] CHAR(36),
	CONSTRAINT [FK_Owner_Account] FOREIGN KEY (OwnerId) REFERENCES [Owner]([OwnerId]) 
		ON UPDATE CASCADE ON DELETE CASCADE 
)

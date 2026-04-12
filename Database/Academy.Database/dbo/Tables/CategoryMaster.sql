CREATE TABLE [dbo].[CategoryMaster]
(
	CategoryId SMALLINT NOT NULL IDENTITY,
	CategoryName VARCHAR(255) NOT NULL,
	ParentCategoryId SMALLINT NULL, 
	[IsActive] BIT NOT NULL DEFAULT 1,
	[CreatedBy] INT NOT NULL, 
	[CreatedOn] DATETIME2(0) NOT NULL, 
	[UpdatedBy] INT NULL, 
	[UpdatedOn] DATETIME2(0) NULL, 
    CONSTRAINT [PK_CategoryMaster] PRIMARY KEY ([CategoryId])
)

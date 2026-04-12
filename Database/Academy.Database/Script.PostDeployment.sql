/*
Post-Deployment Script Template							
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be appended to the build script.		
 Use SQLCMD syntax to include a file in the post-deployment script.			
 Example:      :r .\myfile.sql								
 Use SQLCMD syntax to reference a variable in the post-deployment script.		
 Example:      :setvar TableName MyTable							
               SELECT * FROM [$(TableName)]					
--------------------------------------------------------------------------------------
*/
:r ".\MasterDataScripts\ProficiencyMasterInsert.sql"
:r ".\MasterDataScripts\KnowledgeMasterInsert.sql"
:r ".\MasterDataScripts\RoleMasterInsert.sql"
:r ".\MasterDataScripts\SeniorityMasterInsert.sql"
:r ".\MasterDataScripts\TrainingStatusMasterInsert.sql"
:r ".\MasterDataScripts\EcosystemMasterInsert.sql"
:r ".\MasterDataScripts\ActivityMasterInsert.sql"
:r ".\MasterDataScripts\ReportTypeMasterInsert.sql"
:r ".\MasterDataScripts\ReportColumnConfigurationMasterInsert.sql"
:r ".\MasterDataScripts\CountryLocationInsert.sql"
:r ".\MasterDataScripts\GoogleSheetConfigurationInsert.sql"

:r ".\MigrationScripts\release_v1.0\Academy_1.0.sql"
:r ".\MigrationScripts\release_v1.0\Academy_1.1.sql"
:r ".\MigrationScripts\release_v1.0\Academy_1.2.sql"
:r ".\MigrationScripts\release_v1.0\Academy_1.3.sql"
:r ".\MigrationScripts\release_v1.0\Academy_1.4.sql"
:r ".\MigrationScripts\release_v1.0\Academy_1.5.sql"
:r ".\MigrationScripts\release_v1.0\Academy_1.6.sql"
:r ".\MigrationScripts\release_v1.0\Academy_1.7.sql"
:r ".\MigrationScripts\release_v1.0\Academy_1.8.sql"
:r ".\MigrationScripts\release_v1.0\Academy_1.9.sql"
:r ".\MigrationScripts\release_v1.0\Academy_1.10.sql"
:r ".\MigrationScripts\release_v1.0\Academy_1.11.sql"
:r ".\MigrationScripts\release_v1.0\Academy_1.12.sql"
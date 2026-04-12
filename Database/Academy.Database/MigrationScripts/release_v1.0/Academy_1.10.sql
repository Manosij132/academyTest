DECLARE @scriptName VARCHAR(255) = 'Academy_1.10.sql';
DECLARE @reqVersion VARCHAR(20) = '1.9';
DECLARE @newVersion VARCHAR(20) = '1.10';

-- Script Body/Content
BEGIN
	PRINT 'CURRENT DB VERSION: ' + sysdata.GetDbVersion();

	IF (sysdata.IsDbVersionApplied(@reqVersion) = 1
			AND sysdata.IsDbVersionApplied(@newVersion) = 0)
	BEGIN
		BEGIN TRY
			BEGIN TRANSACTION;
			
			INSERT INTO dbo.AiStudioClientMap
			(AiStudioName, Client, IsActive, CreatedBy, CreatedOn)
			SELECT AiStudioName, Client, IsActive, CreatedBy, CreatedOn
			FROM
			(
				VALUES
				('CPG, Retail & Automotive', 'GAP', 1, 0, GETUTCDATE()),
				('CPG, Retail & Automotive', 'Levis',  1, 0, GETUTCDATE()),
				('CPG, Retail & Automotive', 'Valmont Industries Inc.',  1, 0, GETUTCDATE()),
				('CPG, Retail & Automotive', 'McCormick - gA', 1, 0, GETUTCDATE()),
				('CPG, Retail & Automotive', 'Corteva Agriscience',  1, 0, GETUTCDATE()),
				('CPG, Retail & Automotive', 'Pepsico US', 1, 0, GETUTCDATE()),
				('CPG, Retail & Automotive', 'Beam Suntory US', 1, 0, GETUTCDATE()),
				('CPG, Retail & Automotive', 'Louis Dreyfus Company', 1, 0, GETUTCDATE()),
				('CPG, Retail & Automotive', 'Jacobs Douwe Egberts International B.V', 1, 0, GETUTCDATE()),
				('CPG, Retail & Automotive', 'Foot Locker', 1, 0, GETUTCDATE()),
				('CPG, Retail & Automotive', 'Falabella TECNOLOGIA', 1, 0, GETUTCDATE()),
				('CPG, Retail & Automotive', 'Adidas',  1, 0, GETUTCDATE()),
				('CPG, Retail & Automotive', 'Paccar', 1, 0, GETUTCDATE()),
				('CPG, Retail & Automotive', 'Dick-s Sporting Goods', 1, 0, GETUTCDATE()),
				('Energy & Telco', 'Cellnex', 1, 0, GETUTCDATE()),
				('Energy & Telco', 'Kent Plc', 1, 0, GETUTCDATE()),
				('Financial Services', 'Convera', 1, 0, GETUTCDATE()),
				('Financial Services', 'Fiserv Solutions, LLC', 1, 0, GETUTCDATE()),
				('Financial Services', 'Alpha Bank', 1, 0, GETUTCDATE()),
				('Financial Services', 'Thredd', 1, 0, GETUTCDATE()),
				('Financial Services', 'Empower', 1, 0, GETUTCDATE()),
				('Financial Services', 'Tradition Broker', 1, 0, GETUTCDATE()),
				('Financial Services', 'JPMorgan', 1, 0, GETUTCDATE()),
				('Financial Services', 'AIB',1, 0, GETUTCDATE()),
				('Financial Services', 'Guggenheim Partners', 1, 0, GETUTCDATE()),
				('Financial Services', 'ALLIANZ TECHNOLOGY SPA', 1, 0, GETUTCDATE()),
				('Financial Services', 'Salt Bank', 1, 0, GETUTCDATE()),
				('Financial Services', 'AkBank', 1, 0, GETUTCDATE()),
				('Financial Services', 'Portfolio FS Europe', 1, 0, GETUTCDATE()),
				('Financial Services', 'Temenos (Avoka) - AVX', 1, 0, GETUTCDATE()),
				('Financial Services', 'Worldpay_Blankfactor',1, 0, GETUTCDATE()),
				('Financial Services', 'Nuvei', 1, 0, GETUTCDATE()),
				('Financial Services', 'Finastra', 1, 0, GETUTCDATE()),
				('Financial Services', 'American Century Services', 1, 0, GETUTCDATE()),
				('Financial Services', 'AON NA', 1, 0, GETUTCDATE()),
				('Financial Services', 'Nedbank', 1, 0, GETUTCDATE()),
				('Financial Services', 'West Technology Group', 1, 0, GETUTCDATE()),
				('Financial Services', 'HSBC Bank plc', 1, 0, GETUTCDATE()),
				('Financial Services', 'FactSet Research Systems Inc', 1, 0, GETUTCDATE()),
				('Financial Services', 'TTT Moneycorp Limited', 1, 0, GETUTCDATE()),
				('Financial Services', 'Banco de Sabadell', 1, 0, GETUTCDATE()),
				('Financial Services', 'Citibanamex', 1, 0, GETUTCDATE()),
				('Financial Services', 'IDB',  1, 0, GETUTCDATE()),
				('Financial Services', 'HealthEquity, Inc.', 1, 0, GETUTCDATE()),
				('Gaming & Ed. Tech', 'American Public Education', 1, 0, GETUTCDATE()), 
				('Gaming & Ed. Tech', 'McGraw Hill', 1, 0, GETUTCDATE()), 
				('Gaming & Ed. Tech', 'Riot Games', 1, 0, GETUTCDATE()),
				('Gaming & Ed. Tech', 'Zynga Game Network', 1, 0, GETUTCDATE()),
				('Healthcare & Life Sciences & Private Equity', 'Artivion', 1, 0, GETUTCDATE()), 
				('Healthcare & Life Sciences & Private Equity', 'Baker McKenzie', 1, 0, GETUTCDATE()),
				('Healthcare & Life Sciences & Private Equity', 'Concord Group, Inc.', 1, 0, GETUTCDATE()),
				('Healthcare & Life Sciences & Private Equity', 'Cordis', 1, 0, GETUTCDATE()),
				('Healthcare & Life Sciences & Private Equity', 'DLA Piper', 1, 0, GETUTCDATE()),
				('Healthcare & Life Sciences & Private Equity', 'Elevance Health', 1, 0, GETUTCDATE()),
				('Healthcare & Life Sciences & Private Equity', 'Employbridge Holding Company', 1, 0, GETUTCDATE()),
				('Healthcare & Life Sciences & Private Equity', 'Fragomen', 1, 0, GETUTCDATE()), 
				('Healthcare & Life Sciences & Private Equity', 'GetInsured', 1, 0, GETUTCDATE()), 
				('Healthcare & Life Sciences & Private Equity', 'J&J KenVue', 1, 0, GETUTCDATE()), 
				('Healthcare & Life Sciences & Private Equity', 'J&J MD&D EUROPE', 1, 0, GETUTCDATE()), 
				('Healthcare & Life Sciences & Private Equity', 'Johnson & Johnson', 1, 0, GETUTCDATE()), 
				('Healthcare & Life Sciences & Private Equity', 'Medifast', 1, 0, GETUTCDATE()), 
				('Healthcare & Life Sciences & Private Equity', 'Medline', 1, 0, GETUTCDATE()), 
				('Healthcare & Life Sciences & Private Equity', 'Medtronic - gA', 1, 0, GETUTCDATE()), 
				('Healthcare & Life Sciences & Private Equity', 'Novartis Pharma AG', 1, 0, GETUTCDATE()),
				('Healthcare & Life Sciences & Private Equity', 'PhactMI', 1, 0, GETUTCDATE()), 
				('Healthcare & Life Sciences & Private Equity', 'Roche', 1, 0, GETUTCDATE()), 
				('Healthcare & Life Sciences & Private Equity', 'Slimming World', 1, 0, GETUTCDATE()), 
				('Healthcare & Life Sciences & Private Equity', 'Stryker Corporation', 1, 0, GETUTCDATE()),
				('Media, Entertainment, Travel & Leisure', 'Carnival Corporation & PLC', 1, 0, GETUTCDATE()), 
				('Media, Entertainment, Travel & Leisure', 'COMCAST - Universal City Studios LLC', 1, 0, GETUTCDATE()),
				('Media, Entertainment, Travel & Leisure', 'Disney Entertainment', 1, 0, GETUTCDATE()), 
				('Media, Entertainment, Travel & Leisure', 'Disney Parks', 1, 0, GETUTCDATE()), 
				('Media, Entertainment, Travel & Leisure', 'Formula One', 1, 0, GETUTCDATE()), 
				('Media, Entertainment, Travel & Leisure', 'Holiday Inn Club Vacations X', 1, 0, GETUTCDATE()),
				('Media, Entertainment, Travel & Leisure', 'InterContinental Hotels Group', 1, 0, GETUTCDATE()), 
				('Media, Entertainment, Travel & Leisure', 'KANTAR', 1, 0, GETUTCDATE()), 
				('Media, Entertainment, Travel & Leisure', 'MGM Resorts', 1, 0, GETUTCDATE()), 
				('Media, Entertainment, Travel & Leisure', 'OBT Live', 1, 0, GETUTCDATE()), 
				('Media, Entertainment, Travel & Leisure', 'Portfolio Sportian', 1, 0, GETUTCDATE()), 
				('Media, Entertainment, Travel & Leisure', 'Princess Cruise Lines', 1, 0, GETUTCDATE()), 
				('Media, Entertainment, Travel & Leisure', 'Royal Caribbean', 1, 0, GETUTCDATE()),
				('Media, Entertainment, Travel & Leisure', 'WarnerBros Discovery', 1, 0, GETUTCDATE()),
				('Media, Entertainment, Travel & Leisure', 'Which', 1, 0, GETUTCDATE()),
				('N/A', 'Globant', 1, 0, GETUTCDATE()),
				('New Markets', 'Amazon Web Services EMEA SARL - Abu Dhabi', 1, 0, GETUTCDATE()), 
				('New Markets', 'Commonwealth Bank of Australia', 1, 0, GETUTCDATE()), 
				('New Markets', 'Diriyah Company', 1, 0, GETUTCDATE()), 
				('New Markets', 'Majid Al Futtaim', 1, 0, GETUTCDATE()), 
				('New Markets', 'Mawsons Concrete', 1, 0, GETUTCDATE()), 
				('New Markets', 'Myer', 1, 0, GETUTCDATE()), 
				('New Markets', 'Qiddiya', 1, 0, GETUTCDATE()), 
				('New Markets', 'Red Sea', 1, 0, GETUTCDATE()), 
				('New Markets', 'Region - New Markets', 1, 0, GETUTCDATE()), 
				('New Markets', 'Saudi 2027', 1, 0, GETUTCDATE()), 
				('New Markets', 'Saudi Tourism Authority', 1, 0, GETUTCDATE()),
				('New Markets', 'Elite Group Holding Limited', 1, 0, GETUTCDATE()),
				('Professional Services & High Tech', 'Adobe Systems Incorporated', 1, 0, GETUTCDATE()), 
				('Professional Services & High Tech', 'Autodesk', 1, 0, GETUTCDATE()),
				('Professional Services & High Tech', 'Avayler', 1, 0, GETUTCDATE()), 
				('Professional Services & High Tech', 'BDO Global', 1, 0, GETUTCDATE()), 
				('Professional Services & High Tech', 'Dell EMC', 1, 0, GETUTCDATE()), 
				('Professional Services & High Tech', 'Deloitte USA', 1, 0, GETUTCDATE()), 
				('Professional Services & High Tech', 'Ernst & Young', 1, 0, GETUTCDATE()), 
				('Professional Services & High Tech', 'ExlService Technology Solutions LLC', 1, 0, GETUTCDATE()), 
				('Professional Services & High Tech', 'GoDaddy.com', 1, 0, GETUTCDATE()), 
				('Professional Services & High Tech', 'Google', 1, 0, GETUTCDATE()), 
				('Professional Services & High Tech', 'Intertek', 1, 0, GETUTCDATE()), 
				('Professional Services & High Tech', 'LEMPIRE', 1, 0, GETUTCDATE()), 
				('Professional Services & High Tech', 'McKinsey & Company', 1, 0, GETUTCDATE()), 
				('Professional Services & High Tech', 'MongoDB', 1, 0, GETUTCDATE()), 
				('Professional Services & High Tech', 'RSM', 1, 0, GETUTCDATE()), 
				('Professional Services & High Tech', 'Salesforce.com', 1, 0, GETUTCDATE()), 
				('Professional Services & High Tech', 'Trace One', 1, 0, GETUTCDATE()), 
				('Professional Services & High Tech', 'Wolters Kluwer', 1, 0, GETUTCDATE())
			) AS AiSCl 
			(AiStudioName, Client, IsActive, CreatedBy, CreatedOn)
			WHERE NOT EXISTS 
			(
				SELECT 1 
				FROM dbo.AiStudioClientMap 
				WHERE AiStudioName = AiSCl.AiStudioName AND Client = AiSCl.Client
			);

			COMMIT TRANSACTION;

			EXEC sysdata.SetDBVersion @newVersion, @scriptName;

			PRINT 'Script ' + @scriptName + ' completed successfully.';
		END TRY
		BEGIN CATCH
			-- Rollback the transactions
			PRINT 'ERROR OCCURRED! All changes will be rolled back ' + @scriptName;
			PRINT ERROR_MESSAGE();

			IF (@@TRANCOUNT > 0)
				ROLLBACK TRANSACTION;

			THROW
		END CATCH
	END
	ELSE
	BEGIN
		IF (sysdata.IsDbVersionApplied(@newVersion) = 1)
			PRINT 'Script (' + @scriptName + ') Version' + @newVersion + ' already applied!';

		IF (sysdata.IsDbVersionApplied(@reqVersion) = 0)
			PRINT 'ERROR: The script (' + @scriptName + ') requires DB version ' + @reqVersion;
	END
END
GO
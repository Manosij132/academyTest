DECLARE @scriptName VARCHAR(255) = 'Academy_1.11.sql';
DECLARE @reqVersion VARCHAR(20) = '1.10';
DECLARE @newVersion VARCHAR(20) = '1.11';

-- Script Body/Content
BEGIN
	PRINT 'CURRENT DB VERSION: ' + sysdata.GetDbVersion();

	IF (sysdata.IsDbVersionApplied(@reqVersion) = 1
			AND sysdata.IsDbVersionApplied(@newVersion) = 0)
	BEGIN
		BEGIN TRY
			BEGIN TRANSACTION;
			--Insert GK employee in PanelDetails
			INSERT INTO PanelDetails (Name, EmployeeId, IsActive, CreatedBy, CreatedOn, UpdatedBy, UpdatedOn)
			SELECT 'GK', ID , 1, 0, GETUTCDATE(), NULL, NULL
			FROM Employee
			WHERE IsActive = 1 and GlobantEmailAddress in('nitin.randive@globant.com','jitendra.nevase@globant.com','tejas.malvi@globant.com','ravikiran.gole@globant.com',
			'prateek.singh@globant.com','jignesh.varia@globant.com','rohan.rewale@globant.com','vaibhav.nimbalkar@globant.com','nakul.chandak@globant.com',
			'nirav.halbe@globant.com','gautam.patil@globant.com','arun.dave@globant.com','rohan.sapkal@globant.com','vishal.salunkhe@globant.com',
			'nitin.jain@globant.com','sandeep.gangwar@globant.com','pradip.shinde@globant.com','latesh.sarode@globant.com','pavan.ingale@globant.com',
			'harshala.gadkari@globant.com','nilesh.more@globant.com','ajinkyasingh.bais@globant.com','nikhil.babar@globant.com','appasaheb.tipali@globant.com',
			'yogesh.dhage@globant.com','fajil.sutar@globant.com','mahesh.dhembre@globant.com','santosh.panchal@globant.com','suraj.rane@globant.com',
			'pratik.nandagawali@globant.com','chetan.dravekar@globant.com','swapnil.bhagwatkar@globant.com','ashbin.kumar@globant.com','naveen.bhogi@globant.com',
			'avinash.bhudke@globant.com','manmohan.namdeo@globant.com','gourav.gupta@globant.com','amitkumar.gangurde@globant.com','salil.oak@globant.com',
			'bhushan.saler@globant.com','dhananjay.sahu@globant.com','sandeep.gadhave@globant.com','amol.charde@globant.com','bhushan.nikhar@globant.com',
			'rohitkumar.patel@globant.com','suvendu.pani@globant.com','azad.patel@globant.com','amol.salunke@globant.com','abhijit.khatal@globant.com',
			'jayesh.pawar@globant.com','rajesh.katadi@globant.com','shital.devalkar@globant.com','mangesh.pendhare@globant.com','mranal.dubey@globant.com',
			'rakesh.ravlekar@globant.com','vijayalaxmi.shetty@globant.com','shital.medsinge@globant.com','vishal.kuvalekar@globant.com','mohammad.alam@globant.com',
			'akash.bhingole@globant.com','shreyas.gokodikar@globant.com','r.raghuwanshi@globant.com','shirsendu.nayak@globant.com','harsh.sharma@globant.com',
			'abhijeet.kakade@globant.com','abhishek.kumar@globant.com','anuruddh.yadav@globant.com','ashish.jain@globant.com','ashish.subandh@globant.com',
			'c.putta@globant.com','deepak.sharma@globant.com','d.shirsat@globant.com','gokul.sonawane@globant.com','jitendra.godani@globant.com','kshitij.pandkar@globant.com',
			'kunal.ramdasi@globant.com','mangesh.kothawade@globant.com','manish.gokhru@globant.com','m.bhattacharya@globant.com','nitin.mudgal@globant.com',
			'rahul.gourshettiwar@globant.com','roshan.oswal@globant.com','rupesh.deshmukh@globant.com','sameer.shintre@globant.com','sudhir.makwana@globant.com',
			'tausif.vhora@globant.com','tayyab.tamboli@globant.com','vikas.pawar@globant.com','yogesh.kokare@globant.com','ritesh.gajera@globant.com','amit.kumar@globant.com',
			'ankush.sharma@globant.com','dheerendra.pandey@globant.com','fahim.patel@globant.com','ganesh.mahajan@globant.com','r.bhadoriya@globant.com',
			'rutuja.pole@globant.com','sagar.barawade@globant.com','siddharth.gujrathi@globant.com','utkarsh.yeolekar@globant.com','vikram.rao@globant.com')

			--Insert L1 employee in PanelDetails
			INSERT INTO PanelDetails (Name, EmployeeId, IsActive, CreatedBy, CreatedOn, UpdatedBy, UpdatedOn)
			SELECT 'L1', ID , 1, 0, GETUTCDATE(), NULL, NULL
			FROM Employee
			WHERE IsActive = 1 and GlobantEmailAddress in('suyash.rasal@globant.com','amajad.mulla@globant.com','chinmay.phatak@globant.com','minal.dethe@globant.com',
			'rupali.more@globant.com','abhijeet.pandkar@globant.com','zeeshan.khan@globant.com','snehal.walke@globant.com','satish.kharade@globant.com',
			'rutuja.brahamankar@globant.com','dhanraj.patil@globant.com','kartik.kolte@globant.com','dnyaneshwar.kawathe@globant.com','rohit.ghatage@globant.com',
			'rajiv.bhardwaj@globant.com','sanjay.kondawar@globant.com','tushar.fulmali@globant.com','krunal.luhar@globant.com','santosh.patil@globant.com',
			'sujit.raul@globant.com','nilesh.bamane@globant.com','ravindra.gojare@globant.com','rajkamal.maurya@globant.com','swapnil.javal@globant.com',
			'ujwala.chaudhari@globant.com','adarsh.vishnoi@globant.com','ashwinikumar.shende@globant.com','sujit.patil@globant.com','vijay.pandey@globant.com',
			'vaibhav.lande@globant.com','anirudha.tate@globant.com','shivshankar.padnoore@globant.com','vivekanand.bidri@globant.com','siddhant.srivastava@globant.com',
			'anukul.sharma@globant.com','dattatray.thorat@globant.com','gaurav.jain@globant.com','ketan.kakade@globant.com','s.pottipati@globant.com',
			'aishwarya.sahu@globant.com','abhay.wagh@globant.com','rohit.patil02@globant.com','akash.kakadiya@globant.com','gunjal.ghagre@globant.com',
			'devanand.dhage@globant.com','sandesh.kanade@globant.com','ajinkya.gajane@globant.com','reena.rana@globant.com','asha.sonavane@globant.com',
			'saurabh.rakhade@globant.com','shahaji.babar@globant.com','ankit.vishwakarma@globant.com','sushant.raje@globant.com','amol.mahale@globant.com',
			'vivek.joshi@globant.com','p.kulkarni@globant.com','vaidehi.gore@globant.com','vikrant.dalvi@globant.com','prachi.pande@globant.com','vaibhav.apugade@globant.com',
			'jayesh.patil@globant.com','anurag.chauhan@globant.com','navprabhat.singh@globant.com','sandip.thite@globant.com','pranav.dharmadhikari@globant.com',
			'vinuta.kolekar@globant.com','narendra.karmalkar@globant.com','rahul.bagale@globant.com','devdan.gaikwad@globant.com','juhi.batra@globant.com',
			'sanyukta.naik@globant.com','gaurav.kanikdale@globant.com','gajanan.koli@globant.com','amit.sisodiya@globant.com','sachin.mahandule@globant.com',
			'alhad.alsi@globant.com','ashish.markande@globant.com','amey.kamat@globant.com','rohit.dubey@globant.com','akshay.banokar@globant.com','vijay.rana@globant.com',
			'aniruddha.mane@globant.com','bankim.pandey@globant.com','krunal.shah@globant.com','ashish.amritkar@globant.com','shreyas.padhye@globant.com',
			'lokesh.chandore@globant.com','aparna.gadgil@globant.com','aniket.firke@globant.com','vikram.dhakar@globant.com','siddhesh.bramhankar@globant.com',
			'ritesh.lokhande@globant.com','vinay.gupta@globant.com','shubham.manmode@globant.com','sagar.akubattin@globant.com','arun.chougule@globant.com',
			'rupali.deshmukh@globant.com','shashikant.chitalkar@globant.com','vishal.balkote@globant.com','ankit.raghuvanshi@globant.com','neeraj.chand@globant.com',
			'nikhil.joshi@globant.com','kamlesh.panchbhaiye@globant.com','pradip.kumar@globant.com','sujit.ghegade@globant.com','sudipto.sarkar@globant.com',
			'gayatri.mahajan@globant.com','Payal.Sharma@globant.com','apeksha.jain@globant.com','chandrahas.gaikwad@globant.com','dnyaneshwar.biradar@globant.com',
			'nishant.modi@globant.com','pratiksha.more@globant.com','priti.garala@globant.com','shital.deore@globant.com','shubham.kapadnis@globant.com',
			'shubham.malviya@globant.com','vinayak.watve@globant.com','vineet.pandey@globant.com','vivek.sharma@globant.com','ajit.fawade@globant.com',
			'amit.kumar01@globant.com','anita.barbade@globant.com','ankit.s@globant.com','anurag.jadoun@globant.com','ayesha.shaikh@globant.com','bhaurao.birajdar@globant.com',
			'd.patil@globant.com','geeta.padwal@globant.com','harshal.chandile@globant.com','janit.bansal@globant.com','kirti.singh@globant.com','kranti.nikam@globant.com',
			'manish.ranjan@globant.com','muddassar.shaikh@globant.com','nidhi.teli@globant.com','nikeshkumar.rathod@globant.com','nikhil.lende@globant.com',
			'nikhil.tanpure@globant.com','nilesh.kumrawat@globant.com','nilkumar.shah@globant.com','nitish.jha@globant.com','prateek.soni@globant.com',
			'rahul.chavan@globant.com','rahulrajaram.patil@globant.com','rahul.sharma01@globant.com','rashmi.tale@globant.com','rohit.kumar@globant.com',
			'roopal.gupta@globant.com','sagar.kulthe@globant.com','shashank.sahu@globant.com','sumit.dahiya@globant.com','vikash.rathore@globant.com',
			'vinayak.kedari@globant.com','vipul.jha@globant.com','vishal.magar@globant.com','vishal.nashani@globant.com','vivek.dande@globant.com','pravin.mahale@globant.com',
			'kaustubh.vyas@globant.com')

			--Insert Community
			INSERT INTO Community (Name, IsActive, CreatedBy, CreatedOn, UpdatedBy, UpdatedOn)
			SELECT Distinct(community), 1, 0, GETUTCDATE(), NULL, NULL
			FROM Employee

			--Insert Employee and Community in EmployeeCommunityMap
			INSERT INTO EmployeeCommunityMap (CommunityId, EmployeeId,IsActive, CreatedBy, CreatedOn, UpdatedBy, UpdatedOn)
			SELECT C.Id, E.Id, 1, 0, GETUTCDATE(), NULL, NULL
			FROM Employee E
			Join
			Community C
			on E.Community = C.Name
			

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
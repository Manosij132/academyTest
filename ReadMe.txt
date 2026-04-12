======================================================================================================
					Academy Backend
======================================================================================================
Architecture
------------
We are using Clean Architecture in the backend.

Guidelines
----------

Proficiency
----------
	1. New Proficiencies can be added by System Admin only.
	2. Proficiency can be viewed by Glober itself, It's CM / GexLeader, It's Account / TDC / Community / Ecosystem / System Admin.
	3. Proficiency cab be upgraded by It's CM / GexLeader and System Admin
Seniority
----------
	1. SeniorityMaster table only contains seniorities which have proficiencies mapped.
	2. Seniority can be viewed and added by System Admin only
Ecosystem
----------
	1. EcosystemMaster table contains primary and secondary ecosystems both.
	2. Primary Ecosystem cannot be added by anyone, its background job's responsibility only.
	3. Secondary Ecosystem can be viewed and added by Ecosystem Admin and System Admin only.
Skill
----------
	1. Skills can be added by System Admin only
Training
----------
	1. Trainings (Manage Trainings Page) can be added by Account / TDC / Community / Ecosystem / System Admin. 
		However except the system admin rest can add trainings in thier respective Account / TDC / Community / Ecosystem only.
	2. Trainings (Spin Training page) can be spinned by Account / TDC / Community / Ecosystem / System Admin.
		However except the system admin rest can spin trainings in thier respective Account / TDC / Community / Ecosystem only.
Comments
----------
	1. Comments can viewed and added by Glober itself, It's CM / GexLeader, It's Account / TDC / Community / Ecosystem / System Admin.
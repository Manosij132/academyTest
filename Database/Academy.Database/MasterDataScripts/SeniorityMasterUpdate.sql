ALTER TABLE [dbo].[SeniorityMaster] ADD experience VARCHAR(10);

UPDATE SeniorityMaster
SET experience = '0-1'
WHERE SeniorityName LIKE '%Jr%' AND SeniorityName NOT LIKE '%Adv%';

UPDATE SeniorityMaster
SET experience = '1-2'
WHERE SeniorityName LIKE '%Jr%' AND SeniorityName LIKE '%Adv%';

UPDATE SeniorityMaster
SET experience = '2-4'
WHERE SeniorityName LIKE '%SSr%' AND SeniorityName NOT LIKE '%Adv%';

UPDATE SeniorityMaster
SET experience = '4-6'
WHERE SeniorityName LIKE '%SSr%' AND SeniorityName LIKE '%Adv%';

UPDATE SeniorityMaster
SET experience = '6-8'
WHERE SeniorityName IN ('Sr', 'Sr Level 1');

UPDATE SeniorityMaster
SET experience = '8-10'
WHERE SeniorityName IN ('Sr Level 2', 'Software Designer');

UPDATE SeniorityMaster
SET experience = '10-14'
WHERE SeniorityName IN ('Sr Level 3', 'Architect'); 
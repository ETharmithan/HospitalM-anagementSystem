-- Clear all users from the database (development only)
DELETE FROM [PatientContactInformation];
DELETE FROM [PatientIdentificationDetails]; 
DELETE FROM [Patients];
DELETE FROM [Users];

-- Reset identity columns
DBCC CHECKIDENT ('[Users]', RESEED, 1);
DBCC CHECKIDENT ('[Patients]', RESEED, 1);

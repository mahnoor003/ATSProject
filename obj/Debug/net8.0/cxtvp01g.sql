IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [Candidates] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    [Email] nvarchar(100) NOT NULL,
    [Phone] nvarchar(20) NOT NULL,
    [RoleApplied] nvarchar(50) NOT NULL,
    [Skills] nvarchar(200) NOT NULL,
    [Experience] nvarchar(50) NOT NULL,
    [DateApplied] datetime2 NOT NULL,
    [Source] nvarchar(20) NOT NULL,
    [CvFilePath] nvarchar(200) NOT NULL,
    [UniqueId] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_Candidates] PRIMARY KEY ([Id])
);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250827151002_InitialCreate', N'9.0.8');

DECLARE @var sysname;
SELECT @var = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Candidates]') AND [c].[name] = N'UniqueId');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [Candidates] DROP CONSTRAINT [' + @var + '];');
ALTER TABLE [Candidates] ALTER COLUMN [UniqueId] nvarchar(50) NULL;

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Candidates]') AND [c].[name] = N'Source');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Candidates] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [Candidates] ALTER COLUMN [Source] nvarchar(20) NULL;

DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Candidates]') AND [c].[name] = N'Skills');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Candidates] DROP CONSTRAINT [' + @var2 + '];');
ALTER TABLE [Candidates] ALTER COLUMN [Skills] nvarchar(max) NULL;

DECLARE @var3 sysname;
SELECT @var3 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Candidates]') AND [c].[name] = N'Experience');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [Candidates] DROP CONSTRAINT [' + @var3 + '];');
ALTER TABLE [Candidates] ALTER COLUMN [Experience] nvarchar(50) NULL;

DECLARE @var4 sysname;
SELECT @var4 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Candidates]') AND [c].[name] = N'CvFilePath');
IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [Candidates] DROP CONSTRAINT [' + @var4 + '];');
ALTER TABLE [Candidates] ALTER COLUMN [CvFilePath] nvarchar(200) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250828132838_MakeOptionalFieldsNullable', N'9.0.8');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250829153028_IncreaseSkillsLength', N'9.0.8');

DECLARE @var5 sysname;
SELECT @var5 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Candidates]') AND [c].[name] = N'Skills');
IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [Candidates] DROP CONSTRAINT [' + @var5 + '];');
UPDATE [Candidates] SET [Skills] = N'' WHERE [Skills] IS NULL;
ALTER TABLE [Candidates] ALTER COLUMN [Skills] nvarchar(1000) NOT NULL;
ALTER TABLE [Candidates] ADD DEFAULT N'' FOR [Skills];

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250829154237_IncreaseSkills', N'9.0.8');

DECLARE @var6 sysname;
SELECT @var6 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Candidates]') AND [c].[name] = N'Skills');
IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [Candidates] DROP CONSTRAINT [' + @var6 + '];');
ALTER TABLE [Candidates] ALTER COLUMN [Skills] nvarchar(max) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250829171042_IncreaseSkillsLengthV2', N'9.0.8');

ALTER TABLE Candidates ALTER COLUMN Skills NVARCHAR(MAX) NULL

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250829171503_ForceSkillsToMax', N'9.0.8');

DECLARE @var7 sysname;
SELECT @var7 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Candidates]') AND [c].[name] = N'Skills');
IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [Candidates] DROP CONSTRAINT [' + @var7 + '];');
ALTER TABLE [Candidates] ALTER COLUMN [Skills] nvarchar(max) NULL;

ALTER TABLE [Candidates] ADD [GmailMessageId] nvarchar(max) NOT NULL DEFAULT N'';

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250906111027_AddGmailMessageIdToCandidate', N'9.0.8');

ALTER TABLE [Candidates] ADD [Education] nvarchar(max) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250906134659_AddEducationToCandidate', N'9.0.8');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250909205649_UpdateExperienceLength', N'9.0.8');

DECLARE @var8 sysname;
SELECT @var8 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Candidates]') AND [c].[name] = N'Experience');
IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [Candidates] DROP CONSTRAINT [' + @var8 + '];');
ALTER TABLE [Candidates] ALTER COLUMN [Experience] nvarchar(max) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250909211135_ExpandExperienceColumn', N'9.0.8');

DECLARE @var9 sysname;
SELECT @var9 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Candidates]') AND [c].[name] = N'GmailMessageId');
IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [Candidates] DROP CONSTRAINT [' + @var9 + '];');
ALTER TABLE [Candidates] ALTER COLUMN [GmailMessageId] nvarchar(max) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250910111038_MakeGmailMessageIdNullable', N'9.0.8');

CREATE TABLE [CandidateFields] (
    [Id] int NOT NULL IDENTITY,
    [FieldName] nvarchar(max) NULL,
    [FieldType] nvarchar(max) NULL,
    [IsMandatory] bit NOT NULL,
    CONSTRAINT [PK_CandidateFields] PRIMARY KEY ([Id])
);

CREATE TABLE [Jobs] (
    [Id] int NOT NULL IDENTITY,
    [JobTitle] nvarchar(max) NULL,
    [Description] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Jobs] PRIMARY KEY ([Id])
);

CREATE TABLE [CandidateResponses] (
    [Id] int NOT NULL IDENTITY,
    [JobId] int NOT NULL,
    [CandidateFieldId] int NOT NULL,
    [Response] nvarchar(max) NULL,
    CONSTRAINT [PK_CandidateResponses] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CandidateResponses_CandidateFields_CandidateFieldId] FOREIGN KEY ([CandidateFieldId]) REFERENCES [CandidateFields] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_CandidateResponses_Jobs_JobId] FOREIGN KEY ([JobId]) REFERENCES [Jobs] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [JobRequiredFields] (
    [Id] int NOT NULL IDENTITY,
    [JobId] int NOT NULL,
    [CandidateFieldId] int NOT NULL,
    CONSTRAINT [PK_JobRequiredFields] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_JobRequiredFields_CandidateFields_CandidateFieldId] FOREIGN KEY ([CandidateFieldId]) REFERENCES [CandidateFields] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_JobRequiredFields_Jobs_JobId] FOREIGN KEY ([JobId]) REFERENCES [Jobs] ([Id]) ON DELETE NO ACTION
);

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'FieldName', N'FieldType', N'IsMandatory') AND [object_id] = OBJECT_ID(N'[CandidateFields]'))
    SET IDENTITY_INSERT [CandidateFields] ON;
INSERT INTO [CandidateFields] ([Id], [FieldName], [FieldType], [IsMandatory])
VALUES (1, N'Full Name', N'Text', CAST(1 AS bit)),
(2, N'Gender', N'Text', CAST(1 AS bit)),
(3, N'Date of Birth', N'Date', CAST(1 AS bit)),
(4, N'Nationality', N'Text', CAST(1 AS bit)),
(5, N'CNIC / Passport No.', N'Text', CAST(1 AS bit)),
(6, N'Marital Status', N'Text', CAST(0 AS bit)),
(7, N'Religion', N'Text', CAST(0 AS bit)),
(8, N'Contact Number(s)', N'Text', CAST(1 AS bit)),
(9, N'Email Address', N'Email', CAST(1 AS bit)),
(10, N'Current Address', N'Text', CAST(1 AS bit)),
(11, N'Permanent Address', N'Text', CAST(1 AS bit)),
(12, N'LinkedIn Profile URL', N'Text', CAST(0 AS bit)),
(13, N'Other Social Media / Portfolio Links', N'Text', CAST(0 AS bit)),
(14, N'Professional Website / Blog', N'Text', CAST(0 AS bit)),
(15, N'Photograph', N'File', CAST(0 AS bit)),
(16, N'Job Titles Applying For', N'Text', CAST(1 AS bit)),
(17, N'Job Requisition ID', N'Text', CAST(0 AS bit)),
(18, N'Location(s)', N'Text', CAST(1 AS bit)),
(19, N'Open to Relocation?', N'Text', CAST(0 AS bit)),
(20, N'Relocation Preference', N'Text', CAST(0 AS bit)),
(21, N'Willing to Travel?', N'Text', CAST(0 AS bit)),
(22, N'Desired Employment Type', N'Text', CAST(1 AS bit)),
(23, N'Remote/Hybrid/On-site Preference', N'Text', CAST(0 AS bit)),
(24, N'Earliest Possible Joining Date', N'Date', CAST(1 AS bit)),
(25, N'Notice Period', N'Text', CAST(0 AS bit)),
(26, N'Highest Qualification Obtained', N'Text', CAST(1 AS bit)),
(27, N'Education Level(s)', N'Text', CAST(0 AS bit)),
(28, N'Institute / University Name', N'Text', CAST(1 AS bit)),
(29, N'Country of Education', N'Text', CAST(0 AS bit)),
(30, N'Major / Field of Study', N'Text', CAST(1 AS bit)),
(31, N'Graduation Year(s)', N'Text', CAST(0 AS bit)),
(32, N'GPA / Percentage / Division', N'Text', CAST(0 AS bit)),
(33, N'Certifications', N'Text', CAST(0 AS bit)),
(34, N'Ongoing Courses / Learning Programs', N'Text', CAST(0 AS bit)),
(35, N'Employer Name', N'Text', CAST(1 AS bit)),
(36, N'Industry', N'Text', CAST(1 AS bit)),
(37, N'Job Title', N'Text', CAST(1 AS bit)),
(38, N'Start Date – End Date', N'Text', CAST(1 AS bit)),
(39, N'Employment Type', N'Text', CAST(1 AS bit)),
(40, N'Job Responsibilities', N'Text', CAST(0 AS bit)),
(41, N'Key Achievements', N'Text', CAST(0 AS bit)),
(42, N'Reason for Leaving', N'Text', CAST(0 AS bit));
INSERT INTO [CandidateFields] ([Id], [FieldName], [FieldType], [IsMandatory])
VALUES (43, N'Last Drawn Salary & Benefits', N'Text', CAST(0 AS bit)),
(44, N'Reporting Manager’s Name & Contact', N'Text', CAST(0 AS bit)),
(45, N'International Work Experience?', N'Text', CAST(0 AS bit)),
(46, N'Startup vs Corporate Experience', N'Text', CAST(0 AS bit)),
(47, N'Freelance / Contract Experience', N'Text', CAST(0 AS bit)),
(48, N'Technical Skills', N'Text', CAST(1 AS bit)),
(49, N'Soft Skills', N'Text', CAST(0 AS bit)),
(50, N'Tools/Software Knowledge', N'Text', CAST(0 AS bit)),
(51, N'Programming Languages / Frameworks', N'Text', CAST(0 AS bit)),
(52, N'Certifications (Professional / IT / HR / Finance / Language)', N'Text', CAST(0 AS bit)),
(53, N'Language Proficiency', N'Text', CAST(0 AS bit)),
(54, N'Current Industry', N'Text', CAST(1 AS bit)),
(55, N'Previous Industry Experience', N'Text', CAST(0 AS bit)),
(56, N'Preferred Industry', N'Text', CAST(0 AS bit)),
(57, N'Sector Exposure', N'Text', CAST(0 AS bit)),
(58, N'Functional Expertise', N'Text', CAST(0 AS bit)),
(59, N'Sub-Domain Expertise', N'Text', CAST(0 AS bit)),
(60, N'Management Level', N'Text', CAST(1 AS bit)),
(61, N'Preferred Job Function', N'Text', CAST(0 AS bit)),
(62, N'Preferred Work Environment', N'Text', CAST(0 AS bit)),
(63, N'Preferred Industry Sector(s)', N'Text', CAST(0 AS bit)),
(64, N'Preferred Currency for Salary', N'Text', CAST(0 AS bit)),
(65, N'Work Authorization Status', N'Text', CAST(0 AS bit)),
(66, N'Visa / Work Permit Status', N'Text', CAST(0 AS bit)),
(67, N'Convicted of a crime?', N'Text', CAST(0 AS bit)),
(68, N'Legal restrictions for employment?', N'Text', CAST(0 AS bit)),
(69, N'Contractual obligations / non-compete clauses?', N'Text', CAST(0 AS bit)),
(70, N'Medically fit for applied role?', N'Text', CAST(0 AS bit)),
(71, N'Tax Registration Number', N'Text', CAST(0 AS bit)),
(72, N'Willingness to undergo background check?', N'Text', CAST(0 AS bit)),
(73, N'Willingness to undergo medical examination?', N'Text', CAST(0 AS bit)),
(74, N'Reference 1: Name', N'Text', CAST(0 AS bit)),
(75, N'Reference 1: CNIC #', N'Text', CAST(0 AS bit)),
(76, N'Reference 1: Designation', N'Text', CAST(0 AS bit)),
(77, N'Reference 1: Organization', N'Text', CAST(0 AS bit)),
(78, N'Reference 1: Relationship', N'Text', CAST(0 AS bit)),
(79, N'Reference 1: Contact Info', N'Text', CAST(0 AS bit)),
(80, N'Reference 2: Name', N'Text', CAST(0 AS bit)),
(81, N'Reference 2: CNIC #', N'Text', CAST(0 AS bit)),
(82, N'Reference 2: Designation', N'Text', CAST(0 AS bit)),
(83, N'Reference 2: Organization', N'Text', CAST(0 AS bit)),
(84, N'Reference 2: Relationship', N'Text', CAST(0 AS bit));
INSERT INTO [CandidateFields] ([Id], [FieldName], [FieldType], [IsMandatory])
VALUES (85, N'Reference 2: Contact Info', N'Text', CAST(0 AS bit)),
(86, N'How did you hear about this job?', N'Text', CAST(0 AS bit)),
(87, N'Referral Name (if any)', N'Text', CAST(0 AS bit)),
(88, N'Availability for interviews', N'Text', CAST(0 AS bit)),
(89, N'Upload Resume / CV', N'File', CAST(1 AS bit)),
(90, N'Upload Cover Letter', N'File', CAST(0 AS bit)),
(91, N'Upload Portfolio', N'File', CAST(0 AS bit)),
(92, N'Upload Certificates / Transcripts', N'File', CAST(0 AS bit)),
(93, N'Declaration Checkbox', N'Text', CAST(1 AS bit)),
(94, N'Digital Signature / Typed Name', N'Text', CAST(1 AS bit)),
(95, N'Date/Time of Submission', N'Date', CAST(1 AS bit));
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'FieldName', N'FieldType', N'IsMandatory') AND [object_id] = OBJECT_ID(N'[CandidateFields]'))
    SET IDENTITY_INSERT [CandidateFields] OFF;

CREATE INDEX [IX_CandidateResponses_CandidateFieldId] ON [CandidateResponses] ([CandidateFieldId]);

CREATE INDEX [IX_CandidateResponses_JobId] ON [CandidateResponses] ([JobId]);

CREATE INDEX [IX_JobRequiredFields_CandidateFieldId] ON [JobRequiredFields] ([CandidateFieldId]);

CREATE INDEX [IX_JobRequiredFields_JobId] ON [JobRequiredFields] ([JobId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250913192121_CandidateJobSetup', N'9.0.8');

ALTER TABLE [CandidateResponses] DROP CONSTRAINT [FK_CandidateResponses_CandidateApplications_CandidateApplicationId];

ALTER TABLE [CandidateResponses] ADD CONSTRAINT [FK_CandidateResponses_CandidateApplications_CandidateApplicationId] FOREIGN KEY ([CandidateApplicationId]) REFERENCES [CandidateApplications] ([Id]) ON DELETE NO ACTION;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250916151727_AddCandidateResponsesToApplication', N'9.0.8');

ALTER TABLE [CandidateResponses] DROP CONSTRAINT [FK_CandidateResponses_CandidateFields_CandidateFieldId];

ALTER TABLE [CandidateResponses] DROP CONSTRAINT [FK_CandidateResponses_Jobs_JobId];

ALTER TABLE [CandidateResponses] ADD CONSTRAINT [FK_CandidateResponses_CandidateFields_CandidateFieldId] FOREIGN KEY ([CandidateFieldId]) REFERENCES [CandidateFields] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [CandidateResponses] ADD CONSTRAINT [FK_CandidateResponses_Jobs_JobId] FOREIGN KEY ([JobId]) REFERENCES [Jobs] ([Id]) ON DELETE NO ACTION;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250916152405_Init', N'9.0.8');

ALTER TABLE [CandidateApplications] DROP CONSTRAINT [FK_CandidateApplications_Jobs_JobId];

ALTER TABLE [JobRequiredFields] DROP CONSTRAINT [FK_JobRequiredFields_CandidateFields_CandidateFieldId];

ALTER TABLE [JobRequiredFields] DROP CONSTRAINT [FK_JobRequiredFields_Jobs_JobId];

ALTER TABLE [CandidateApplications] ADD CONSTRAINT [FK_CandidateApplications_Jobs_JobId] FOREIGN KEY ([JobId]) REFERENCES [Jobs] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [JobRequiredFields] ADD CONSTRAINT [FK_JobRequiredFields_CandidateFields_CandidateFieldId] FOREIGN KEY ([CandidateFieldId]) REFERENCES [CandidateFields] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [JobRequiredFields] ADD CONSTRAINT [FK_JobRequiredFields_Jobs_JobId] FOREIGN KEY ([JobId]) REFERENCES [Jobs] ([Id]) ON DELETE NO ACTION;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250916162436_InitialFixed', N'9.0.8');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250917132633_AddCandidateApplication', N'9.0.8');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250917134805_AddCandidateApplicationProper', N'9.0.8');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250917135700_AddCandidateApplicationTable', N'9.0.8');

COMMIT;
GO


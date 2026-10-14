CREATE TABLE "Audits" (
    "Id" uuid NOT NULL,
    "Actor" uuid NOT NULL,
    "Action" text NOT NULL,
    "Resource" text NOT NULL,
    "Details" text NOT NULL,
    "Ip" text NOT NULL,
    "Created" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Audits" PRIMARY KEY ("Id")
);


CREATE TABLE "Curricula" (
    "Id" uuid NOT NULL,
    "Name" text NOT NULL,
    "Level" text NOT NULL,
    "Track" text NOT NULL,
    "Speciality" text NOT NULL,
    "Version" text NOT NULL,
    "Status" text NOT NULL,
    "Content" text NOT NULL,
    CONSTRAINT "PK_Curricula" PRIMARY KEY ("Id")
);


CREATE TABLE "Documents" (
    "Id" uuid NOT NULL,
    "ClassId" uuid,
    "OwnerStudentId" uuid,
    "Title" text NOT NULL,
    "Category" text NOT NULL,
    "Metadata" text NOT NULL,
    "Filename" text NOT NULL,
    "StorageKey" text NOT NULL,
    "Mime" text NOT NULL,
    "Hash" text NOT NULL,
    "Approved" boolean NOT NULL,
    CONSTRAINT "PK_Documents" PRIMARY KEY ("Id")
);


CREATE TABLE "Notifications" (
    "Id" uuid NOT NULL,
    "StudentId" uuid NOT NULL,
    "Message" text NOT NULL,
    "Read" boolean NOT NULL,
    "Created" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Notifications" PRIMARY KEY ("Id")
);


CREATE TABLE "Roles" (
    "Id" uuid NOT NULL,
    "SystemCode" text NOT NULL,
    "Name" text NOT NULL,
    "Permissions" text NOT NULL,
    CONSTRAINT "PK_Roles" PRIMARY KEY ("Id")
);


CREATE TABLE "Students" (
    "Id" uuid NOT NULL,
    "Number" text NOT NULL,
    "FirstName" text NOT NULL,
    "LastName" text NOT NULL,
    "Profile" text NOT NULL,
    "Status" text NOT NULL,
    "ActivationHash" text NOT NULL,
    "Track" text NOT NULL,
    CONSTRAINT "PK_Students" PRIMARY KEY ("Id")
);


CREATE TABLE "Years" (
    "Id" uuid NOT NULL,
    "Code" text NOT NULL,
    "Start" date NOT NULL,
    "End" date NOT NULL,
    "Welcome" text NOT NULL,
    "Status" text NOT NULL,
    CONSTRAINT "PK_Years" PRIMARY KEY ("Id"),
    CONSTRAINT "CK_Year_Dates" CHECK ("Start" < "End")
);


CREATE TABLE "CurriculumUnit" (
    "Id" uuid NOT NULL,
    "CurriculumId" uuid NOT NULL,
    "Semester" integer NOT NULL,
    "Code" text NOT NULL,
    "Name" text NOT NULL,
    "Credits" numeric,
    CONSTRAINT "PK_CurriculumUnit" PRIMARY KEY ("Id"),
    CONSTRAINT "CK_Ue_Credits" CHECK ("Credits" IS NULL OR CAST("Credits" AS NUMERIC) >= 0),
    CONSTRAINT "FK_CurriculumUnit_Curricula_CurriculumId" FOREIGN KEY ("CurriculumId") REFERENCES "Curricula" ("Id") ON DELETE RESTRICT
);


CREATE TABLE "Users" (
    "Id" uuid NOT NULL,
    "MfaSecret" text NOT NULL,
    "MfaStep" bigint NOT NULL,
    "Username" text NOT NULL,
    "PasswordHash" text NOT NULL,
    "StudentId" uuid,
    "Active" boolean NOT NULL,
    "Version" integer NOT NULL,
    "Roles" text NOT NULL,
    CONSTRAINT "PK_Users" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Users_Students_StudentId" FOREIGN KEY ("StudentId") REFERENCES "Students" ("Id") ON DELETE RESTRICT
);


CREATE TABLE "Classes" (
    "Id" uuid NOT NULL,
    "YearId" uuid NOT NULL,
    "Name" text NOT NULL,
    "Level" text NOT NULL,
    "Speciality" text NOT NULL,
    "Tracks" text NOT NULL,
    "JoinHash" text NOT NULL,
    "JoinExpires" timestamp with time zone,
    CONSTRAINT "PK_Classes" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Classes_Years_YearId" FOREIGN KEY ("YearId") REFERENCES "Years" ("Id") ON DELETE RESTRICT
);


CREATE TABLE "CurriculumElement" (
    "Id" uuid NOT NULL,
    "UnitId" uuid NOT NULL,
    "Code" text NOT NULL,
    "Name" text NOT NULL,
    "Coefficient" numeric NOT NULL,
    "Cm" integer NOT NULL,
    "Td" integer NOT NULL,
    "Tp" integer NOT NULL,
    CONSTRAINT "PK_CurriculumElement" PRIMARY KEY ("Id"),
    CONSTRAINT "CK_Ec_Coefficient" CHECK (CAST("Coefficient" AS NUMERIC) > 0),
    CONSTRAINT "FK_CurriculumElement_CurriculumUnit_UnitId" FOREIGN KEY ("UnitId") REFERENCES "CurriculumUnit" ("Id") ON DELETE RESTRICT
);


CREATE TABLE "Equivalence" (
    "Id" uuid NOT NULL,
    "SourceUe" uuid NOT NULL,
    "TargetUe" uuid NOT NULL,
    "Reason" text NOT NULL,
    "Actor" uuid NOT NULL,
    CONSTRAINT "PK_Equivalence" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Equivalence_CurriculumUnit_SourceUe" FOREIGN KEY ("SourceUe") REFERENCES "CurriculumUnit" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Equivalence_CurriculumUnit_TargetUe" FOREIGN KEY ("TargetUe") REFERENCES "CurriculumUnit" ("Id") ON DELETE RESTRICT
);


CREATE TABLE "Delegations" (
    "Id" uuid NOT NULL,
    "ClassId" uuid NOT NULL,
    "Granter" uuid NOT NULL,
    "Grantee" uuid NOT NULL,
    "Expires" timestamp with time zone NOT NULL,
    "Revoked" boolean NOT NULL,
    CONSTRAINT "PK_Delegations" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Delegations_Classes_ClassId" FOREIGN KEY ("ClassId") REFERENCES "Classes" ("Id") ON DELETE RESTRICT
);


CREATE TABLE "Enrollments" (
    "Id" uuid NOT NULL,
    "StudentId" uuid NOT NULL,
    "ClassId" uuid NOT NULL,
    "YearId" uuid NOT NULL,
    "CurriculumId" uuid NOT NULL,
    "Attempt" integer NOT NULL,
    "Status" text NOT NULL,
    "Created" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Enrollments" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Enrollments_Classes_ClassId" FOREIGN KEY ("ClassId") REFERENCES "Classes" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Enrollments_Curricula_CurriculumId" FOREIGN KEY ("CurriculumId") REFERENCES "Curricula" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Enrollments_Students_StudentId" FOREIGN KEY ("StudentId") REFERENCES "Students" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Enrollments_Years_YearId" FOREIGN KEY ("YearId") REFERENCES "Years" ("Id") ON DELETE RESTRICT
);


CREATE TABLE "Imports" (
    "Id" uuid NOT NULL,
    "ClassId" uuid NOT NULL,
    "Semester" integer NOT NULL,
    "Rows" text NOT NULL,
    "Errors" text NOT NULL,
    "Hash" text NOT NULL,
    "Status" text NOT NULL,
    CONSTRAINT "PK_Imports" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Imports_Classes_ClassId" FOREIGN KEY ("ClassId") REFERENCES "Classes" ("Id") ON DELETE RESTRICT
);


CREATE TABLE "Officers" (
    "Id" uuid NOT NULL,
    "ClassId" uuid NOT NULL,
    "StudentId" uuid NOT NULL,
    "Position" text NOT NULL,
    CONSTRAINT "PK_Officers" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Officers_Classes_ClassId" FOREIGN KEY ("ClassId") REFERENCES "Classes" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Officers_Students_StudentId" FOREIGN KEY ("StudentId") REFERENCES "Students" ("Id") ON DELETE RESTRICT
);


CREATE TABLE "Publications" (
    "Id" uuid NOT NULL,
    "ClassId" uuid NOT NULL,
    "Semester" integer NOT NULL,
    "Status" text NOT NULL,
    "Created" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Publications" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Publications_Classes_ClassId" FOREIGN KEY ("ClassId") REFERENCES "Classes" ("Id") ON DELETE RESTRICT
);


CREATE TABLE "Sessions" (
    "Id" uuid NOT NULL,
    "ClassId" uuid NOT NULL,
    "EcId" uuid NOT NULL,
    "Course" text NOT NULL,
    "Secret" text NOT NULL,
    "Opened" timestamp with time zone NOT NULL,
    "Closed" timestamp with time zone,
    CONSTRAINT "PK_Sessions" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Sessions_Classes_ClassId" FOREIGN KEY ("ClassId") REFERENCES "Classes" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Sessions_CurriculumElement_EcId" FOREIGN KEY ("EcId") REFERENCES "CurriculumElement" ("Id") ON DELETE RESTRICT
);


CREATE TABLE "Discipline" (
    "Id" uuid NOT NULL,
    "EnrollmentId" uuid NOT NULL,
    "Semester" integer NOT NULL,
    "Delta" numeric NOT NULL,
    "Reason" text NOT NULL,
    "Actor" uuid NOT NULL,
    "Created" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Discipline" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Discipline_Enrollments_EnrollmentId" FOREIGN KEY ("EnrollmentId") REFERENCES "Enrollments" ("Id") ON DELETE RESTRICT
);


CREATE TABLE "Grades" (
    "Id" uuid NOT NULL,
    "EnrollmentId" uuid NOT NULL,
    "EcId" uuid NOT NULL,
    "Semester" integer NOT NULL,
    "Value" numeric NOT NULL,
    CONSTRAINT "PK_Grades" PRIMARY KEY ("Id"),
    CONSTRAINT "CK_Grade_Range" CHECK (CAST("Value" AS NUMERIC) >= 0 AND CAST("Value" AS NUMERIC) <= 20),
    CONSTRAINT "FK_Grades_CurriculumElement_EcId" FOREIGN KEY ("EcId") REFERENCES "CurriculumElement" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Grades_Enrollments_EnrollmentId" FOREIGN KEY ("EnrollmentId") REFERENCES "Enrollments" ("Id") ON DELETE RESTRICT
);


CREATE TABLE "Rescues" (
    "Id" uuid NOT NULL,
    "EnrollmentId" uuid NOT NULL,
    "UeId" uuid NOT NULL,
    "Reason" text NOT NULL,
    "Actor" uuid NOT NULL,
    CONSTRAINT "PK_Rescues" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Rescues_CurriculumUnit_UeId" FOREIGN KEY ("UeId") REFERENCES "CurriculumUnit" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Rescues_Enrollments_EnrollmentId" FOREIGN KEY ("EnrollmentId") REFERENCES "Enrollments" ("Id") ON DELETE RESTRICT
);


CREATE TABLE "Snapshots" (
    "Id" uuid NOT NULL,
    "Header" text NOT NULL,
    "EnrollmentId" uuid NOT NULL,
    "Semester" integer NOT NULL,
    "Version" integer NOT NULL,
    "Content" text NOT NULL,
    "Hash" text NOT NULL,
    "Created" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Snapshots" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Snapshots_Enrollments_EnrollmentId" FOREIGN KEY ("EnrollmentId") REFERENCES "Enrollments" ("Id") ON DELETE RESTRICT
);


CREATE TABLE "Attendances" (
    "Id" uuid NOT NULL,
    "SessionId" uuid NOT NULL,
    "StudentId" uuid NOT NULL,
    "Status" text NOT NULL,
    "Method" text NOT NULL,
    "Checked" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Attendances" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Attendances_Sessions_SessionId" FOREIGN KEY ("SessionId") REFERENCES "Sessions" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Attendances_Students_StudentId" FOREIGN KEY ("StudentId") REFERENCES "Students" ("Id") ON DELETE RESTRICT
);


CREATE TABLE "Justifications" (
    "Id" uuid NOT NULL,
    "SessionId" uuid NOT NULL,
    "StudentId" uuid NOT NULL,
    "Reason" text NOT NULL,
    "Status" text NOT NULL,
    "Decision" text NOT NULL,
    "DocumentId" uuid,
    CONSTRAINT "PK_Justifications" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Justifications_Sessions_SessionId" FOREIGN KEY ("SessionId") REFERENCES "Sessions" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Justifications_Students_StudentId" FOREIGN KEY ("StudentId") REFERENCES "Students" ("Id") ON DELETE RESTRICT
);


CREATE UNIQUE INDEX "IX_Attendances_SessionId_StudentId" ON "Attendances" ("SessionId", "StudentId");


CREATE INDEX "IX_Attendances_StudentId" ON "Attendances" ("StudentId");


CREATE INDEX "IX_Classes_YearId" ON "Classes" ("YearId");


CREATE INDEX "IX_CurriculumElement_UnitId" ON "CurriculumElement" ("UnitId");


CREATE INDEX "IX_CurriculumUnit_CurriculumId" ON "CurriculumUnit" ("CurriculumId");


CREATE INDEX "IX_Delegations_ClassId" ON "Delegations" ("ClassId");


CREATE INDEX "IX_Discipline_EnrollmentId" ON "Discipline" ("EnrollmentId");


CREATE INDEX "IX_Enrollments_ClassId" ON "Enrollments" ("ClassId");


CREATE INDEX "IX_Enrollments_CurriculumId" ON "Enrollments" ("CurriculumId");


CREATE UNIQUE INDEX "IX_Enrollments_StudentId_YearId" ON "Enrollments" ("StudentId", "YearId") WHERE "Status" = 'ACTIVE';


CREATE INDEX "IX_Enrollments_YearId" ON "Enrollments" ("YearId");


CREATE UNIQUE INDEX "IX_Equivalence_SourceUe_TargetUe" ON "Equivalence" ("SourceUe", "TargetUe");


CREATE INDEX "IX_Equivalence_TargetUe" ON "Equivalence" ("TargetUe");


CREATE INDEX "IX_Grades_EcId" ON "Grades" ("EcId");


CREATE UNIQUE INDEX "IX_Grades_EnrollmentId_EcId" ON "Grades" ("EnrollmentId", "EcId");


CREATE INDEX "IX_Imports_ClassId" ON "Imports" ("ClassId");


CREATE INDEX "IX_Justifications_SessionId" ON "Justifications" ("SessionId");


CREATE INDEX "IX_Justifications_StudentId" ON "Justifications" ("StudentId");


CREATE UNIQUE INDEX "IX_Officers_ClassId_Position" ON "Officers" ("ClassId", "Position");


CREATE INDEX "IX_Officers_StudentId" ON "Officers" ("StudentId");


CREATE UNIQUE INDEX "IX_Publications_ClassId_Semester" ON "Publications" ("ClassId", "Semester");


CREATE UNIQUE INDEX "IX_Rescues_EnrollmentId_UeId" ON "Rescues" ("EnrollmentId", "UeId");


CREATE INDEX "IX_Rescues_UeId" ON "Rescues" ("UeId");


CREATE INDEX "IX_Sessions_ClassId" ON "Sessions" ("ClassId");


CREATE INDEX "IX_Sessions_EcId" ON "Sessions" ("EcId");


CREATE UNIQUE INDEX "IX_Snapshots_EnrollmentId_Semester_Version" ON "Snapshots" ("EnrollmentId", "Semester", "Version");


CREATE UNIQUE INDEX "IX_Students_Number" ON "Students" ("Number");


CREATE UNIQUE INDEX "IX_Users_StudentId" ON "Users" ("StudentId");


CREATE UNIQUE INDEX "IX_Users_Username" ON "Users" ("Username");


CREATE UNIQUE INDEX "IX_Years_Code" ON "Years" ("Code");


CREATE UNIQUE INDEX "IX_Years_Status" ON "Years" ("Status") WHERE "Status" = 'OPEN';




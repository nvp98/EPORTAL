-- ================================================================
-- View360 - Consolidated Migration (gop tat ca migrations cua module)
-- ================================================================
-- File nay gop tat ca migrations rieng le cua module View360 thanh 1 file
-- chay 1 lan. Tat ca section IDEMPOTENT - chay lai khong gay loi.
--
-- Thu tu chay (KHONG doi):
--   1.  V360 Scene Calibration + Tour Config tables (Virtual Tour viewer)
--   2.  ProjectsGroup.ParentIDGroup column (hierarchy 2-cap)
--   3.  Insert parent groups + sub-groups (data seed cho DB moi)
--   4.  Re-link sub-groups (name-tolerant, sua data inconsistency)
--   5.  Cleanup duplicates do migration cu tao (GUARDED - safe trong production)
--   6.  ProjectsGroup.SortOrder column (admin sap xep folder)
--   7.  View360_AccessLog table (page-hit tracking, da/chua xem)
--   8.  Kuula scene cache + uuid drift audit (Kuula fault tolerance)
--   9.  Permission hybrid: AuthorizationUSER_Group + 3 SP _select_USER
--   10. Cleanup orphan group-grants (chi chay sau khi SECTION 9 thanh cong)
--   11. Verify - hien thi final state
--
-- Cach chay:
--   sqlcmd -S <server> -d EPORTAL -i v360-all.sql -b
--   HOAC mo SSMS, paste content, F5
--   Co `-b` de sqlcmd EXIT khi co loi - quan trong cho production deploy.
--
-- =====  PRODUCTION SAFETY CHECKLIST  =====
--   [BAT BUOC] BACKUP DB truoc khi chay (CREATE/DROP/ALTER table + SP + INSERT/DELETE data).
--   [BAT BUOC] BACKUP 3 SP truoc khi SECTION 9 chay:
--                EXEC sp_helptext 'Project_select_USER';
--                EXEC sp_helptext 'Virtual_select_USER';
--                EXEC sp_helptext 'Video_select';
--              Luu output cua moi SP vao file txt rieng (rollback script).
--   [BAT BUOC] SQL Server 2016+ (SECTION 9.4 dung CREATE OR ALTER syntax).
--              Pre-check: SELECT SERVERPROPERTY('ProductMajorVersion'); ket qua >= 13.
--              Neu < 13: chay v360-auth-group-hybrid.sql (DROP+CREATE) thay the cho SECTION 9.
--   [KHUYEN NGHI] Test tren staging clone cua production truoc.
--   [KHUYEN NGHI] Chay trong gio it user (off-peak).
--
-- =====  IDEMPOTENCY GUARANTEE  =====
--   - Tat ca CREATE TABLE/INDEX/COLUMN dung IF NOT EXISTS pattern.
--   - Tat ca INSERT data dung NOT EXISTS check + UNIQUE constraint.
--   - SECTION 5 (cleanup duplicates) DELETE guarded boi NOT EXISTS children/projects.
--   - SECTION 9 (3 SP) dung DROP+CREATE - GHI DE SP cu hoan toan moi lan chay.
--   - SECTION 10 (orphans) DELETE guarded boi NOT EXISTS target row.
-- ================================================================

-- BAT BUOC cho filtered index (View360_KuulaSceneCache.IX_KuulaSceneCache_GPS) -
-- SQL Server yeu cau QUOTED_IDENTIFIER + ANSI_NULLS ON moi tao duoc.
-- sqlcmd default QUOTED_IDENTIFIER OFF -> CREATE INDEX fail neu khong set.
-- SSMS default ON nen chay binh thuong, nhung muon idempotent voi ca 2 client, set day.
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO


-- ================================================================
-- SECTION 1: V360 module tables (Virtual Tour viewer + Chatbot)
-- ================================================================
-- V360_SceneCalibration: per-scene override (offset huong, hide pin, custom title)
-- V360_FeaturedScene:    admin pin scenes len Intro page "CAC KHU VUC THAM QUAN CHINH"
-- V360_TourConfig:       per-collection visual config (pin size, color, cone, ...)
-- V360_ChatbotTourInfo:  per-tour Overview + SystemPrompt (admin config chatbot)
-- V360_ChatbotSceneInfo: per-scene context (intro/detail/keywords) cho chatbot
-- V360_ChatbotMessage:   chat session log (auto-populate khi user chat)

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'V360_SceneCalibration' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.V360_SceneCalibration (
        Id            INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        CollectionId  NVARCHAR(50)      NOT NULL,
        SceneUuid     NVARCHAR(100)     NOT NULL,
        [Offset]      FLOAT             NULL,
        HidePin       BIT               NOT NULL CONSTRAINT DF_V360SC_HidePin DEFAULT (0),
        CustomTitle   NVARCHAR(500)     NULL,
        UpdatedAt     DATETIME          NOT NULL CONSTRAINT DF_V360SC_UpdAt DEFAULT (GETDATE()),
        UpdatedBy     INT               NULL,
        CONSTRAINT UQ_V360SC_CidUuid UNIQUE (CollectionId, SceneUuid)
    );
    CREATE INDEX IX_V360SC_Cid ON dbo.V360_SceneCalibration(CollectionId);
    PRINT '[1] Created table V360_SceneCalibration';
END
ELSE
BEGIN
    PRINT '[1] V360_SceneCalibration already exists - skipped';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'V360_FeaturedScene' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.V360_FeaturedScene (
        Id            INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        CollectionId  NVARCHAR(50)      NOT NULL,
        SceneUuid     NVARCHAR(100)     NOT NULL,
        DisplayOrder  INT               NOT NULL CONSTRAINT DF_V360FS_Ord DEFAULT (0),
        CustomTitle   NVARCHAR(500)     NULL,
        ImagePath     NVARCHAR(500)     NULL,
        UpdatedAt     DATETIME          NOT NULL CONSTRAINT DF_V360FS_UpdAt DEFAULT (GETDATE()),
        UpdatedBy     INT               NULL,
        CONSTRAINT UQ_V360FS_CidUuid UNIQUE (CollectionId, SceneUuid)
    );
    CREATE INDEX IX_V360FS_CidOrd ON dbo.V360_FeaturedScene(CollectionId, DisplayOrder);
    PRINT '[1] Created table V360_FeaturedScene';
END
ELSE
BEGIN
    PRINT '[1] V360_FeaturedScene already exists - skipped';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'V360_TourConfig' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.V360_TourConfig (
        Id             INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        CollectionId   NVARCHAR(50)      NOT NULL UNIQUE,
        PinSize        INT               NULL,
        PinColor       NVARCHAR(20)      NULL,
        SelectedColor  NVARCHAR(20)      NULL,
        ConeColor      NVARCHAR(20)      NULL,
        ConeFanDeg     FLOAT             NULL,
        ConeRadius     INT               NULL,
        UpdatedAt      DATETIME          NOT NULL CONSTRAINT DF_V360TC_UpdAt DEFAULT (GETDATE()),
        UpdatedBy      INT               NULL
    );
    PRINT '[1] Created table V360_TourConfig';
END
ELSE
BEGIN
    PRINT '[1] V360_TourConfig already exists - skipped';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'V360_ChatbotTourInfo' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.V360_ChatbotTourInfo (
        Id            INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        CollectionId  NVARCHAR(50)  NOT NULL UNIQUE,
        Overview      NVARCHAR(MAX) NULL,
        SystemPrompt  NVARCHAR(MAX) NULL,
        IsEnabled     BIT           NOT NULL CONSTRAINT DF_V360CTI_En DEFAULT (1),
        UpdatedAt     DATETIME      NOT NULL CONSTRAINT DF_V360CTI_UpdAt DEFAULT (GETDATE()),
        UpdatedBy     INT           NULL
    );
    PRINT '[1] Created table V360_ChatbotTourInfo';
END
ELSE
BEGIN
    PRINT '[1] V360_ChatbotTourInfo already exists - skipped';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'V360_ChatbotSceneInfo' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.V360_ChatbotSceneInfo (
        Id              INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        CollectionId    NVARCHAR(50)  NOT NULL,
        SceneUuid       NVARCHAR(100) NOT NULL,
        ShortIntro      NVARCHAR(500) NULL,
        DetailContent   NVARCHAR(MAX) NULL,
        UpdatedAt       DATETIME      NOT NULL CONSTRAINT DF_V360CSI_UpdAt DEFAULT (GETDATE()),
        UpdatedBy       INT           NULL,
        CONSTRAINT UQ_V360CSI_CidUuid UNIQUE (CollectionId, SceneUuid)
    );
    CREATE INDEX IX_V360CSI_Cid ON dbo.V360_ChatbotSceneInfo(CollectionId);
    PRINT '[1] Created table V360_ChatbotSceneInfo';
END
ELSE
BEGIN
    PRINT '[1] V360_ChatbotSceneInfo already exists - skipped';
END
GO

-- Drop unused Keywords column (idempotent). DB cu da co column nay -> drop khi deploy.
IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'Keywords' AND Object_ID = Object_ID('dbo.V360_ChatbotSceneInfo'))
BEGIN
    ALTER TABLE dbo.V360_ChatbotSceneInfo DROP COLUMN Keywords;
    PRINT '[1] Dropped V360_ChatbotSceneInfo.Keywords (unused, replaced by CustomTitle in SceneCalibration)';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'V360_ChatbotMessage' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.V360_ChatbotMessage (
        Id            BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        SessionGuid   UNIQUEIDENTIFIER NOT NULL,
        NhanVienID    INT           NULL,
        CollectionId  NVARCHAR(50)  NOT NULL,
        SceneUuid     NVARCHAR(100) NULL,
        Role          TINYINT       NOT NULL,
        Content       NVARCHAR(MAX) NOT NULL,
        [Action]      NVARCHAR(100) NULL,
        TokensIn      INT           NULL,
        TokensOut     INT           NULL,
        LatencyMs     INT           NULL,
        CreatedAt     DATETIME      NOT NULL CONSTRAINT DF_V360CM_At DEFAULT (GETDATE())
    );
    CREATE INDEX IX_V360CM_Session ON dbo.V360_ChatbotMessage(SessionGuid, CreatedAt);
    CREATE INDEX IX_V360CM_Cid     ON dbo.V360_ChatbotMessage(CollectionId, CreatedAt);
    PRINT '[1] Created table V360_ChatbotMessage';
END
ELSE
BEGIN
    PRINT '[1] V360_ChatbotMessage already exists - skipped';
END
GO


-- ================================================================
-- SECTION 2: ProjectsGroup.ParentIDGroup column (hierarchy 2-cap)
-- ================================================================
-- Cho phep nhom du an co cau truc parent -> child (HPDQ 2 chua 2D, Can 4, ...)

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'ParentIDGroup' AND Object_ID = Object_ID('dbo.ProjectsGroup'))
BEGIN
    ALTER TABLE dbo.ProjectsGroup ADD ParentIDGroup INT NULL;
    PRINT '[2] Added column ProjectsGroup.ParentIDGroup';
END
ELSE
BEGIN
    PRINT '[2] ProjectsGroup.ParentIDGroup already exists - skipped';
END
GO


-- ================================================================
-- SECTION 3: Insert parent groups + sub-groups (data seed)
-- ================================================================
-- Chi insert neu CHUA co (NOT EXISTS check). Cho DB moi setup.
-- Parents: HPDQ1, HPDQ2, CTH, KHO, KSMB, Cong-Camera, 3D-VR 360, Phu Yen, DA. ThepRay
-- Sub HPDQ1: Cang 11 Keo dai
-- Sub HPDQ2: 2D, NM.LG2, Du an Can 4, Du an Duc 4, Du an Lo Quay Day, Du an Kho than keo dai

DECLARE @parentGroups TABLE (Name NVARCHAR(200));
INSERT INTO @parentGroups (Name) VALUES
    (N'HPDQ1'), (N'HPDQ2'), (N'CTH'), (N'KHO'), (N'KSMB'),
    (N'Cổng-Camera'), (N'3D-VR 360'), (N'Phú Yên'), (N'DA. ThépRay');

INSERT INTO dbo.ProjectsGroup (GroupName, ParentIDGroup)
SELECT pg.Name, NULL FROM @parentGroups pg
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.ProjectsGroup g
    WHERE LTRIM(RTRIM(g.GroupName)) = LTRIM(RTRIM(pg.Name))
);
PRINT '[3] Inserted ' + CAST(@@ROWCOUNT AS NVARCHAR) + ' parent groups';
GO

DECLARE @hpdq1ID INT = (
    SELECT TOP 1 IDGroup FROM dbo.ProjectsGroup
    WHERE LTRIM(RTRIM(GroupName)) IN (N'HPDQ1', N'HPDQ 1')
      AND ParentIDGroup IS NULL ORDER BY IDGroup
);
IF @hpdq1ID IS NOT NULL
BEGIN
    DECLARE @hpdq1Subs TABLE (Name NVARCHAR(200));
    INSERT INTO @hpdq1Subs (Name) VALUES (N'Cảng 11 Kéo dài');

    INSERT INTO dbo.ProjectsGroup (GroupName, ParentIDGroup)
    SELECT s.Name, @hpdq1ID FROM @hpdq1Subs s
    WHERE NOT EXISTS (
        SELECT 1 FROM dbo.ProjectsGroup g
        WHERE LTRIM(RTRIM(g.GroupName)) = LTRIM(RTRIM(s.Name))
    );
    PRINT '[3] Inserted ' + CAST(@@ROWCOUNT AS NVARCHAR) + ' sub-groups cho HPDQ1';
END;
GO

DECLARE @hpdq2ID INT = (
    SELECT TOP 1 IDGroup FROM dbo.ProjectsGroup
    WHERE LTRIM(RTRIM(GroupName)) IN (N'HPDQ2', N'HPDQ 2')
      AND ParentIDGroup IS NULL ORDER BY IDGroup
);
IF @hpdq2ID IS NOT NULL
BEGIN
    DECLARE @hpdq2Subs TABLE (Name NVARCHAR(200));
    INSERT INTO @hpdq2Subs (Name) VALUES
        (N'2D'), (N'NM.LG2'), (N'Dự án Cán 4'), (N'Dự án Đúc 4'),
        (N'Dự án Lò Quay Đáy'), (N'Dự án Kho than kéo dài');

    INSERT INTO dbo.ProjectsGroup (GroupName, ParentIDGroup)
    SELECT s.Name, @hpdq2ID FROM @hpdq2Subs s
    WHERE NOT EXISTS (
        SELECT 1 FROM dbo.ProjectsGroup g
        WHERE LTRIM(RTRIM(g.GroupName)) = LTRIM(RTRIM(s.Name))
    );
    PRINT '[3] Inserted ' + CAST(@@ROWCOUNT AS NVARCHAR) + ' sub-groups cho HPDQ2';
END;
GO


-- ================================================================
-- SECTION 4: Re-link sub-groups voi name tolerance
-- ================================================================
-- Xu ly data inconsistency: "HPDQ 1" co space khac "HPDQ1" -> linking sai.
-- LTRIM/RTRIM va alternate names list de tim parent ID.

DECLARE @hpdq1ID INT = (
    SELECT TOP 1 IDGroup FROM dbo.ProjectsGroup
    WHERE LTRIM(RTRIM(GroupName)) IN (N'HPDQ1', N'HPDQ 1', N'HPDQ-1')
      AND ParentIDGroup IS NULL ORDER BY IDGroup
);
IF @hpdq1ID IS NOT NULL
BEGIN
    UPDATE dbo.ProjectsGroup
    SET ParentIDGroup = @hpdq1ID
    WHERE LTRIM(RTRIM(GroupName)) IN (
        N'Cảng 11 Kéo dài', N'Cang 11 Keo dai', N'Cảng 11 keo dài'
    )
      AND IDGroup <> @hpdq1ID
      AND (ParentIDGroup IS NULL OR ParentIDGroup <> @hpdq1ID);
    PRINT '[4] Re-linked ' + CAST(@@ROWCOUNT AS NVARCHAR) + ' sub-groups vao HPDQ1';
END;
GO

DECLARE @hpdq2ID INT = (
    SELECT TOP 1 IDGroup FROM dbo.ProjectsGroup
    WHERE LTRIM(RTRIM(GroupName)) IN (N'HPDQ2', N'HPDQ 2', N'HPDQ-2')
      AND ParentIDGroup IS NULL ORDER BY IDGroup
);
IF @hpdq2ID IS NOT NULL
BEGIN
    UPDATE dbo.ProjectsGroup
    SET ParentIDGroup = @hpdq2ID
    WHERE LTRIM(RTRIM(GroupName)) IN (
        N'2D',
        N'NM.LG2', N'NM.LG 2',
        N'Dự án Cán 4', N'Du an Can 4',
        N'Dự án Đúc 4', N'Du an Duc 4',
        N'Dự án Lò Quay Đáy', N'Du an Lo Quay Day',
        N'Dự án Kho than kéo dài', N'Dự án Kho than keo dài', N'Du an Kho than keo dai'
    )
      AND IDGroup <> @hpdq2ID
      AND (ParentIDGroup IS NULL OR ParentIDGroup <> @hpdq2ID);
    PRINT '[4] Re-linked ' + CAST(@@ROWCOUNT AS NVARCHAR) + ' sub-groups vao HPDQ2';
END;
GO


-- ================================================================
-- SECTION 5: Cleanup duplicates (historical fix, GUARDED for production safety)
-- ================================================================
-- Migration cu (hierarchy.sql ban dau) khong dung LTRIM check -> tao duplicates
-- khi DB co names voi trailing space ("HPDQ 1" vs "HPDQ1"). Cleanup logic:
--   - Reassign children cua duplicate parents sang parent goc
--   - Xoa duplicate rows ID 23-33 (specific to original dev DB state)
--
-- !!! PRODUCTION SAFETY: ID 23-33 la hardcode tu dev DB. Production co the
--     co data hop le tai cac ID nay. Guard:
--     CHI chay neu detect duoc DUPLICATE PATTERN (cung GroupName, ParentIDGroup NULL).
--     Production binh thuong khong co duplicate -> section nay skip silently.

-- Cleanup chi chay tren cac TEN da biet tu bug migration cu (HPDQ1/HPDQ2 voi space).
-- Production binh thuong khong co duplicate -> skip silently.
-- Allowlist nay tranh xoa cap duplicate legit (vd: 2 phong ban thuc su trung ten).
DECLARE @KnownDupNames TABLE (Nm NVARCHAR(200));
INSERT INTO @KnownDupNames (Nm) VALUES
    (N'HPDQ1'), (N'HPDQ 1'), (N'HPDQ-1'),
    (N'HPDQ2'), (N'HPDQ 2'), (N'HPDQ-2');

DECLARE @hasDupes INT = (
    SELECT COUNT(*) FROM (
        SELECT LTRIM(RTRIM(GroupName)) AS Nm
        FROM dbo.ProjectsGroup
        WHERE ParentIDGroup IS NULL
          AND LTRIM(RTRIM(GroupName)) IN (SELECT Nm FROM @KnownDupNames)
        GROUP BY LTRIM(RTRIM(GroupName))
        HAVING COUNT(*) > 1
    ) d
);

IF @hasDupes > 0
BEGIN
    PRINT '[5] Detected ' + CAST(@hasDupes AS NVARCHAR)
        + ' duplicate KNOWN-NAME parents - running cleanup (allowlist: HPDQ1/HPDQ2 variants)';

    DECLARE @cleanupReassigned INT = 0;
    ;WITH RankedParents AS (
        SELECT IDGroup, GroupName,
               ROW_NUMBER() OVER (PARTITION BY LTRIM(RTRIM(GroupName)) ORDER BY IDGroup) AS rn
        FROM dbo.ProjectsGroup
        WHERE ParentIDGroup IS NULL
          AND LTRIM(RTRIM(GroupName)) IN (SELECT Nm FROM @KnownDupNames)
    ),
    Mapping AS (
        SELECT dup.IDGroup AS DupId, orig.IDGroup AS OrigId
        FROM RankedParents dup
        JOIN RankedParents orig ON LTRIM(RTRIM(dup.GroupName)) = LTRIM(RTRIM(orig.GroupName))
                               AND orig.rn = 1
        WHERE dup.rn > 1
    )
    UPDATE g SET ParentIDGroup = m.OrigId
    FROM dbo.ProjectsGroup g
    JOIN Mapping m ON g.ParentIDGroup = m.DupId;
    SET @cleanupReassigned = @@ROWCOUNT;
    PRINT '[5] Reassigned ' + CAST(@cleanupReassigned AS NVARCHAR) + ' children tu duplicate parents';

    DECLARE @cleanupDeleted INT = 0;
    ;WITH RankedParents AS (
        SELECT IDGroup, GroupName,
               ROW_NUMBER() OVER (PARTITION BY LTRIM(RTRIM(GroupName)) ORDER BY IDGroup) AS rn
        FROM dbo.ProjectsGroup
        WHERE ParentIDGroup IS NULL
          AND LTRIM(RTRIM(GroupName)) IN (SELECT Nm FROM @KnownDupNames)
    )
    DELETE g
    FROM dbo.ProjectsGroup g
    JOIN RankedParents rp ON rp.IDGroup = g.IDGroup AND rp.rn > 1
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Projects p WHERE p.IDGroup = g.IDGroup)
      AND NOT EXISTS (SELECT 1 FROM dbo.ProjectsGroup c WHERE c.ParentIDGroup = g.IDGroup);
    SET @cleanupDeleted = @@ROWCOUNT;
    PRINT '[5] Deleted ' + CAST(@cleanupDeleted AS NVARCHAR) + ' empty duplicate parent rows';
END
ELSE
BEGIN
    PRINT '[5] No known-name duplicate parents detected - cleanup skipped (production-safe)';
END;
GO


-- ================================================================
-- SECTION 6: ProjectsGroup.SortOrder (admin sap xep thu tu folder)
-- ================================================================
-- Folders cung sibling sap xep theo SortOrder ASC -> GroupName ASC.
-- Init: ROW_NUMBER theo alphabet trong moi parent group (admin doi sau).

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'SortOrder' AND Object_ID = Object_ID('dbo.ProjectsGroup'))
BEGIN
    ALTER TABLE dbo.ProjectsGroup ADD SortOrder INT NOT NULL CONSTRAINT DF_ProjectsGroup_SortOrder DEFAULT 0;
    PRINT '[6] Added column ProjectsGroup.SortOrder';
END
ELSE
BEGIN
    PRINT '[6] ProjectsGroup.SortOrder already exists - skipped';
END
GO

-- Init SortOrder: chi chay khi TAT CA rows = 0 (lan dau setup, chua admin nao tuy chinh).
-- Tranh ghi de SortOrder=0 admin co the set co y (vd: pin lan top hoac default order).
DECLARE @totalRows INT, @zeroRows INT;
SELECT @totalRows = COUNT(*), @zeroRows = SUM(CASE WHEN SortOrder = 0 THEN 1 ELSE 0 END)
FROM dbo.ProjectsGroup;

IF @totalRows > 0 AND @totalRows = @zeroRows
BEGIN
    ;WITH cte AS (
        SELECT IDGroup,
               ROW_NUMBER() OVER (PARTITION BY ISNULL(ParentIDGroup, 0) ORDER BY GroupName) AS rn
        FROM dbo.ProjectsGroup
    )
    UPDATE g SET SortOrder = cte.rn
    FROM dbo.ProjectsGroup g
    JOIN cte ON g.IDGroup = cte.IDGroup;
    PRINT '[6] Initialized SortOrder cho ' + CAST(@@ROWCOUNT AS NVARCHAR) + ' rows (first-time setup)';
END
ELSE
    PRINT '[6] SortOrder already initialized for some/all rows - init skipped (preserve admin customization)';
GO


-- ================================================================
-- SECTION 7: View360_AccessLog (page-hit tracking)
-- ================================================================
-- Ghi mot row khi user mo Details cua Project/Virtual tour.
-- Phuc vu CA:
--   - Admin dashboard: DAU/WAU/MAU, top content, permission utilization
--   - User-facing: "da xem / chua xem" indicator tren card
-- Dedupe 30 phut/session o tang ung dung de tranh dem F5.

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'View360_AccessLog' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.View360_AccessLog (
        ID          BIGINT IDENTITY(1,1) NOT NULL,
        NhanVienID  INT      NOT NULL,
        ContentType TINYINT  NOT NULL,   -- 1=Project, 2=Virtual, 3=Video
        ContentID   INT      NOT NULL,
        AccessAt    DATETIME NOT NULL CONSTRAINT DF_View360_AccessLog_AccessAt DEFAULT (GETDATE()),
        SessionID   VARCHAR(64) NULL,
        CONSTRAINT PK_View360_AccessLog PRIMARY KEY CLUSTERED (ID)
    );
    PRINT '[7] Created table View360_AccessLog';
END
ELSE
BEGIN
    PRINT '[7] View360_AccessLog already exists - skipped';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_View360_AccessLog_AccessAt' AND object_id = OBJECT_ID('dbo.View360_AccessLog'))
    CREATE INDEX IX_View360_AccessLog_AccessAt
        ON dbo.View360_AccessLog (AccessAt DESC);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_View360_AccessLog_NhanVien' AND object_id = OBJECT_ID('dbo.View360_AccessLog'))
    CREATE INDEX IX_View360_AccessLog_NhanVien
        ON dbo.View360_AccessLog (NhanVienID, AccessAt DESC);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_View360_AccessLog_Content' AND object_id = OBJECT_ID('dbo.View360_AccessLog'))
    CREATE INDEX IX_View360_AccessLog_Content
        ON dbo.View360_AccessLog (ContentType, ContentID, AccessAt DESC);
GO


-- ================================================================
-- SECTION 8: Kuula scene cache - persistent fallback + GPS uuid drift detection
-- ================================================================
-- KuulaCollectionFetcher lay scenes tu share page HTML cache 24h MemoryCache.
-- Khi Kuula doi schema/break, pool recycle hoac doi uuid -> mat data / orphan calib.
-- Bang nay luu snapshot scenes da fetch, phuc vu:
--   1) Fallback khi fetch fail (last-known data)
--   2) GPS drift detection: cung lat/lng nhung uuid khac -> auto-migrate calib

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'View360_KuulaSceneCache' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.View360_KuulaSceneCache (
        Id              BIGINT IDENTITY(1,1) NOT NULL,
        CollectionId    NVARCHAR(50)  NOT NULL,
        SceneUuid       NVARCHAR(100) NOT NULL,
        SceneRuntimeId  NVARCHAR(50)  NULL,
        Title           NVARCHAR(500) NULL,
        Description     NVARCHAR(MAX) NULL,
        Lat             FLOAT         NULL,
        Lng             FLOAT         NULL,
        CameraHeading   FLOAT         NULL,
        SpotsJson       NVARCHAR(MAX) NULL,
        FirstSeenAt     DATETIME      NOT NULL CONSTRAINT DF_KSC_FirstSeen DEFAULT (GETDATE()),
        LastSeenAt      DATETIME      NOT NULL CONSTRAINT DF_KSC_LastSeen  DEFAULT (GETDATE()),
        UpdatedAt       DATETIME      NOT NULL CONSTRAINT DF_KSC_Updated   DEFAULT (GETDATE()),
        CONSTRAINT PK_KuulaSceneCache PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT UQ_KuulaSceneCache UNIQUE (CollectionId, SceneUuid)
    );
    PRINT '[8] Created table View360_KuulaSceneCache';
END
ELSE
    PRINT '[8] View360_KuulaSceneCache already exists - skipped';
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_KuulaSceneCache_Cid' AND object_id = OBJECT_ID('dbo.View360_KuulaSceneCache'))
    CREATE INDEX IX_KuulaSceneCache_Cid
        ON dbo.View360_KuulaSceneCache (CollectionId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_KuulaSceneCache_GPS' AND object_id = OBJECT_ID('dbo.View360_KuulaSceneCache'))
    CREATE INDEX IX_KuulaSceneCache_GPS
        ON dbo.View360_KuulaSceneCache (CollectionId, Lat, Lng)
        WHERE Lat IS NOT NULL AND Lng IS NOT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'View360_KuulaUuidDrift' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.View360_KuulaUuidDrift (
        Id             BIGINT IDENTITY(1,1) NOT NULL,
        CollectionId   NVARCHAR(50)  NOT NULL,
        OldUuid        NVARCHAR(100) NOT NULL,
        NewUuid        NVARCHAR(100) NOT NULL,
        Lat            FLOAT         NULL,
        Lng            FLOAT         NULL,
        DistanceMeters FLOAT         NULL,
        DetectedAt     DATETIME      NOT NULL CONSTRAINT DF_KUD_Detected DEFAULT (GETDATE()),
        CalibMigrated  BIT           NOT NULL CONSTRAINT DF_KUD_Migrated  DEFAULT (0),
        CONSTRAINT PK_KuulaUuidDrift PRIMARY KEY CLUSTERED (Id)
    );
    CREATE INDEX IX_KuulaUuidDrift_Cid
        ON dbo.View360_KuulaUuidDrift (CollectionId, DetectedAt DESC);
    PRINT '[8] Created table View360_KuulaUuidDrift';
END
ELSE
    PRINT '[8] View360_KuulaUuidDrift already exists - skipped';
GO


-- ================================================================
-- SECTION 9: Permission hybrid - AuthorizationUSER_Group + 3 SP _select_USER
-- ================================================================
-- Doi tu schema per-content (legacy AuthorizationUSER/Vitual/Video) sang hybrid:
-- cho phep cap quyen muc NHOM (group-grant) + file-grant le.
--
-- !!! QUAN TRONG !!!
--   - GHI DE 3 SP: Project_select_USER, Virtual_select_USER, Video_select.
--     Backup TRUOC khi chay (xem header file).
--   - Idempotent: NOT EXISTS check + UQ constraint -> chay lai khong duplicate.
--   - File-grant cu KHONG bi xoa -> chay song song voi group-grant moi.

-- 9.1 Create AuthorizationUSER_Group table
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AuthorizationUSER_Group' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.AuthorizationUSER_Group (
        ID          INT IDENTITY(1,1) NOT NULL,
        NhanVienID  INT     NOT NULL,
        ContentType TINYINT NOT NULL,            -- 1=ProjectsGroup, 2=VirtualGroup, 3=Album
        IDGroup     INT     NOT NULL,
        [Recursive] BIT     NOT NULL CONSTRAINT DF_AuthUserGroup_Recursive DEFAULT (0),
        Createdate  DATETIME NOT NULL CONSTRAINT DF_AuthUserGroup_Createdate DEFAULT (GETDATE()),
        CONSTRAINT PK_AuthUserGroup PRIMARY KEY CLUSTERED (ID),
        CONSTRAINT UQ_AuthUserGroup UNIQUE (NhanVienID, ContentType, IDGroup)
    );
    PRINT '[9.1] Created table AuthorizationUSER_Group';
END
ELSE
    PRINT '[9.1] AuthorizationUSER_Group already exists - skipped';
GO

-- 9.2 Indexes
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AuthUserGroup_User' AND object_id = OBJECT_ID('dbo.AuthorizationUSER_Group'))
    CREATE INDEX IX_AuthUserGroup_User
        ON dbo.AuthorizationUSER_Group (NhanVienID);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AuthUserGroup_Group' AND object_id = OBJECT_ID('dbo.AuthorizationUSER_Group'))
    CREATE INDEX IX_AuthUserGroup_Group
        ON dbo.AuthorizationUSER_Group (ContentType, IDGroup);
GO

-- 9.3 Data migration legacy -> group-grant.
-- Rule: user co file-grant tren project MOI NHAT (MAX ID) cua group
--       -> tu dong cap group-grant cho user do.
--
-- GUARD: chi chay LAN DAU (table empty). Re-run khi co data dan den behavior chua xac dinh:
-- neu admin them project moi sau migration, MAX(ID) thay doi -> NOT EXISTS check khong
-- ngan duoc cap grant moi cho user file-grant tren project moi nhat moi. Skip de an toan.
IF NOT EXISTS (SELECT 1 FROM dbo.AuthorizationUSER_Group)
BEGIN
    PRINT '[9.3] Table empty - running legacy data migration';

DECLARE @inserted INT;

INSERT INTO dbo.AuthorizationUSER_Group (NhanVienID, ContentType, IDGroup, [Recursive], Createdate)
SELECT DISTINCT au.NhanVienID, 1, p.IDGroup, 0, GETDATE()
FROM dbo.AuthorizationUSER au
JOIN dbo.Projects p ON au.ProjectID = p.ID
JOIN (
    SELECT IDGroup, MAX(ID) AS MaxId
    FROM dbo.Projects
    WHERE IDGroup IS NOT NULL AND IDGroup > 0
    GROUP BY IDGroup
) latest ON p.IDGroup = latest.IDGroup AND p.ID = latest.MaxId
WHERE au.NhanVienID IS NOT NULL AND p.IDGroup IS NOT NULL AND p.IDGroup > 0
  AND NOT EXISTS (
      SELECT 1 FROM dbo.AuthorizationUSER_Group g
      WHERE g.NhanVienID = au.NhanVienID AND g.ContentType = 1 AND g.IDGroup = p.IDGroup
  );
SET @inserted = @@ROWCOUNT;
PRINT '[9.3a] Project group-grants inserted: ' + CAST(@inserted AS VARCHAR);

INSERT INTO dbo.AuthorizationUSER_Group (NhanVienID, ContentType, IDGroup, [Recursive], Createdate)
SELECT DISTINCT av.NhanVienID, 2, v.IDGroup, 0, GETDATE()
FROM dbo.AuthorizationVitual av
JOIN dbo.Virtual v ON av.VirtualID = v.ID
JOIN (
    SELECT IDGroup, MAX(ID) AS MaxId
    FROM dbo.Virtual
    WHERE IDGroup IS NOT NULL AND IDGroup > 0
    GROUP BY IDGroup
) latest ON v.IDGroup = latest.IDGroup AND v.ID = latest.MaxId
WHERE av.NhanVienID IS NOT NULL AND v.IDGroup IS NOT NULL AND v.IDGroup > 0
  AND NOT EXISTS (
      SELECT 1 FROM dbo.AuthorizationUSER_Group g
      WHERE g.NhanVienID = av.NhanVienID AND g.ContentType = 2 AND g.IDGroup = v.IDGroup
  );
SET @inserted = @@ROWCOUNT;
PRINT '[9.3b] Virtual group-grants inserted: ' + CAST(@inserted AS VARCHAR);

INSERT INTO dbo.AuthorizationUSER_Group (NhanVienID, ContentType, IDGroup, [Recursive], Createdate)
SELECT DISTINCT avi.NhanVienID, 3, vd.AlbumID, 0, GETDATE()
FROM dbo.AuthorizationVideo avi
JOIN dbo.Video vd ON avi.VideoID = vd.IDVideo
JOIN (
    SELECT AlbumID, MAX(IDVideo) AS MaxId
    FROM dbo.Video
    WHERE AlbumID IS NOT NULL AND AlbumID > 0
    GROUP BY AlbumID
) latest ON vd.AlbumID = latest.AlbumID AND vd.IDVideo = latest.MaxId
WHERE avi.NhanVienID IS NOT NULL AND vd.AlbumID IS NOT NULL AND vd.AlbumID > 0
  AND NOT EXISTS (
      SELECT 1 FROM dbo.AuthorizationUSER_Group g
      WHERE g.NhanVienID = avi.NhanVienID AND g.ContentType = 3 AND g.IDGroup = vd.AlbumID
  );
SET @inserted = @@ROWCOUNT;
PRINT '[9.3c] Video album-grants inserted: ' + CAST(@inserted AS VARCHAR);
END
ELSE
    PRINT '[9.3] AuthorizationUSER_Group has data - data migration skipped (run cleanup or manual sync if needed)';
GO

-- 9.4 Recreate 3 SPs. Dung CREATE OR ALTER (SQL 2016+) - atomic, neu CREATE loi
-- thi SP cu giu nguyen (rollback-safe).
--
-- Version check: dung SET NOEXEC ON + RAISERROR de bao loi VA skip 3 batch CREATE
-- duoi (vi CREATE OR ALTER khong parse duoc tren SQL < 2016 -> NOEXEC tat execute,
-- script tiep tuc chay den SECTION 10+ binh thuong). Sau SECTION 9.4, NOEXEC OFF.
IF CAST(SERVERPROPERTY('ProductMajorVersion') AS INT) < 13
BEGIN
    PRINT '!!! SECTION 9.4 SKIPPED: requires SQL Server 2016+ (CREATE OR ALTER syntax).';
    PRINT '!!! Detected ProductMajorVersion = ' + CAST(SERVERPROPERTY('ProductMajorVersion') AS NVARCHAR);
    PRINT '!!! To deploy SP changes on older SQL, run v360-auth-group-hybrid.sql separately (DROP+CREATE pattern).';
    PRINT '!!! Continuing with SECTION 10+ ...';
    SET NOEXEC ON;
END;
GO

CREATE OR ALTER PROCEDURE dbo.Project_select_USER
    @search NVARCHAR(MAX) = '',
    @NhanVienID INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @AccessibleGroups TABLE (IDGroup INT PRIMARY KEY);

    -- (a) Group-grant direct (Recursive=0)
    INSERT INTO @AccessibleGroups (IDGroup)
    SELECT DISTINCT IDGroup
    FROM dbo.AuthorizationUSER_Group
    WHERE NhanVienID = @NhanVienID AND ContentType = 1 AND [Recursive] = 0;

    -- (b) Group-grant recursive: expand sang tat ca con chau
    ;WITH RecursiveRoots AS (
        SELECT IDGroup
        FROM dbo.AuthorizationUSER_Group
        WHERE NhanVienID = @NhanVienID AND ContentType = 1 AND [Recursive] = 1
    ),
    Descendants AS (
        SELECT IDGroup, IDGroup AS RootId FROM RecursiveRoots
        UNION ALL
        SELECT pg.IDGroup, d.RootId
        FROM dbo.ProjectsGroup pg
        JOIN Descendants d ON pg.ParentIDGroup = d.IDGroup
    )
    INSERT INTO @AccessibleGroups (IDGroup)
    SELECT DISTINCT d.IDGroup
    FROM Descendants d
    WHERE NOT EXISTS (SELECT 1 FROM @AccessibleGroups ag WHERE ag.IDGroup = d.IDGroup);

    -- Tra ket qua: file-grant HOAC IDGroup trong @AccessibleGroups
    SELECT
        p.ID, p.Title, p.Images, p.Note, p.FilePDF, p.URL, p.Date,
        p.IDPhongBan, pb.TenPhongBan, p.IDGroup
    FROM dbo.Projects p
    LEFT JOIN dbo.PhongBan pb ON p.IDPhongBan = pb.IDPhongBan
    WHERE
    (
        EXISTS (
            SELECT 1 FROM dbo.AuthorizationUSER au
            WHERE au.NhanVienID = @NhanVienID AND au.ProjectID = p.ID
        )
        OR p.IDGroup IN (SELECT IDGroup FROM @AccessibleGroups)
    )
    AND (@search IS NULL OR @search = '' OR p.Title LIKE '%' + @search + '%');
END;
GO
PRINT '[9.4a] (Re)created dbo.Project_select_USER';
GO

CREATE OR ALTER PROCEDURE dbo.Virtual_select_USER
    @search NVARCHAR(MAX) = '',
    @NhanVienID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        v.ID, v.Title, v.Images, v.Note, v.FilePDF, v.URL, v.Date,
        v.IDPhongBan, pb.TenPhongBan, v.IDGroup
    FROM dbo.Virtual v
    LEFT JOIN dbo.PhongBan pb ON v.IDPhongBan = pb.IDPhongBan
    WHERE
    (
        EXISTS (
            SELECT 1 FROM dbo.AuthorizationVitual av
            WHERE av.NhanVienID = @NhanVienID AND av.VirtualID = v.ID
        )
        OR EXISTS (
            SELECT 1 FROM dbo.AuthorizationUSER_Group g
            WHERE g.NhanVienID = @NhanVienID AND g.ContentType = 2 AND g.IDGroup = v.IDGroup
        )
    )
    AND (@search IS NULL OR @search = '' OR v.Title LIKE '%' + @search + '%');
END;
GO
PRINT '[9.4b] (Re)created dbo.Virtual_select_USER';
GO

CREATE OR ALTER PROCEDURE dbo.Video_select
    @search NVARCHAR(MAX) = '',
    @NhanVienID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        vd.IDVideo, vd.Title, vd.Images, vd.Note, vd.URL, vd.Date,
        vd.IDPhongBan, pb.TenPhongBan, vd.AlbumID
    FROM dbo.Video vd
    LEFT JOIN dbo.PhongBan pb ON vd.IDPhongBan = pb.IDPhongBan
    WHERE
    (
        EXISTS (
            SELECT 1 FROM dbo.AuthorizationVideo avi
            WHERE avi.NhanVienID = @NhanVienID AND avi.VideoID = vd.IDVideo
        )
        OR EXISTS (
            SELECT 1 FROM dbo.AuthorizationUSER_Group g
            WHERE g.NhanVienID = @NhanVienID AND g.ContentType = 3 AND g.IDGroup = vd.AlbumID
        )
    )
    AND (@search IS NULL OR @search = '' OR vd.Title LIKE '%' + @search + '%');
END;
GO
PRINT '[9.4c] (Re)created dbo.Video_select';
GO

-- Re-enable execution (NOEXEC ON co the da bat trong version-check tren).
SET NOEXEC OFF;
GO


-- ================================================================
-- SECTION 10: Cleanup orphan group-grants
-- ================================================================
-- Xoa rows IDGroup=0 (do Project/Virtual/Album=0 thay vi NULL) hoac
-- IDGroup tro toi target khong ton tai (xoa data sau migration).
-- Guard: chi chay khi AuthorizationUSER_Group da ton tai (SECTION 9 chay xong).
-- Idempotent: re-run no-op khi khong con orphan.

IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AuthorizationUSER_Group' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    DECLARE @delProj INT, @delVir INT, @delVid INT;

    DELETE g FROM dbo.AuthorizationUSER_Group g
    WHERE g.ContentType = 1
      AND (g.IDGroup = 0
           OR NOT EXISTS (SELECT 1 FROM dbo.ProjectsGroup p WHERE p.IDGroup = g.IDGroup));
    SET @delProj = @@ROWCOUNT;

    DELETE g FROM dbo.AuthorizationUSER_Group g
    WHERE g.ContentType = 2
      AND (g.IDGroup = 0
           OR NOT EXISTS (SELECT 1 FROM dbo.VirtualGroup v WHERE v.IDGroup = g.IDGroup));
    SET @delVir = @@ROWCOUNT;

    DELETE g FROM dbo.AuthorizationUSER_Group g
    WHERE g.ContentType = 3
      AND (g.IDGroup = 0
           OR NOT EXISTS (SELECT 1 FROM dbo.Album a WHERE a.IDAlbum = g.IDGroup));
    SET @delVid = @@ROWCOUNT;

    PRINT '[10] Orphans deleted - Project=' + CAST(@delProj AS VARCHAR)
        + ' Virtual=' + CAST(@delVir AS VARCHAR)
        + ' Video=' + CAST(@delVid AS VARCHAR);
END
ELSE
    PRINT '[10] AuthorizationUSER_Group missing - cleanup skipped (SECTION 9 failed?)';
GO


-- ================================================================
-- SECTION 11: Verify - hien thi final state
-- ================================================================

PRINT '';
PRINT '=== FINAL VERIFY ===';
PRINT '';
PRINT '--- ProjectsGroup hierarchy ---';
SELECT
    g.IDGroup,
    g.GroupName,
    g.ParentIDGroup,
    p.GroupName AS ParentName,
    g.SortOrder,
    (SELECT COUNT(*) FROM dbo.Projects pj WHERE pj.IDGroup = g.IDGroup) AS DirectProjects
FROM dbo.ProjectsGroup g
LEFT JOIN dbo.ProjectsGroup p ON p.IDGroup = g.ParentIDGroup
ORDER BY ISNULL(g.ParentIDGroup, g.IDGroup), g.SortOrder, g.GroupName;
GO

PRINT '';
PRINT '--- View360 tables ---';
SELECT
    t.name AS TableName,
    SUM(p.rows) AS RowCount_Approx
FROM sys.tables t
JOIN sys.partitions p ON p.object_id = t.object_id AND p.index_id IN (0, 1)
WHERE t.name IN ('V360_SceneCalibration', 'V360_FeaturedScene', 'V360_TourConfig',
                 'V360_ChatbotTourInfo', 'V360_ChatbotSceneInfo', 'V360_ChatbotMessage',
                 'View360_AccessLog', 'View360_KuulaSceneCache', 'View360_KuulaUuidDrift',
                 'AuthorizationUSER_Group')
GROUP BY t.name
ORDER BY t.name;
GO

PRINT '';
PRINT '--- ProjectsGroup column check ---';
SELECT
    c.name AS ColumnName,
    ty.name AS DataType,
    c.is_nullable AS IsNullable
FROM sys.columns c
JOIN sys.types ty ON ty.user_type_id = c.user_type_id
WHERE c.object_id = OBJECT_ID('dbo.ProjectsGroup')
  AND c.name IN ('ParentIDGroup', 'SortOrder');
GO

PRINT '';
PRINT '--- Permission hybrid grants (by ContentType) ---';
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AuthorizationUSER_Group' AND schema_id = SCHEMA_ID('dbo'))
    SELECT
        ContentType,
        CASE ContentType
            WHEN 1 THEN 'Project'
            WHEN 2 THEN 'Virtual'
            WHEN 3 THEN 'Video (Album)'
            ELSE 'Unknown'
        END AS Loai,
        COUNT(*) AS GrantCount,
        SUM(CAST([Recursive] AS INT)) AS RecursiveCount
    FROM dbo.AuthorizationUSER_Group
    GROUP BY ContentType
    ORDER BY ContentType;
GO

PRINT '';
PRINT '--- Stored procedures recreated by SECTION 9 ---';
SELECT p.name AS ProcedureName, m.modify_date AS LastModified
FROM sys.procedures p
JOIN sys.objects m ON m.object_id = p.object_id
WHERE p.name IN ('Project_select_USER', 'Virtual_select_USER', 'Video_select')
ORDER BY p.name;
GO

PRINT '';
PRINT '=== MIGRATION COMPLETE ===';
GO

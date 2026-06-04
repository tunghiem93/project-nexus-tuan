-- =============================================================================
-- User Service — Nexus_User
-- SQL Server 2025/2026 | T-SQL
-- =============================================================================

USE [Nexus_User];
GO

-- ---------------------------------------------------------------------------
-- RBAC
-- ---------------------------------------------------------------------------
CREATE TABLE dbo.privileges (
    id          UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_privileges_id DEFAULT NEWSEQUENTIALID(),
    code        NVARCHAR(100)    NOT NULL,
    description NVARCHAR(MAX)    NULL,
    created_at  DATETIMEOFFSET   NOT NULL CONSTRAINT DF_privileges_created DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT PK_privileges PRIMARY KEY (id),
    CONSTRAINT UQ_privileges_code UNIQUE (code)
);
GO

CREATE TABLE dbo.roles (
    id          UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_roles_id DEFAULT NEWSEQUENTIALID(),
    code        NVARCHAR(50)     NOT NULL,
    name        NVARCHAR(100)    NOT NULL,
    description NVARCHAR(MAX)    NULL,
    is_system   BIT              NOT NULL CONSTRAINT DF_roles_is_system DEFAULT 0,
    deleted_at  DATETIMEOFFSET   NULL,
    created_at  DATETIMEOFFSET   NOT NULL CONSTRAINT DF_roles_created DEFAULT SYSDATETIMEOFFSET(),
    updated_at  DATETIMEOFFSET   NOT NULL CONSTRAINT DF_roles_updated DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT PK_roles PRIMARY KEY (id),
    CONSTRAINT UQ_roles_code UNIQUE (code)
);
GO

CREATE TABLE dbo.role_privileges (
    role_id       UNIQUEIDENTIFIER NOT NULL,
    privilege_id  UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT PK_role_privileges PRIMARY KEY (role_id, privilege_id),
    CONSTRAINT FK_role_privileges_role FOREIGN KEY (role_id) REFERENCES dbo.roles(id),
    CONSTRAINT FK_role_privileges_privilege FOREIGN KEY (privilege_id) REFERENCES dbo.privileges(id)
);
GO

-- ---------------------------------------------------------------------------
-- Users
-- ---------------------------------------------------------------------------
CREATE TABLE dbo.users (
    id              UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_users_id DEFAULT NEWSEQUENTIALID(),
    email           NVARCHAR(255)    NOT NULL,
    phone           NVARCHAR(20)     NOT NULL,
    username        NVARCHAR(100)    NOT NULL,
    password_hash   NVARCHAR(255)    NOT NULL,
    full_name       NVARCHAR(200)    NOT NULL,
    identify_number NVARCHAR(50)     NOT NULL,
    gender          NVARCHAR(20)     NOT NULL CONSTRAINT DF_users_gender DEFAULT N'UNSPECIFIED',
    address         NVARCHAR(MAX)    NOT NULL,
    date_of_birth   DATE             NOT NULL,
    status          NVARCHAR(20)     NOT NULL CONSTRAINT DF_users_status DEFAULT N'ACTIVE',
    deleted_at      DATETIMEOFFSET   NULL,
    created_at      DATETIMEOFFSET   NOT NULL CONSTRAINT DF_users_created DEFAULT SYSDATETIMEOFFSET(),
    updated_at      DATETIMEOFFSET   NOT NULL CONSTRAINT DF_users_updated DEFAULT SYSDATETIMEOFFSET(),
    created_by      UNIQUEIDENTIFIER NULL,
    updated_by      UNIQUEIDENTIFIER NULL,
    CONSTRAINT PK_users PRIMARY KEY (id),
    CONSTRAINT CK_users_gender CHECK (gender IN (N'MALE', N'FEMALE', N'OTHER', N'UNSPECIFIED')),
    CONSTRAINT UQ_users_email UNIQUE (email),
    CONSTRAINT UQ_users_username UNIQUE (username)
);
GO

CREATE INDEX IX_users_full_name ON dbo.users(full_name);
CREATE INDEX IX_users_status ON dbo.users(status) WHERE deleted_at IS NULL;
GO

CREATE TABLE dbo.user_roles (
    user_id     UNIQUEIDENTIFIER NOT NULL,
    role_id     UNIQUEIDENTIFIER NOT NULL,
    assigned_at DATETIMEOFFSET   NOT NULL CONSTRAINT DF_user_roles_assigned DEFAULT SYSDATETIMEOFFSET(),
    assigned_by UNIQUEIDENTIFIER NULL,
    CONSTRAINT PK_user_roles PRIMARY KEY (user_id, role_id),
    CONSTRAINT FK_user_roles_user FOREIGN KEY (user_id) REFERENCES dbo.users(id),
    CONSTRAINT FK_user_roles_role FOREIGN KEY (role_id) REFERENCES dbo.roles(id)
);
GO

-- ---------------------------------------------------------------------------
-- Authentication
-- ---------------------------------------------------------------------------
CREATE TABLE dbo.user_sessions (
    id            UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_user_sessions_id DEFAULT NEWSEQUENTIALID(),
    user_id       UNIQUEIDENTIFIER NOT NULL,
    refresh_token NVARCHAR(512)    NOT NULL,
    access_jti    NVARCHAR(64)     NULL,
    ip_address    NVARCHAR(45)     NULL,
    user_agent    NVARCHAR(MAX)    NULL,
    expires_at    DATETIMEOFFSET   NOT NULL,
    revoked_at    DATETIMEOFFSET   NULL,
    created_at    DATETIMEOFFSET   NOT NULL CONSTRAINT DF_user_sessions_created DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT PK_user_sessions PRIMARY KEY (id),
    CONSTRAINT UQ_user_sessions_refresh UNIQUE (refresh_token),
    CONSTRAINT FK_user_sessions_user FOREIGN KEY (user_id) REFERENCES dbo.users(id)
);
GO

CREATE INDEX IX_user_sessions_user ON dbo.user_sessions(user_id);
GO

CREATE TABLE dbo.login_attempts (
    id           UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_login_attempts_id DEFAULT NEWSEQUENTIALID(),
    username     NVARCHAR(100)    NOT NULL,
    ip_address   NVARCHAR(45)     NULL,
    success      BIT              NOT NULL,
    attempted_at DATETIMEOFFSET   NOT NULL CONSTRAINT DF_login_attempts_at DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT PK_login_attempts PRIMARY KEY (id)
);
GO

CREATE INDEX IX_login_attempts_username_time ON dbo.login_attempts(username, attempted_at DESC);
GO

CREATE TABLE dbo.password_reset_tokens (
    id         UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_password_reset_id DEFAULT NEWSEQUENTIALID(),
    user_id    UNIQUEIDENTIFIER NOT NULL,
    token_hash NVARCHAR(255)    NOT NULL,
    expires_at DATETIMEOFFSET   NOT NULL,
    used_at    DATETIMEOFFSET   NULL,
    created_at DATETIMEOFFSET   NOT NULL CONSTRAINT DF_password_reset_created DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT PK_password_reset_tokens PRIMARY KEY (id),
    CONSTRAINT UQ_password_reset_token UNIQUE (token_hash),
    CONSTRAINT FK_password_reset_user FOREIGN KEY (user_id) REFERENCES dbo.users(id)
);
GO

-- ---------------------------------------------------------------------------
-- Reputation
-- ---------------------------------------------------------------------------
CREATE TABLE dbo.reputation_scores (
    user_id              UNIQUEIDENTIFIER NOT NULL,
    score                DECIMAL(5,2)     NOT NULL CONSTRAINT DF_reputation_score DEFAULT 50.00,
    trust_level          NVARCHAR(20)     NOT NULL CONSTRAINT DF_reputation_trust DEFAULT N'MEDIUM',
    total_ratings        INT              NOT NULL CONSTRAINT DF_reputation_total DEFAULT 0,
    avg_rating           DECIMAL(3,2)     NULL,
    penalty_count        INT              NOT NULL CONSTRAINT DF_reputation_penalty_count DEFAULT 0,
    last_calculated_at   DATETIMEOFFSET   NOT NULL CONSTRAINT DF_reputation_calc DEFAULT SYSDATETIMEOFFSET(),
    updated_at           DATETIMEOFFSET   NOT NULL CONSTRAINT DF_reputation_updated DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT PK_reputation_scores PRIMARY KEY (user_id),
    CONSTRAINT FK_reputation_scores_user FOREIGN KEY (user_id) REFERENCES dbo.users(id),
    CONSTRAINT CK_reputation_score_range CHECK (score BETWEEN 0 AND 100),
    CONSTRAINT CK_reputation_trust_level CHECK (trust_level IN (N'UNTRUSTED', N'LOW', N'MEDIUM', N'HIGH', N'VERIFIED'))
);
GO

CREATE TABLE dbo.reputation_ratings (
    id                 UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_reputation_ratings_id DEFAULT NEWSEQUENTIALID(),
    rater_id           UNIQUEIDENTIFIER NOT NULL,
    ratee_id           UNIQUEIDENTIFIER NOT NULL,
    transaction_type   NVARCHAR(20)     NOT NULL,
    transaction_ref_id UNIQUEIDENTIFIER NOT NULL,
    score              SMALLINT         NOT NULL,
    comment            NVARCHAR(MAX)    NULL,
    dispute_flag       BIT              NOT NULL CONSTRAINT DF_reputation_ratings_dispute DEFAULT 0,
    created_at         DATETIMEOFFSET   NOT NULL CONSTRAINT DF_reputation_ratings_created DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT PK_reputation_ratings PRIMARY KEY (id),
    CONSTRAINT FK_reputation_ratings_rater FOREIGN KEY (rater_id) REFERENCES dbo.users(id),
    CONSTRAINT FK_reputation_ratings_ratee FOREIGN KEY (ratee_id) REFERENCES dbo.users(id),
    CONSTRAINT UQ_one_rating_per_party UNIQUE (rater_id, transaction_type, transaction_ref_id),
    CONSTRAINT CK_no_self_rating CHECK (rater_id <> ratee_id),
    CONSTRAINT CK_rating_score CHECK (score BETWEEN 1 AND 5)
);
GO

CREATE TABLE dbo.reputation_penalties (
    id             UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_reputation_penalties_id DEFAULT NEWSEQUENTIALID(),
    user_id        UNIQUEIDENTIFIER NOT NULL,
    penalty_type   NVARCHAR(50)     NOT NULL,
    score_delta    DECIMAL(5,2)     NOT NULL,
    reason         NVARCHAR(MAX)    NOT NULL,
    reference_type NVARCHAR(30)     NULL,
    reference_id   UNIQUEIDENTIFIER NULL,
    applied_by     UNIQUEIDENTIFIER NULL,
    applied_at     DATETIMEOFFSET   NOT NULL CONSTRAINT DF_reputation_penalties_at DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT PK_reputation_penalties PRIMARY KEY (id),
    CONSTRAINT FK_reputation_penalties_user FOREIGN KEY (user_id) REFERENCES dbo.users(id),
    CONSTRAINT FK_reputation_penalties_applied_by FOREIGN KEY (applied_by) REFERENCES dbo.users(id)
);
GO

CREATE TABLE dbo.reputation_transaction_summary (
    user_id               UNIQUEIDENTIFIER NOT NULL,
    completed_orders      INT              NOT NULL CONSTRAINT DF_rep_summary_completed DEFAULT 0,
    cancelled_orders      INT              NOT NULL CONSTRAINT DF_rep_summary_cancelled DEFAULT 0,
    auctions_won          INT              NOT NULL CONSTRAINT DF_rep_summary_won DEFAULT 0,
    auctions_lost_payment INT              NOT NULL CONSTRAINT DF_rep_summary_lost DEFAULT 0,
    updated_at            DATETIMEOFFSET   NOT NULL CONSTRAINT DF_rep_summary_updated DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT PK_reputation_transaction_summary PRIMARY KEY (user_id),
    CONSTRAINT FK_reputation_transaction_summary_user FOREIGN KEY (user_id) REFERENCES dbo.users(id)
);
GO

-- ---------------------------------------------------------------------------
-- Audit & Outbox
-- ---------------------------------------------------------------------------
CREATE TABLE dbo.audit_logs (
    id          UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_audit_logs_id DEFAULT NEWSEQUENTIALID(),
    actor_id    UNIQUEIDENTIFIER NULL,
    action      NVARCHAR(100)    NOT NULL,
    entity_type NVARCHAR(50)     NOT NULL,
    entity_id   UNIQUEIDENTIFIER NULL,
    old_value   NVARCHAR(MAX)    NULL,
    new_value   NVARCHAR(MAX)    NULL,
    ip_address  NVARCHAR(45)     NULL,
    created_at  DATETIMEOFFSET   NOT NULL CONSTRAINT DF_audit_logs_created DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT PK_audit_logs PRIMARY KEY (id)
);
GO

CREATE INDEX IX_audit_logs_entity ON dbo.audit_logs(entity_type, entity_id);
CREATE INDEX IX_audit_logs_created ON dbo.audit_logs(created_at DESC);
GO

CREATE TABLE dbo.outbox_events (
    id             UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_outbox_events_id DEFAULT NEWSEQUENTIALID(),
    aggregate_type NVARCHAR(50)     NOT NULL,
    aggregate_id   UNIQUEIDENTIFIER NOT NULL,
    event_type     NVARCHAR(100)    NOT NULL,
    payload        NVARCHAR(MAX)    NOT NULL,
    published_at   DATETIMEOFFSET   NULL,
    created_at     DATETIMEOFFSET   NOT NULL CONSTRAINT DF_outbox_events_created DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT PK_outbox_events PRIMARY KEY (id)
);
GO

CREATE INDEX IX_outbox_unpublished ON dbo.outbox_events(created_at) WHERE published_at IS NULL;
GO

CREATE TABLE [User] (
    user_id             UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    email               VARCHAR(255)     NOT NULL,
    phone_number        VARCHAR(20)      NULL,
    full_name           NVARCHAR(150)    NOT NULL,
    identify_number     VARCHAR(20)      NULL,
    gender              VARCHAR(10)      NULL,
    address             NVARCHAR(MAX)    NULL,
    date_of_birth       DATE             NULL,
    password_hash       VARCHAR(255)     NOT NULL,
    status              VARCHAR(20)      NOT NULL,
    is_email_verified   BIT              NOT NULL DEFAULT 0,
    email_verified_at   DATETIME2        NULL,
    failed_login_count  INT              NOT NULL DEFAULT 0,
    locked_until        DATETIME2        NULL,
    last_login_at       DATETIME2        NULL,
    is_deleted          BIT              NOT NULL DEFAULT 0,
    created_at          DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    updated_at          DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_User PRIMARY KEY (user_id),
    CONSTRAINT UQ_User_email UNIQUE (email),
    CONSTRAINT CK_User_status CHECK (status IN ('ACTIVE','LOCKED','INACTIVE')),
    CONSTRAINT CK_User_gender CHECK (gender IS NULL OR gender IN ('MALE','FEMALE','OTHER')),
    CONSTRAINT CK_User_failed_login CHECK (failed_login_count >= 0)
);
GO
CREATE INDEX IX_User_status ON [User](status);
GO

CREATE TABLE [Role] (
    role_id           UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    role_name         NVARCHAR(100)    NOT NULL,
    role_code         VARCHAR(50)      NOT NULL,
    role_description  NVARCHAR(MAX)    NULL,
    is_deleted        BIT              NOT NULL DEFAULT 0,
    created_at        DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    updated_at        DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_Role PRIMARY KEY (role_id),
    CONSTRAINT UQ_Role_code UNIQUE (role_code)
);
GO

CREATE TABLE [Privilege] (
    privilege_id    UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    privilege_code  VARCHAR(100)     NOT NULL,
    privilege_name  NVARCHAR(150)    NOT NULL,
    description     NVARCHAR(MAX)    NULL,
    CONSTRAINT PK_Privilege PRIMARY KEY (privilege_id),
    CONSTRAINT UQ_Privilege_code UNIQUE (privilege_code)
);
GO

CREATE TABLE [UserRole] (
    user_role_id  UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    user_id       UNIQUEIDENTIFIER NOT NULL,
    role_id       UNIQUEIDENTIFIER NOT NULL,
    assigned_at   DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    assigned_by   UNIQUEIDENTIFIER NULL,
    CONSTRAINT PK_UserRole PRIMARY KEY (user_role_id),
    CONSTRAINT UQ_UserRole UNIQUE (user_id, role_id),
    CONSTRAINT FK_UserRole_User FOREIGN KEY (user_id) REFERENCES [User](user_id),
    CONSTRAINT FK_UserRole_Role FOREIGN KEY (role_id) REFERENCES [Role](role_id),
    CONSTRAINT FK_UserRole_AssignedBy FOREIGN KEY (assigned_by) REFERENCES [User](user_id)
);
GO
CREATE INDEX IX_UserRole_user ON [UserRole](user_id);
CREATE INDEX IX_UserRole_role ON [UserRole](role_id);
GO

CREATE TABLE [RolePrivilege] (
    role_privilege_id  UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    role_id            UNIQUEIDENTIFIER NOT NULL,
    privilege_id       UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT PK_RolePrivilege PRIMARY KEY (role_privilege_id),
    CONSTRAINT UQ_RolePrivilege UNIQUE (role_id, privilege_id),
    CONSTRAINT FK_RolePrivilege_Role FOREIGN KEY (role_id) REFERENCES [Role](role_id),
    CONSTRAINT FK_RolePrivilege_Privilege FOREIGN KEY (privilege_id) REFERENCES [Privilege](privilege_id)
);
GO
CREATE INDEX IX_RolePrivilege_role ON [RolePrivilege](role_id);
CREATE INDEX IX_RolePrivilege_privilege ON [RolePrivilege](privilege_id);
GO

CREATE TABLE [UserSession] (
    session_id          UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    user_id             UNIQUEIDENTIFIER NOT NULL,
    token_hash          VARCHAR(255)     NOT NULL,
    refresh_token_hash  VARCHAR(255)     NULL,
    refresh_expires_at  DATETIME2        NULL,
    ip_address          VARCHAR(45)      NULL,
    user_agent          NVARCHAR(500)    NULL,
    login_at            DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    expires_at          DATETIME2        NOT NULL,
    logout_at           DATETIME2        NULL,
    status              VARCHAR(20)      NOT NULL,
    CONSTRAINT PK_UserSession PRIMARY KEY (session_id),
    CONSTRAINT FK_UserSession_User FOREIGN KEY (user_id) REFERENCES [User](user_id),
    CONSTRAINT CK_UserSession_status CHECK (status IN ('ACTIVE','EXPIRED','REVOKED'))
);
GO
CREATE INDEX IX_UserSession_user ON [UserSession](user_id);
CREATE INDEX IX_UserSession_token ON [UserSession](token_hash);
GO

CREATE TABLE [PasswordReset] (
    reset_id          UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    user_id           UNIQUEIDENTIFIER NOT NULL,
    reset_token_hash  VARCHAR(255)     NOT NULL,
    requested_at      DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    expires_at        DATETIME2        NOT NULL,
    used_at           DATETIME2        NULL,
    status            VARCHAR(20)      NOT NULL,
    CONSTRAINT PK_PasswordReset PRIMARY KEY (reset_id),
    CONSTRAINT FK_PasswordReset_User FOREIGN KEY (user_id) REFERENCES [User](user_id),
    CONSTRAINT CK_PasswordReset_status CHECK (status IN ('PENDING','USED','EXPIRED'))
);
GO
CREATE INDEX IX_PasswordReset_user ON [PasswordReset](user_id);
CREATE INDEX IX_PasswordReset_token ON [PasswordReset](reset_token_hash);
GO

CREATE TABLE [EmailVerification] (
    verification_id          UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    user_id                  UNIQUEIDENTIFIER NOT NULL,
    verification_token_hash  VARCHAR(255)     NOT NULL,
    requested_at             DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    expires_at               DATETIME2        NOT NULL,
    verified_at              DATETIME2        NULL,
    status                   VARCHAR(20)      NOT NULL,
    CONSTRAINT PK_EmailVerification PRIMARY KEY (verification_id),
    CONSTRAINT FK_EmailVerification_User FOREIGN KEY (user_id) REFERENCES [User](user_id),
    CONSTRAINT CK_EmailVerification_status CHECK (status IN ('PENDING','VERIFIED','EXPIRED'))
);
GO
CREATE INDEX IX_EmailVerification_user ON [EmailVerification](user_id);
CREATE INDEX IX_EmailVerification_token ON [EmailVerification](verification_token_hash);
GO

CREATE TABLE [RatingReview] (
    rating_id           UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    transaction_ref_id  UNIQUEIDENTIFIER NOT NULL,
    transaction_type    VARCHAR(20)      NOT NULL,
    rater_user_id       UNIQUEIDENTIFIER NOT NULL,
    rated_user_id       UNIQUEIDENTIFIER NOT NULL,
    feedback_type       VARCHAR(10)      NOT NULL,
    score               INT              NULL,
    comment             NVARCHAR(MAX)    NULL,
    is_disputed         BIT              NOT NULL DEFAULT 0,
    submitted_at        DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_RatingReview PRIMARY KEY (rating_id),
    CONSTRAINT UQ_RatingReview UNIQUE (transaction_ref_id, rater_user_id),
    CONSTRAINT FK_RatingReview_Rater FOREIGN KEY (rater_user_id) REFERENCES [User](user_id),
    CONSTRAINT FK_RatingReview_Rated FOREIGN KEY (rated_user_id) REFERENCES [User](user_id),
    CONSTRAINT CK_RatingReview_type CHECK (transaction_type IN ('ORDER','AUCTION')),
    CONSTRAINT CK_RatingReview_feedback CHECK (feedback_type IN ('POSITIVE','NEUTRAL','NEGATIVE')),
    CONSTRAINT CK_RatingReview_notself CHECK (rater_user_id <> rated_user_id)
);
GO
CREATE INDEX IX_RatingReview_rated ON [RatingReview](rated_user_id);
GO

CREATE TABLE [ReputationProfile] (
    reputation_id                 UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    user_id                       UNIQUEIDENTIFIER NOT NULL,
    reputation_score              DECIMAL(6,2)     NOT NULL DEFAULT 0,
    trust_level                   VARCHAR(20)      NOT NULL,
    successful_transaction_count  INT              NOT NULL DEFAULT 0,
    failed_activity_count         INT              NOT NULL DEFAULT 0,
    auction_win_count             INT              NOT NULL DEFAULT 0,
    auction_fail_count            INT              NOT NULL DEFAULT 0,
    updated_at                    DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_ReputationProfile PRIMARY KEY (reputation_id),
    CONSTRAINT UQ_ReputationProfile_user UNIQUE (user_id),
    CONSTRAINT FK_ReputationProfile_User FOREIGN KEY (user_id) REFERENCES [User](user_id)
);
GO

CREATE TABLE [PenaltyViolation] (
    penalty_id      UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    user_id         UNIQUEIDENTIFIER NOT NULL,
    related_ref_id  UNIQUEIDENTIFIER NULL,
    violation_type  VARCHAR(30)      NOT NULL,
    severity        VARCHAR(20)      NOT NULL,
    penalty_points  DECIMAL(6,2)     NOT NULL,
    reason          NVARCHAR(MAX)    NULL,
    created_at      DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_PenaltyViolation PRIMARY KEY (penalty_id),
    CONSTRAINT FK_PenaltyViolation_User FOREIGN KEY (user_id) REFERENCES [User](user_id)
);
GO
CREATE INDEX IX_PenaltyViolation_user ON [PenaltyViolation](user_id);
GO

CREATE TABLE [ReputationAudit] (
    audit_id            UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    user_id             UNIQUEIDENTIFIER NOT NULL,
    action_type         VARCHAR(30)      NOT NULL,
    transaction_ref_id  UNIQUEIDENTIFIER NULL,
    violation_ref_id    UNIQUEIDENTIFIER NULL,
    old_score           DECIMAL(6,2)     NULL,
    new_score           DECIMAL(6,2)     NULL,
    detail_json         NVARCHAR(MAX)    NULL,
    created_at          DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_ReputationAudit PRIMARY KEY (audit_id),
    CONSTRAINT FK_ReputationAudit_User FOREIGN KEY (user_id) REFERENCES [User](user_id)
);
GO
CREATE INDEX IX_ReputationAudit_user ON [ReputationAudit](user_id);
GO

CREATE TABLE [AuditLog] (
    audit_log_id    UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    actor_user_id   UNIQUEIDENTIFIER NULL,
    action          VARCHAR(50)      NOT NULL,
    target_type     VARCHAR(50)      NOT NULL,
    target_ref_id   UNIQUEIDENTIFIER NULL,
    detail_json     NVARCHAR(MAX)    NULL,
    ip_address      VARCHAR(45)      NULL,
    created_at      DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_AuditLog PRIMARY KEY (audit_log_id)
);
GO
CREATE INDEX IX_AuditLog_actor ON [AuditLog](actor_user_id);
CREATE INDEX IX_AuditLog_target ON [AuditLog](target_type, target_ref_id);
CREATE INDEX IX_AuditLog_created ON [AuditLog](created_at);
GO
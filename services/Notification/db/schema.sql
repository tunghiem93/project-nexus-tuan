-- =============================================================================
-- Notification Service — Nexus_Notification | SQL Server T-SQL
-- =============================================================================

USE [Nexus_Notification];
GO

CREATE TABLE dbo.event_logs (
    id              UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_event_logs_id DEFAULT NEWSEQUENTIALID(),
    event_type      NVARCHAR(100)    NOT NULL,
    event_message   NVARCHAR(MAX)    NOT NULL,
    operator_id     UNIQUEIDENTIFIER NULL,
    operator_name   NVARCHAR(200)    NULL,
    source_service  NVARCHAR(50)     NULL,
    payload         NVARCHAR(MAX)    NULL,
    correlation_id  NVARCHAR(64)     NULL,
    created_at      DATETIMEOFFSET   NOT NULL CONSTRAINT DF_event_logs_created DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT PK_event_logs PRIMARY KEY (id)
);
GO

CREATE INDEX IX_event_logs_created ON dbo.event_logs(created_at DESC);
CREATE INDEX IX_event_logs_type ON dbo.event_logs(event_type);
GO

CREATE TABLE dbo.processed_events (
    event_id     NVARCHAR(100)  NOT NULL,
    processed_at DATETIMEOFFSET NOT NULL CONSTRAINT DF_notification_processed_at DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT PK_notification_processed_events PRIMARY KEY (event_id)
);
GO

CREATE TABLE dbo.notification_templates (
    id               UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_notification_templates_id DEFAULT NEWSEQUENTIALID(),
    event_type       NVARCHAR(100)    NOT NULL,
    channel          NVARCHAR(10)     NOT NULL,
    subject          NVARCHAR(300)    NULL,
    body_template    NVARCHAR(MAX)    NOT NULL,
    is_transactional BIT              NOT NULL CONSTRAINT DF_templates_transactional DEFAULT 0,
    created_at       DATETIMEOFFSET   NOT NULL CONSTRAINT DF_templates_created DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT PK_notification_templates PRIMARY KEY (id),
    CONSTRAINT UQ_template_event_channel UNIQUE (event_type, channel),
    CONSTRAINT CK_templates_channel CHECK (channel IN (N'EMAIL', N'PUSH', N'SMS'))
);
GO

CREATE TABLE dbo.notifications (
    id               UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_notifications_id DEFAULT NEWSEQUENTIALID(),
    user_id          UNIQUEIDENTIFIER NOT NULL,
    event_type       NVARCHAR(100)    NOT NULL,
    title            NVARCHAR(300)    NOT NULL,
    body             NVARCHAR(MAX)    NOT NULL,
    is_transactional BIT              NOT NULL CONSTRAINT DF_notifications_transactional DEFAULT 0,
    metadata         NVARCHAR(MAX)    NULL,
    created_at       DATETIMEOFFSET   NOT NULL CONSTRAINT DF_notifications_created DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT PK_notifications PRIMARY KEY (id)
);
GO

CREATE INDEX IX_notifications_user ON dbo.notifications(user_id, created_at DESC);
GO

CREATE TABLE dbo.notification_deliveries (
    id              UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_notification_deliveries_id DEFAULT NEWSEQUENTIALID(),
    notification_id UNIQUEIDENTIFIER NOT NULL,
    channel         NVARCHAR(10)     NOT NULL,
    status          NVARCHAR(10)     NOT NULL CONSTRAINT DF_deliveries_status DEFAULT N'PENDING',
    attempt_count   SMALLINT         NOT NULL CONSTRAINT DF_deliveries_attempts DEFAULT 0,
    last_error      NVARCHAR(MAX)    NULL,
    sent_at         DATETIMEOFFSET   NULL,
    created_at      DATETIMEOFFSET   NOT NULL CONSTRAINT DF_deliveries_created DEFAULT SYSDATETIMEOFFSET(),
    updated_at      DATETIMEOFFSET   NOT NULL CONSTRAINT DF_deliveries_updated DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT PK_notification_deliveries PRIMARY KEY (id),
    CONSTRAINT FK_deliveries_notification FOREIGN KEY (notification_id) REFERENCES dbo.notifications(id),
    CONSTRAINT CK_deliveries_channel CHECK (channel IN (N'EMAIL', N'PUSH', N'SMS')),
    CONSTRAINT CK_deliveries_status CHECK (status IN (N'PENDING', N'SENT', N'FAILED', N'SKIPPED'))
);
GO

CREATE TABLE dbo.notification_preferences (
    id         UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_notification_preferences_id DEFAULT NEWSEQUENTIALID(),
    user_id    UNIQUEIDENTIFIER NOT NULL,
    event_type NVARCHAR(100)    NOT NULL,
    channel    NVARCHAR(10)     NOT NULL,
    enabled    BIT              NOT NULL CONSTRAINT DF_preferences_enabled DEFAULT 1,
    updated_at DATETIMEOFFSET   NOT NULL CONSTRAINT DF_preferences_updated DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT PK_notification_preferences PRIMARY KEY (id),
    CONSTRAINT UQ_user_pref UNIQUE (user_id, event_type, channel),
    CONSTRAINT CK_preferences_channel CHECK (channel IN (N'EMAIL', N'PUSH', N'SMS'))
);
GO

CREATE TABLE dbo.broker_health_logs (
    id          UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_broker_health_id DEFAULT NEWSEQUENTIALID(),
    broker_name NVARCHAR(50)     NOT NULL CONSTRAINT DF_broker_name DEFAULT N'rabbitmq',
    status      NVARCHAR(20)     NOT NULL,
    detail      NVARCHAR(MAX)    NULL,
    checked_at  DATETIMEOFFSET   NOT NULL CONSTRAINT DF_broker_checked DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT PK_broker_health_logs PRIMARY KEY (id)
);
GO

CREATE TABLE dbo.outbox_events (
    id             UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_notification_outbox_id DEFAULT NEWSEQUENTIALID(),
    aggregate_type NVARCHAR(50)     NOT NULL,
    aggregate_id   UNIQUEIDENTIFIER NOT NULL,
    event_type     NVARCHAR(100)    NOT NULL,
    payload        NVARCHAR(MAX)    NOT NULL,
    published_at   DATETIMEOFFSET   NULL,
    created_at     DATETIMEOFFSET   NOT NULL CONSTRAINT DF_notification_outbox_created DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT PK_notification_outbox PRIMARY KEY (id)
);
GO

CREATE INDEX IX_notification_outbox_unpublished ON dbo.outbox_events(created_at) WHERE published_at IS NULL;
GO

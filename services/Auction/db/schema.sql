-- =============================================================================
-- Auction Service — Nexus_Auction | SQL Server T-SQL
-- =============================================================================

USE [Nexus_Auction];
GO

CREATE TABLE dbo.auctions (
    id                       UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_auctions_id DEFAULT NEWSEQUENTIALID(),
    product_id               UNIQUEIDENTIFIER NOT NULL,
    seller_id                UNIQUEIDENTIFIER NOT NULL,
    sku_code                 NVARCHAR(100)    NOT NULL,
    status                   NVARCHAR(20)     NOT NULL CONSTRAINT DF_auctions_status DEFAULT N'DRAFT',
    visibility               NVARCHAR(20)     NOT NULL CONSTRAINT DF_auctions_visibility DEFAULT N'PUBLIC',
    starting_price           DECIMAL(14,2)    NOT NULL,
    bid_increment            DECIMAL(14,2)    NOT NULL CONSTRAINT DF_auctions_increment DEFAULT 10,
    current_price            DECIMAL(14,2)    NULL,
    current_bidder_id        UNIQUEIDENTIFIER NULL,
    bid_count                INT              NOT NULL CONSTRAINT DF_auctions_bid_count DEFAULT 0,
    scheduled_start_at       DATETIMEOFFSET   NOT NULL,
    scheduled_end_at         DATETIMEOFFSET   NOT NULL,
    actual_start_at          DATETIMEOFFSET   NULL,
    actual_end_at            DATETIMEOFFSET   NULL,
    extension_count          INT              NOT NULL CONSTRAINT DF_auctions_extensions DEFAULT 0,
    inventory_reservation_id UNIQUEIDENTIFIER NULL,
    cancelled_by             UNIQUEIDENTIFIER NULL,
    cancel_reason            NVARCHAR(MAX)    NULL,
    version                  BIGINT           NOT NULL CONSTRAINT DF_auctions_version DEFAULT 0,
    created_at               DATETIMEOFFSET   NOT NULL CONSTRAINT DF_auctions_created DEFAULT SYSDATETIMEOFFSET(),
    updated_at               DATETIMEOFFSET   NOT NULL CONSTRAINT DF_auctions_updated DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT PK_auctions PRIMARY KEY (id),
    CONSTRAINT CK_auctions_starting_price CHECK (starting_price >= 0),
    CONSTRAINT CK_auctions_status CHECK (status IN (
        N'DRAFT', N'SCHEDULED', N'ACTIVE', N'ENDED', N'CANCELLED', N'SETTLING', N'SETTLED', N'FAILED'
    )),
    CONSTRAINT CK_auctions_visibility CHECK (visibility IN (N'PUBLIC', N'RESTRICTED')),
    CONSTRAINT CK_auction_duration CHECK (
        scheduled_end_at > scheduled_start_at
        AND scheduled_end_at <= DATEADD(HOUR, 168, scheduled_start_at)
        AND scheduled_end_at >= DATEADD(MINUTE, 60, scheduled_start_at)
    )
);
GO

CREATE INDEX IX_auctions_seller_active ON dbo.auctions(seller_id, status)
    WHERE status IN (N'SCHEDULED', N'ACTIVE');
CREATE INDEX IX_auctions_product ON dbo.auctions(product_id);
CREATE INDEX IX_auctions_end ON dbo.auctions(scheduled_end_at) WHERE status = N'ACTIVE';
CREATE UNIQUE INDEX UQ_active_auction_per_product ON dbo.auctions(product_id)
    WHERE status IN (N'SCHEDULED', N'ACTIVE');
GO

CREATE TABLE dbo.auction_bids (
    id              UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_auction_bids_id DEFAULT NEWSEQUENTIALID(),
    auction_id      UNIQUEIDENTIFIER NOT NULL,
    bidder_id       UNIQUEIDENTIFIER NOT NULL,
    amount          DECIMAL(14,2)    NOT NULL,
    is_winning      BIT              NOT NULL CONSTRAINT DF_auction_bids_winning DEFAULT 0,
    idempotency_key NVARCHAR(64)     NULL,
    placed_at       DATETIMEOFFSET   NOT NULL CONSTRAINT DF_auction_bids_placed DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT PK_auction_bids PRIMARY KEY (id),
    CONSTRAINT FK_auction_bids_auction FOREIGN KEY (auction_id) REFERENCES dbo.auctions(id),
    CONSTRAINT UQ_auction_bids_idempotency UNIQUE (idempotency_key)
);
GO

CREATE INDEX IX_auction_bids_auction ON dbo.auction_bids(auction_id, placed_at DESC);
CREATE INDEX IX_auction_bids_bidder ON dbo.auction_bids(bidder_id);
GO

CREATE TABLE dbo.auction_extensions (
    id                UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_auction_ext_id DEFAULT NEWSEQUENTIALID(),
    auction_id        UNIQUEIDENTIFIER NOT NULL,
    trigger_bid_id    UNIQUEIDENTIFIER NOT NULL,
    previous_end_at   DATETIMEOFFSET   NOT NULL,
    new_end_at        DATETIMEOFFSET   NOT NULL,
    extension_minutes INT              NOT NULL CONSTRAINT DF_auction_ext_minutes DEFAULT 5,
    created_at        DATETIMEOFFSET   NOT NULL CONSTRAINT DF_auction_ext_created DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT PK_auction_extensions PRIMARY KEY (id),
    CONSTRAINT FK_auction_ext_auction FOREIGN KEY (auction_id) REFERENCES dbo.auctions(id),
    CONSTRAINT FK_auction_ext_bid FOREIGN KEY (trigger_bid_id) REFERENCES dbo.auction_bids(id)
);
GO

CREATE TABLE dbo.auction_settlements (
    id                 UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_auction_settlements_id DEFAULT NEWSEQUENTIALID(),
    auction_id         UNIQUEIDENTIFIER NOT NULL,
    status             NVARCHAR(20)     NOT NULL,
    winner_id          UNIQUEIDENTIFIER NULL,
    winning_bid_id     UNIQUEIDENTIFIER NULL,
    final_price        DECIMAL(14,2)    NULL,
    payment_deadline   DATETIMEOFFSET   NULL,
    settled_at         DATETIMEOFFSET   NULL,
    settlement_payload NVARCHAR(MAX)    NULL,
    idempotency_key    NVARCHAR(64)     NULL,
    created_at         DATETIMEOFFSET   NOT NULL CONSTRAINT DF_auction_settlements_created DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT PK_auction_settlements PRIMARY KEY (id),
    CONSTRAINT UQ_auction_settlements_auction UNIQUE (auction_id),
    CONSTRAINT FK_auction_settlements_auction FOREIGN KEY (auction_id) REFERENCES dbo.auctions(id),
    CONSTRAINT FK_auction_settlements_bid FOREIGN KEY (winning_bid_id) REFERENCES dbo.auction_bids(id),
    CONSTRAINT UQ_auction_settlements_idempotency UNIQUE (idempotency_key),
    CONSTRAINT CK_auction_settlements_status CHECK (status IN (N'PENDING', N'WON', N'NO_WINNER', N'PAYMENT_TIMEOUT'))
);
GO

CREATE TABLE dbo.processed_events (
    event_id     NVARCHAR(100)  NOT NULL,
    processed_at DATETIMEOFFSET NOT NULL CONSTRAINT DF_auction_processed_at DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT PK_auction_processed_events PRIMARY KEY (event_id)
);
GO

CREATE TABLE dbo.outbox_events (
    id             UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_auction_outbox_id DEFAULT NEWSEQUENTIALID(),
    aggregate_type NVARCHAR(50)     NOT NULL,
    aggregate_id   UNIQUEIDENTIFIER NOT NULL,
    event_type     NVARCHAR(100)    NOT NULL,
    payload        NVARCHAR(MAX)    NOT NULL,
    published_at   DATETIMEOFFSET   NULL,
    created_at     DATETIMEOFFSET   NOT NULL CONSTRAINT DF_auction_outbox_created DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT PK_auction_outbox PRIMARY KEY (id)
);
GO

CREATE INDEX IX_auction_outbox_unpublished ON dbo.outbox_events(created_at) WHERE published_at IS NULL;
GO

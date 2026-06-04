-- =============================================================================
-- Fulfillment Service — Nexus_Fulfillment | SQL Server T-SQL
-- =============================================================================

USE [Nexus_Fulfillment];
GO

CREATE TABLE dbo.warehouses (
    id         UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_warehouses_id DEFAULT NEWSEQUENTIALID(),
    code       NVARCHAR(20)     NOT NULL,
    name       NVARCHAR(200)    NOT NULL,
    address    NVARCHAR(MAX)    NULL,
    is_active  BIT              NOT NULL CONSTRAINT DF_warehouses_active DEFAULT 1,
    created_at DATETIMEOFFSET   NOT NULL CONSTRAINT DF_warehouses_created DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT PK_warehouses PRIMARY KEY (id),
    CONSTRAINT UQ_warehouses_code UNIQUE (code)
);
GO

CREATE TABLE dbo.inventory_records (
    id                   UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_inventory_id DEFAULT NEWSEQUENTIALID(),
    sku_code             NVARCHAR(100)    NOT NULL,
    warehouse_id         UNIQUEIDENTIFIER NOT NULL,
    total_quantity       INT              NOT NULL CONSTRAINT DF_inventory_total DEFAULT 0,
    available_quantity   INT              NOT NULL CONSTRAINT DF_inventory_available DEFAULT 0,
    reserved_quantity    INT              NOT NULL CONSTRAINT DF_inventory_reserved DEFAULT 0,
    unavailable_quantity INT              NOT NULL CONSTRAINT DF_inventory_unavailable DEFAULT 0,
    version              BIGINT           NOT NULL CONSTRAINT DF_inventory_version DEFAULT 0,
    updated_at           DATETIMEOFFSET   NOT NULL CONSTRAINT DF_inventory_updated DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT PK_inventory_records PRIMARY KEY (id),
    CONSTRAINT FK_inventory_warehouse FOREIGN KEY (warehouse_id) REFERENCES dbo.warehouses(id),
    CONSTRAINT UQ_inventory_sku_wh UNIQUE (sku_code, warehouse_id),
    CONSTRAINT CK_inventory_total CHECK (total_quantity >= 0),
    CONSTRAINT CK_inventory_available CHECK (available_quantity >= 0),
    CONSTRAINT CK_inventory_reserved CHECK (reserved_quantity >= 0),
    CONSTRAINT CK_inventory_unavailable CHECK (unavailable_quantity >= 0),
    CONSTRAINT CK_inventory_balance CHECK (
        total_quantity = available_quantity + reserved_quantity + unavailable_quantity
    )
);
GO

CREATE INDEX IX_inventory_sku ON dbo.inventory_records(sku_code);
GO

CREATE TABLE dbo.inventory_reservations (
    id              UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_reservations_id DEFAULT NEWSEQUENTIALID(),
    sku_code        NVARCHAR(100)    NOT NULL,
    warehouse_id    UNIQUEIDENTIFIER NOT NULL,
    quantity        INT              NOT NULL,
    reference_type  NVARCHAR(30)     NOT NULL,
    reference_id    UNIQUEIDENTIFIER NOT NULL,
    status          NVARCHAR(20)     NOT NULL CONSTRAINT DF_reservations_status DEFAULT N'ACTIVE',
    expires_at      DATETIMEOFFSET   NOT NULL,
    idempotency_key NVARCHAR(64)     NULL,
    created_at      DATETIMEOFFSET   NOT NULL CONSTRAINT DF_reservations_created DEFAULT SYSDATETIMEOFFSET(),
    updated_at      DATETIMEOFFSET   NOT NULL CONSTRAINT DF_reservations_updated DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT PK_inventory_reservations PRIMARY KEY (id),
    CONSTRAINT FK_reservations_warehouse FOREIGN KEY (warehouse_id) REFERENCES dbo.warehouses(id),
    CONSTRAINT UQ_reservations_idempotency UNIQUE (idempotency_key),
    CONSTRAINT CK_reservations_qty CHECK (quantity > 0),
    CONSTRAINT CK_reservations_status CHECK (status IN (N'ACTIVE', N'COMMITTED', N'RELEASED', N'EXPIRED'))
);
GO

CREATE INDEX IX_reservations_active ON dbo.inventory_reservations(expires_at) WHERE status = N'ACTIVE';
GO

CREATE TABLE dbo.inventory_ledger (
    id             UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_ledger_id DEFAULT NEWSEQUENTIALID(),
    sku_code       NVARCHAR(100)    NOT NULL,
    warehouse_id   UNIQUEIDENTIFIER NOT NULL,
    movement_type  NVARCHAR(20)     NOT NULL,
    quantity_delta INT              NOT NULL,
    reference_type NVARCHAR(30)     NULL,
    reference_id   UNIQUEIDENTIFIER NULL,
    note           NVARCHAR(MAX)    NULL,
    created_at     DATETIMEOFFSET   NOT NULL CONSTRAINT DF_ledger_created DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT PK_inventory_ledger PRIMARY KEY (id),
    CONSTRAINT FK_ledger_warehouse FOREIGN KEY (warehouse_id) REFERENCES dbo.warehouses(id),
    CONSTRAINT CK_ledger_movement CHECK (movement_type IN (
        N'INTAKE', N'RESERVE', N'RELEASE', N'COMMIT', N'SHIP', N'ADJUST', N'RETURN'
    ))
);
GO

CREATE INDEX IX_ledger_sku ON dbo.inventory_ledger(sku_code, created_at DESC);
GO

CREATE TABLE dbo.shipments (
    id                 UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_shipments_id DEFAULT NEWSEQUENTIALID(),
    order_id           UNIQUEIDENTIFIER NOT NULL,
    warehouse_id       UNIQUEIDENTIFIER NOT NULL,
    carrier_code       NVARCHAR(20)     NOT NULL,
    service_type       NVARCHAR(50)     NULL,
    tracking_number    NVARCHAR(100)    NULL,
    shipping_fee       DECIMAL(14,2)    NOT NULL CONSTRAINT DF_shipments_fee DEFAULT 0,
    status             NVARCHAR(30)     NOT NULL CONSTRAINT DF_shipments_status DEFAULT N'CREATED',
    recipient_address  NVARCHAR(MAX)    NOT NULL,
    carrier_raw_status NVARCHAR(100)    NULL,
    idempotency_key    NVARCHAR(64)     NULL,
    created_at         DATETIMEOFFSET   NOT NULL CONSTRAINT DF_shipments_created DEFAULT SYSDATETIMEOFFSET(),
    updated_at         DATETIMEOFFSET   NOT NULL CONSTRAINT DF_shipments_updated DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT PK_shipments PRIMARY KEY (id),
    CONSTRAINT FK_shipments_warehouse FOREIGN KEY (warehouse_id) REFERENCES dbo.warehouses(id),
    CONSTRAINT UQ_shipments_idempotency UNIQUE (idempotency_key),
    CONSTRAINT CK_shipments_status CHECK (status IN (
        N'CREATED', N'PICKED_UP', N'IN_TRANSIT', N'OUT_FOR_DELIVERY',
        N'DELIVERED', N'FAILED', N'RETURNED', N'CANCELLED'
    ))
);
GO

CREATE INDEX IX_shipments_order ON dbo.shipments(order_id);
CREATE INDEX IX_shipments_tracking ON dbo.shipments(tracking_number);
GO

CREATE TABLE dbo.shipment_status_history (
    id          UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_shipment_history_id DEFAULT NEWSEQUENTIALID(),
    shipment_id UNIQUEIDENTIFIER NOT NULL,
    from_status NVARCHAR(30)     NULL,
    to_status   NVARCHAR(30)     NOT NULL,
    source      NVARCHAR(30)     NOT NULL,
    raw_payload NVARCHAR(MAX)    NULL,
    changed_at  DATETIMEOFFSET   NOT NULL CONSTRAINT DF_shipment_history_at DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT PK_shipment_status_history PRIMARY KEY (id),
    CONSTRAINT FK_shipment_history_shipment FOREIGN KEY (shipment_id) REFERENCES dbo.shipments(id)
);
GO

CREATE TABLE dbo.carrier_performance (
    id                 UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_carrier_perf_id DEFAULT NEWSEQUENTIALID(),
    carrier_code       NVARCHAR(20)     NOT NULL,
    total_shipments    INT              NOT NULL CONSTRAINT DF_carrier_total DEFAULT 0,
    delivered_count    INT              NOT NULL CONSTRAINT DF_carrier_delivered DEFAULT 0,
    failed_count       INT              NOT NULL CONSTRAINT DF_carrier_failed DEFAULT 0,
    avg_delivery_hours DECIMAL(8,2)     NULL,
    updated_at         DATETIMEOFFSET   NOT NULL CONSTRAINT DF_carrier_updated DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT PK_carrier_performance PRIMARY KEY (id),
    CONSTRAINT UQ_carrier_perf_code UNIQUE (carrier_code)
);
GO

CREATE TABLE dbo.processed_events (
    event_id     NVARCHAR(100)  NOT NULL,
    processed_at DATETIMEOFFSET NOT NULL CONSTRAINT DF_fulfillment_processed_at DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT PK_fulfillment_processed_events PRIMARY KEY (event_id)
);
GO

CREATE TABLE dbo.outbox_events (
    id             UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_fulfillment_outbox_id DEFAULT NEWSEQUENTIALID(),
    aggregate_type NVARCHAR(50)     NOT NULL,
    aggregate_id   UNIQUEIDENTIFIER NOT NULL,
    event_type     NVARCHAR(100)    NOT NULL,
    payload        NVARCHAR(MAX)    NOT NULL,
    published_at   DATETIMEOFFSET   NULL,
    created_at     DATETIMEOFFSET   NOT NULL CONSTRAINT DF_fulfillment_outbox_created DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT PK_fulfillment_outbox PRIMARY KEY (id)
);
GO

CREATE INDEX IX_fulfillment_outbox_unpublished ON dbo.outbox_events(created_at) WHERE published_at IS NULL;
GO

INSERT INTO dbo.warehouses (code, name, address)
VALUES (N'WH_001', N'Default Warehouse', N'Ho Chi Minh City, Vietnam');
GO

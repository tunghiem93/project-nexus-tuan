-- =============================================================================
-- Catalog Service — Nexus_Catalog | SQL Server T-SQL
-- =============================================================================

USE [Nexus_Catalog];
GO

CREATE TABLE dbo.categories (
    id          UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_categories_id DEFAULT NEWSEQUENTIALID(),
    parent_id   UNIQUEIDENTIFIER NULL,
    name        NVARCHAR(200)    NOT NULL,
    slug        NVARCHAR(200)    NOT NULL,
    description NVARCHAR(MAX)    NULL,
    level       SMALLINT         NOT NULL,
    sort_order  INT              NOT NULL CONSTRAINT DF_categories_sort DEFAULT 0,
    deleted_at  DATETIMEOFFSET   NULL,
    created_at  DATETIMEOFFSET   NOT NULL CONSTRAINT DF_categories_created DEFAULT SYSDATETIMEOFFSET(),
    updated_at  DATETIMEOFFSET   NOT NULL CONSTRAINT DF_categories_updated DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT PK_categories PRIMARY KEY (id),
    CONSTRAINT UQ_categories_slug UNIQUE (slug),
    CONSTRAINT FK_categories_parent FOREIGN KEY (parent_id) REFERENCES dbo.categories(id),
    CONSTRAINT CK_categories_level CHECK (level BETWEEN 1 AND 3)
);
GO

CREATE INDEX IX_categories_parent ON dbo.categories(parent_id);
CREATE INDEX IX_categories_name ON dbo.categories(name);
GO

CREATE TABLE dbo.products (
    id              UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_products_id DEFAULT NEWSEQUENTIALID(),
    seller_id       UNIQUEIDENTIFIER NOT NULL,
    sku_code        NVARCHAR(100)    NOT NULL,
    name            NVARCHAR(300)    NOT NULL,
    slug            NVARCHAR(300)    NOT NULL,
    description     NVARCHAR(MAX)    NULL,
    base_price      DECIMAL(14,2)    NOT NULL,
    currency        CHAR(3)          NOT NULL CONSTRAINT DF_products_currency DEFAULT 'VND',
    status          NVARCHAR(20)     NOT NULL CONSTRAINT DF_products_status DEFAULT N'DRAFT',
    auction_enabled BIT              NOT NULL CONSTRAINT DF_products_auction DEFAULT 1,
    category_id     UNIQUEIDENTIFIER NOT NULL,
    deleted_at      DATETIMEOFFSET   NULL,
    created_at      DATETIMEOFFSET   NOT NULL CONSTRAINT DF_products_created DEFAULT SYSDATETIMEOFFSET(),
    updated_at      DATETIMEOFFSET   NOT NULL CONSTRAINT DF_products_updated DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT PK_products PRIMARY KEY (id),
    CONSTRAINT FK_products_category FOREIGN KEY (category_id) REFERENCES dbo.categories(id),
    CONSTRAINT UQ_products_seller_sku UNIQUE (seller_id, sku_code),
    CONSTRAINT CK_products_status CHECK (status IN (N'DRAFT', N'ACTIVE', N'INACTIVE')),
    CONSTRAINT CK_products_base_price CHECK (base_price >= 0)
);
GO

CREATE INDEX IX_products_seller ON dbo.products(seller_id);
CREATE INDEX IX_products_category ON dbo.products(category_id);
CREATE INDEX IX_products_status ON dbo.products(status) WHERE deleted_at IS NULL;
CREATE INDEX IX_products_name ON dbo.products(name);
GO

CREATE TABLE dbo.product_images (
    id         UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_product_images_id DEFAULT NEWSEQUENTIALID(),
    product_id UNIQUEIDENTIFIER NOT NULL,
    url        NVARCHAR(MAX)    NOT NULL,
    sort_order SMALLINT         NOT NULL CONSTRAINT DF_product_images_sort DEFAULT 0,
    created_at DATETIMEOFFSET   NOT NULL CONSTRAINT DF_product_images_created DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT PK_product_images PRIMARY KEY (id),
    CONSTRAINT FK_product_images_product FOREIGN KEY (product_id) REFERENCES dbo.products(id) ON DELETE CASCADE,
    CONSTRAINT CK_max_images CHECK (sort_order BETWEEN 0 AND 4)
);
GO

CREATE TABLE dbo.product_discovery_stats (
    product_id       UNIQUEIDENTIFIER NOT NULL,
    view_count       BIGINT           NOT NULL CONSTRAINT DF_discovery_views DEFAULT 0,
    order_count      BIGINT           NOT NULL CONSTRAINT DF_discovery_orders DEFAULT 0,
    popularity_score DECIMAL(10,4)    NOT NULL CONSTRAINT DF_discovery_score DEFAULT 0,
    updated_at       DATETIMEOFFSET   NOT NULL CONSTRAINT DF_discovery_updated DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT PK_product_discovery_stats PRIMARY KEY (product_id),
    CONSTRAINT FK_discovery_product FOREIGN KEY (product_id) REFERENCES dbo.products(id)
);
GO

CREATE TABLE dbo.outbox_events (
    id             UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_catalog_outbox_id DEFAULT NEWSEQUENTIALID(),
    aggregate_type NVARCHAR(50)     NOT NULL,
    aggregate_id   UNIQUEIDENTIFIER NOT NULL,
    event_type     NVARCHAR(100)    NOT NULL,
    payload        NVARCHAR(MAX)    NOT NULL,
    published_at   DATETIMEOFFSET   NULL,
    created_at     DATETIMEOFFSET   NOT NULL CONSTRAINT DF_catalog_outbox_created DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT PK_catalog_outbox PRIMARY KEY (id)
);
GO

CREATE INDEX IX_catalog_outbox_unpublished ON dbo.outbox_events(created_at) WHERE published_at IS NULL;
GO

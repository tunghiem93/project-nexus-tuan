-- =============================================================================
-- Commerce Service — Nexus_Commerce | SQL Server T-SQL
-- =============================================================================

USE [Nexus_Commerce];
GO

CREATE TABLE dbo.carts (
    id                UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_carts_id DEFAULT NEWSEQUENTIALID(),
    user_id           UNIQUEIDENTIFIER NULL,
    guest_token       NVARCHAR(64)     NULL,
    status            NVARCHAR(20)     NOT NULL CONSTRAINT DF_carts_status DEFAULT N'ACTIVE',
    currency          CHAR(3)          NOT NULL CONSTRAINT DF_carts_currency DEFAULT 'VND',
    subtotal          DECIMAL(14,2)    NOT NULL CONSTRAINT DF_carts_subtotal DEFAULT 0,
    tax_amount        DECIMAL(14,2)    NOT NULL CONSTRAINT DF_carts_tax DEFAULT 0,
    discount_amount   DECIMAL(14,2)    NOT NULL CONSTRAINT DF_carts_discount DEFAULT 0,
    shipping_estimate DECIMAL(14,2)    NOT NULL CONSTRAINT DF_carts_shipping DEFAULT 0,
    expires_at        DATETIMEOFFSET   NOT NULL,
    created_at        DATETIMEOFFSET   NOT NULL CONSTRAINT DF_carts_created DEFAULT SYSDATETIMEOFFSET(),
    updated_at        DATETIMEOFFSET   NOT NULL CONSTRAINT DF_carts_updated DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT PK_carts PRIMARY KEY (id),
    CONSTRAINT CK_carts_owner CHECK (user_id IS NOT NULL OR guest_token IS NOT NULL),
    CONSTRAINT CK_carts_status CHECK (status IN (N'ACTIVE', N'MERGED', N'CHECKOUT', N'EXPIRED', N'CONVERTED'))
);
GO

CREATE UNIQUE INDEX UQ_active_cart_per_user ON dbo.carts(user_id)
    WHERE status = N'ACTIVE' AND user_id IS NOT NULL;
GO

CREATE TABLE dbo.cart_items (
    id           UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_cart_items_id DEFAULT NEWSEQUENTIALID(),
    cart_id      UNIQUEIDENTIFIER NOT NULL,
    product_id   UNIQUEIDENTIFIER NOT NULL,
    sku_code     NVARCHAR(100)    NOT NULL,
    product_name NVARCHAR(300)    NOT NULL,
    unit_price   DECIMAL(14,2)    NOT NULL,
    quantity     INT              NOT NULL,
    line_total   DECIMAL(14,2)    NOT NULL,
    created_at   DATETIMEOFFSET   NOT NULL CONSTRAINT DF_cart_items_created DEFAULT SYSDATETIMEOFFSET(),
    updated_at   DATETIMEOFFSET   NOT NULL CONSTRAINT DF_cart_items_updated DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT PK_cart_items PRIMARY KEY (id),
    CONSTRAINT FK_cart_items_cart FOREIGN KEY (cart_id) REFERENCES dbo.carts(id) ON DELETE CASCADE,
    CONSTRAINT UQ_cart_product UNIQUE (cart_id, product_id),
    CONSTRAINT CK_cart_items_qty CHECK (quantity > 0)
);
GO

CREATE TABLE dbo.checkout_sessions (
    id               UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_checkout_id DEFAULT NEWSEQUENTIALID(),
    cart_id          UNIQUEIDENTIFIER NOT NULL,
    user_id          UNIQUEIDENTIFIER NOT NULL,
    status           NVARCHAR(20)     NOT NULL CONSTRAINT DF_checkout_status DEFAULT N'STARTED',
    shipping_address NVARCHAR(MAX)    NOT NULL,
    payment_method   NVARCHAR(30)     NOT NULL CONSTRAINT DF_checkout_payment DEFAULT N'STRIPE',
    price_snapshot   NVARCHAR(MAX)    NOT NULL,
    reservation_refs NVARCHAR(MAX)    NULL,
    expires_at       DATETIMEOFFSET   NOT NULL,
    created_at       DATETIMEOFFSET   NOT NULL CONSTRAINT DF_checkout_created DEFAULT SYSDATETIMEOFFSET(),
    updated_at       DATETIMEOFFSET   NOT NULL CONSTRAINT DF_checkout_updated DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT PK_checkout_sessions PRIMARY KEY (id),
    CONSTRAINT FK_checkout_cart FOREIGN KEY (cart_id) REFERENCES dbo.carts(id),
    CONSTRAINT CK_checkout_status CHECK (status IN (N'STARTED', N'CONFIRMED', N'EXPIRED', N'COMPLETED', N'ABANDONED'))
);
GO

CREATE TABLE dbo.orders (
    id               UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_orders_id DEFAULT NEWSEQUENTIALID(),
    order_number     NVARCHAR(30)     NOT NULL,
    buyer_id         UNIQUEIDENTIFIER NOT NULL,
    seller_id        UNIQUEIDENTIFIER NULL,
    source           NVARCHAR(20)     NOT NULL CONSTRAINT DF_orders_source DEFAULT N'CART_CHECKOUT',
    source_ref_id    UNIQUEIDENTIFIER NULL,
    status           NVARCHAR(30)     NOT NULL CONSTRAINT DF_orders_status DEFAULT N'PENDING_PAYMENT',
    currency         CHAR(3)          NOT NULL CONSTRAINT DF_orders_currency DEFAULT 'VND',
    subtotal         DECIMAL(14,2)    NOT NULL,
    tax_amount       DECIMAL(14,2)    NOT NULL CONSTRAINT DF_orders_tax DEFAULT 0,
    discount_amount  DECIMAL(14,2)    NOT NULL CONSTRAINT DF_orders_discount DEFAULT 0,
    shipping_fee     DECIMAL(14,2)    NOT NULL CONSTRAINT DF_orders_shipping DEFAULT 0,
    total_amount     DECIMAL(14,2)    NOT NULL,
    shipping_address NVARCHAR(MAX)    NOT NULL,
    payment_deadline DATETIMEOFFSET   NULL,
    idempotency_key  NVARCHAR(64)     NULL,
    created_at       DATETIMEOFFSET   NOT NULL CONSTRAINT DF_orders_created DEFAULT SYSDATETIMEOFFSET(),
    updated_at       DATETIMEOFFSET   NOT NULL CONSTRAINT DF_orders_updated DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT PK_orders PRIMARY KEY (id),
    CONSTRAINT UQ_orders_number UNIQUE (order_number),
    CONSTRAINT UQ_orders_idempotency UNIQUE (idempotency_key),
    CONSTRAINT CK_orders_source CHECK (source IN (N'CART_CHECKOUT', N'AUCTION')),
    CONSTRAINT CK_orders_status CHECK (status IN (
        N'PENDING_PAYMENT', N'PAID', N'PROCESSING', N'PARTIALLY_SHIPPED',
        N'SHIPPED', N'DELIVERED', N'COMPLETED', N'CANCELLED',
        N'REFUND_REQUESTED', N'REFUNDED', N'PAYMENT_FAILED'
    ))
);
GO

CREATE INDEX IX_orders_buyer ON dbo.orders(buyer_id, created_at DESC);
CREATE INDEX IX_orders_status ON dbo.orders(status);
GO

CREATE TABLE dbo.order_items (
    id           UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_order_items_id DEFAULT NEWSEQUENTIALID(),
    order_id     UNIQUEIDENTIFIER NOT NULL,
    product_id   UNIQUEIDENTIFIER NOT NULL,
    sku_code     NVARCHAR(100)    NOT NULL,
    product_name NVARCHAR(300)    NOT NULL,
    unit_price   DECIMAL(14,2)    NOT NULL,
    quantity     INT              NOT NULL,
    line_total   DECIMAL(14,2)    NOT NULL,
    CONSTRAINT PK_order_items PRIMARY KEY (id),
    CONSTRAINT FK_order_items_order FOREIGN KEY (order_id) REFERENCES dbo.orders(id) ON DELETE CASCADE,
    CONSTRAINT CK_order_items_qty CHECK (quantity > 0)
);
GO

CREATE TABLE dbo.order_status_history (
    id          UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_order_history_id DEFAULT NEWSEQUENTIALID(),
    order_id    UNIQUEIDENTIFIER NOT NULL,
    from_status NVARCHAR(30)     NULL,
    to_status   NVARCHAR(30)     NOT NULL,
    reason      NVARCHAR(MAX)    NULL,
    changed_by  UNIQUEIDENTIFIER NULL,
    changed_at  DATETIMEOFFSET   NOT NULL CONSTRAINT DF_order_history_at DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT PK_order_status_history PRIMARY KEY (id),
    CONSTRAINT FK_order_history_order FOREIGN KEY (order_id) REFERENCES dbo.orders(id)
);
GO

CREATE TABLE dbo.payments (
    id                  UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_payments_id DEFAULT NEWSEQUENTIALID(),
    order_id            UNIQUEIDENTIFIER NOT NULL,
    provider            NVARCHAR(30)     NOT NULL CONSTRAINT DF_payments_provider DEFAULT N'STRIPE',
    provider_payment_id NVARCHAR(100)    NULL,
    amount              DECIMAL(14,2)    NOT NULL,
    currency            CHAR(3)          NOT NULL CONSTRAINT DF_payments_currency DEFAULT 'VND',
    status              NVARCHAR(20)     NOT NULL CONSTRAINT DF_payments_status DEFAULT N'INITIATED',
    idempotency_key     NVARCHAR(64)     NOT NULL,
    metadata            NVARCHAR(MAX)    NULL,
    created_at          DATETIMEOFFSET   NOT NULL CONSTRAINT DF_payments_created DEFAULT SYSDATETIMEOFFSET(),
    updated_at          DATETIMEOFFSET   NOT NULL CONSTRAINT DF_payments_updated DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT PK_payments PRIMARY KEY (id),
    CONSTRAINT FK_payments_order FOREIGN KEY (order_id) REFERENCES dbo.orders(id),
    CONSTRAINT UQ_payments_idempotency UNIQUE (idempotency_key),
    CONSTRAINT CK_payments_status CHECK (status IN (N'INITIATED', N'PENDING', N'SUCCEEDED', N'FAILED', N'CANCELLED'))
);
GO

CREATE TABLE dbo.payment_attempts (
    id            UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_payment_attempts_id DEFAULT NEWSEQUENTIALID(),
    payment_id    UNIQUEIDENTIFIER NOT NULL,
    attempt_no    SMALLINT         NOT NULL,
    status        NVARCHAR(20)     NOT NULL,
    error_code    NVARCHAR(50)     NULL,
    error_message NVARCHAR(MAX)    NULL,
    attempted_at  DATETIMEOFFSET   NOT NULL CONSTRAINT DF_payment_attempts_at DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT PK_payment_attempts PRIMARY KEY (id),
    CONSTRAINT FK_payment_attempts_payment FOREIGN KEY (payment_id) REFERENCES dbo.payments(id)
);
GO

CREATE TABLE dbo.refunds (
    id                 UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_refunds_id DEFAULT NEWSEQUENTIALID(),
    order_id           UNIQUEIDENTIFIER NOT NULL,
    payment_id         UNIQUEIDENTIFIER NOT NULL,
    amount             DECIMAL(14,2)    NOT NULL,
    reason             NVARCHAR(MAX)    NULL,
    status             NVARCHAR(20)     NOT NULL CONSTRAINT DF_refunds_status DEFAULT N'REQUESTED',
    provider_refund_id NVARCHAR(100)    NULL,
    requested_by       UNIQUEIDENTIFIER NOT NULL,
    idempotency_key    NVARCHAR(64)     NULL,
    created_at         DATETIMEOFFSET   NOT NULL CONSTRAINT DF_refunds_created DEFAULT SYSDATETIMEOFFSET(),
    updated_at         DATETIMEOFFSET   NOT NULL CONSTRAINT DF_refunds_updated DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT PK_refunds PRIMARY KEY (id),
    CONSTRAINT FK_refunds_order FOREIGN KEY (order_id) REFERENCES dbo.orders(id),
    CONSTRAINT FK_refunds_payment FOREIGN KEY (payment_id) REFERENCES dbo.payments(id),
    CONSTRAINT UQ_refunds_idempotency UNIQUE (idempotency_key)
);
GO

CREATE TABLE dbo.outbox_events (
    id             UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_commerce_outbox_id DEFAULT NEWSEQUENTIALID(),
    aggregate_type NVARCHAR(50)     NOT NULL,
    aggregate_id   UNIQUEIDENTIFIER NOT NULL,
    event_type     NVARCHAR(100)    NOT NULL,
    payload        NVARCHAR(MAX)    NOT NULL,
    published_at   DATETIMEOFFSET   NULL,
    created_at     DATETIMEOFFSET   NOT NULL CONSTRAINT DF_commerce_outbox_created DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT PK_commerce_outbox PRIMARY KEY (id)
);
GO

CREATE INDEX IX_commerce_outbox_unpublished ON dbo.outbox_events(created_at) WHERE published_at IS NULL;
GO

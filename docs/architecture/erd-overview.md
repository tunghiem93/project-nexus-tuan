# ERD Overview — Project Nexus

Mỗi service **sở hữu database riêng** (`Nexus_*` trên SQL Server). Không có FK cross-database; chỉ lưu `UNIQUEIDENTIFIER` reference.

Schema SQL đầy đủ: `services/<service-name>/db/schema.sql`

---

## 1. User Service (`Nexus_User`)

```mermaid
erDiagram
    users ||--o{ user_roles : has
    roles ||--o{ user_roles : assigned
    roles ||--o{ role_privileges : grants
    privileges ||--o{ role_privileges : included
    users ||--o| reputation_scores : has
    users ||--o{ reputation_ratings : gives
    users ||--o{ reputation_ratings : receives
    users ||--o{ reputation_penalties : receives
    users ||--o| reputation_transaction_summary : has
    users ||--o{ user_sessions : has
    users ||--o{ password_reset_tokens : has

    users {
        uuid id PK
        string email
        string username
        string password_hash
        string full_name
        string identify_number
        enum gender
        text address
        date date_of_birth
        string status
        timestamptz deleted_at
    }

    roles {
        uuid id PK
        string code UK
        string name
        text description
    }

    privileges {
        uuid id PK
        string code UK
    }

    reputation_scores {
        uuid user_id PK_FK
        numeric score
        enum trust_level
    }
```

**Quan hệ chính:**
- `users` ↔ `roles`: N-N qua `user_roles`
- `roles` ↔ `privileges`: N-N qua `role_privileges`
- Reputation gắn 1-1 với user (`reputation_scores`)
- Rating tham chiếu `transaction_ref_id` (order/auction ID — external)

---

## 2. Catalog Service (`Nexus_Catalog`)

```mermaid
erDiagram
    categories ||--o{ categories : parent
    categories ||--o{ products : contains
    products ||--o{ product_images : has
    products ||--o| product_discovery_stats : stats

    categories {
        uuid id PK
        uuid parent_id FK
        string name
        string slug UK
        smallint level
    }

    products {
        uuid id PK
        uuid seller_id
        string sku_code
        string name
        numeric base_price
        enum status
        uuid category_id FK
    }

    product_images {
        uuid id PK
        uuid product_id FK
        text url
        smallint sort_order
    }
```

**Lưu ý:** `seller_id` là UUID từ User Service, không FK. `sku_code` liên kết logic với Fulfillment.

---

## 3. Commerce Service (`Nexus_Commerce`)

```mermaid
erDiagram
    carts ||--o{ cart_items : contains
    carts ||--o{ checkout_sessions : initiates
    checkout_sessions ||--o| orders : creates
    orders ||--o{ order_items : contains
    orders ||--o{ order_status_history : tracks
    orders ||--o{ payments : paid_by
    payments ||--o{ payment_attempts : retries
    orders ||--o{ refunds : may_have

    carts {
        uuid id PK
        uuid user_id
        string guest_token
        enum status
        timestamptz expires_at
    }

    orders {
        uuid id PK
        string order_number UK
        uuid buyer_id
        enum source
        enum status
        numeric total_amount
        string idempotency_key UK
    }

    payments {
        uuid id PK
        uuid order_id FK
        enum status
        string provider_payment_id
    }
```

**Partial unique index:** chỉ 1 cart `ACTIVE` / user (`MAX_ACTIVE_CART_PER_USER=1`).

---

## 4. Auction Service (`Nexus_Auction`)

```mermaid
erDiagram
    auctions ||--o{ auction_bids : receives
    auctions ||--o{ auction_extensions : extended_by
    auctions ||--o| auction_settlements : settles

    auctions {
        uuid id PK
        uuid product_id
        uuid seller_id
        enum status
        numeric starting_price
        numeric current_price
        timestamptz scheduled_end_at
        bigint version
    }

    auction_bids {
        uuid id PK
        uuid auction_id FK
        uuid bidder_id
        numeric amount
        boolean is_winning
    }

    auction_settlements {
        uuid id PK
        uuid auction_id FK UK
        enum status
        uuid winner_id
        jsonb settlement_payload
    }
```

**Concurrency:** `auctions.version` cho optimistic locking khi đặt bid.

---

## 5. Fulfillment Service (`Nexus_Fulfillment`)

```mermaid
erDiagram
    warehouses ||--o{ inventory_records : stores
    warehouses ||--o{ inventory_reservations : reserves
    warehouses ||--o{ inventory_ledger : logs
    warehouses ||--o{ shipments : ships

    shipments ||--o{ shipment_status_history : tracks

    inventory_records {
        uuid id PK
        string sku_code
        uuid warehouse_id FK
        int total_quantity
        int available_quantity
        int reserved_quantity
        bigint version
    }

    inventory_reservations {
        uuid id PK
        string sku_code
        uuid warehouse_id FK
        int quantity
        string reference_type
        uuid reference_id
        enum status
        timestamptz expires_at
    }

    inventory_ledger {
        uuid id PK
        string sku_code
        enum movement_type
        int quantity_delta
        timestamptz created_at
    }

    shipments {
        uuid id PK
        uuid order_id
        string carrier_code
        string tracking_number
        enum status
    }
```

**Ràng buộc:** `total = available + reserved + unavailable` (CHECK constraint).

---

## 6. Notification Service (`Nexus_Notification`)

```mermaid
erDiagram
    notifications ||--o{ notification_deliveries : delivered_via
    notification_templates }o--|| event_type : maps

    event_logs {
        uuid id PK
        string event_type
        text event_message
        uuid operator_id
        jsonb payload
    }

    notifications {
        uuid id PK
        uuid user_id
        string event_type
        boolean is_transactional
    }

    notification_deliveries {
        uuid id PK
        uuid notification_id FK
        enum channel
        enum status
        smallint attempt_count
    }

    notification_preferences {
        uuid user_id
        string event_type
        enum channel
        boolean enabled
    }
```

---

## Cross-Service References (không FK)

| Field | Owner DB | Referenced entity | Source service |
|-------|----------|-------------------|----------------|
| `products.seller_id` | catalog | User | user-service |
| `orders.buyer_id` | commerce | User | user-service |
| `auctions.product_id` | auction | Product | catalog-service |
| `inventory_reservations.reference_id` | fulfillment | Order/Checkout/Auction | commerce/auction |
| `shipments.order_id` | fulfillment | Order | commerce-service |
| `reputation_ratings.transaction_ref_id` | user | Order/Auction | commerce/auction |

**Kiểu khóa:** `UNIQUEIDENTIFIER` (SQL Server) thay cho UUID (PostgreSQL).

Validation cross-service: **sync API call** hoặc **eventual consistency qua events**.

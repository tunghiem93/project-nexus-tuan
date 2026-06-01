# ERD cho dbdiagram.io — Project Nexus

dbdiagram.io dùng ngôn ngữ **DBML**. Copy nội dung file `.dbml` và paste vào editor.

## Cách import

1. Mở https://dbdiagram.io
2. **New Diagram** (hoặc Import)
3. Xóa code mẫu → paste toàn bộ nội dung file `.dbml`
4. Diagram tự render; kéo thả để sắp xếp layout
5. Export: **Export → PNG / PDF / SQL** (tùy gói)

## Files

| File | Mục đích |
|------|----------|
| `nexus-all-modules.dbml` | **Tất cả 6 module** trong 1 diagram (có TableGroup) |
| `01-user-service.dbml` | Chỉ User + RBAC + Reputation |
| `02-catalog-service.dbml` | Catalog |
| `03-commerce-service.dbml` | Cart, Checkout, Order, Payment |
| `04-auction-service.dbml` | Auction, Bid, Settlement |
| `05-fulfillment-service.dbml` | Inventory, Warehouse, Shipment |
| `06-notification-service.dbml` | Event log, Notification |

## Gợi ý sử dụng

- **Báo cáo / overview:** dùng `nexus-all-modules.dbml`
- **Thiết kế chi tiết từng service:** dùng file `01` → `06` riêng (diagram gọn hơn)
- **Cross-service refs:** trong file tổng hợp, bỏ comment ở cuối file để hiện liên kết logic giữa services

## Mapping DB ↔ DBML

| SQL Server | DBML |
|------------|------|
| `UNIQUEIDENTIFIER` | `uuid` |
| `NVARCHAR(n)` | `varchar(n)` |
| `NVARCHAR(MAX)` | `text` |
| `DATETIMEOFFSET` | `datetime` |
| `BIT` | `boolean` |
| `DECIMAL(p,s)` | `decimal(p,s)` |

## Database thật (SQL Server 2026)

| Module | Database |
|--------|----------|
| User | `Nexus_User` |
| Catalog | `Nexus_Catalog` |
| Commerce | `Nexus_Commerce` |
| Auction | `Nexus_Auction` |
| Fulfillment | `Nexus_Fulfillment` |
| Notification | `Nexus_Notification` |

Schema SQL gốc: `services/<module>/db/schema.sql`

# Project Nexus — Ma trận chức năng toàn hệ thống

> **4 cột:** Chức năng | Ai xử lý | Hướng đi | Note  
> **Actors:** Buyer, Seller, Admin, Support Staff | **Services:** User, Catalog, Commerce, Auction, Fulfillment, Notification

---

## 1. User Service — Identity & Access Management

| Chức năng | Ai xử lý | Hướng đi | Note |
|-----------|----------|----------|------|
| Đăng ký tài khoản (self-register) | **User Service** ← Buyer (public) | Client → `POST /auth/register` → validate → hash password → gán role `USER` → emit `UserRegistered` → Notification gửi email | `ALLOW_SELF_REGISTRATION=true`, `DEFAULT_USER_ROLE=USER` |
| Admin tạo user | **User Service** ← Admin | Admin Portal → `POST /users` → validate → tạo user + gán role → audit log → `UserRegistered` | Admin cần `USER.CREATE` |
| Cập nhật thông tin user | **User Service** ← Admin / Seller / chính user | Client → `PUT /users/{id}` → validate → update DB → audit → `UserUpdated` | Seller chỉ sửa user trong phạm vi quyền |
| Xem danh sách user | **User Service** ← Admin / Seller | Client → `GET /users` (search, filter, sort) → trả page; empty → "No Data" | Default sort: full name |
| Xem chi tiết user | **User Service** ← Admin / Seller | `GET /users/{id}` → chỉ user chưa soft-delete | `USER_SOFT_DELETE=true` |
| Xóa user (soft delete) | **User Service** ← Admin | `DELETE /users/{id}` → set `deleted_at` → audit → `UserDeleted` | Không xóa vật lý |
| Đăng nhập | **User Service** ← All actors | Client → `POST /auth/login` → check attempts → verify password → JWT + refresh token → `UserLoggedIn` | `MAX_LOGIN_ATTEMPTS=5`, token 60 phút |
| Đăng xuất | **User Service** ← All actors | `POST /auth/logout` → revoke session → `UserLoggedOut` | Session timeout 30 phút |
| Quên mật khẩu | **User Service** ← All actors | Email → token reset → link → `POST /auth/reset-password` → `UserPasswordReset` → Notification | Không tiết lộ email có tồn tại hay không (best practice) |
| Đổi mật khẩu | **User Service** ← User / Admin | Validate policy + khác password cũ → update hash → audit → `UserPasswordChanged` | `PASSWORD_POLICY=STANDARD` |
| Tạo role | **User Service** ← Admin | `POST /roles` → validate code unique → gán privilege hoặc READ_ONLY default → `RoleCreated` | `DEFAULT_ROLE_PRIVILEGES=READ_ONLY` |
| Cập nhật / xóa role | **User Service** ← Admin | CRUD role + privilege mapping → audit | Role hệ thống có thể bảo vệ không xóa |
| Gán role cho user | **User Service** ← Admin | Update `user_roles` → `UserRoleAssigned` | |
| Phân quyền API (RBAC) | **User Service** (middleware) + **API Gateway** | JWT chứa privileges → middleware check `PRIVILEGE.CODE` trước controller | Mọi service validate token; User Service là issuer |
| Xem reputation | **User Service** ← All (public summary) / Admin (chi tiết) | `GET /reputation/{userId}` → đọc `reputation_scores` + summary | Buyer/Seller thấy mức cao; Admin thấy penalty history |
| Rate đối tác giao dịch | **User Service** ← Buyer / Seller | Sau `OrderCompleted` hoặc auction settled → `POST /reputation/ratings` → recalc score → `UserRated` | 1 rating/phía/transaction; không tự rate |
| Tính reputation & trust level | **User Service** (background) | Aggregate ratings + penalties + txn summary → update `reputation_scores` → `ReputationScoreUpdated`, `TrustLevelChanged` | `TRUST_LEVEL_THRESHOLDS` configurable |
| Áp penalty reputation | **User Service** ← Admin / System | Admin manual hoặc consumer event (`AuctionPaymentTimeout`) → insert penalty → trừ score | `PAYMENT_TIMEOUT_PENALTY_SCORE=10` |
| Chặn bid/tạo auction theo reputation | **User Service** (API check) được gọi bởi **Auction Service** | Auction → gọi User API hoặc cache trust level → pass/fail | `MIN_REPUTATION_TO_BID=40`, `MIN_REPUTATION_TO_SELL=50` |
| Audit log nội bộ User | **User Service** | Mọi thay đổi IAM/reputation → `audit_logs` | Admin xem qua `AUDIT_LOG.VIEW` (future admin module) |

---

## 2. Catalog Service — Product & Category

| Chức năng | Ai xử lý | Hướng đi | Note |
|-----------|----------|----------|------|
| Tạo danh mục (category) | **Catalog Service** ← Admin | `POST /categories` → validate level ≤ 3 → save → `CategoryCreated` | `CATEGORY_MAX_LEVEL=3` |
| Sửa / xóa category | **Catalog Service** ← Admin | Update/delete nếu không vi phạm rule → event | Không xóa nếu còn product |
| Xem / list category | **Catalog Service** ← All | Tree/list + search; hierarchical navigation | Default sort `NAME_ASC` |
| Tạo sản phẩm | **Catalog Service** ← Seller / Admin | `POST /products` → status `DRAFT` → `ProductCreated` → Fulfillment tạo SKU record | Max 5 ảnh, `sku_code` link Fulfillment |
| Cập nhật sản phẩm | **Catalog Service** ← Seller / Admin | Validate → update → `ProductUpdated` | |
| Đổi trạng thái product | **Catalog Service** ← Seller / Admin | `DRAFT → ACTIVE → INACTIVE` → `ProductStatusChanged` → Commerce validate cart | |
| Xóa sản phẩm | **Catalog Service** ← Seller / Admin | Chặn nếu có order liên quan | `ALLOW_PRODUCT_DELETE_WITH_ORDER=false` |
| Xem / list / search product | **Catalog Service** ← All | Search name, category, price; filter & sort | Support Staff cũng xem được |
| Product discovery (gợi ý) | **Catalog Service** (read model) | Đọc `product_discovery_stats` (popularity, auction timing) | Cập nhật qua events |
| Upload ảnh sản phẩm | **Catalog Service** + **Object Storage** (external) | Upload file → lưu URL vào `product_images` | `MAX_PRODUCT_IMAGES=5` |

---

## 3. Commerce Service — Cart, Checkout, Order, Payment

| Chức năng | Ai xử lý | Hướng đi | Note |
|-----------|----------|----------|------|
| Tạo / xem giỏ hàng | **Commerce Service** ← Buyer / Guest | Auto-create cart → `CartCreated`; guest dùng `guest_token` | `MAX_ACTIVE_CART_PER_USER=1`, guest cart enabled |
| Thêm / sửa / xóa item cart | **Commerce Service** ← Buyer | Validate product ACTIVE + qty → update line → recalc price → `CartItem*` events | |
| Merge guest cart khi login | **Commerce Service** ← Buyer | Login xong → merge items guest → user cart → `CartMerged` | `CART_MERGE_ON_LOGIN=true` |
| Hết hạn cart | **Commerce Service** (scheduled job) | Job quét cart inactive → `EXPIRED` → `CartExpired` → Fulfillment release reservation nếu có | `CART_EXPIRATION_DAYS=7` |
| Tính giá cart (subtotal, ship estimate) | **Commerce Service** + **Fulfillment** (quote) | Commerce gọi Fulfillment quote API → cộng vào cart | Shipping estimate, revalidate lúc checkout |
| Validate cart trước checkout | **Commerce Service** + **Catalog** (product status) | Check product active, giá, tồn kho (via Fulfillment) | |
| Bắt đầu checkout | **Commerce Service** ← Buyer | `POST /checkout/start` → reprice → **Fulfillment reserve inventory** → lock 15 phút → `CheckoutStarted` | `CHECKOUT_TIMEOUT_MINUTES=15` |
| Xác nhận checkout & tạo order | **Commerce Service** ← Buyer | Confirm → `ORDER.CREATE` (idempotent) → status `PENDING_PAYMENT` → `OrderCreated` | `ORDER_IDEMPOTENCY_ENABLED=true` |
| Khởi tạo thanh toán Stripe | **Commerce Service** → **Stripe** (external) | Tạo PaymentIntent → trả client_secret cho frontend | `PAYMENT_PROVIDER=STRIPE` |
| Webhook xác nhận thanh toán | **Stripe** → **Commerce Service** | Webhook → verify signature → idempotent update → `OrderPaid` hoặc `OrderPaymentFailed` | Retry 3, window 60 phút |
| Xem order / lịch sử order | **Commerce Service** ← Buyer / Seller / Admin / Support | `GET /orders`, filter status/time | Snapshot giá tại thời điểm tạo |
| Hủy order | **Commerce Service** ← Buyer / Seller / Admin | Validate state → cancel → release inventory → refund nếu đã trả | Rule theo state machine |
| Yêu cầu / xử lý refund | **Commerce Service** ← Buyer / Admin / Support | `POST /refunds` → Stripe refund → `OrderRefund*` | Seller/Admin có `ORDER.REFUND` |
| Tạo order từ auction thắng | **Commerce Service** ← **Auction** (event consumer) | Nhận `AuctionWon` payload → `POST /orders/from-auction` → `PENDING_PAYMENT` + deadline 24h | Auction **không** tạo order trực tiếp |
| Invoice / receipt | **Commerce Service** → **Notification** | Order completed → generate PDF/HTML → event gửi email | |
| Order state machine | **Commerce Service** (owner) | PENDING_PAYMENT → PAID → PROCESSING → SHIPPED → DELIVERED → COMPLETED | Xem `state-machines.md` |

---

## 4. Auction Service — Đấu giá & Bidding

| Chức năng | Ai xử lý | Hướng đi | Note |
|-----------|----------|----------|------|
| Tạo auction | **Auction Service** ← Seller | Validate product, reputation, max 5 active/seller → `DRAFT/SCHEDULED` → optional reserve inventory → `AuctionCreated` | `MAX_ACTIVE_AUCTIONS_PER_SELLER=5` |
| Cấu hình auction (giá, thời gian) | **Auction Service** ← Seller | Chỉ **trước khi** auction ACTIVE | Min 60 phút, max 168 giờ |
| Mở auction (scheduled → active) | **Auction Service** (scheduler) | Cron đến `scheduled_start_at` → ACTIVE → `AuctionStarted` | |
| Đặt bid | **Auction Service** ← Buyer | Validate active + increment + reputation → atomic update (version lock) → `BidPlaced`, có thể `Outbid` | `MIN_REPUTATION_TO_BID=40` |
| Xử lý bid đồng thời | **Auction Service** (DB transaction) | Optimistic lock `auctions.version` → highest valid bid wins | |
| Anti-sniping | **Auction Service** (trong Place Bid) | Bid trong 5 phút cuối → extend thêm 5 phút → log `auction_extensions` | `ANTI_SNIPING_ENABLED=true` |
| Xem auction / bid history | **Auction Service** ← All | List/detail/bids theo visibility rules | Public hoặc Restricted |
| Seller hủy auction | **Auction Service** ← Seller | Check eligibility → CANCELLED → `AuctionCancelled` → release inventory | Không hủy khi đã có bid (rule) |
| Admin force cancel | **Auction Service** ← Admin | `AUCTION.ADMIN_CANCEL` → cancel + audit | |
| Kết thúc auction | **Auction Service** (scheduler) | End time → ENDED → lock bid → determine winner | |
| Xác định winner | **Auction Service** | Highest bid → `AuctionWon`; không bid → `AuctionFailed` | |
| Settlement & gửi Commerce | **Auction Service** → event → **Commerce** | `auction_settlements` → emit payload → Commerce tạo order | Exactly-once, idempotent |
| Deadline thanh toán winner | **Auction Service** (job) + **Commerce** | 24h không trả → `AuctionPaymentTimeout` → User penalty | `AUCTION_PAYMENT_DEADLINE_HOURS=24` |
| Reserve inventory cho auction | **Fulfillment Service** ← **Auction** (API/event) | Create auction → reserve SKU | Tránh oversell listing |

---

## 5. Fulfillment Service — Inventory & Shipping

| Chức năng | Ai xử lý | Hướng đi | Note |
|-----------|----------|----------|------|
| Quản lý kho (warehouse) | **Fulfillment Service** ← Admin / System | CRUD warehouse; MVP dùng `WH_001` fixed | `FEATURE_MULTI_WAREHOUSE=false` MVP |
| Quản lý tồn kho SKU | **Fulfillment Service** ← Seller / System | `inventory_records` per SKU per warehouse | SKU-level only, không product-level |
| Nhập / điều chỉnh tồn | **Fulfillment Service** ← Seller / Admin | Movement INTAKE/ADJUST → append `inventory_ledger` | Ledger immutable |
| Reserve inventory (checkout) | **Fulfillment Service** ← **Commerce** | API reserve → trừ available, tăng reserved → `InventoryReserved` | Timeout 30 phút |
| Release reservation | **Fulfillment Service** ← Commerce / Auction / Job | Cancel checkout / timeout → `InventoryReleased` | |
| Commit inventory (sau thanh toán) | **Fulfillment Service** ← **Commerce** (OrderPaid event) | RESERVED → committed (OUT) → `InventoryCommitted` | `ALLOW_OVERSELL=false` |
| Hết hạn reservation | **Fulfillment Service** (job) | Quét expired → release → `InventoryReservationExpired` | |
| Báo giá phí ship (quote) | **Fulfillment Service** → **GHN/GHTK** (adapter) | Nhận địa chỉ + weight → gọi carrier adapter → trả options + fee | Timeout 3s, retry 3 |
| Free shipping | **Fulfillment Service** (rule engine) | Order ≥ 100 → fee = 0 | `FREE_SHIPPING_THRESHOLD=100` |
| Tạo shipment sau OrderPaid | **Fulfillment Service** ← **Commerce** (event) | Chọn carrier → gọi API carrier → lưu tracking → `ShipmentCreated` | Async, không block order |
| Cập nhật trạng thái shipment | **Fulfillment Service** ← Carrier webhook / polling | Map carrier status → internal status → `Shipment*` events → Commerce update order | Idempotent |
| Manual override shipment | **Fulfillment Service** ← Admin / Support | `SHIPPING.MANUAL_OVERRIDE` → audit | |
| Theo dõi hiệu suất carrier | **Fulfillment Service** (analytics) | Aggregate `carrier_performance` | GHN, GHTK |
| Pick / pack / ship workflow | **Fulfillment Service** + **Seller** (portal) | PROCESSING → pick reserved → update ledger SHIP | Phase 1 web portal seller |

---

## 6. Notification Service — Event Log, Email, Push

| Chức năng | Ai xử lý | Hướng đi | Note |
|-----------|----------|----------|------|
| Ghi event log tự động | **Notification Service** (consumer all events) | Mọi domain event → insert `event_logs` | Admin/Support xem `EVENT_LOG.VIEW` |
| Xem list / chi tiết event log | **Notification Service** ← Admin / Support | Search, filter, sort; default mới nhất | |
| Health check message broker | **Notification Service** (job) | Ping RabbitMQ → UP/DOWN → `MessageBrokerUnavailable/Recovered` | Pause consumer khi broker down |
| Trigger notification từ event | **Notification Service** (consumer) | Map event → template → tạo notification → queue gửi | 1 event có thể nhiều channel |
| Gửi email | **Notification Service** → **SMTP/SendGrid** (external) | Transactional + marketing theo preference | MVP: email + push |
| Gửi push notification | **Notification Service** → **FCM/APNs** (external) | Real-time auction, order, outbid | |
| Gửi SMS | **Notification Service** (future) | Tắt MVP | `FEATURE_SMS_NOTIFICATION_ENABLED=false` |
| User notification preferences | **Notification Service** ← User | Bật/tắt loại + channel | Transactional **không** tắt hoàn toàn |
| Retry gửi thất bại | **Notification Service** (job) | Temporary fail → retry 3 lần | `NOTIFICATION_RETRY_LIMIT=3` |
| Lịch sử notification | **Notification Service** ← User | `GET /notifications` | |
| Welcome email đăng ký | **Notification** ← `UserRegistered` | Template + gửi email | |

---

## 7. Chức năng theo Actor (tổng hợp góc nhìn người dùng)

### Buyer

| Chức năng | Ai xử lý | Hướng đi | Note |
|-----------|----------|----------|------|
| Duyệt & tìm sản phẩm | Catalog | Search/filter → xem detail | |
| Thêm giỏ & checkout | Commerce + Fulfillment | Cart → checkout → reserve → pay | |
| Thanh toán | Commerce + Stripe | Frontend Stripe Elements → webhook | |
| Theo dõi đơn hàng | Commerce + Fulfillment | Order detail + shipment tracking | |
| Tham gia đấu giá | Auction + User (reputation check) | Bid → thắng → order → pay trong 24h | |
| Rate seller | User | Sau order completed | |
| Quản lý notification prefs | Notification | Settings trong profile | |

### Seller

| Chức năng | Ai xử lý | Hướng đi | Note |
|-----------|----------|----------|------|
| Quản lý sản phẩm | Catalog + Fulfillment (stock) | CRUD product + nhập kho SKU | |
| Tạo & quản lý auction | Auction + Catalog | Product ACTIVE → create auction | Reputation ≥ 50 |
| Xử lý đơn (xem, cập nhật) | Commerce + Fulfillment | List orders của seller → ship | |
| Tạo shipment | Fulfillment | Sau OrderPaid, seller confirm ship | |
| Rate buyer | User | Sau giao dịch hoàn tất | |

### Admin

| Chức năng | Ai xử lý | Hướng đi | Note |
|-----------|----------|----------|------|
| Quản lý user / role | User | Full IAM CRUD | |
| Quản lý category | Catalog | CRUD category tree | |
| Quản lý toàn bộ order | Commerce | View all, cancel, refund exception | |
| Force cancel auction | Auction | Admin cancel + audit | |
| System config | Config Service / User (phase 1) | Key-value runtime config | `SYSTEM.CONFIG.*` |
| Xem audit / event log | Notification + User | Monitoring & compliance | |
| Điều chỉnh reputation | User | Manual penalty/adjust | |
| Health monitoring | All services + Notification | `/health`, broker, carrier status | |

### Support Staff

| Chức năng | Ai xử lý | Hướng đi | Note |
|-----------|----------|----------|------|
| Xem event log | Notification | Tra cứu sự cố theo correlation_id | |
| Xem order / shipment | Commerce + Fulfillment | Read-only + refund trong phạm vi quyền | |
| Manual shipment override | Fulfillment | Can thiệp trạng thái giao hàng | |
| Xử lý dispute (reputation) | User + Support (future) | Flag rating, adjust sau outcome | SRS mức cao, chưa chi tiết API |

### External Systems

| Chức năng | Ai xử lý | Hướng đi | Note |
|-----------|----------|----------|------|
| Thanh toán thẻ | Stripe ↔ Commerce | PaymentIntent + webhook | Idempotent webhook |
| Vận chuyển GHN/GHTK | Fulfillment ↔ Carrier | Adapter pattern, async | Circuit breaker per carrier |
| Message broker | All services ↔ RabbitMQ | Outbox → publish → consume | Transactional outbox |
| Email provider | Notification ↔ SMTP/API | Async send + retry | |

---

## 8. Luồng nghiệp vụ end-to-end (hướng đi tổng thể)

### 8.1 Mua hàng thường (Cart → Delivery)

```
Buyer → Catalog (search)
     → Commerce (cart)
     → Commerce (checkout start)
     → Fulfillment (reserve inventory)
     → Commerce (create order PENDING_PAYMENT)
     → Stripe (pay)
     → Commerce (OrderPaid)
     → Fulfillment (commit + create shipment)
     → Carrier (pickup → deliver)
     → Fulfillment (ShipmentDelivered)
     → Commerce (OrderCompleted)
     → User (enable rating)
     → Notification (email/push throughout)
```

### 8.2 Đấu giá thắng → Thanh toán

```
Seller → Catalog (product) → Auction (create)
Buyer  → Auction (bid) → Notification (outbid push)
Scheduler → Auction (end) → AuctionWon event
Commerce (create order, deadline 24h)
Buyer → Stripe pay OR timeout → User (penalty)
```

### 8.3 Event-driven backbone

```
Service A: DB transaction + outbox_events
Outbox Publisher → RabbitMQ
Service B: consumer → idempotent → side effect
Notification: always log to event_logs
```

---

## 9. Phân công service ownership (tóm tắt)

| Service | Sở hữu (owner) | Database |
|---------|----------------|----------|
| **User** | Identity, RBAC, JWT, Reputation | `Nexus_User` |
| **Catalog** | Product, Category, Search metadata | `Nexus_Catalog` |
| **Commerce** | Cart, Checkout, Order, Payment, Refund | `Nexus_Commerce` |
| **Auction** | Auction lifecycle, Bid, Settlement | `Nexus_Auction` |
| **Fulfillment** | SKU inventory, Warehouse, Shipment | `Nexus_Fulfillment` |
| **Notification** | Event log, Notification delivery, Preferences | `Nexus_Notification` |

**Quy tắc vàng:** Service A **không** đọc DB của Service B. Chỉ giao tiếp qua **REST API** hoặc **domain events**.

---

## 10. Out-of-scope V1.0 (không ai xử lý trong phase này)

| Chức năng | Note |
|-----------|------|
| Logistics nội bộ (WMS đầy đủ) | Chỉ orchestrate GHN/GHTK |
| HR / Kế toán nội bộ | Không thuộc Nexus |
| App mobile Seller | Phase 1 web portal |
| Promotion / discount code (chi tiết) | In-scope SRS 1.2 nhưng chưa có module riêng — gắn Commerce phase 2 |
| Ticket dispute đầy đủ | Support + reputation flag — thiết kế sau |

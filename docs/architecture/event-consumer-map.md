# Event → Consumer Service Map

Exchange topology (RabbitMQ): **topic exchange** `nexus.events`  
Routing key pattern: `{service}.{eventType}` — ví dụ: `user.UserRegistered`

Mọi consumer phải **idempotent** (`processed_events` table).

Legend:
- **P** = Publisher (sở hữu aggregate, ghi outbox)
- **C** = Consumer (subscribe, xử lý side effect)
- **L** = Event log only (Notification auto-capture tất cả)

---

## User Domain Events

| Event | Publisher | Consumers | Hành động consumer |
|-------|-----------|-----------|-------------------|
| `UserRegistered` | User | Notification (C+L), Catalog† | Welcome email; init default notification prefs |
| `UserUpdated` | User | Notification (L) | — |
| `UserDeleted` | User | Notification (L), Commerce† | Anonymize/hold orders |
| `UserPasswordChanged` | User | Notification (C+L) | Security alert email |
| `UserPasswordReset` | User | Notification (C+L) | Confirmation email |
| `UserLoggedIn` | User | Notification (L) | — |
| `UserLoggedOut` | User | Notification (L) | — |
| `UserRoleAssigned` | User | Notification (L) | — |
| `UserRated` | User | Notification (C) | Notify ratee |
| `ReputationScoreUpdated` | User | Notification (L) | — |
| `ReputationPenaltyApplied` | User | Notification (C) | Notify user + restrict check cache |
| `TrustLevelChanged` | User | Auction (C), Commerce (C) | Refresh eligibility cache |

† Optional/async — không block registration.

---

## Catalog Domain Events

| Event | Publisher | Consumers | Hành động |
|-------|-----------|-----------|-----------|
| `ProductCreated` | Catalog | Notification (L), Fulfillment (C) | Create SKU inventory record (qty=0 or sync) |
| `ProductUpdated` | Catalog | Notification (L), Commerce† | Refresh cart price on next validation |
| `ProductDeleted` | Catalog | Notification (L), Auction (C) | Cancel scheduled auctions if any |
| `ProductStatusChanged` | Catalog | Commerce (C), Auction (C) | Invalidate cart items; block inactive in auction |
| `CategoryCreated` | Catalog | Notification (L) | — |
| `CategoryUpdated` | Catalog | Notification (L) | — |
| `CategoryDeleted` | Catalog | Notification (L) | — |

---

## Commerce Domain Events

| Event | Publisher | Consumers | Hành động |
|-------|-----------|-----------|-----------|
| `CartCreated` | Commerce | Notification (L) | — |
| `CartItemAdded` | Commerce | Notification (L) | — |
| `CartItemUpdated` | Commerce | Notification (L) | — |
| `CartItemRemoved` | Commerce | Notification (L) | — |
| `CartExpired` | Commerce | Notification (L), Fulfillment (C) | Release dangling reservations if any |
| `CheckoutStarted` | Commerce | Fulfillment (C), Notification (L) | Reserve inventory (`InventoryReserved`) |
| `OrderCreated` | Commerce | Notification (C+L), Fulfillment (L) | Order confirmation (transactional) |
| `OrderCancelled` | Commerce | Fulfillment (C), Notification (C), User (C) | Release/commit rollback; update reputation summary |
| `OrderPaid` | Commerce | Fulfillment (C), Notification (C), User (C) | Commit inventory; create shipment; update txn summary |
| `OrderPaymentFailed` | Commerce | Fulfillment (C), Notification (C) | Release reservation; notify buyer |
| `OrderCompleted` | Commerce | Notification (C), User (C) | Enable rating window; update reputation summary |
| `OrderRefundRequested` | Commerce | Notification (C), Notification (L) | Alert admin/support |
| `OrderRefundCompleted` | Commerce | Fulfillment (C), Notification (C) | Restock if returned; confirm refund email |

---

## Auction Domain Events

| Event | Publisher | Consumers | Hành động |
|-------|-----------|-----------|-----------|
| `AuctionCreated` | Auction | Notification (L), Fulfillment (C) | Optional inventory reserve |
| `AuctionScheduled` | Auction | Notification (C), Catalog (C) | Reminder; mark product auction flag |
| `AuctionStarted` | Auction | Notification (C push) | Real-time push to watchers |
| `BidPlaced` | Auction | Notification (L) | — |
| `Outbid` | Auction | Notification (C push) | Push to previous highest bidder |
| `AuctionCancelled` | Auction | Fulfillment (C), Notification (C), Catalog (C) | Release inventory; notify bidders |
| `AuctionEnded` | Auction | Notification (L) | — |
| `AuctionWon` | Auction | Commerce (C), Notification (C), Fulfillment (C) | **Create order** (PENDING_PAYMENT); commit reservation |
| `AuctionFailed` | Auction | Fulfillment (C), Notification (C) | Release inventory; notify seller |
| `AuctionPaymentTimeout` | Auction | User (C), Notification (C), Fulfillment (C) | Apply penalty; release inventory |
| `AuctionSettled` | Auction | Notification (L), User (C) | Finalize reputation summary |

---

## Fulfillment / Inventory Events

| Event | Publisher | Consumers | Hành động |
|-------|-----------|-----------|-----------|
| `InventoryChecked` | Fulfillment | Notification (L) | — |
| `InventoryReserved` | Fulfillment | Commerce (C), Notification (L) | Attach reservation IDs to checkout session |
| `InventoryReleased` | Fulfillment | Commerce (L), Notification (L) | — |
| `InventoryCommitted` | Fulfillment | Notification (L) | — |
| `InventoryAdjusted` | Fulfillment | Notification (L) | — |
| `InventoryReservationExpired` | Fulfillment | Commerce (C) | Fail checkout if still open |
| `ShippingQuoteRequested` | Fulfillment | Notification (L) | — |
| `ShipmentCreated` | Fulfillment | Commerce (C), Notification (C) | Update order → PROCESSING/SHIPPED |
| `ShipmentPickedUp` | Fulfillment | Commerce (C), Notification (C push) | Order status update |
| `ShipmentInTransit` | Fulfillment | Commerce (C), Notification (C) | Tracking update |
| `ShipmentDelivered` | Fulfillment | Commerce (C), Notification (C) | Order → DELIVERED |
| `ShipmentFailed` | Fulfillment | Commerce (C), Notification (C) | Alert buyer/seller |
| `ShipmentReturned` | Fulfillment | Commerce (C), User (C) | May trigger refund flow |

---

## Notification / System Events

| Event | Publisher | Consumers | Hành động |
|-------|-----------|-----------|-----------|
| `NotificationCreated` | Notification | Notification (L) | — |
| `NotificationSent` | Notification | Notification (L) | — |
| `NotificationFailed` | Notification | Notification (L) | Retry job |
| `SystemConfigUpdated` | Config‡ | All services (C) | Refresh local config cache |
| `SystemHealthCheckFailed` | Notification | Notification (L) | Ops alert |
| `SystemHealthCheckRecovered` | Notification | Notification (L) | — |
| `MessageBrokerUnavailable` | Notification | All (C) | Pause outbox publisher |
| `MessageBrokerRecovered` | Notification | All (C) | Resume publisher |

‡ Config có thể nằm trong User hoặc service riêng Admin — tùy phase.

---

## Critical Path Flows (end-to-end)

### A. Cart Checkout → Delivery

```
CheckoutStarted
  → Fulfillment: InventoryReserved
  → OrderCreated
  → (Stripe) OrderPaid
  → Fulfillment: InventoryCommitted + ShipmentCreated
  → ShipmentDelivered
  → Commerce: OrderCompleted
  → User: enable rating
```

### B. Auction Win → Payment

```
AuctionEnded → AuctionWon
  → Commerce: create order (PENDING_PAYMENT)
  → Notification: winner alert
  → OrderPaid OR AuctionPaymentTimeout
    → User: penalty if timeout
```

---

## RabbitMQ Queue Naming Convention

| Queue | Binds to | Consumer service |
|-------|----------|------------------|
| `notification.all-events` | `*.*` | notification-service (event log) |
| `commerce.order-events` | `commerce.Order*` | commerce-service |
| `fulfillment.inventory-events` | `*.Inventory*` | fulfillment-service |
| `fulfillment.shipping-events` | `fulfillment.Shipment*` | fulfillment-service |
| `auction.lifecycle` | `auction.*` | auction-service |
| `user.reputation` | `user.Reputation*` | user-service |

---

## Consumer implementation checklist

1. Deserialize event envelope: `{ eventId, eventType, occurredAt, payload, correlationId }`
2. Check `processed_events` — skip if exists
3. Execute handler in local transaction
4. Insert `processed_events` in same transaction
5. On broker down: pause polling; rely on outbox replay when recovered

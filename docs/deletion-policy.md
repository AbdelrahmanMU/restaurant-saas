# Safe Deletion Policy

## Core Rules

1. **Soft-delete only** for all business entities — no hard deletes.
2. **No cascade deletes** anywhere — all FK constraints are `NO ACTION` / `RESTRICT`.
3. **Deletion is rejected** if the record is actively referenced by another table.
4. **Join/link tables** (UserRole, ProductVariantModifierGroup, BundleSlotChoice) may be hard-deleted — they *are* the reference, not a record being protected.

---

## Soft-Delete Fields by Entity

| Entity | Soft-delete field | Notes |
|---|---|---|
| Restaurant | `IsActive` | No delete endpoint yet |
| Branch | `IsActive` | No delete endpoint yet |
| User | `IsActive` | No delete endpoint yet |
| MenuSection | `IsActive` | DELETE endpoint available |
| Product | `IsActive` | DELETE endpoint available |
| ProductVariant | `IsActive` | DELETE endpoint available |
| ModifierGroup | `IsActive` | DELETE endpoint available |
| ModifierOption | `IsActive` | DELETE endpoint available |
| BundleSlot | `IsActive` | DELETE endpoint available (soft) |
| Bundle | — | Lifecycle tied to Product |
| BundleSlotChoice | — | Link table; hard delete |
| BranchProductVariant | `IsAvailable` | Availability flag; hard delete for record |
| ProductVariantModifierGroup | — | Link table; hard delete |
| UserRole | — | Link table; hard delete |
| Order | — | Completed/Cancelled status, no delete |

---

## Reference-Check Rules (deletion rejected if violated)

| Entity being deleted | Blocking condition | Error code |
|---|---|---|
| MenuSection | Has active Products (`MenuSectionId` FK) | `SECTION_HAS_PRODUCTS` |
| Product | Has active ProductVariants | `PRODUCT_HAS_VARIANTS` |
| ProductVariant | Referenced by BranchProductVariants OR BundleSlotChoices | `VARIANT_IN_USE` |
| ModifierGroup | Has active ModifierOptions | `GROUP_HAS_OPTIONS` |
| ModifierGroup | Referenced by ProductVariantModifierGroups (linked to variants) | `GROUP_HAS_LINKS` |
| BundleSlot | Has BundleSlotChoices | `SLOT_HAS_CHOICES` |

### Required Deletion Order (bottom-up)

```
BranchProductVariants      ← remove first (hard delete)
  ↓
BundleSlotChoices          ← remove first (hard delete)
  ↓
ProductVariantModifierGroups ← unlink first (hard delete)
  ↓
ModifierOptions            ← deactivate (soft delete)
  ↓
ModifierGroup              ← deactivate (soft delete)
  ↓
ProductVariant             ← deactivate (soft delete)
  ↓
BundleSlotChoices          ← remove from slot (hard delete)
  ↓
BundleSlots                ← deactivate (soft delete)
  ↓
Product                    ← deactivate (soft delete)
  ↓
MenuSection                ← deactivate (soft delete)
```

---

## API Error Codes → Arabic Messages

| Error code | HTTP | Arabic message |
|---|---|---|
| `SECTION_HAS_PRODUCTS` | 409 Conflict | يتعذّر حذف القسم لأنه يحتوي على منتجات نشطة. أوقف المنتجات أولاً |
| `PRODUCT_HAS_VARIANTS` | 409 Conflict | يتعذّر حذف المنتج لأنه يحتوي على متغيرات نشطة. أوقف المتغيرات أولاً |
| `VARIANT_IN_USE` | 409 Conflict | يتعذّر حذف هذا المتغير لأنه مستخدم في فروع أو وجبات. أزل الاستخدامات أولاً |
| `GROUP_HAS_OPTIONS` | 409 Conflict | يتعذّر حذف المجموعة لأنها تحتوي على خيارات نشطة. أوقف الخيارات أولاً |
| `GROUP_HAS_LINKS` | 409 Conflict | يتعذّر حذف المجموعة لأنها مرتبطة بمنتجات. أزل الروابط أولاً |
| `SLOT_HAS_CHOICES` | 409 Conflict | يتعذّر حذف الخانة لأنها تحتوي على اختيارات. أزل الاختيارات أولاً |

---

## Database FK Constraints

All foreign keys use `ON DELETE NO ACTION` (enforced at DB level as a second line of defense).
No `CASCADE` or `SET NULL` rules remain.

Changed constraints (previously CASCADE or SET NULL):
- `ProductVariants.ProductId` → NO ACTION
- `ModifierOptions.ModifierGroupId` → NO ACTION
- `ProductVariantModifierGroups.ProductVariantId` → NO ACTION
- `ProductVariantModifierGroups.ModifierGroupId` → NO ACTION
- `Bundles.ProductId` → NO ACTION
- `BundleSlots.BundleId` → NO ACTION
- `BundleSlotChoices.BundleSlotId` → NO ACTION
- `BundleSlotChoices.ProductVariantId` → NO ACTION
- `BranchProductVariants.ProductVariantId` → NO ACTION
- `BranchProductVariants.BranchId` → NO ACTION
- `Products.MenuSectionId` → NO ACTION (was SET NULL)
- `Branches.RestaurantId` → NO ACTION
- `MenuSections.BranchId / RestaurantId` → NO ACTION
- `ModifierGroups.BranchId / RestaurantId` → NO ACTION
- `Products.BranchId / RestaurantId` → NO ACTION
- `Orders.BranchId` → NO ACTION
- `OrderEventLogs.OrderId` → NO ACTION
- `UserRoles.UserId` → NO ACTION
- `Drivers.UserId` → NO ACTION
- `DriverAttributes/Documents/BranchAccess.DriverId` → NO ACTION
- `DriverBranchAccesses.BranchId` → NO ACTION
- `DriverActiveOrders.DriverId / OrderId` → NO ACTION
- `RestaurantSettings.RestaurantId` → NO ACTION

---

## Schema Changes

- `BundleSlots.IsActive` column added (`boolean NOT NULL DEFAULT true`)
- EF query filter on `BundleSlot`: `HasQueryFilter(s => s.IsActive)` — inactive slots are invisible to all queries

# TESTING_NOTES

> Fill the two `TODO` values after you run `dotnet test` and the coverage commands on your machine.

## 1. Tests

- Total tests: 21 test cases (18 test methods; the two `[Theory]`s expand to 2 + 3 cases) — **TODO: confirm the number shown by `dotnet test`**
- Passing: **TODO** / Failing: **TODO** (expected 0 failing)

## 2. Line coverage

- Line coverage: **TODO %** (copy from `coverage-report/Summary.txt`; target ≥ 80% for Domain and Application)

## 3. Libraries

- **Moq:** mocked `IOrderRepository`, `IInventoryService` and `INotificationService` in `CheckoutServiceTests` to verify `Times.Once` / `Times.Never` for save, inventory and notification calls.
- **AutoFixture:** `TestData/AutoFixtureFactory.cs` generates an `Order` with 3 items, customized so quantity > 0 and price ≥ 0, used in `GeneratedDataCheckoutTests`.
- **Bogus:** `TestData/CustomerFaker.cs` generates a realistic customer name and email with a fixed seed (12345), used to check the receipt copies them.

## 4. Fairness of Examples B and C

No, it is not fair: a customer paying 999.99 (no discount) pays more than one who buys 1,000 and pays 900, so the discount threshold creates a cliff where spending slightly more costs less.

## 5. Bugs found

- None that break the business rules. Observations (not fixed, `src/` unchanged):
  - If saving succeeds but sending the notification throws, the order stays saved with no confirmation (no rollback).
  - Inventory is checked per line, so the same product appearing in two lines is never checked for the combined quantity.

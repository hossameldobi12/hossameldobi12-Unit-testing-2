# Testing Notes

### 1. Test Results
- Total tests: 26
- Passing: 26
- Failing: 0

### 2. Line Coverage
- Overall Line Coverage: 97.4%
- Application: 95.8%
- Domain: 100%

### 3. Test Data and Mocking Tools
- **Moq:** Used to mock dependencies such as `IInventoryService`, `IOrderRepository`, and `INotificationService`, and verify their interactions.
- **AutoFixture:** Used to automatically generate test objects such as `OrderItem` and `Order`.
- **Bogus:** Used to generate fake customer names and email addresses with a fixed seed for repeatable test data.

### 4. Discount Fairness
No, it is not fair because a customer with a subtotal of 1,000 pays 900 after the discount, while a customer with a subtotal of 999.99 pays the full amount.

### 5. Potential Bugs
The discount threshold creates a large price difference between orders just below and exactly at 1,000, which may be an unfair pricing edge case depending on the intended business rules.
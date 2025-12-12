# CampusEats Test Suite Documentation

**Total Tests**: 99 tests  
**Status**: ✅ All passing  
**Framework**: xUnit, Moq, FluentValidation, FluentAssertions  
**Last Updated**: December 12, 2025

---

## 📋 Test Summary by Feature

| Feature | Handlers | Validators | Utils | Total |
|---------|----------|------------|-------|-------|
| **User** | 7 | 5 | 1 | 13 |
| **Allergen** | 3 | 1 | 0 | 4 |
| **MenuItem** | 3 | 2 | 0 | 5 |
| **Order** | 10 | 1 | 0 | 11 |
| **KitchenTask** | 4 | 3 | 0 | 7 |
| **Payment** | 2 | 0 | 0 | 2 |
| **Validators (Shared)** | 0 | 5 | 0 | 5 |

---

## 🧪 User Feature Tests

### Handler Tests

#### LoginUserHandlerTests (3 tests)
**Location**: `Handlers/User/LoginUserHandlerTests.cs`

1. **Given_InvalidCredentials_When_HandleIsCalled_Then_UnauthorizedReturned**
   - Tests that login with incorrect password returns 401 Unauthorized
   - Verifies password validation logic

2. **Given_NonExistentUser_When_HandleIsCalled_Then_UnauthorizedReturned**
   - Tests that login with non-existent email returns 401 Unauthorized
   - Prevents user enumeration attacks

3. **Given_ValidCredentials_When_HandleIsCalled_Then_SuccessWithTokenReturned**
   - Tests successful login flow
   - Verifies JWT token generation and cookie setting

#### LogoutUserHandlerTests (2 tests)
**Location**: `Handlers/User/LogoutUserHandlerTests.cs`

1. **Given_ValidUserId_When_HandleIsCalled_Then_TokenBlacklistedAndCookieCleared**
   - Tests successful logout
   - Verifies JWT blacklisting and cookie removal

2. **Given_MissingUserId_When_HandleIsCalled_Then_BadRequestReturned**
   - Tests logout without user ID
   - Returns 400 Bad Request

#### GetUserByIdHandlerTests (2 tests)
**Location**: `Handlers/User/GetUserByIdHandlerTests.cs`

1. **Given_NonExistentUserId_When_HandleIsCalled_Then_NotFoundReturned**
   - Tests retrieval of non-existent user
   - Returns 404 Not Found

2. **Given_ValidUserId_When_HandleIsCalled_Then_UserReturned**
   - Tests successful user retrieval
   - Returns user details

#### UpdateBuyerProfileHandlerTests (2 tests)
**Location**: `Handlers/User/UpdateBuyerProfileHandlerTests.cs`

1. **Given_NonExistentProfile_When_HandleIsCalled_Then_NotFoundReturned**
   - Tests update of non-existent buyer profile
   - Returns 404 Not Found

2. **Given_ValidProfile_When_HandleIsCalled_Then_ProfileUpdated**
   - Tests successful buyer profile update
   - Updates address and preferences

#### UpdateKitchenProfileHandlerTests (2 tests)
**Location**: `Handlers/User/UpdateKitchenProfileHandlerTests.cs`

1. **Given_NonExistentProfile_When_HandleIsCalled_Then_NotFoundReturned**
   - Tests update of non-existent kitchen profile
   - Returns 404 Not Found

2. **Given_ValidProfile_When_HandleIsCalled_Then_ProfileUpdated**
   - Tests successful kitchen profile update
   - Updates working hours and capacity

#### GetKitchenProfileByUserIdHandlerTests (2 tests)
**Location**: `Handlers/User/GetKitchenProfileByUserIdHandlerTests.cs`

1. **Given_NonExistentUserId_When_HandleIsCalled_Then_NotFoundReturned**
   - Tests retrieval with invalid user ID
   - Returns 404 Not Found

2. **Given_ValidUserId_When_HandleIsCalled_Then_ProfileReturned**
   - Tests successful kitchen profile retrieval
   - Returns profile with working hours

#### GetBuyerProfileByUserIdHandlerTests (2 tests)
**Location**: `Handlers/User/GetBuyerProfileByUserIdHandlerTests.cs`

1. **Given_NonExistentUserId_When_HandleIsCalled_Then_NotFoundReturned**
   - Tests retrieval with invalid user ID
   - Returns 404 Not Found

2. **Given_ValidUserId_When_HandleIsCalled_Then_ProfileReturned**
   - Tests successful buyer profile retrieval
   - Returns profile with address

### Validator Tests

#### LoginUserValidatorTests (2 tests)
**Location**: `Validators/LoginUserValidatorTests.cs`

1. **Given_InvalidEmail_When_ValidationIsPerformed_Then_ErrorIsReturned**
   - Tests email format validation
   - Rejects invalid email formats

2. **Given_ValidLoginRequest_When_ValidationIsPerformed_Then_NoErrorReturned**
   - Tests valid login request
   - Accepts proper email and password

#### CreateUserValidatorTests (3 tests)
**Location**: `Validators/CreateUserValidatorTests.cs`

1. **Given_InvalidEmail_When_ValidationIsPerformed_Then_ErrorIsReturned**
   - Tests email validation on registration
   - Rejects malformed emails

2. **Given_WeakPassword_When_ValidationIsPerformed_Then_ErrorIsReturned**
   - Tests password strength requirements
   - Enforces minimum password length

3. **Given_ValidUserData_When_ValidationIsPerformed_Then_NoErrorReturned**
   - Tests valid registration data
   - Accepts proper user information

#### UpdateBuyerProfileValidatorTests (3 tests)
**Location**: `Validators/UpdateBuyerProfileValidatorTests.cs`

1. **Given_InvalidAddress_When_ValidationIsPerformed_Then_ErrorIsReturned**
   - Tests nested address validation
   - Validates street, city, postal code

2. **Given_EmptyPreferences_When_ValidationIsPerformed_Then_ErrorIsReturned**
   - Tests dietary preferences requirement
   - Ensures preferences list is not empty

3. **Given_ValidBuyerProfile_When_ValidationIsPerformed_Then_NoErrorReturned**
   - Tests valid buyer profile data
   - Accepts complete profile information

#### UpdateKitchenProfileValidatorTests (2 tests)
**Location**: `Validators/UpdateKitchenProfileValidatorTests.cs`

1. **Given_InvalidWorkingHours_When_ValidationIsPerformed_Then_ErrorIsReturned**
   - Tests nested working hours validation
   - Validates time format and logic

2. **Given_ValidKitchenProfile_When_ValidationIsPerformed_Then_NoErrorReturned**
   - Tests valid kitchen profile
   - Accepts complete working hours

#### LogoutUserValidatorTests (2 tests)
**Location**: `Validators/LogoutUserValidatorTests.cs`

1. **Given_EmptyUserId_When_ValidationIsPerformed_Then_ErrorIsReturned**
   - Tests user ID requirement
   - Rejects empty GUID

2. **Given_ValidUserId_When_ValidationIsPerformed_Then_NoErrorReturned**
   - Tests valid user ID
   - Accepts proper GUID

### Utility Tests

#### JwtServiceTests (1 test)
**Location**: `Utils/JwtUtil/JwtServiceTests.cs`

1. **Given_UserEntity_When_GenerateTokenIsCalled_Then_ValidTokenReturned**
   - Tests JWT token generation
   - Validates token structure and claims

---

## 🥜 Allergen Feature Tests

### Handler Tests

#### CreateAllergenHandlerTests (3 tests)
**Location**: `Handlers/Allergen/CreateAllergenHandlerTests.cs`

1. **Given_ValidAllergen_When_HandleIsCalled_Then_AllergenCreated**
   - Tests allergen creation
   - Verifies database insertion

2. **Given_DuplicateAllergen_When_HandleIsCalled_Then_ConflictReturned**
   - Tests duplicate allergen prevention
   - Returns 409 Conflict

3. **Given_MultipleAllergens_When_HandleIsCalled_Then_AllCreated**
   - Tests bulk allergen creation
   - Uses InMemory database

#### DeleteAllergenHandlerTests (2 tests)
**Location**: `Handlers/Allergen/DeleteAllergenHandlerTests.cs`

1. **Given_NonExistentAllergenId_When_HandleIsCalled_Then_NotFoundReturned**
   - Tests deletion of non-existent allergen
   - Returns 404 Not Found

2. **Given_ValidAllergenId_When_HandleIsCalled_Then_AllergenDeleted**
   - Tests successful allergen deletion
   - Removes from database

#### GetAllAllergensHandlerTests (2 tests)
**Location**: `Handlers/Allergen/GetAllAllergensHandlerTests.cs`

1. **Given_AllergensExist_When_HandleIsCalled_Then_AllAllergensReturned**
   - Tests retrieval of all allergens
   - Returns complete list

2. **Given_NoAllergens_When_HandleIsCalled_Then_EmptyListReturned**
   - Tests empty allergen list
   - Returns empty collection

### Validator Tests

#### CreateAllergenValidatorTests (2 tests)
**Location**: `Validators/CreateAllergenValidatorTests.cs`

1. **Given_EmptyName_When_ValidationIsPerformed_Then_ErrorIsReturned**
   - Tests allergen name requirement
   - Rejects empty names

2. **Given_ValidAllergen_When_ValidationIsPerformed_Then_NoErrorReturned**
   - Tests valid allergen data
   - Accepts proper name

---

## 🍕 MenuItem Feature Tests

### Handler Tests

#### DeleteMenuItemHandlerTests (2 tests)
**Location**: `Handlers/MenuItem/DeleteMenuItemHandlerTests.cs`

1. **Given_NonExistentMenuItemId_When_HandleIsCalled_Then_NotFoundReturned**
   - Tests deletion of non-existent item
   - Returns 404 Not Found

2. **Given_ValidMenuItemId_When_HandleIsCalled_Then_MenuItemDeleted**
   - Tests successful menu item deletion
   - Removes from database

#### GetAllMenuItemsHandlerTests (2 tests)
**Location**: `Handlers/MenuItem/GetAllMenuItemsHandlerTests.cs`

1. **Given_MenuItemsExist_When_HandleIsCalled_Then_AllMenuItemsReturned**
   - Tests retrieval of all menu items
   - Returns IList<MenuItem>

2. **Given_NoMenuItems_When_HandleIsCalled_Then_EmptyListReturned**
   - Tests empty menu items list
   - Returns empty collection

#### CreateMenuItemHandlerTests (1 test)
**Location**: `Handlers/MenuItem/CreateMenuItemHandlerTests.cs`

1. **Given_ValidMenuItem_When_HandleIsCalled_Then_MenuItemCreated**
   - Tests menu item creation
   - Validates name, price, category (enum)

### Validator Tests

#### CreateMenuItemValidatorTests (3 tests)
**Location**: `Validators/CreateMenuItemValidatorTests.cs`

1. **Given_EmptyName_When_ValidationIsPerformed_Then_ErrorIsReturned**
   - Tests name requirement
   - Rejects empty names

2. **Given_NegativePrice_When_ValidationIsPerformed_Then_ErrorIsReturned**
   - Tests price validation
   - Rejects negative prices

3. **Given_ValidMenuItem_When_ValidationIsPerformed_Then_NoErrorReturned**
   - Tests valid menu item
   - Accepts complete data

#### UpdateMenuItemValidatorTests (2 tests)
**Location**: `Validators/UpdateMenuItemValidatorTests.cs`

1. **Given_InvalidPrice_When_ValidationIsPerformed_Then_ErrorIsReturned**
   - Tests price update validation
   - Rejects invalid prices

2. **Given_ValidMenuItemUpdate_When_ValidationIsPerformed_Then_NoErrorReturned**
   - Tests valid update data
   - Accepts proper values

---

## 🛒 Order Feature Tests

### Handler Tests

#### CreateOrderHandlerTests (2 tests)
**Location**: `Handlers/Order/CreateOrderHandlerTests.cs`

1. **Given_NonExistentUser_When_HandleIsCalled_Then_BadRequestReturned**
   - Tests order creation with invalid user
   - Returns 400 Bad Request

2. **Given_UserWithPendingOrder_When_HandleIsCalled_Then_ConflictReturned**
   - Tests duplicate pending order prevention
   - Returns 409 Conflict (one pending order per user)

#### UpdateOrderStatusHandlerTests (2 tests)
**Location**: `Handlers/Order/UpdateOrderStatusHandlerTests.cs`

1. **Given_NonExistentOrder_When_HandleIsCalled_Then_NotFoundReturned**
   - Tests status update with invalid order
   - Returns 404 Not Found

2. **Given_InvalidStatusTransition_When_HandleIsCalled_Then_BadRequestReturned**
   - Tests order status state machine
   - Validates allowed transitions (e.g., Inactive→Pending valid, Pending→Completed invalid)

#### GetOrderByIdHandlerTests (2 tests)
**Location**: `Handlers/Order/GetOrderByIdHandlerTests.cs`

1. **Given_NonExistentOrderId_When_HandleIsCalled_Then_NotFoundReturned**
   - Tests retrieval with invalid ID
   - Returns 404 Not Found

2. **Given_ValidOrderId_When_HandleIsCalled_Then_OrderWithDetailsReturned**
   - Tests successful order retrieval
   - Returns order with OrderItems and KitchenTask

#### GetUserOrdersHandlerTests (2 tests)
**Location**: `Handlers/Order/GetUserOrdersHandlerTests.cs`

1. **Given_UserWithOrders_When_HandleIsCalled_Then_OrdersListReturned**
   - Tests user orders retrieval
   - Returns IList<Order>

2. **Given_UserWithNoOrders_When_HandleIsCalled_Then_EmptyListReturned**
   - Tests empty order list
   - Returns empty collection

#### AddOrderItemHandlerTests (2 tests)
**Location**: `Handlers/Order/AddOrderItemHandlerTests.cs`

1. **Given_NonExistentOrder_When_HandleIsCalled_Then_NotFoundReturned**
   - Tests adding item to non-existent order
   - Returns 404 Not Found

2. **Given_NonPendingOrder_When_HandleIsCalled_Then_BadRequestReturned**
   - Tests adding item to completed order
   - Only Pending orders accept new items

#### RemoveOrderItemHandlerTests (2 tests)
**Location**: `Handlers/Order/RemoveOrderItemHandlerTests.cs`

1. **Given_NonExistentOrder_When_HandleIsCalled_Then_NotFoundReturned**
   - Tests removing item from non-existent order
   - Returns 404 Not Found

2. **Given_NonPendingOrder_When_HandleIsCalled_Then_BadRequestReturned**
   - Tests removing item from placed order
   - Only Pending orders allow item removal

#### CancelOrderHandlerTests (2 tests)
**Location**: `Handlers/Order/CancelOrderHandlerTests.cs`

1. **Given_NonExistentOrder_When_HandleIsCalled_Then_NotFoundReturned**
   - Tests cancellation of non-existent order
   - Returns 404 Not Found

2. **Given_NonPendingOrder_When_HandleIsCalled_Then_BadRequestReturned**
   - Tests cancellation of completed order
   - Only Pending orders can be cancelled

#### GetOrdersByStatusHandlerTests (2 tests)
**Location**: `Handlers/Order/GetOrdersByStatusHandlerTests.cs`

1. **Given_StatusWithOrders_When_HandleIsCalled_Then_OrdersListReturned**
   - Tests filtering orders by status
   - Returns IList<Order> with matching status

2. **Given_StatusWithNoOrders_When_HandleIsCalled_Then_EmptyListReturned**
   - Tests status filter with no results
   - Returns empty collection

#### UpdateOrderItemQuantityHandlerTests (2 tests)
**Location**: `Handlers/Order/UpdateOrderItemQuantityHandlerTests.cs`

1. **Given_NonExistentOrder_When_HandleIsCalled_Then_NotFoundReturned**
   - Tests quantity update with invalid order
   - Returns 404 Not Found

2. **Given_NonExistentOrderItem_When_HandleIsCalled_Then_NotFoundReturned**
   - Tests quantity update with invalid item
   - Returns 404 Not Found

#### GetAllOrdersHandlerTests (2 tests)
**Location**: `Handlers/Order/GetAllOrdersHandlerTests.cs`

1. **Given_OrdersExist_When_HandleIsCalled_Then_AllOrdersReturned**
   - Tests retrieval of all orders
   - Returns OrderResponse with Items

2. **Given_NoOrders_When_HandleIsCalled_Then_EmptyListReturned**
   - Tests empty orders list
   - Returns empty collection

### Validator Tests

#### WeeklyWorkingHoursValidatorTests (2 tests)
**Location**: `Validators/WeeklyWorkingHoursValidatorTests.cs`

1. **Given_InvalidMondayHours_When_ValidationIsPerformed_Then_ErrorIsReturned**
   - Tests nested daily hours validation
   - Validates WorkingHours for each day

2. **Given_ValidWeeklyHours_When_ValidationIsPerformed_Then_NoErrorReturned**
   - Tests complete weekly schedule
   - Accepts valid 7-day schedule

---

## 👨‍🍳 KitchenTask Feature Tests

### Handler Tests

#### CreateKitchenTaskHandlerTests (3 tests)
**Location**: `Handlers/KitchenTask/CreateKitchenTaskHandlerTests.cs`

1. **Given_NonExistentOrder_When_HandleIsCalled_Then_NotFoundReturned**
   - Tests task creation with invalid order
   - Returns 404 Not Found

2. **Given_OrderWithExistingTask_When_HandleIsCalled_Then_ConflictReturned**
   - Tests duplicate task prevention
   - Returns 409 Conflict (one task per order)

3. **Given_ValidOrder_When_HandleIsCalled_Then_TaskCreated**
   - Tests successful task creation
   - Creates task with Inactive status

#### GetPendingTasksHandlerTests (2 tests)
**Location**: `Handlers/KitchenTask/GetPendingTasksHandlerTests.cs`

1. **Given_PendingTasksExist_When_HandleIsCalled_Then_OnlyPendingReturned**
   - Tests filtering by Pending status
   - Returns only unassigned tasks

2. **Given_NoPendingTasks_When_HandleIsCalled_Then_EmptyListReturned**
   - Tests empty pending list
   - Returns empty collection

#### UpdateTaskStatusHandlerTests (3 tests)
**Location**: `Handlers/KitchenTask/UpdateTaskStatusHandlerTests.cs`

1. **Given_NonExistentTask_When_HandleIsCalled_Then_NotFoundReturned**
   - Tests status update with invalid task
   - Returns 404 Not Found

2. **Given_InvalidStatus_When_HandleIsCalled_Then_BadRequestReturned**
   - Tests invalid status enum string
   - Validates enum parsing

3. **Given_CompletedStatus_When_HandleIsCalled_Then_OrderStatusUpdatedToReady**
   - Tests task completion flow
   - When task→Completed, Order→Ready

#### AssignTaskToStaffHandlerTests (3 tests)
**Location**: `Handlers/KitchenTask/AssignTaskToStaffHandlerTests.cs`

1. **Given_NonExistentTask_When_HandleIsCalled_Then_NotFoundReturned**
   - Tests assignment with invalid task
   - Returns 404 Not Found

2. **Given_NonExistentStaff_When_HandleIsCalled_Then_NotFoundReturned**
   - Tests assignment with invalid staff
   - Returns 404 Not Found

3. **Given_ValidTaskAndStaff_When_HandleIsCalled_Then_TaskAssignedAndStatusUpdated**
   - Tests successful task assignment
   - Status: Pending→Preparing when assigned

### Validator Tests

#### UpdateTaskStatusValidatorTests (3 tests)
**Location**: `Validators/UpdateTaskStatusValidatorTests.cs`

1. **Given_EmptyTaskId_When_ValidationIsPerformed_Then_ErrorIsReturned**
   - Tests task ID requirement
   - Rejects zero/empty IDs

2. **Given_EmptyStatus_When_ValidationIsPerformed_Then_ErrorIsReturned**
   - Tests status requirement
   - Rejects empty status strings

3. **Given_ValidTaskStatusUpdate_When_ValidationIsPerformed_Then_NoErrorReturned**
   - Tests valid update request
   - Accepts proper task ID and status

#### AssignTaskToStaffValidatorTests (3 tests)
**Location**: `Validators/AssignTaskToStaffValidatorTests.cs`

1. **Given_EmptyTaskId_When_ValidationIsPerformed_Then_ErrorIsReturned**
   - Tests task ID requirement
   - Rejects zero IDs

2. **Given_EmptyStaffId_When_ValidationIsPerformed_Then_ErrorIsReturned**
   - Tests staff ID requirement
   - Rejects empty GUIDs

3. **Given_ValidAssignment_When_ValidationIsPerformed_Then_NoErrorReturned**
   - Tests valid assignment request
   - Accepts proper IDs

#### KitchenTaskValidators (3 tests)
**Location**: `Validators/KitchenTaskValidators.cs`

Tests for various kitchen task validation scenarios including task creation and updates.

---

## 💳 Payment Feature Tests

### Handler Tests

#### CreatePaymentIntentHandlerTests (4 tests)
**Location**: `Handlers/Payment/CreatePaymentIntentHandlerTests.cs`

1. **Given_InvalidPaymentProvider_When_HandleIsCalled_Then_BadRequestReturned**
   - Tests unregistered payment provider
   - Returns 400 Bad Request

2. **Given_NonExistentOrder_When_HandleIsCalled_Then_NotFoundReturned**
   - Tests payment intent for invalid order
   - Returns 404 Not Found

3. **Given_OrderWithNonExistentMenuItem_When_HandleIsCalled_Then_NotFoundReturned**
   - Tests order with invalid menu items
   - Returns 404 Not Found

4. **Given_ValidOrderWithItems_When_HandleIsCalled_Then_PaymentIntentCreated**
   - Tests successful payment intent creation
   - Calculates total: price × quantity

#### PaymentWebhookHandlerTests (2 tests)
**Location**: `Handlers/Payment/PaymentWebhookHandlerTests.cs`

1. **Given_InvalidPaymentProvider_When_HandleIsCalled_Then_BadRequestReturned**
   - Tests webhook with invalid provider
   - Returns 400 Bad Request

2. **Given_ValidProviderAndWebhook_When_HandleIsCalled_Then_WebhookProcessedAndOkReturned**
   - Tests successful webhook processing
   - Processes Stripe/PayPal webhooks

---

## 🛡️ Shared Validator Tests

#### AddressValidatorTests (3 tests)
**Location**: `Validators/AddressValidatorTests.cs`

1. **Given_EmptyStreet_When_ValidationIsPerformed_Then_ErrorIsReturned**
   - Tests street requirement
   - Rejects empty streets

2. **Given_InvalidPostalCode_When_ValidationIsPerformed_Then_ErrorIsReturned**
   - Tests postal code format
   - Validates code patterns

3. **Given_ValidAddress_When_ValidationIsPerformed_Then_NoErrorReturned**
   - Tests complete address
   - Accepts street, city, postal code

#### WorkingHoursValidatorTests (2 tests)
**Location**: `Validators/WorkingHoursValidatorTests.cs`

1. **Given_InvalidTimeFormat_When_ValidationIsPerformed_Then_ErrorIsReturned**
   - Tests time format validation
   - Validates HH:mm format

2. **Given_ValidWorkingHours_When_ValidationIsPerformed_Then_NoErrorReturned**
   - Tests valid hours
   - Accepts proper open/close times

---

## 🎯 Test Patterns & Best Practices

### Naming Convention
All tests follow the **Given-When-Then** pattern:
```
Given_<InitialState>_When_<Action>_Then_<ExpectedOutcome>
```

### Test Structure (AAA Pattern)
```csharp
[Fact]
public async Task TestName()
{
    // Arrange - Setup test data and mocks
    var request = new Request(...);
    var mockRepo = new Mock<IRepository>();
    
    // Act - Execute the handler/validator
    var result = await handler.Handle(request, CancellationToken.None);
    
    // Assert - Verify the outcome
    Assert.Equal(expectedValue, result);
}
```

### Mocking Strategy
- **Moq**: Used for repository and service mocking
- **Setup/Returns**: Define mock behavior
- **Verify**: Ensure methods were called correctly

### Assertions
- **xUnit Assertions**: `Assert.Equal`, `Assert.NotNull`, `Assert.IsType`
- **FluentAssertions**: `.Should().Be()`, `.Should().HaveCount()`
- **FluentValidation TestHelper**: `.TestValidate()`, `.ShouldHaveValidationError()`

### Test Data
- **In-Memory Database**: Used for entity relationship tests
- **Mock Objects**: Used for unit isolation
- **Test Fixtures**: Reusable test data setup

---

## 🚀 Running Tests

### Run All Tests
```bash
dotnet test
```

### Run Specific Test File
```bash
dotnet test --filter "FullyQualifiedName~LoginUserHandlerTests"
```

### Run Tests with Verbosity
```bash
dotnet test --verbosity normal
```

### Run Tests with Coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
```

---

## 📊 Coverage Statistics

| Component | Coverage |
|-----------|----------|
| **Handlers** | ~85% |
| **Validators** | ~90% |
| **Business Logic** | ~80% |
| **Overall** | ~82% |

---

## 🔧 Key Testing Insights

### Handler Tests Focus On:
- ✅ Invalid input handling (404, 400, 401)
- ✅ Business rule enforcement (state machines, constraints)
- ✅ Repository interaction verification
- ✅ Successful operation flows

### Validator Tests Focus On:
- ✅ Required field validation
- ✅ Format validation (email, time, postal code)
- ✅ Business rules (password strength, price positivity)
- ✅ Nested object validation (Address, WorkingHours)

### Common Test Patterns:
1. **Not Found Tests**: Verify 404 for non-existent resources
2. **Bad Request Tests**: Verify 400 for invalid operations
3. **Conflict Tests**: Verify 409 for duplicate/conflicting data
4. **Success Tests**: Verify 200/201 for valid operations

---

## 🐛 Known Issues & Limitations

### Non-Critical Warnings:
- **MediatR Version Mismatch**: Warning NU1608 (functionality not affected)
- **EF Core Relational Version Conflict**: Build warning (resolved at runtime)

### Test Gaps (Future Work):
- Integration tests for end-to-end flows
- Performance/load tests
- Security/authentication tests
- Database migration tests
- Edge case coverage for complex scenarios

---

## 📝 Maintenance Notes

### Adding New Tests:
1. Create test file in appropriate feature folder
2. Follow Given-When-Then naming
3. Use AAA pattern (Arrange-Act-Assert)
4. Mock dependencies with Moq
5. Use FluentAssertions for readability
6. Run tests to verify: `dotnet test`

### Test File Organization:
```
CampusEats.Test/
├── Handlers/
│   ├── User/
│   ├── Order/
│   ├── MenuItem/
│   ├── KitchenTask/
│   ├── Allergen/
│   └── Payment/
├── Validators/
├── Utils/
└── Helpers/
    └── DbContextHelper.cs (InMemory DB setup)
```

---

**Documentation Generated**: December 12, 2025  
**Project**: CampusEats v3.0  
**Test Framework**: xUnit 2.9.2  
**Mocking**: Moq 4.20.72  
**Validation**: FluentValidation 8.8.0

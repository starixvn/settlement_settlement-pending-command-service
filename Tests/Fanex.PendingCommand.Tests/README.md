# Fanex.PendingCommand.Tests

Unit tests cho library **Fanex.PendingCommand** sử dụng NSubstitute và XUnit.

## Cấu trúc Test

### Services Tests
- **PendingCommandControllerTests.cs** - Tests cho controller chính xử lý pending commands
- **CommandServiceTests.cs** - Tests cho service lấy pending commands từ database

### System Name Tests
- **CommandProcessorTests.cs** - Tests cho các command processors (AutoPayout, Broadcast)

### Test Helpers
- **TestDataBuilder.cs** - Helper class để tạo test data

## Cách chạy Tests

### Sử dụng Visual Studio
1. Mở solution trong Visual Studio
2. Mở Test Explorer (Test > Test Explorer)
3. Click "Run All" để chạy tất cả tests

### Sử dụng Command Line
```bash
# Chạy tất cả tests
dotnet test

# Chạy tests với coverage
dotnet test --collect:"XPlat Code Coverage"

# Chạy tests cụ thể
dotnet test --filter "FullyQualifiedName~PendingCommandControllerTests"

# Chạy tests với verbose output
dotnet test --verbosity normal
```

## Test Patterns

### Naming Convention
- Không sử dụng underscore prefix cho tên biến
- Test method names: `MethodName_Scenario_ExpectedResult`
- Ví dụ: `ProcessAsync_WhenNotUnderMaintenance_ShouldProcessCommands`

### Test Structure (AAA Pattern)
```csharp
[Fact]
public async Task MethodName_Scenario_ExpectedResult()
{
    // Arrange
    var dependency = Substitute.For<IDependency>();
    var service = new Service(dependency);
    
    // Act
    var result = await service.MethodAsync();
    
    // Assert
    Assert.NotNull(result);
    await dependency.Received().MethodAsync();
}
```

### Mocking với NSubstitute
```csharp
// Setup mock
var mock = Substitute.For<IService>();
mock.MethodAsync().Returns(expectedValue);

// Verify calls
await mock.Received().MethodAsync();
await mock.DidNotReceive().MethodAsync();

// Setup exceptions
mock.MethodAsync().ThrowsAsync(new Exception("Test"));
```

## Test Data

Sử dụng `TestDataBuilder` để tạo test data:

```csharp
// Tạo single objects
var command = TestDataBuilder.CreatePendingCommand();
var siteDb = TestDataBuilder.CreateSiteDb();

// Tạo collections
var commands = TestDataBuilder.CreatePendingCommands(5);
var siteDatabases = TestDataBuilder.CreateSiteDatabases(3);
```

## Coverage

Tests bao phủ các scenarios chính:
- ✅ Happy path scenarios
- ✅ Error handling
- ✅ Edge cases
- ✅ Different input combinations
- ✅ Async operations
- ✅ Exception propagation

## Dependencies

- **NSubstitute**: Mocking framework
- **XUnit**: Testing framework
- **FluentAssertions**: Enhanced assertions (optional)
- **Microsoft.NET.Test.Sdk**: Test SDK 
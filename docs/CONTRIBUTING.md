# Contribution Guidelines

## Branch Naming Conventions
- Use 'feature/*' for contributing new features
- Use 'bugfix/*' for contributing bug fixes
- Use 'hotfix/*' for contributing hot fixes
- Use 'documentation/*' for contributing documentation
- Use 'refactor/*' for refactorization contributions
- Use 'other/*' for contributing any other category

## General Guidelines
- Fork the repository and create a new branch for your changes.
- Make sure to open a pull request when your changes are ready for review.
- Follow [this template](./technical/templates/PULL_REQUEST_TEMPLATE.md) for your pull request.
- Write clear and descriptive commit messages in third person singular (e.g., "fixes xx").
- Keep your pull requests focused on a single issue or feature.
- Contributors are expected to add themselves to the list at the end of this file.

## Documentation Standards
- **Every** construct (class, interface, struct, record, method, property, etc.) should have documentation. For example:
```csharp
/// <summary>
/// Represents a user in the application.
/// </summary>
public class User
{
    /// <summary>
    /// Gets or sets the user's name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Updates the user's profile information.
    /// </summary>
    public void UpdateProfile()
    {
        // Method body
    }
}

/// <summary>
/// Query for getting a user identified by <param name="UserId">.
/// </summary>
/// <param name="UserId">The id of the user to get.</param>
public record GetUserQuery(int UserId);

/// <summary>
/// Enumeration for theme styles.
/// </summary>
public enum ThemeStyle
{
    /// <summary>
    /// Light variant of the theme.
    /// </summary>
    Light,

    /// <summary>
    /// Dark variant of the theme.
    /// </summary>
    Dark
}

/// <summary>
/// Defines the contract for user services.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Gets a user identified by <param name="userId">.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>The user associated with the specified identifier.</returns>
    User GetUserById(Guid userId);

    /// <summary>
    /// Updates the profile of the specified <param name="user">.
    /// </summary>
    /// <param name="user">The user object containing updated information.</param>
    void UpdateUserProfile(User user);
}

/// <summary>
/// Defines the contract for user services.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Checks if the specified user is an admin.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns><see langword="true"/> if the user is an admin; <see langword="false"/> otherwise.</returns>
    bool IsUserAdmin(Guid userId);
}
```

## Code style
### General
- Use 4 spaces for indentation.

### C# Specific
#### Code Conventions:
- Group `using`s with the following `#region`:
```csharp
#region ========================================================================= USING =====================================================================================
using System;
#endregion
```
- Enforce one type per file. For example:
```csharp
public class MyClass // Correct
{
    public void MyMethod()
    {
        // Method body
    }
}

public class MyClass
{
    public void MyMethod()
    {
        // Method body
    }

    public class NestedClass // Correct, when really needed
    {
        public void AnotherMethod()
        {
            // Method body
        }
    }
}

namespace MyNamespace // Not allowed
{
    public class MyClass
    {
        public void MyMethod()
        {
            // Method body
        }
    }

    public class MyOtherClass
    {
        public void MyMethod()
        {
            // Method body
        }
    }
}

namespace MyNamespace // Not allowed
{
    public record GetUserQuery(int UserId);

    public class GetUserQueryHandler
    {
        public User Handle(GetUserQuery query)
        {
            // Method body
        }
    }
}
```
- Avoid using primary constructors unless they will be re-designed to be assigned to readonly fields. For example:
```csharp
public class User(IUserRepository userRepository) // Not Allowed
{
    
}

public class User
{
    private readonly IUserRepository _userRepository;

    public User(IUserRepository userRepository) // Correct
    {
        _userRepository = userRepository
    }
}
```

#### Naming Conventions:
- Use PascalCase for class, struct, enum, enum member, interface, constructor, method and property names. For example:
```csharp
public class UserProfile // Correct
{
    public string FirstName { get; set; } // Correct

    public void UpdateProfile() // Correct
    {
        // method body
    }
}

public struct myStruct // Not Allowed
{
    public string first_name { get; set; } // Not Allowed

    public void myStruct() // Not Allowed
    {
        // constructor body
    }
}
```
- Use camelCase for local variables and method parameters. For example:
```csharp
public class UserProfile 
{
    public void GreetUser(string firstName) // Correct
    {
        string userGreeting = $"Hello, {firstName}!"; // Correct
    }
}

public class UserProfile 
{
    public void GreetUser(string _firstName) // Not Allowed
    {
        string user_greeting = $"Hello, {_firstName}!"; // Not Allowed
    }
}
```
- Prefix interface names with 'I'. For example:
```csharp
public interface IMyInterface // Correct
{
    void DoSomething();
}

public interface MyInterface // Not Allowed
{
    void DoSomething();
}
```
- Prefix static fields with 's_'.
```csharp
public class ExampleClass
{
    public static int s_myStaticField = 42; // Correct

    public static int myStaticField = 42; // Not Allowed
    public static int _myStaticField = 42; // Not Allowed
}
```
- Prefix readonly fields with an underscore, and use camelCase. For example:
```csharp
public class ExampleClass
{
    private readonly int _myReadonlyField = 100; // Correct

    private readonly int myReadonlyField = 100; // Not Allowed
    private readonly int _MyReadonlyField = 100; // Not Allowed
}
```
- Use ALL_CAPS for constants, separating words with underscores. For example:
```csharp
public class Constants
{
    public const int WATER_BOILING_TEMP = 100; // Correct

    public const int WaterBoilingTemp = 100; // Not Allowed
    public const int WATERBOILINGTEMP = 100; // Not Allowed
}
```

#### Variable Declarations:
- Do not use the `var` keyword. Always specify the type explicitly, unless for anonymous types. For example:
```csharp
MyClass myClassInstance = new(); // Correct
int result = myClassInstance.CalculateValue(); // Correct
List<string> names = []; // Correct
var anonymousType = new { Name = "Jane Doe" }; // Correct

var myClassInstance = new MyClass(); // Not Allowed
var result = myClassInstance.CalculateValue(); // Not Allowed
var names = new List<string>(); // Not Allowed
for (var i = 0; i < 10; i++) // Not Allowed
    Console.WriteLine(i);
```
- Use target-typed `new` for object instantiation when the type is already specified on the left-hand side. For example:
```csharp
Person person = new(); // Correct

Person person = new Person(); // Not Allowed
```

#### Braces and Code Blocks:
- Place opening braces on a new line. For example:
```csharp
// Correct
public class ExampleClass
{
    public void MyMethod()
    {
        // Method body
    }
}

// Not Allowed
public class ExampleClass {
    public void MyMethod() {
        // Method body
    }
}
```
- Do not use curly brackets for single conditional or iterative statements. For example:
```csharp
// Correct
if (condition)
    DoSomething();

// Not Allowed
if (condition)
{
    DoSomething();
}

// Correct
for (int i = 0; i < 10; i++)
    Console.WriteLine(i);

// Not Allowed
for (int i = 0; i < 10; i++)
{
    Console.WriteLine(i);
}
```
- Use curly brackets for nested if-else statements when needing to avoid ambiguity, even for single-line blocks. For example:
```csharp
// Incorrect and ambiguous
if (condition1)
    if (condition2)
        DoSomething();
else
    DoSomethingElse();

// Correct and clear
if (condition1)
{
    if (condition2)
        DoSomething();
}
else
    DoSomethingElse();
```

#### Expression-bodied Members:
- Use expression-bodied members only for simple readonly property getters. For example:
```csharp
public class ExampleClass
{
    public int Value => 42; // Correct
    public int Value { get; } // Correct
}

// Not Allowed
public class ExampleClass
{
    public int Value { get { return 42; } } // Not allowed
    public int GetValue() => 42; // Not allowed
    public int ReadOnlyValue { get; private set { return 42; } } // Not allowed
}
```
- Avoid expression-bodied members for methods, constructors, and other complex members. This does not include lambda expressions. For example:
```csharp
public class ExampleClass
{
    public void MyMethod() // Correct
    {
        // Method body
    }

    public int MyComplexMethod(int value) // Correct
    {
        return value * 2; // Method body
    }

    public void MyLambdaMethod() // Correct
    {
        Action myAction = () => Console.WriteLine("Hello, World!"); // Correct
    }
}

public class ExampleClass
{
    public void MyMethod() => Console.WriteLine("Hello, World!"); // Not allowed
    public int MyComplexMethod(int value) => value * 2; // Not allowed
    public int MyOtherComplexMethod(int value) 
        => value / 2; // Not allowed
}
```

#### Null Checking:
- Use pattern matching for null checks. For example:
```csharp
public void ProcessValue(object? value)
{
    if (value is null) // Correct
    {
        // Handle null case
    }
}

public Expression<Func<MyClass, bool>> CreateExpression()
{
    return x => x.Property != null; // Correct for expression trees (pattern matching not supported)
}

public void ProcessValue(object? value)
{
    if (value == null) // Not Allowed
    {
        // Handle null case
    }
}
```

#### Language Features:
- Use file-scoped namespaces. For example:
```csharp
namespace MyNamespace; // Correct

public class MyClass
{
    public void MyMethod()
    {
        // Method body
    }
}

namespace MyNamespace // Not Allowed
{
    public class MyClass
    {
        public void MyMethod()
        {
            // Method body
        }
    }
}
```

#### Formatting:
- Use a single space after keywords in control flow statements. For example:
```csharp
if (condition) // Correct
{
    // Do something
}

for (int i = 0; i < 10; i++) // Correct
{
    // Iterate
}

if(condition)// Not Allowed
{
    // Do something
}

for(int i = 0; i < 10; i++)// Not Allowed
{
    // Iterate
}
```
- Do not use spaces inside parentheses or square brackets. For example:
```csharp
if (condition) // Correct
{
    var items = new List<int> { 1, 2, 3 }; // Correct
}

if ( condition ) // Not Allowed
{
    var items = new List<int> {1, 2, 3}; // Not Allowed
}
```
- Use a single space before and after binary operators. For example:
```csharp
int sum = a + b; // Correct
bool isActive = true && false; // Correct

int sum=a+b; // Not Allowed
bool isActive=true&&false; // Not Allowed
```

#### Other Preferences:
- Prefer `is null` over `object.ReferenceEquals` for `null` checks. For example:

```csharp
if (myObject is null) // Correct
{
    // Handle null case
}

if (object.ReferenceEquals(myObject, null)) // Not Allowed
{
    // Handle null case
}
```
- Use readonly fields *where possible*. For example:
```csharp
public class User
{
    public readonly string UserId; // Correct

    public User(string userId)
    {
        UserId = userId; // Initialized in constructor
    }
}

public class User
{
    public string UserId; // Not Allowed

    public User(string userId)
    {
        UserId = userId; // Can be changed later
    }
}
```

### Javascript specific
#### Braces and Code Blocks:
- Do not place opening braces on a new line. For example:
```csharp
// Correct
function myFunction() {
    // Function body
}

// Not Allowed
function myFunction()
{
    // Function body
}
```

## Code Review Process
### Requesting a Review
#### Ensure your code is ready for review:
- All tests are passing
- Code is properly formatted
- Necessary documentation is updated
#### Create a pull request (PR) with a clear title and description:
- Summarize the changes made
- Explain the reasoning behind the changes
- Link to any related issues
#### Assign reviewers:
- Choose at least two reviewers familiar with the affected area of code
- If your changes impact multiple areas, consider assigning additional reviewers

### Reviewing Code
#### When reviewing code, focus on:
- Functionality: Does the code work as intended?
- Readability: Is the code easy to understand?
- Consistence: Is the code following the template and style of the rest of the project?
- Maintainability: Will the code be easy to modify in the future?
- Performance: Are there any potential performance issues?
- Security: Are there any security vulnerabilities?
#### Provide constructive feedback:
- Be specific and clear in your comments
- Explain the reasoning behind your suggestions
- Use a respectful and collaborative tone

### Addressing Feedback
#### When addressing review feedback:
- Respond to all comments, even if just to acknowledge them
- Make requested changes promptly
- If you disagree with a suggestion, explain your reasoning politely
- Mark comments as resolved once addressed

### Merging
#### A pull request can be merged when:
- It has received approvals from at least two reviewers
- All requested changes have been addressed
- CI/CD checks are passing
- The author of the PR is responsible for merging once all conditions are met.

## Testing Requirements
All contributors are expected to write and maintain tests for their code, to ensure code quality and prevent regressions. Follow these guidelines for testing:

### General Testing Principles
- Aim for high code coverage, but do not make this the purpose of testing
- Tests should be independent and repeatable
- Test both happy paths and error scenarios
- Follow the Arrange-Act-Assert (AAA) pattern:
```csharp
[Fact]
public void Add_WhenCalledWithTwoNumbers_ShouldReturnTheirSum()
{
    // Arrange
    var calculator = new Calculator();
    
    // Act
    int result = calculator.Add(2, 3);
    
    // Assert
    result.Should().Be(5);
}
```
- Only the following libraries are permited (proposals for additional libraries will be reviewed first):
1. **[NSubstitute](https://github.com/nsubstitute/NSubstitute)**
2. **[xunit](https://github.com/xunit/xunit)**
3. **[AutoFixture.AutoNSubstitute](https://github.com/AutoFixture/AutoFixture/tree/master/Src/AutoNSubstitute)**
4. **[AutoFixture.Xunit2](https://github.com/AutoFixture/AutoFixture/tree/master/Src/AutoFixture.xUnit2)**
5. **[Bogus](https://github.com/bchavez/Bogus)**
6. **[Microsoft.EntityFrameworkCore.InMemory](https://github.com/dotnet/efcore/tree/main/src/EFCore.InMemory)**
7. **[Microsoft.Extensions.DependencyInjection](https://github.com/dotnet/runtime/tree/main/src/libraries/Microsoft.Extensions.DependencyInjection)**
8. **[EntityFrameworkCore.Testing.NSubstitute](https://github.com/rgvlee/EntityFrameworkCore.Testing)**

### Types of Tests
#### Unit Tests
- Write unit tests for all public methods and functions
- Use mocking frameworks to isolate units of code
#### Integration Tests
- Write integration tests for components that interact with external systems (databases, APIs, etc.)
- Use in-memory database for SQLite
#### End-to-End Tests
- Write end-to-end tests for important user journeys
- Keep these tests minimal due to their typically slower execution time

### Test Naming Conventions
#### Use descriptive names that explain the test's purpose:
`MethodToBeTested_WhenSomeCondition_ShouldSomeResult()`. Examples:
```csharp
public void CreateRepository_WhenCalled_ShouldReturnCorrectRepository()
public void GetAllAsync_WhenCalled_ShouldReturnAllBooks()
public void CreateRepository_WhenUnregisteredTypeRequested_ShouldThrowException()
```

### Test Organization
- Group related tests in test classes that mirror the structure of the code being tested, for each kind of test (unit, integration) project.
- Group fixtures in directories called `Fixture` for each class of tests
### Assertions
- Use specific assertions rather than general ones (e.g., Assert.Equal instead of Assert.True)
- Do not use fluent style of assertions, use default xUnit assertions

### Test Data
- Use meaningful test data that covers various scenarios. For parameterized tests, use a variety of inputs:
```csharp
[Theory]
[InlineData(1, 2, 3)]
[InlineData(-1, -2, -3)]
[InlineData(0, 0, 0)]
public void Add_WhenCalledWithVariousInputs_ShouldReturnExpectedSum(int a, int b, int expected)
{
    // Arrange
    var calculator = new Calculator();
    
    // Act & Assert
    calculator.Add(a, b).Should().Be(expected);
}
```

### Maintaining Tests
- Update tests when changing related code
- Regularly review and refactor tests to keep them maintainable
- Delete obsolete tests

### Performance Testing
- Write performance tests for critical paths in your application
- Set performance benchmarks and ensure they are met

### Security Testing
- Include tests that verify security measures (e.g., authentication, authorization)

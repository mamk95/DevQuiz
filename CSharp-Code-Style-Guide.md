# C# / ASP.NET Code Style Guide

Audience: developers and coding agents working on this codebase.
Scope: code style and formatting, not focusing on third‑party libraries.

## Files and layout

- One public type per file; file name matches the type.
- Program.cs should use top-level statements unless there are special circumstances requiring a class-based approach (e.g., complex initialization, multiple entry points, or specific framework requirements).
- File‑scoped namespaces; namespace at the top of the file; `using` directives immediately follow the namespace (no blank line after namespace).

  ```csharp
  // ✅ CORRECT:
  namespace MyProject.API.Controllers;
  using System.Text.Json;

  // ❌ INCORRECT (blank line after namespace):
  namespace MyProject.API.Controllers;

  using System.Text.Json;
  ```

- Group code by feature (e.g., `ExampleSlice`, `ExampleFeature`).
- Regions can be used sparingly to structure long methods (e.g., `#region Data fetching`).

## Formatting

- Indentation: 4 spaces; no tabs.
- Braces: Allman style (open on a new line for types and members).
- Blank lines: one between members; avoid trailing whitespace.
- Keep route/attribute lines on one line when readable; wrap parameters to the next line when they get long.
- Max line length: soft limit of 140.
- Trailing commas are required in multi‑line initializers and in multi‑line argument lists.
- Method parameter lists stay on one line; only break them when they become extremely long.
- Within types: separate public/protected properties with a blank line; private fields can be contiguous with no blank lines.

## Using directives

- For top-level programs (Program.cs): Place `using` directives at the top of the file before any statements.
- For regular files: Place `using` directives immediately after the file‑scoped namespace with no blank line between them.
- `System.*` first; the remaining usings are sorted alphabetically. Do not separate groups with blank lines.
- No unused `using`s.

## Naming

- PascalCase for types, methods, and public properties; camelCase for locals/parameters.
- Private fields use a leading underscore: `_camelCase`.
- DTOs end with `Dto`, controllers with `Controller`, request messages/handlers with `Command`/`CommandHandler`.
- Async methods are suffixed `Async` in libraries/services; controller action names aren't suffixed.
- Names should be descriptive and pronounceable; avoid abbreviations and cryptic short forms.
- Favor intent‑revealing names; don't shorten common words (`customer`, not `cust`).
- Include units in variable names when not obvious: `timeoutSeconds`, `sizeBytes`, `angleRadians`.
- For domain‑specific constants, use descriptive names: `EarthRadiusMeters` instead of `R`.
- Single‑letter variables are only acceptable in narrow scopes (loop indices, lambda parameters).

### Async Method Naming Exceptions

- Don't use `Async` suffix when implementing interfaces that don't use it
- Don't use `Async` suffix in controller actions (ASP.NET convention)
- Don't use `Async` suffix for event handlers
- Example:

  ```csharp
  // Interface without Async suffix
  public interface IDataProvider
  {
      Task<Data> GetData();
  }

  // Implementation matches interface (no Async suffix)
  public class DataProvider : IDataProvider
  {
      public async Task<Data> GetData() { ... }
  }

  // But standalone methods should use Async
  public async Task<Data> LoadDataAsync() { ... }
  ```

## Language usage

- Prefer `var` when the type is obvious; use explicit types when it significantly improves clarity.
- Use modern pattern‑matching, `is`, `or`, `and` where it reads well.
- Prefer `using var` declarations for disposables.
- Use records for request messages (CQRS) where appropriate.
- Prefer `required` members on DTOs; use `init` accessors for config/options classes.
- Nullable reference types: enabled; model nullability explicitly via `?`.
- **Never use the null-forgiving operator (`!`) without justification**:
  - Prefer defensive null checks over `!`
  - If `!` is absolutely necessary, add a comment explaining why it's safe
  - Consider alternatives: null-coalescing (`??`), conditional access (`?.`), or explicit null checks
- LINQ/fluent chains: dot-at-start style — chains beyond 3 calls must use new lines with dot-at-start; one call per new indented line; readability over micro‑optimizing. Multiple `Where` clauses are fine.

### Collection Expressions (C# 12+)

- Prefer collection expressions for simple initialization: `[1, 2, 3]` over `new[] { 1, 2, 3 }`
- Use traditional syntax when type clarity is important or for complex initialization
- Examples:

  ```csharp
  // Preferred for simple cases
  int[] numbers = [1, 2, 3];
  List<string> names = ["Alice", "Bob"];

  // Use traditional syntax when type needs emphasis
  var complexItems = new Dictionary<string, List<int>>
  {
      ["key1"] = [1, 2, 3],
      ["key2"] = [4, 5, 6],
  };
  ```

### Primary Constructors

- Use primary constructors for simple dependency injection in services and controllers
- Avoid for classes with complex initialization logic
- Don't use if you need to perform validation on constructor parameters
- Example:

  ```csharp
  // Good for simple DI
  public class UserService(IUserRepository repo, ILogger<UserService> logger)
  {
      public async Task<User> GetUserAsync(int id) => await repo.GetByIdAsync(id);
  }

  // Avoid for complex initialization
  public class ComplexService
  {
      private readonly IService _service;

      public ComplexService(IService service)
      {
          _service = service ?? throw new ArgumentNullException(nameof(service));
          // Additional setup logic
      }
  }
  ```

## Readability

- Prefer clear, conventional patterns over clever one‑liners. Optimize for how quickly a reader understands the code.
- Only trade clarity for performance in measured hot paths; include a short comment and a benchmark/reference if you do.
- Keep expressions simple; favor early returns over deep nesting; keep cyclomatic complexity low. Use guard clauses.

## Control Flow

### If Statements

- Simple one-line statements (return, throw, continue, break) don't require braces:
- If any block in an if/else chain has braces, all blocks must have braces:

## ASP.NET conventions

- Attribute routing with lowercase paths and constraints (e.g., `:guid`, `:int`).
- Controllers use primary constructors for DI where it keeps code succinct.
- Actions return `ActionResult` and declare `[ProducesResponseType]` for success and error statuses.
- For file responses, use `PhysicalFile`/`File` with explicit content types.
- Cancellation tokens are accepted as `CancellationToken ct` and passed through to I/O.

## Error handling and logging

- In actions, map known exceptions to proper HTTP results; otherwise log and rethrow.
- Use `ILogger<T>` with structured templates and named placeholders (no string interpolation).
- Log start/end of long operations where helpful.

## Strings, culture, and dates

- Use `DateOnly` for date‑only values.
- For comparisons, normalize using `ToLowerInvariant()` or specify an explicit comparer; prefer invariant culture for formatting persisted metadata (e.g., `"yyyy-MM-dd"`).
- When emitting metadata, specify formats and include UTC where appropriate (e.g., `yyyy-MM-ddTHH:mm:ssZ`).

## Comments and documentation

### When to write comments

- **Magic numbers and constants**: Always explain domain‑specific values (e.g., `6_371_000 // Earth radius in meters`).
- **Valid values**: Document constraints and allowed values.
- **Complex algorithms**: Explain the approach, especially multi‑step fallbacks (e.g., `// Priority 1: Use PQTMEPE data if available`).
- **Non‑obvious decisions**: Explain WHY, not what (e.g., `// Use conservative estimate for modern GNSS`).
- **Units and formats**: Clarify measurement units or data formats (e.g., `// meters`, `// UTC timestamp`).

### XML documentation

- Write XML docs for all public methods in libraries and APIs.
- Include meaningful descriptions for parameters that aren't self‑explanatory.
- Focus on purpose and usage, not implementation details.
- Use `<inheritdoc/>` on interface implementations unless adding specific details.
- Ensure `summary`, `param`, and `returns` contain meaningful text; remove empty elements.

### Variable naming instead of comments

- Prefer descriptive names over comments: `EarthRadiusMeters` instead of `R // Earth radius`.
- However, still add comments for domain knowledge that isn't obvious even with good names.

### Comment style

- Keep comments concise and to the point.
- Place comments on their own line for better readability when explaining complex logic.
- Prefer inline comments when intent is not obvious.
- Maintain comments as you maintain code — keep them current.
- Remove stale TODO/FIXME comments or link them to a work item.

## File Organization

### File Length Limits

- **Target**: Keep files under 300 lines when possible.
- **Warning**: Files over 500 lines should be considered for splitting.
- **Critical**: Files over 1000 lines must be split immediately.
- **Exceptions**:
  - Generated code (e.g., migrations, scaffolded code)
  - Configuration files with extensive setup
  - Test files with many test cases (though consider splitting by test category)
- Group related functionality together (e.g., validation methods near the main methods they validate).
- Split on natural boundaries: separate concerns, different abstraction levels, or distinct features.

### Method Complexity

- **Target**: Keep methods under 50 lines when practical.
- Extract helper methods for complex logic blocks.
- Use descriptive method names that explain the "what": `ValidateTimeSystemHeaders()` vs `CheckHeaders()`.
- Complex algorithms should be broken into logical steps with helper methods.

## Constants and Magic Numbers

### Domain-Specific Constants

Always explain domain-specific values with units:

```csharp
const double EarthRadius = 6371000; // meters
const double SpeedOfLight = 299792458; // meters per second
const double MaxBaselineKm = 50.0; // Maximum RTK baseline distance in kilometers
```

### Constant Organization

Group related constants in static classes for better organization:

```csharp
public static class PhysicalConstants
{
    public const double EarthRadiusMeters = 6371000;
    public const double SpeedOfLightMs = 299792458;
}

public static class GnssConstants
{
    public const double HorizontalPpm = 0.5; // RTK horizontal degradation rate
    public const double VerticalPpm = 1.0;   // RTK vertical degradation rate
}
```

## Trailing Commas Rules

### Multi-line Expressions

Trailing commas are required when the closing brace/bracket is on its own line:

```csharp
var config = new Configuration
{
    Timeout = 30,
    RetryCount = 3,
    EnableLogging = true, // Required trailing comma
};

var items = new[]
{
    "first",
    "second",
    "third", // Required trailing comma
};
```

### Single-line Expressions

Don't use trailing commas in single-line expressions:

```csharp
var numbers = new[] { 1, 2, 3 }; // No trailing comma
var config = new Config { Name = "test", Value = 42 }; // No trailing comma
```

## Test Organization

### Arrange-Act-Assert Pattern

Test methods should use the AAA pattern with clear comment markers on separate lines: Arrange, Act, Assert

### Test Method Naming

- Use descriptive names: `MethodName_Scenario_ExpectedBehavior`
- Alternative patterns: `Should_ExpectedBehavior_When_Scenario` for BDD-style
- Be specific about the scenario being tested
- Include the expected outcome in the name
- Examples:
  - `ParseNmea_WithInvalidChecksum_ThrowsException`
  - `Should_ReturnEmptyList_When_NoFilesFound`

### Test Attributes

- Use `[Fact]` for single test cases
- Use `[Theory]` with `[InlineData]` or `[MemberData]` for parameterized tests
- Consider `[Trait]` for categorizing tests (e.g., "Category", "Integration")

### Test Data Setup

- Keep test data close to the test method
- Use builder patterns for complex test objects
- Consider using fixture classes for expensive shared setup
- Use meaningful variable names for test data

### Test Comments

- Always include `// Arrange`, `// Act`, and `// Assert` comments on separate lines
- Additional explanatory comments should be concise and describe "why" not "what"
- Keep assertion messages brief and informative
- Remove verbose assertion messages unless they add specific debugging value

## Enum Documentation

### XMLDoc for Enums

Use XMLDoc summaries for both the enum and its values when they represent domain-specific concepts.

### Enum Best Practices

- Use XMLDoc `<summary>` tags for both enum and values - consistent with other public API documentation.
- Include units or accuracy ranges in XMLDoc summaries where relevant.
- Group related enum values together with spacing when appropriate.
- Consider using `[Flags]` attribute for combinable options.
- XMLDoc enables IntelliSense and API documentation generation, unlike inline comments.

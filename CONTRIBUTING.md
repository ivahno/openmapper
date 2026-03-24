# Contributing to OpenAutoMapper

Thank you for your interest in contributing to OpenAutoMapper! This guide will help you get started.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) or later
- [Git](https://git-scm.com/)
- A code editor (Visual Studio 2022, VS Code with C# Dev Kit, or JetBrains Rider recommended)

## Getting Started

1. **Fork** the repository on GitHub.
2. **Clone** your fork locally:
   ```bash
   git clone https://github.com/<your-username>/openmapper.git
   cd openmapper
   ```
3. **Create a feature branch** from `dev`:
   ```bash
   git checkout -b feature/your-feature-name dev
   ```

## Building

Build the entire solution in Release mode:

```bash
dotnet build -c Release
```

## Testing

Run all tests:

```bash
dotnet test
```

Run tests with coverage:

```bash
dotnet test --collect:"XPlat Code Coverage"
```

## Pull Request Process

1. **Create a feature branch** from `dev` (not `main`).
2. **Make your changes** following the coding standards below.
3. **Run all tests** and ensure they pass.
4. **Commit your changes** using the conventional commit format.
5. **Push your branch** to your fork.
6. **Submit a pull request** to the `dev` branch.

A maintainer will review your PR. Please be patient and responsive to feedback.

## Coding Standards

- Use **file-scoped namespaces** (`namespace Foo;` not `namespace Foo { }`).
- Use **`_camelCase`** for private fields.
- Use **`PascalCase`** for public members, types, and methods.
- Match the **AutoMapper public API** exactly where applicable (method names, parameter names, generic constraints).
- Keep source generator code **incremental** — avoid allocations in hot paths.
- Ensure all public APIs have **XML documentation comments**.
- All code must be **AOT-compatible** with no trim warnings.

## Commit Message Format

This project follows [Conventional Commits](https://www.conventionalcommits.org/):

```
<type>(<scope>): <short summary>

<optional body>

<optional footer>
```

**Types:**

| Type         | Description                                      |
|--------------|--------------------------------------------------|
| `feat`       | A new feature                                    |
| `fix`        | A bug fix                                        |
| `docs`       | Documentation only changes                       |
| `refactor`   | Code change that neither fixes a bug nor adds a feature |
| `test`       | Adding or updating tests                         |
| `chore`      | Build process or tooling changes                 |
| `perf`       | Performance improvement                          |

**Examples:**

```
feat(generator): add nested object mapping support
fix(core): resolve circular reference in profile registration
docs: update README with quick start guide
test(generator): add verify tests for collection mapping
```

## Branch Protection

- All changes to `main` require a pull request.
- CI must pass before merging.
- Linear history is enforced (rebase merging).

## Questions?

If you have questions about contributing, please open a [Discussion](https://github.com/ivahno/openmapper/discussions) on GitHub.

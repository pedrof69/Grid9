# Contributing to Grid9

Thank you for your interest in contributing to this project! This document provides guidelines for contributing.

## Code of Conduct

This project adheres to a code of conduct that we expect all participants to follow. Please be respectful and constructive in all interactions.

## Getting Started

1. Fork the repository
2. Clone your fork locally
3. Create a new branch for your feature or bug fix
4. Make your changes
5. Test your changes thoroughly
6. Submit a pull request

## Development Setup

### Prerequisites

- .NET 8.0 SDK or later
- Git
- Your favorite IDE (Visual Studio, VS Code, Rider, etc.)

### Building

```bash
git clone https://github.com/pedrof69/Grid9.git
cd Grid9
dotnet build
```

### Running Tests

```bash
dotnet test
```

### Running Demo

```bash
dotnet run --project demo
```

## Contribution Guidelines

### Code Style

- Follow standard C# coding conventions
- Use meaningful variable and method names
- Add XML documentation comments for public APIs
- Keep methods focused and concise
- Use `var` for local variables when the type is obvious

### Performance Considerations

This is a performance-critical library. When contributing:

- Profile your changes with BenchmarkDotNet
- Avoid allocations in hot paths
- Use `MethodImpl(MethodImplOptions.AggressiveInlining)` for small, frequently-called methods
- Consider using `Span<T>` and `stackalloc` for temporary arrays
- Validate that changes don't regress performance

### Testing

- Write unit tests for all new functionality
- Ensure test coverage remains high
- Include edge case testing (boundaries, invalid inputs)
- Test precision requirements rigorously
- Add performance benchmarks for core functionality changes

### Documentation

- Update README.md if adding new features
- Add XML documentation comments for all public APIs
- Include code examples in documentation
- Update API documentation if changing interfaces

## Types of Contributions

### Bug Fixes

- Create an issue describing the bug
- Include steps to reproduce
- Write a test that demonstrates the bug
- Fix the bug and ensure the test passes

### Features

- Discuss significant features in an issue first
- Keep features focused and atomic
- Maintain backward compatibility
- Update documentation and examples

### Performance Improvements

- Include benchmarks showing the improvement
- Ensure accuracy is not compromised
- Document any trade-offs made

### Documentation

- Fix typos and improve clarity
- Add examples and use cases
- Improve API documentation

## Mathematical Constraints

This library operates at the theoretical limits of coordinate compression. Any changes must respect:

- **Shannon's Information Theory**: Cannot compress below entropy limits
- **3-meter precision requirement**: Land accuracy must be maintained
- **9-character length constraint**: Fixed output length is required
- **Hybrid quantization**: Meter-based latitude + degree-based longitude

## Testing Requirements

All contributions must include appropriate tests:

### Unit Tests
- Test normal operation
- Test boundary conditions
- Test error conditions
- Test precision requirements

### Integration Tests
- Test real-world coordinate scenarios
- Test batch operations
- Test spatial locality properties

### Performance Tests
- Benchmark encoding/decoding rates
- Memory allocation testing
- Accuracy verification

## Pull Request Process

1. **Create Feature Branch**: Use descriptive branch names like `feature/add-geofencing` or `fix/precision-at-poles`

2. **Make Changes**: Follow coding guidelines and write tests

3. **Test Thoroughly**: Ensure all tests pass and performance is maintained

4. **Update Documentation**: Include relevant documentation updates

5. **Submit PR**: Use the pull request template and provide detailed description

6. **Code Review**: Address feedback and make requested changes

7. **Merge**: Maintainers will merge approved PRs

## Pull Request Template

```markdown
## Description
Brief description of changes

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Performance improvement
- [ ] Documentation update
- [ ] Breaking change

## Testing
- [ ] Unit tests added/updated
- [ ] Integration tests added/updated
- [ ] Performance benchmarks run
- [ ] Manual testing completed

## Checklist
- [ ] Code follows style guidelines
- [ ] Self-review completed
- [ ] Documentation updated
- [ ] Tests pass locally
- [ ] No performance regressions
```

## Performance Benchmarking

Use the built-in benchmarking:

```bash
dotnet run --project benchmarks -c Release
```

For detailed profiling, use:

```csharp
PerformanceBenchmark.RunBenchmark(1000000);
```

Expected performance baselines:
- Encoding: >6M operations/second
- Decoding: >7M operations/second
- 32 bytes per operation
- ~2.6m average error on land

## Mathematical Verification

When making changes that affect the core algorithm:

1. Verify information theory constraints are respected
2. Test precision at various latitudes
3. Validate hybrid quantization algorithm
4. Ensure land coverage is maintained

## Questions?

Feel free to open an issue for:
- Questions about the codebase
- Clarification on contribution guidelines
- Discussion of potential features
- Help with setup or development

## Recognition

Contributors will be recognized in:
- README.md contributors section
- Release notes for significant contributions
- Package metadata for major contributions

Thank you for helping make Grid9 the most efficient 9-character coordinate compression system!

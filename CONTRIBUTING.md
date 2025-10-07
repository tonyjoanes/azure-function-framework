# Contributing to Azure Function Framework

Thank you for your interest in contributing to the Azure Function Framework! 🎉

## 🚀 Getting Started

### Prerequisites
- .NET 8 SDK or later
- Git
- Your favorite IDE (VS Code, Visual Studio, Rider, etc.)
- Azure Functions Core Tools v4.x (for local testing)

### Development Setup

1. **Fork and Clone**
   ```bash
   git clone https://github.com/yourusername/azure-function-framework.git
   cd azure-function-framework
   ```

2. **Restore Dependencies**
   ```bash
   dotnet restore
   ```

3. **Build and Test**
   ```bash
   dotnet build
   dotnet test
   ```

## 🛠️ Development Workflow

### Branch Naming
- `feature/description` - New features
- `bugfix/description` - Bug fixes
- `docs/description` - Documentation updates
- `refactor/description` - Code refactoring

### Code Style
- Follow C# coding conventions
- Use meaningful variable and method names
- Add XML documentation for public APIs
- Write unit tests for new functionality

### Testing
- All new code must include unit tests
- Aim for >80% code coverage
- Test both success and failure scenarios
- Use descriptive test names

## 📝 Pull Request Process

### Before Submitting
1. **Run Tests**: `dotnet test`
2. **Check Build**: `dotnet build --configuration Release`
3. **Update Documentation**: Update README or docs if needed
4. **Check Code Style**: Ensure consistent formatting

### PR Template
```markdown
## Description
Brief description of changes

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Breaking change
- [ ] Documentation update

## Testing
- [ ] Unit tests pass
- [ ] Manual testing completed
- [ ] No breaking changes (or documented)

## Checklist
- [ ] Code follows style guidelines
- [ ] Self-review completed
- [ ] Documentation updated
- [ ] Tests added/updated
```

## 🏗️ Architecture Guidelines

### Adding New Trigger Types
1. Create base class in `src/AzureFunctionFramework/BaseClasses/`
2. Add corresponding attribute in `src/AzureFunctionFramework/Attributes/`
3. Update `FunctionDiscoveryService` to include new trigger type
4. Add comprehensive tests
5. Update documentation

### Configuration Management
- Use strongly-typed options classes
- Support environment-specific configuration
- Provide sensible defaults
- Validate configuration at startup

### Error Handling
- Use structured logging
- Provide meaningful error messages
- Implement proper exception handling
- Support dead lettering for message-based triggers

## 🧪 Testing Guidelines

### Unit Tests
- Test all public methods
- Mock external dependencies
- Test error scenarios
- Use descriptive test names

### Integration Tests
- Test with real Azure resources when possible
- Use test-specific configuration
- Clean up resources after tests

### Example Test Structure
```csharp
[Fact]
public async Task HandleMessage_ValidMessage_ProcessesSuccessfully()
{
    // Arrange
    var function = new TestFunction(_logger);
    var message = new TestMessage { Id = "123" };
    
    // Act
    await function.HandleMessage(message, _context);
    
    // Assert
    // Verify expected behavior
}
```

## 📚 Documentation

### Code Documentation
- Add XML documentation for all public APIs
- Include usage examples in documentation
- Document parameters and return values
- Add remarks for important behavior

### README Updates
- Update examples when adding new features
- Keep installation instructions current
- Document breaking changes
- Add troubleshooting section if needed

## 🐛 Bug Reports

### Before Reporting
1. Check existing issues
2. Try latest version
3. Reproduce the issue

### Bug Report Template
```markdown
**Describe the bug**
Clear description of the issue

**To Reproduce**
Steps to reproduce the behavior

**Expected behavior**
What you expected to happen

**Environment**
- .NET Version:
- Azure Functions Version:
- OS:
- Package Version:

**Additional context**
Any other relevant information
```

## 💡 Feature Requests

### Before Requesting
1. Check existing feature requests
2. Consider the framework's scope
3. Think about implementation complexity

### Feature Request Template
```markdown
**Is your feature request related to a problem?**
Description of the problem

**Describe the solution you'd like**
Clear description of the desired feature

**Describe alternatives you've considered**
Other approaches you've thought about

**Additional context**
Any other relevant information
```

## 📋 Release Process

### Version Numbering
- Follow Semantic Versioning (SemVer)
- `MAJOR.MINOR.PATCH`
- Update version in `AzureFunctionFramework.csproj`

### Release Checklist
- [ ] All tests pass
- [ ] Documentation updated
- [ ] Version bumped
- [ ] Release notes prepared
- [ ] NuGet package created
- [ ] GitHub release created

## 🎯 Project Goals

### Primary Goals
- Reduce boilerplate code for Azure Functions
- Simplify configuration management
- Provide excellent developer experience
- Maintain backward compatibility

### Scope
- Focus on Azure Functions (.NET 8+ isolated worker)
- Support common triggers (HTTP, Service Bus, Timer, Blob)
- Provide configuration abstractions
- Enable easy testing and mocking

## 📞 Getting Help

- **Issues**: Use GitHub Issues for bugs and feature requests
- **Discussions**: Use GitHub Discussions for questions
- **Documentation**: Check the README and docs folder

## 🙏 Recognition

Contributors will be recognized in:
- README.md contributors section
- Release notes
- GitHub contributors list

Thank you for contributing to make Azure Functions development easier for everyone! 🚀

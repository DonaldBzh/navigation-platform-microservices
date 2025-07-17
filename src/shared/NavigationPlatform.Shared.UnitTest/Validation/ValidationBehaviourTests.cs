using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using NavigationPlatform.Shared.Validation;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace NavigationPlatform.Shared.UnitTest.Validation;

public class ValidationBehaviorTests
{
    private readonly List<IValidator<TestRequest>> _validators;
    private readonly ValidationBehavior<TestRequest, TestResponse> _validationBehavior;
    private readonly RequestHandlerDelegate<TestResponse> _next;

    public ValidationBehaviorTests()
    {
        _validators = new List<IValidator<TestRequest>>();
        _validationBehavior = new ValidationBehavior<TestRequest, TestResponse>(_validators);
        _next = Substitute.For<RequestHandlerDelegate<TestResponse>>();
    }

    [Fact]
    public async Task Handle_WhenNoValidators_CallsNextAndReturnsResponse()
    {
        // Arrange
        var request = new TestRequest { Name = "Test" };
        var expectedResponse = new TestResponse { Success = true };
        var cancellationToken = CancellationToken.None;

        _next().Returns(expectedResponse);

        // Act
        var result = await _validationBehavior.Handle(request, _next, cancellationToken);

        // Assert
        result.Should().Be(expectedResponse);
        await _next.Received(1)();
    }

    [Fact]
    public async Task Handle_WhenValidatorsExistButNoErrors_CallsNextAndReturnsResponse()
    {
        // Arrange
        var request = new TestRequest { Name = "Valid Name" };
        var expectedResponse = new TestResponse { Success = true };
        var cancellationToken = CancellationToken.None;

        var validator = Substitute.For<IValidator<TestRequest>>();
        validator.Validate(Arg.Any<ValidationContext<TestRequest>>())
            .Returns(new ValidationResult()); // No errors

        _validators.Add(validator);
        var validationBehaviorWithValidators = new ValidationBehavior<TestRequest, TestResponse>(_validators);

        _next().Returns(expectedResponse);

        // Act
        var result = await validationBehaviorWithValidators.Handle(request, _next, cancellationToken);

        // Assert
        result.Should().Be(expectedResponse);
        await _next.Received(1)();
    }

    [Fact]
    public async Task Handle_WhenSingleValidatorHasErrors_ThrowsValidationException()
    {
        // Arrange
        var request = new TestRequest { Name = "" };
        var cancellationToken = CancellationToken.None;

        var validator = Substitute.For<IValidator<TestRequest>>();
        var validationFailures = new List<ValidationFailure>
            {
                new ValidationFailure("Name", "Name is required"),
                new ValidationFailure("Name", "Name must not be empty")
            };
        validator.Validate(Arg.Any<ValidationContext<TestRequest>>())
            .Returns(new ValidationResult(validationFailures));

        _validators.Add(validator);
        var validationBehaviorWithValidators = new ValidationBehavior<TestRequest, TestResponse>(_validators);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => validationBehaviorWithValidators.Handle(request, _next, cancellationToken));

        exception.Errors.Should().HaveCount(2);
        exception.Errors.Should().Contain(error => error.ErrorMessage == "Name is required");
        exception.Errors.Should().Contain(error => error.ErrorMessage == "Name must not be empty");

        await _next.DidNotReceive()();
    }

    [Fact]
    public async Task Handle_WhenMultipleValidatorsHaveErrors_AggregatesAllErrors()
    {
        // Arrange
        var request = new TestRequest { Name = "", Email = "invalid-email" };
        var cancellationToken = CancellationToken.None;

        var nameValidator = Substitute.For<IValidator<TestRequest>>();
        nameValidator.Validate(Arg.Any<ValidationContext<TestRequest>>())
            .Returns(new ValidationResult(new[]
            {
                    new ValidationFailure("Name", "Name is required")
            }));

        var emailValidator = Substitute.For<IValidator<TestRequest>>();
        emailValidator.Validate(Arg.Any<ValidationContext<TestRequest>>())
            .Returns(new ValidationResult(new[]
            {
                    new ValidationFailure("Email", "Email format is invalid"),
                    new ValidationFailure("Email", "Email domain is not allowed")
            }));

        _validators.Add(nameValidator);
        _validators.Add(emailValidator);
        var validationBehaviorWithValidators = new ValidationBehavior<TestRequest, TestResponse>(_validators);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => validationBehaviorWithValidators.Handle(request, _next, cancellationToken));

        exception.Errors.Should().HaveCount(3);
        exception.Errors.Should().Contain(error => error.ErrorMessage == "Name is required");
        exception.Errors.Should().Contain(error => error.ErrorMessage == "Email format is invalid");
        exception.Errors.Should().Contain(error => error.ErrorMessage == "Email domain is not allowed");

        await _next.DidNotReceive()();
    }

    [Fact]
    public async Task Handle_WhenSomeValidatorsPassAndSomeFail_ThrowsValidationExceptionWithOnlyErrors()
    {
        // Arrange
        var request = new TestRequest { Name = "Valid Name", Email = "invalid-email" };
        var cancellationToken = CancellationToken.None;

        var passingValidator = Substitute.For<IValidator<TestRequest>>();
        passingValidator.Validate(Arg.Any<ValidationContext<TestRequest>>())
            .Returns(new ValidationResult()); // No errors

        var failingValidator = Substitute.For<IValidator<TestRequest>>();
        failingValidator.Validate(Arg.Any<ValidationContext<TestRequest>>())
            .Returns(new ValidationResult(new[]
            {
                    new ValidationFailure("Email", "Email format is invalid")
            }));

        _validators.Add(passingValidator);
        _validators.Add(failingValidator);
        var validationBehaviorWithValidators = new ValidationBehavior<TestRequest, TestResponse>(_validators);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => validationBehaviorWithValidators.Handle(request, _next, cancellationToken));

        exception.Errors.Should().HaveCount(1);
        exception.Errors.Should().Contain(error => error.ErrorMessage == "Email format is invalid");

        await _next.DidNotReceive()();
    }

    [Fact]
    public async Task Handle_PassesCorrectValidationContextToValidators()
    {
        // Arrange
        var request = new TestRequest { Name = "Test" };
        var expectedResponse = new TestResponse { Success = true };
        var cancellationToken = CancellationToken.None;

        var validator = Substitute.For<IValidator<TestRequest>>();
        validator.Validate(Arg.Any<ValidationContext<TestRequest>>())
            .Returns(new ValidationResult());

        _validators.Add(validator);
        var validationBehaviorWithValidators = new ValidationBehavior<TestRequest, TestResponse>(_validators);

        _next().Returns(expectedResponse);

        // Act
        await validationBehaviorWithValidators.Handle(request, _next, cancellationToken);

        // Assert
        validator.Received(1).Validate(Arg.Is<ValidationContext<TestRequest>>(
            context => context.InstanceToValidate == request));
    }

    [Fact]
    public async Task Handle_CallsValidateOnAllValidators()
    {
        // Arrange
        var request = new TestRequest { Name = "Test" };
        var expectedResponse = new TestResponse { Success = true };
        var cancellationToken = CancellationToken.None;

        var validator1 = Substitute.For<IValidator<TestRequest>>();
        var validator2 = Substitute.For<IValidator<TestRequest>>();
        var validator3 = Substitute.For<IValidator<TestRequest>>();

        validator1.Validate(Arg.Any<ValidationContext<TestRequest>>()).Returns(new ValidationResult());
        validator2.Validate(Arg.Any<ValidationContext<TestRequest>>()).Returns(new ValidationResult());
        validator3.Validate(Arg.Any<ValidationContext<TestRequest>>()).Returns(new ValidationResult());

        _validators.Add(validator1);
        _validators.Add(validator2);
        _validators.Add(validator3);
        var validationBehaviorWithValidators = new ValidationBehavior<TestRequest, TestResponse>(_validators);

        _next().Returns(expectedResponse);

        // Act
        await validationBehaviorWithValidators.Handle(request, _next, cancellationToken);

        // Assert
        validator1.Received(1).Validate(Arg.Any<ValidationContext<TestRequest>>());
        validator2.Received(1).Validate(Arg.Any<ValidationContext<TestRequest>>());
        validator3.Received(1).Validate(Arg.Any<ValidationContext<TestRequest>>());
    }

    [Fact]
    public async Task Handle_WhenNextThrowsException_PropagatesException()
    {
        // Arrange
        var request = new TestRequest { Name = "Test" };
        var cancellationToken = CancellationToken.None;
        var expectedException = new InvalidOperationException("Handler failed");

        _next().Throws(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _validationBehavior.Handle(request, _next, cancellationToken));

        exception.Should().Be(expectedException);
    }

    [Fact]
    public async Task Handle_WithCancellationToken_PassesTokenToNext()
    {
        // Arrange
        var request = new TestRequest { Name = "Test" };
        var expectedResponse = new TestResponse { Success = true };
        var cancellationToken = new CancellationToken(true);

        _next().Returns(expectedResponse);

        // Act
        var result = await _validationBehavior.Handle(request, _next, cancellationToken);

        // Assert
        result.Should().Be(expectedResponse);
        await _next.Received(1)();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Handle_WithInvalidNames_ThrowsValidationException(string invalidName)
    {
        // Arrange
        var request = new TestRequest { Name = invalidName };
        var cancellationToken = CancellationToken.None;

        var validator = new TestRequestValidator();
        _validators.Add(validator);
        var validationBehaviorWithValidators = new ValidationBehavior<TestRequest, TestResponse>(_validators);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => validationBehaviorWithValidators.Handle(request, _next, cancellationToken));

        exception.Errors.Should().NotBeEmpty();
        await _next.DidNotReceive()();
    }

    [Fact]
    public async Task Handle_WithValidRequest_UsingRealValidator_CallsNext()
    {
        // Arrange
        var request = new TestRequest { Name = "Valid Name", Email = "test@example.com" };
        var expectedResponse = new TestResponse { Success = true };
        var cancellationToken = CancellationToken.None;

        var validator = new TestRequestValidator();
        _validators.Add(validator);
        var validationBehaviorWithValidators = new ValidationBehavior<TestRequest, TestResponse>(_validators);

        _next().Returns(expectedResponse);

        // Act
        var result = await validationBehaviorWithValidators.Handle(request, _next, cancellationToken);

        // Assert
        result.Should().Be(expectedResponse);
        await _next.Received(1)();
    }
}

// Test models
public class TestRequest
{
    public string Name { get; set; }
    public string Email { get; set; }
}

public class TestResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
}
// Real validator for integration-style tests
public class TestRequestValidator : AbstractValidator<TestRequest>
{
    public TestRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required")
            .MinimumLength(2)
            .WithMessage("Name must be at least 2 characters");

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage("Email format is invalid");
    }
}
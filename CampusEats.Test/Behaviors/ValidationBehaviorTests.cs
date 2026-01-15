using CampusEats.Api.Behaviors;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace CampusEats.Test.Behaviors;

public class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_NoValidators_CallsNext()
    {
        var behavior = new ValidationBehavior<TestRequest, string>(Enumerable.Empty<IValidator<TestRequest>>());
        var request = new TestRequest();
        var nextCalled = false;
        
        var result = await behavior.Handle(request, (ct) => 
        {
            nextCalled = true;
            return Task.FromResult("success");
        }, CancellationToken.None);

        result.Should().Be("success");
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ValidRequest_CallsNext()
    {
        var validator = new TestValidator(new ValidationResult());
        var behavior = new ValidationBehavior<TestRequest, string>(new[] { validator });
        var request = new TestRequest();
        var nextCalled = false;
        
        var result = await behavior.Handle(request, (ct) => 
        {
            nextCalled = true;
            return Task.FromResult("success");
        }, CancellationToken.None);

        result.Should().Be("success");
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_InvalidRequest_ThrowsValidationException()
    {
        var failures = new List<ValidationFailure>
        {
            new ValidationFailure("Property", "Error message")
        };
        var validator = new TestValidator(new ValidationResult(failures));
        var behavior = new ValidationBehavior<TestRequest, string>(new[] { validator });
        var request = new TestRequest();
        var nextCalled = false;

        var act = async () => await behavior.Handle(request, (ct) => 
        {
            nextCalled = true;
            return Task.FromResult("success");
        }, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        nextCalled.Should().BeFalse();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public async Task Handle_MultipleValidators_AllValidatorsExecuted(int validatorCount)
    {
        var validators = new List<TestValidator>();
        for (int i = 0; i < validatorCount; i++)
        {
            validators.Add(new TestValidator(new ValidationResult()));
        }
        
        var behavior = new ValidationBehavior<TestRequest, string>(validators);
        var request = new TestRequest();
        
        await behavior.Handle(request, (ct) => Task.FromResult("success"), CancellationToken.None);

        foreach (var validator in validators)
        {
            validator.WasCalled.Should().BeTrue();
        }
    }

    [Fact]
    public async Task Handle_MultipleValidatorsWithFailures_ThrowsValidationExceptionWithAllFailures()
    {
        var validator1 = new TestValidator(new ValidationResult(new[] { new ValidationFailure("Property1", "Error1") }));
        var validator2 = new TestValidator(new ValidationResult(new[] { new ValidationFailure("Property2", "Error2") }));
        
        var behavior = new ValidationBehavior<TestRequest, string>(new[] { validator1, validator2 });
        var request = new TestRequest();

        var act = async () => await behavior.Handle(request, (ct) => Task.FromResult("success"), CancellationToken.None);

        var exception = await act.Should().ThrowAsync<ValidationException>();
        exception.Which.Errors.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_ValidatorReturnsNull_IgnoresNullFailure()
    {
        var validationResult = new ValidationResult();
        validationResult.Errors.Add(null!);
        var validator = new TestValidator(validationResult);
        
        var behavior = new ValidationBehavior<TestRequest, string>(new[] { validator });
        var request = new TestRequest();
        
        var result = await behavior.Handle(request, (ct) => Task.FromResult("success"), CancellationToken.None);

        result.Should().Be("success");
    }

    [Fact]
    public async Task Handle_CancellationRequested_PassesCancellationToken()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel();
        
        var validator = new TestValidator(new ValidationResult());
        var behavior = new ValidationBehavior<TestRequest, string>(new[] { validator });
        var request = new TestRequest();

        await behavior.Handle(request, (ct) => Task.FromResult("success"), cts.Token);

        validator.CapturedToken.Should().Be(cts.Token);
    }

    private class TestRequest : IRequest<string>
    {
    }

    private class TestValidator : AbstractValidator<TestRequest>
    {
        private readonly ValidationResult _result;
        public bool WasCalled { get; private set; }
        public CancellationToken CapturedToken { get; private set; }

        public TestValidator(ValidationResult result)
        {
            _result = result;
        }

        public override Task<ValidationResult> ValidateAsync(ValidationContext<TestRequest> context, CancellationToken cancellation = default)
        {
            WasCalled = true;
            CapturedToken = cancellation;
            return Task.FromResult(_result);
        }
    }
}

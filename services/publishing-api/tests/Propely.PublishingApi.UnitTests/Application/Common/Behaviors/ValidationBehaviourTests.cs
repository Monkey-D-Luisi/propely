// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Propely.PublishingApi.Application.Common.Behaviors;
using ValidationException = Propely.PublishingApi.Domain.Common.Exceptions.ValidationException;

namespace Propely.PublishingApi.UnitTests.Application.Common.Behaviors;

public record TestCommand(string Name) : IRequest<string>;

public sealed class PassingValidator : AbstractValidator<TestCommand>
{
    // No rules = always passes
}

public sealed class FailingValidator : AbstractValidator<TestCommand>
{
    public FailingValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
    }
}

public sealed class MultiFieldFailingValidator : AbstractValidator<TestCommand>
{
    public MultiFieldFailingValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
        RuleFor(x => x.Name).MinimumLength(3).WithMessage("Name must be at least 3 characters.");
    }
}

public sealed class ValidationBehaviourTests
{
    [Fact]
    public async Task Handle_WhenNoValidators_ShouldCallNext()
    {
        var validators = Enumerable.Empty<IValidator<TestCommand>>();
        var behaviour = new ValidationBehaviour<TestCommand, string>(validators);
        var request = new TestCommand("test");

        var result = await behaviour.Handle(
            request,
            () => Task.FromResult("success"),
            CancellationToken.None);

        result.Should().Be("success");
    }

    [Fact]
    public async Task Handle_WhenValidatorsPass_ShouldCallNext()
    {
        var validator = new PassingValidator();
        var behaviour = new ValidationBehaviour<TestCommand, string>(new[] { validator });
        var request = new TestCommand("test");

        var result = await behaviour.Handle(
            request,
            () => Task.FromResult("success"),
            CancellationToken.None);

        result.Should().Be("success");
    }

    [Fact]
    public async Task Handle_WhenValidatorsFail_ShouldThrowValidationException()
    {
        var validator = new FailingValidator();
        var behaviour = new ValidationBehaviour<TestCommand, string>(new[] { validator });
        var request = new TestCommand("");

        var act = () => behaviour.Handle(
            request,
            () => Task.FromResult("should not reach"),
            CancellationToken.None);

        var exception = await act.Should().ThrowAsync<ValidationException>();
        exception.Which.Errors.Should().ContainKey("Name");
        exception.Which.Errors["Name"].Should().Contain("Name is required.");
    }

    [Fact]
    public async Task Handle_WithMultipleFailures_ShouldGroupByProperty()
    {
        var validator = new MultiFieldFailingValidator();
        var behaviour = new ValidationBehaviour<TestCommand, string>(new[] { validator });
        var request = new TestCommand("");

        var act = () => behaviour.Handle(
            request,
            () => Task.FromResult("should not reach"),
            CancellationToken.None);

        var exception = await act.Should().ThrowAsync<ValidationException>();
        exception.Which.Errors.Should().ContainKey("Name");
        exception.Which.Errors["Name"].Should().HaveCountGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task Handle_WhenValidatorsFail_ShouldNotCallNext()
    {
        var validator = new FailingValidator();
        var behaviour = new ValidationBehaviour<TestCommand, string>(new[] { validator });
        var nextCalled = false;

        var act = () => behaviour.Handle(
            new TestCommand(""),
            () =>
            {
                nextCalled = true;
                return Task.FromResult("result");
            },
            CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        nextCalled.Should().BeFalse();
    }
}

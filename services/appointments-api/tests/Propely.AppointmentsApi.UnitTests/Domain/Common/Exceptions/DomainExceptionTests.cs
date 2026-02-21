// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Propely.AppointmentsApi.Domain.Common.Exceptions;

namespace Propely.AppointmentsApi.UnitTests.Domain.Common.Exceptions;

public sealed class DomainExceptionTests
{
    [Fact]
    public void DomainException_WithMessage_ShouldSetMessage()
    {
        var ex = new DomainException("test error");
        ex.Message.Should().Be("test error");
    }

    [Fact]
    public void DomainException_WithMessageAndInner_ShouldSetBoth()
    {
        var inner = new InvalidOperationException("inner");
        var ex = new DomainException("outer", inner);
        ex.Message.Should().Be("outer");
        ex.InnerException.Should().Be(inner);
    }

    [Fact]
    public void DomainException_Parameterless_ShouldCreate()
    {
        var ex = new DomainException();
        ex.Should().BeOfType<DomainException>();
    }

    [Fact]
    public void ConflictException_ShouldExtendDomainException()
    {
        var ex = new ConflictException("conflict");
        ex.Should().BeAssignableTo<DomainException>();
        ex.Message.Should().Be("conflict");
    }

    [Fact]
    public void ConflictException_WithInnerException_ShouldSetBoth()
    {
        var inner = new Exception("inner");
        var ex = new ConflictException("msg", inner);
        ex.InnerException.Should().Be(inner);
    }

    [Fact]
    public void ConflictException_Parameterless_ShouldCreate()
    {
        var ex = new ConflictException();
        ex.Should().BeAssignableTo<DomainException>();
    }

    [Fact]
    public void ForbiddenException_ShouldExtendDomainException()
    {
        var ex = new ForbiddenException("forbidden");
        ex.Should().BeAssignableTo<DomainException>();
        ex.Message.Should().Be("forbidden");
    }

    [Fact]
    public void ForbiddenException_WithInnerException_ShouldSetBoth()
    {
        var inner = new Exception("inner");
        var ex = new ForbiddenException("msg", inner);
        ex.InnerException.Should().Be(inner);
    }

    [Fact]
    public void ForbiddenException_Parameterless_ShouldCreate()
    {
        var ex = new ForbiddenException();
        ex.Should().BeAssignableTo<DomainException>();
    }

    [Fact]
    public void NotFoundException_ShouldExtendDomainException()
    {
        var ex = new NotFoundException("not found");
        ex.Should().BeAssignableTo<DomainException>();
        ex.Message.Should().Be("not found");
    }

    [Fact]
    public void NotFoundException_WithInnerException_ShouldSetBoth()
    {
        var inner = new Exception("inner");
        var ex = new NotFoundException("msg", inner);
        ex.InnerException.Should().Be(inner);
    }

    [Fact]
    public void NotFoundException_Parameterless_ShouldCreate()
    {
        var ex = new NotFoundException();
        ex.Should().BeAssignableTo<DomainException>();
    }

    [Fact]
    public void TenantMismatchException_ShouldExtendForbiddenException()
    {
        var ex = new TenantMismatchException("mismatch");
        ex.Should().BeAssignableTo<ForbiddenException>();
        ex.Should().BeAssignableTo<DomainException>();
        ex.Message.Should().Be("mismatch");
    }

    [Fact]
    public void TenantMismatchException_WithInnerException_ShouldSetBoth()
    {
        var inner = new Exception("inner");
        var ex = new TenantMismatchException("msg", inner);
        ex.InnerException.Should().Be(inner);
    }

    [Fact]
    public void TenantMismatchException_Parameterless_ShouldCreate()
    {
        var ex = new TenantMismatchException();
        ex.Should().BeAssignableTo<ForbiddenException>();
    }

    [Fact]
    public void ValidationException_ShouldSetErrorsAndMessage()
    {
        var errors = new Dictionary<string, string[]>
        {
            ["Name"] = ["Name is required."]
        }.AsReadOnly();

        var ex = new ValidationException(errors);

        ex.Should().BeAssignableTo<DomainException>();
        ex.Message.Should().Be("One or more validation failures have occurred.");
        ex.Errors.Should().ContainKey("Name");
        ex.Errors["Name"].Should().Contain("Name is required.");
    }
}

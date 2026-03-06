// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Propely.ContactsApi.Application;
using Propely.ContactsApi.Application.Contacts.Commands.CreateContact;

namespace Propely.ContactsApi.UnitTests.Application;

public class DependencyInjectionTests
{
    [Fact]
    public void AddApplicationServices_RegistersMediatRAndValidators()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddApplicationServices();

        var provider = services.BuildServiceProvider();

        var mediator = provider.GetService<IMediator>();
        mediator.Should().NotBeNull();

        var validator = provider.GetService<CreateContactCommandValidator>();
        validator.Should().NotBeNull();
    }
}

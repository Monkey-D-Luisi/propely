// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Propely.AiApi.Application.Actions.Commands.ContactActions;
using Propely.AiApi.Application.Actions.Handlers.ContactActions;
using Propely.AiApi.Application.Actions.Parameters;
using Propely.AiApi.Domain.Actions;
using Propely.ContactsApi.Client;
using Propely.ContactsApi.Client.Models;

namespace Propely.AiApi.UnitTests.Application.Actions.Handlers.ContactActions;

public sealed class CreateContactActionHandlerTests
{
    private readonly IContactsApiClient _contactsClient;
    private readonly CreateContactActionHandler _handler;
    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid AgentId = Guid.NewGuid();

    public CreateContactActionHandlerTests()
    {
        _contactsClient = Substitute.For<IContactsApiClient>();
        var logger = Substitute.For<ILogger<CreateContactActionHandler>>();
        _handler = new CreateContactActionHandler(_contactsClient, logger);
    }

    [Fact]
    public async Task Handle_WithValidParameters_ShouldCreateContactViaSDK()
    {
        // Arrange
        var parameters = new CreateContactParameters(
            FirstName: "Juan",
            LastName: "Lopez",
            Email: "juan@example.com",
            Phone: "650123456",
            Role: "Buyer",
            Company: null,
            Notes: null,
            Source: null);
        var command = new CreateContactActionCommand(parameters, TenantId, AgentId);

        var createdContact = new ContactResponse
        {
            Id = Guid.NewGuid(),
            FirstName = "Juan",
            LastName = "Lopez",
            Email = "juan@example.com",
            Roles = ["Buyer"]
        };

        _contactsClient.CreateContactAsync(Arg.Any<CreateContactClientRequest>(), Arg.Any<CancellationToken>())
            .Returns(createdContact);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.ActionType.Should().Be(ActionType.CreateContact);
        result.Message.Should().Contain("Juan Lopez");
        result.Message.Should().Contain("Buyer");
        await _contactsClient.Received(1).CreateContactAsync(
            Arg.Is<CreateContactClientRequest>(r =>
                r.FirstName == "Juan" &&
                r.LastName == "Lopez" &&
                r.Email == "juan@example.com" &&
                r.Phone == "650123456"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithMissingFirstName_ShouldReturnFailure()
    {
        var parameters = new CreateContactParameters(
            FirstName: null,
            LastName: "Lopez",
            Email: "test@example.com",
            Phone: null,
            Role: null,
            Company: null,
            Notes: null,
            Source: null);
        var command = new CreateContactActionCommand(parameters, TenantId, AgentId);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Errors.Should().Contain("Contact first name is required.");
    }

    [Fact]
    public async Task Handle_WithMissingLastName_ShouldReturnFailure()
    {
        var parameters = new CreateContactParameters(
            FirstName: "Juan",
            LastName: null,
            Email: "test@example.com",
            Phone: null,
            Role: null,
            Company: null,
            Notes: null,
            Source: null);
        var command = new CreateContactActionCommand(parameters, TenantId, AgentId);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Errors.Should().Contain("Contact last name is required.");
    }

    [Fact]
    public async Task Handle_WithMissingEmail_ShouldReturnFailure()
    {
        var parameters = new CreateContactParameters(
            FirstName: "Juan",
            LastName: "Lopez",
            Email: null,
            Phone: null,
            Role: null,
            Company: null,
            Notes: null,
            Source: null);
        var command = new CreateContactActionCommand(parameters, TenantId, AgentId);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Errors.Should().Contain("Contact email is required.");
    }

    [Fact]
    public async Task Handle_WithNoRole_ShouldDefaultToBuyer()
    {
        var parameters = new CreateContactParameters(
            FirstName: "Juan",
            LastName: "Lopez",
            Email: "juan@example.com",
            Phone: null,
            Role: null,
            Company: null,
            Notes: null,
            Source: null);
        var command = new CreateContactActionCommand(parameters, TenantId, AgentId);

        _contactsClient.CreateContactAsync(Arg.Any<CreateContactClientRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ContactResponse { Id = Guid.NewGuid(), FirstName = "Juan", LastName = "Lopez", Email = "juan@example.com", Roles = ["Buyer"] });

        await _handler.Handle(command, CancellationToken.None);

        await _contactsClient.Received(1).CreateContactAsync(
            Arg.Is<CreateContactClientRequest>(r => r.Roles.Contains("Buyer")),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("comprador", "Buyer")]
    [InlineData("vendedor", "Seller")]
    [InlineData("inquilino", "Tenant")]
    [InlineData("propietario", "Landlord")]
    public async Task Handle_WithSpanishRole_ShouldNormalizeToEnglish(string spanishRole, string expectedRole)
    {
        var parameters = new CreateContactParameters(
            FirstName: "Juan",
            LastName: "Lopez",
            Email: "juan@example.com",
            Phone: null,
            Role: spanishRole,
            Company: null,
            Notes: null,
            Source: null);
        var command = new CreateContactActionCommand(parameters, TenantId, AgentId);

        _contactsClient.CreateContactAsync(Arg.Any<CreateContactClientRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ContactResponse { Id = Guid.NewGuid(), FirstName = "Juan", LastName = "Lopez", Email = "juan@example.com", Roles = [expectedRole] });

        await _handler.Handle(command, CancellationToken.None);

        await _contactsClient.Received(1).CreateContactAsync(
            Arg.Is<CreateContactClientRequest>(r => r.Roles.Contains(expectedRole)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSdkThrows_ShouldReturnFailureGracefully()
    {
        var parameters = new CreateContactParameters(
            FirstName: "Juan",
            LastName: "Lopez",
            Email: "juan@example.com",
            Phone: null,
            Role: null,
            Company: null,
            Notes: null,
            Source: null);
        var command = new CreateContactActionCommand(parameters, TenantId, AgentId);

        _contactsClient.CreateContactAsync(Arg.Any<CreateContactClientRequest>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection refused"));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }
}

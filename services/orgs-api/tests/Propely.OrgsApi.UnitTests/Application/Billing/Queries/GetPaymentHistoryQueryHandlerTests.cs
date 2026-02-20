// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.Billing.Interfaces;
using Propely.OrgsApi.Application.Billing.Queries.GetPaymentHistory;
using Propely.OrgsApi.Application.Common.Models;
using Propely.OrgsApi.Application.Organizations.Interfaces;
using Propely.OrgsApi.Domain.Billing;
using Propely.OrgsApi.Domain.Common.Exceptions;
using Propely.OrgsApi.Domain.Organizations;
using FluentAssertions;
using NSubstitute;

namespace Propely.OrgsApi.UnitTests.Application.Billing.Queries;

public sealed class GetPaymentHistoryQueryHandlerTests
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IMembershipRepository _membershipRepository;
    private readonly GetPaymentHistoryQueryHandler _handler;

    public GetPaymentHistoryQueryHandlerTests()
    {
        _paymentRepository = Substitute.For<IPaymentRepository>();
        _membershipRepository = Substitute.For<IMembershipRepository>();
        _handler = new GetPaymentHistoryQueryHandler(_paymentRepository, _membershipRepository);
    }

    [Fact]
    public async Task Handle_WithValidMember_ShouldReturnPagedPayments()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var membership = Membership.Create(userId, orgId, MembershipRole.Member);

        _membershipRepository.GetAsync(orgId, userId, Arg.Any<CancellationToken>()).Returns(membership);

        var payment1 = Payment.Create(orgId, "pi_1", 5000, "usd", "Add-on A", PaymentStatus.Succeeded);
        var payment2 = Payment.Create(orgId, "pi_2", 3000, "usd", "Add-on B", PaymentStatus.Pending);

        _paymentRepository.GetByOrgIdPagedAsync(orgId, 1, 20, Arg.Any<CancellationToken>())
            .Returns(new PagedResult<Payment>(new List<Payment> { payment1, payment2 }, 2, 1, 20));

        var query = new GetPaymentHistoryQuery(orgId, userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.PageNumber.Should().Be(1);
        result.Items.Should().Contain(p => p.Amount == 5000 && p.Currency == "usd" && p.Status == "succeeded");
        result.Items.Should().Contain(p => p.Amount == 3000 && p.Currency == "usd" && p.Status == "pending");
    }

    [Fact]
    public async Task Handle_WhenNotMember_ShouldThrowForbiddenException()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _membershipRepository.GetAsync(orgId, userId, Arg.Any<CancellationToken>()).Returns((Membership?)null);

        var query = new GetPaymentHistoryQuery(orgId, userId);

        // Act
        var act = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_WithNoPayments_ShouldReturnEmptyPagedResult()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var membership = Membership.Create(userId, orgId, MembershipRole.Owner);

        _membershipRepository.GetAsync(orgId, userId, Arg.Any<CancellationToken>()).Returns(membership);
        _paymentRepository.GetByOrgIdPagedAsync(orgId, 1, 20, Arg.Any<CancellationToken>())
            .Returns(new PagedResult<Payment>(new List<Payment>(), 0, 1, 20));

        var query = new GetPaymentHistoryQuery(orgId, userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WithCustomPageSize_ShouldPassPaginationParams()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var membership = Membership.Create(userId, orgId, MembershipRole.Member);

        _membershipRepository.GetAsync(orgId, userId, Arg.Any<CancellationToken>()).Returns(membership);

        var payment = Payment.Create(orgId, "pi_1", 5000, "usd", "Add-on A", PaymentStatus.Succeeded);

        _paymentRepository.GetByOrgIdPagedAsync(orgId, 2, 10, Arg.Any<CancellationToken>())
            .Returns(new PagedResult<Payment>(new List<Payment> { payment }, 15, 2, 10));

        var query = new GetPaymentHistoryQuery(orgId, userId, Page: 2, PageSize: 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(15);
        result.PageNumber.Should().Be(2);
        result.TotalPages.Should().Be(2);
        result.HasPreviousPage.Should().BeTrue();
        result.HasNextPage.Should().BeFalse();
    }
}

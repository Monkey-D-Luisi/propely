// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Propely.AppointmentsApi.Domain.Appointments;
using Propely.AppointmentsApi.Domain.Common.Exceptions;

namespace Propely.AppointmentsApi.UnitTests.Domain.Appointments;

public class SyncOperationTests
{
    private static readonly Guid ValidConnectionId = Guid.NewGuid();
    private static readonly Guid ValidAppointmentId = Guid.NewGuid();

    private static SyncOperation CreateValidOperation(
        Guid? calendarConnectionId = null,
        Guid? appointmentId = null,
        SyncDirection direction = SyncDirection.Outbound,
        SyncOperationType operationType = SyncOperationType.Create)
    {
        return SyncOperation.Create(
            calendarConnectionId ?? ValidConnectionId,
            appointmentId ?? ValidAppointmentId,
            direction,
            operationType);
    }

    // --- Create tests ---

    [Fact]
    public void Create_WithValidData_ReturnsPendingOperation()
    {
        var operation = CreateValidOperation();

        operation.Should().NotBeNull();
        operation.Id.Should().NotBeEmpty();
        operation.CalendarConnectionId.Should().Be(ValidConnectionId);
        operation.AppointmentId.Should().Be(ValidAppointmentId);
        operation.Direction.Should().Be(SyncDirection.Outbound);
        operation.OperationType.Should().Be(SyncOperationType.Create);
        operation.Status.Should().Be(SyncOperationStatus.Pending);
        operation.RetryCount.Should().Be(0);
        operation.ErrorMessage.Should().BeNull();
        operation.StartedAtUtc.Should().BeNull();
        operation.CompletedAtUtc.Should().BeNull();
        operation.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_WithNullAppointmentId_Succeeds()
    {
        var operation = SyncOperation.Create(
            ValidConnectionId, null, SyncDirection.Inbound, SyncOperationType.Create);

        operation.AppointmentId.Should().BeNull();
    }

    [Fact]
    public void Create_WithEmptyConnectionId_ThrowsDomainException()
    {
        var act = () => SyncOperation.Create(
            Guid.Empty, ValidAppointmentId, SyncDirection.Outbound, SyncOperationType.Create);

        act.Should().Throw<DomainException>()
            .WithMessage("*Calendar connection ID*required*");
    }

    [Fact]
    public void Create_WithInboundDirection_Succeeds()
    {
        var operation = CreateValidOperation(direction: SyncDirection.Inbound);

        operation.Direction.Should().Be(SyncDirection.Inbound);
    }

    [Theory]
    [InlineData(SyncOperationType.Create)]
    [InlineData(SyncOperationType.Update)]
    [InlineData(SyncOperationType.Delete)]
    public void Create_WithAllOperationTypes_Succeeds(SyncOperationType operationType)
    {
        var operation = CreateValidOperation(operationType: operationType);

        operation.OperationType.Should().Be(operationType);
    }

    // --- Start tests ---

    [Fact]
    public void Start_FromPending_SetsStatusToInProgress()
    {
        var operation = CreateValidOperation();

        operation.Start();

        operation.Status.Should().Be(SyncOperationStatus.InProgress);
        operation.StartedAtUtc.Should().NotBeNull();
        operation.StartedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Start_FromInProgress_ThrowsDomainException()
    {
        var operation = CreateValidOperation();
        operation.Start();

        var act = () => operation.Start();

        act.Should().Throw<DomainException>()
            .WithMessage("*Cannot start*InProgress*");
    }

    [Fact]
    public void Start_FromCompleted_ThrowsDomainException()
    {
        var operation = CreateValidOperation();
        operation.Start();
        operation.Complete();

        var act = () => operation.Start();

        act.Should().Throw<DomainException>()
            .WithMessage("*Cannot start*Completed*");
    }

    // --- Complete tests ---

    [Fact]
    public void Complete_FromInProgress_SetsStatusToCompleted()
    {
        var operation = CreateValidOperation();
        operation.Start();

        operation.Complete();

        operation.Status.Should().Be(SyncOperationStatus.Completed);
        operation.CompletedAtUtc.Should().NotBeNull();
        operation.CompletedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Complete_FromPending_ThrowsDomainException()
    {
        var operation = CreateValidOperation();

        var act = () => operation.Complete();

        act.Should().Throw<DomainException>()
            .WithMessage("*Cannot complete*Pending*");
    }

    // --- Fail tests ---

    [Fact]
    public void Fail_SetsStatusToFailedWithErrorMessage()
    {
        var operation = CreateValidOperation();
        operation.Start();

        operation.Fail("Network timeout");

        operation.Status.Should().Be(SyncOperationStatus.Failed);
        operation.ErrorMessage.Should().Be("Network timeout");
        operation.CompletedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public void Fail_TruncatesLongErrorMessage()
    {
        var operation = CreateValidOperation();
        operation.Start();
        var longError = new string('A', SyncOperation.ErrorMessageMaxLength + 100);

        operation.Fail(longError);

        operation.ErrorMessage.Should().HaveLength(SyncOperation.ErrorMessageMaxLength);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Fail_WithEmptyMessage_ThrowsDomainException(string? error)
    {
        var operation = CreateValidOperation();
        operation.Start();

        var act = () => operation.Fail(error!);

        act.Should().Throw<DomainException>()
            .WithMessage("*Error message*required*");
    }

    // --- IncrementRetry tests ---

    [Fact]
    public void IncrementRetry_IncrementsCountAndResetsToPending()
    {
        var operation = CreateValidOperation();
        operation.Start();
        operation.Fail("Error");

        operation.IncrementRetry();

        operation.RetryCount.Should().Be(1);
        operation.Status.Should().Be(SyncOperationStatus.Pending);
        operation.ErrorMessage.Should().BeNull();
        operation.StartedAtUtc.Should().BeNull();
        operation.CompletedAtUtc.Should().BeNull();
    }

    [Fact]
    public void IncrementRetry_MultipleTimes_AccumulatesCount()
    {
        var operation = CreateValidOperation();

        operation.Start();
        operation.Fail("Error 1");
        operation.IncrementRetry();

        operation.Start();
        operation.Fail("Error 2");
        operation.IncrementRetry();

        operation.Start();
        operation.Fail("Error 3");
        operation.IncrementRetry();

        operation.RetryCount.Should().Be(3);
        operation.Status.Should().Be(SyncOperationStatus.Pending);
    }
}

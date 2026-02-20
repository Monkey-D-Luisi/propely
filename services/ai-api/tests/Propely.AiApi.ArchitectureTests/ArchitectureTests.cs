// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using NetArchTest.Rules;
using Xunit;
using System.Reflection;
using Propely.AiApi.Domain.Common;
using Propely.AiApi.Domain.WorkItems;

namespace Propely.AiApi.ArchitectureTests;

public class ArchitectureTests
{
    private static readonly Assembly DomainAssembly = typeof(WorkItem).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(Propely.AiApi.Application.DependencyInjection).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(Propely.AiApi.Infrastructure.DependencyInjection).Assembly;
    private static readonly Assembly ApiAssembly = typeof(Propely.AiApi.Api.DependencyInjection).Assembly;

    [Fact]
    public void Domain_Should_Not_Depend_On_Other_Layers()
    {
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOn(ApplicationAssembly.GetName().Name)
            .And().HaveDependencyOn(InfrastructureAssembly.GetName().Name)
            .And().HaveDependencyOn(ApiAssembly.GetName().Name)
            .GetResult();

        Assert.True(result.IsSuccessful, "Domain layer should not depend on Application, Infrastructure, or Api.");
    }

    [Fact]
    public void Application_Should_Not_Depend_On_Infrastructure_Or_Api()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOn(InfrastructureAssembly.GetName().Name)
            .And().HaveDependencyOn(ApiAssembly.GetName().Name)
            .GetResult();

        Assert.True(result.IsSuccessful, "Application layer should not depend on Infrastructure or Api.");
    }

    [Fact]
    public void Infrastructure_Should_Not_Depend_On_Api()
    {
        var result = Types.InAssembly(InfrastructureAssembly)
            .ShouldNot()
            .HaveDependencyOn(ApiAssembly.GetName().Name)
            .GetResult();

        Assert.True(result.IsSuccessful, "Infrastructure layer should not depend on Api.");
    }

    [Fact]
    public void Handlers_Should_Have_Correct_Naming_Convention()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(MediatR.IRequestHandler<,>))
            .Should()
            .HaveNameEndingWith("Handler")
            .GetResult();

        Assert.True(result.IsSuccessful, "Command/Query handlers should end with 'Handler'.");
    }

    [Fact]
    public void Domain_Events_Should_Be_Sealed()
    {
        var result = Types.InAssembly(DomainAssembly)
            .That()
            .ImplementInterface(typeof(IDomainEvent))
            .Should()
            .BeSealed()
            .GetResult();
        
        Assert.True(result.IsSuccessful, "Domain events should be sealed.");
    }
}

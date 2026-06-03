using FluentAssertions;
using NetArchTest.Rules;
using System.Reflection;

namespace FastBiteGroup.Architecture.Tests;

/// <summary>
/// Architecture Tests — Bảo vệ Clean Architecture boundaries
/// Mục đích: Compile-time enforcement → Không ai có thể vi phạm kiến trúc mà không bị phát hiện
/// Dùng NetArchTest.Rules để scan assembly và enforce dependency rules
/// </summary>
public class ArchitectureTests
{
    // ─── Assembly references (load từ project references) ─────────────────

    private static readonly Assembly DomainAssembly =
        typeof(FastBiteGroup.Domain.AssemblyReference).Assembly;

    private static readonly Assembly ApplicationAssembly =
        typeof(FastBiteGroup.Application.AssemblyReference).Assembly;

    private static readonly Assembly ContractAssembly =
        typeof(FastBiteGroup.Contract.AssemblyReference).Assembly;

    private static readonly Assembly PersistenceAssembly =
        typeof(FastBiteGroup.Persistence.AssemblyReference).Assembly;

    private static readonly Assembly InfrastructureAssembly =
        typeof(FastBiteGroup.Infrastructure.AssemblyReference).Assembly;

    private static readonly Assembly PresentationAssembly =
        typeof(FastBiteGroup.Presentation.AssemblyReference).Assembly;

    // ─── Namespace constants ───────────────────────────────────────────────

    private const string DomainNamespace = "FastBiteGroup.Domain";
    private const string ApplicationNamespace = "FastBiteGroup.Application";
    private const string ContractNamespace = "FastBiteGroup.Contract";
    private const string PersistenceNamespace = "FastBiteGroup.Persistence";
    private const string InfrastructureNamespace = "FastBiteGroup.Infrastructure";
    private const string PresentationNamespace = "FastBiteGroup.Presentation";
    private const string ApiNamespace = "FastBiteGroup.API";

    // =====================================================================
    // RULE 1: Domain không được phụ thuộc vào Application
    // =====================================================================

    [Fact]
    public void Domain_ShouldNot_DependOnApplicationLayer()
    {
        // Arrange & Act
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOn(ApplicationNamespace)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            because: $"Domain layer phải THUẦN KHIẾT — không được import từ Application. " +
                     $"Các kiểu vi phạm: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
    }

    // =====================================================================
    // RULE 2: Domain không được phụ thuộc vào Persistence
    // =====================================================================

    [Fact]
    public void Domain_ShouldNot_DependOnPersistenceLayer()
    {
        // Arrange & Act
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOn(PersistenceNamespace)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            because: $"Domain không được biết đến Persistence (EF Core). " +
                     $"Vi phạm: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
    }

    // =====================================================================
    // RULE 3: Domain không được phụ thuộc vào Infrastructure
    // =====================================================================

    [Fact]
    public void Domain_ShouldNot_DependOnInfrastructureLayer()
    {
        // Arrange & Act
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOn(InfrastructureNamespace)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            because: $"Domain không được phụ thuộc vào Infrastructure (Redis, Email, Storage...). " +
                     $"Vi phạm: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
    }

    // =====================================================================
    // RULE 4: Application không được phụ thuộc vào Persistence
    // ⚠️  TEST NÀY SẼ FAIL — Vi phạm đã biết:
    //     TransactionPipelineBehaviors inject IUnitOfWork (Domain), nhưng
    //     IUnitOfWork.GetDbContext() expose DbContext (EF Core) vào Domain
    //     → Application.csproj hiện có ProjectReference đến Persistence
    // =====================================================================

    [Fact]
    public void Application_ShouldNot_DependOnPersistenceLayer()
    {
        // Arrange & Act
        var result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOn(PersistenceNamespace)
            .GetResult();

        // Assert
        // TODO: Sau khi fix vi phạm (tách IUnitOfWork không expose DbContext),
        //       thay dòng dưới bằng: result.IsSuccessful.Should().BeTrue()
        result.IsSuccessful.Should().BeTrue(
            because: $"Application KHÔNG được import từ Persistence. " +
                     $"Vi phạm hiện tại: TransactionPipelineBehaviors → ApplicationDbContext. " +
                     $"Cần fix: Tách IUnitOfWork không expose DbContext. " +
                     $"Các kiểu vi phạm: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
    }

    // =====================================================================
    // RULE 5: Application không được phụ thuộc vào Infrastructure
    // =====================================================================

    [Fact]
    public void Application_ShouldNot_DependOnInfrastructureLayer()
    {
        // Arrange & Act
        var result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOn(InfrastructureNamespace)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            because: $"Application không được gọi Infrastructure trực tiếp — phải qua interface. " +
                     $"Vi phạm: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
    }

    // =====================================================================
    // RULE 6: Presentation không được phụ thuộc trực tiếp vào Domain
    // =====================================================================

    [Fact]
    public void Presentation_ShouldNot_DependOnDomainLayer()
    {
        // Arrange & Act
        var result = Types.InAssembly(PresentationAssembly)
            .ShouldNot()
            .HaveDependencyOn(DomainNamespace)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            because: $"Presentation (Endpoints) không được gọi Domain trực tiếp — phải qua MediatR/Contract. " +
                     $"Vi phạm: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
    }

    // =====================================================================
    // RULE 7: Presentation không được phụ thuộc vào Persistence
    // =====================================================================

    [Fact]
    public void Presentation_ShouldNot_DependOnPersistenceLayer()
    {
        // Arrange & Act
        var result = Types.InAssembly(PresentationAssembly)
            .ShouldNot()
            .HaveDependencyOn(PersistenceNamespace)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            because: $"Presentation không được trực tiếp dùng EF Core / Repository. " +
                     $"Vi phạm: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
    }

    // =====================================================================
    // RULE 8: Contract không được phụ thuộc vào Application, Persistence, Infrastructure
    // =====================================================================

    [Fact]
    public void Contract_ShouldNot_DependOnApplicationOrPersistence()
    {
        // Arrange & Act
        var resultApp = Types.InAssembly(ContractAssembly)
            .ShouldNot()
            .HaveDependencyOn(ApplicationNamespace)
            .GetResult();

        var resultPersistence = Types.InAssembly(ContractAssembly)
            .ShouldNot()
            .HaveDependencyOn(PersistenceNamespace)
            .GetResult();

        // Assert
        resultApp.IsSuccessful.Should().BeTrue(
            because: "Contract (shared abstractions) phải là layer thấp nhất, không phụ thuộc vào bất kỳ layer nào khác.");

        resultPersistence.IsSuccessful.Should().BeTrue(
            because: "Contract không được biết đến Persistence.");
    }

    // =====================================================================
    // RULE 9: Tất cả Entities phải nằm trong Domain layer
    // =====================================================================

    [Fact]
    public void Entities_ShouldResideIn_DomainLayer()
    {
        // Arrange & Act — Classes kế thừa từ EntityBase phải ở trong Domain
        var result = Types.InAssembly(DomainAssembly)
            .That()
            .Inherit(typeof(FastBiteGroup.Domain.Abstractions.EntityBase<>))
            .Should()
            .ResideInNamespace(DomainNamespace)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            because: "Tất cả entities phải được định nghĩa trong Domain layer.");
    }

    // =====================================================================
    // RULE 10: Domain Exceptions phải kế thừa từ DomainException
    // =====================================================================

    [Fact]
    public void DomainExceptions_ShouldInheritFrom_DomainException()
    {
        // Arrange & Act
        var result = Types.InAssembly(DomainAssembly)
            .That()
            .ResideInNamespace("FastBiteGroup.Domain.Exceptions")
            .And()
            .AreClasses()
            .And()
            .AreNotAbstract()
            .And()
            .DoNotHaveName("DomainException") // exclude base class itself
            .Should()
            .Inherit(typeof(FastBiteGroup.Domain.Exceptions.DomainException))
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            because: "Mọi exception trong Domain.Exceptions phải kế thừa từ DomainException để GlobalExceptionHandler xử lý đúng.");
    }
}

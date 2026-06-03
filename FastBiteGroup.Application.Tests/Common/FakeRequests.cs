using FastBiteGroup.Contract.Abstractions.Shared;
using FluentValidation;
using MediatR;

namespace FastBiteGroup.Application.Tests.Common;

// ─── Fake Request/Response để test Behavior ────────────────────────────────

/// <summary>Request giả: kế thừa IRequest&lt;Result&gt; để test ValidationPipelineBehaviors</summary>
public record FakeRequest(string Name, decimal Price) : IRequest<Result>;

/// <summary>Request giả trả về Result&lt;T&gt; để test generic type resolution</summary>
public record FakeRequestWithResponse(string Value) : IRequest<Result<string>>;

// ─── Fake Validators ───────────────────────────────────────────────────────

/// <summary>Validator luôn PASS — không có lỗi</summary>
public class AlwaysPassValidator : AbstractValidator<FakeRequest>
{
    // Empty = luôn pass
}

/// <summary>Validator luôn FAIL với 1 lỗi cố định</summary>
public class AlwaysFailValidator : AbstractValidator<FakeRequest>
{
    public AlwaysFailValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithErrorCode("Name.Required")
            .WithMessage("Tên không được để trống");
    }
}

/// <summary>Validator sinh ra 2 lỗi để test aggregation</summary>
public class MultiErrorValidator : AbstractValidator<FakeRequest>
{
    public MultiErrorValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithErrorCode("Name.Required")
            .WithMessage("Tên không được để trống");

        RuleFor(x => x.Price)
            .GreaterThan(0)
            .WithErrorCode("Price.Invalid")
            .WithMessage("Giá phải lớn hơn 0");
    }
}

/// <summary>Validator cho FakeRequestWithResponse luôn FAIL</summary>
public class FakeResponseRequestFailValidator : AbstractValidator<FakeRequestWithResponse>
{
    public FakeResponseRequestFailValidator()
    {
        RuleFor(x => x.Value)
            .NotEmpty()
            .WithErrorCode("Value.Required")
            .WithMessage("Giá trị không được để trống");
    }
}

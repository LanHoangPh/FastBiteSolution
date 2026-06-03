namespace FastBiteGroup.Contract.Abstractions.Shared;

public class Result<TValue> : Result
{
    private readonly TValue? _value;

    protected internal Result(TValue? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("The value of a failure result can not be accessed."); // kiemtrả nếu IsSuccess là false thì không được phép truy cập vào Value, nếu truy cập sẽ ném ra lỗi InvalidOperationException

    public static implicit operator Result<TValue>(TValue? value) => Create(value);
}

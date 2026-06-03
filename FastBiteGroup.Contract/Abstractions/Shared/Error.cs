namespace FastBiteGroup.Contract.Abstractions.Shared;

//public class Error : IEquatable<Error>
//{
//    public static readonly Error None = new(string.Empty, string.Empty);
//    public static readonly Error NullValue = new("Error.NullValue", "An unknown error occurred.");

//    public Error(string code, string message)
//    {
//        Code = code;
//        Message = message;
//    }

//    public string Code { get; }
//    public string Message { get; }

//    public static implicit operator string(Error error) => error.Code;

//    public static bool operator ==(Error? a, Error? b)
//    {
//        if (a is null && b is null) return true;
//        if (a is null || b is null) return false;
//        return a.Equals(b);
//    }

//    public static bool operator !=(Error? a, Error? b) => !(a == b);

//    public virtual bool Equals(Error? other)
//    {
//        if (other is null) return false;
//        if (ReferenceEquals(this, other)) return true;
//        return Code == other.Code && Message == other.Message;
//    }

//    public override bool Equals(object? obj) => obj is Error error && Equals(error);
//    public override int GetHashCode() => HashCode.Combine(Code, Message);
//}
// 
public sealed record Error(string Code, string Message)  // rcord có thể thay thế cho class vì thằng ko pahỉ xứ lý kiểu so sánh tham chiếu nữa và complier sẽ tự động sinh ra các phương thức Equals và GetHashCode dựa trên các thuộc tính của record, giúp cho việc so sánh hai instance của Error trở nên dễ dàng và chính xác hơn
{
    public static readonly Error None = new(string.Empty, string.Empty);
    public static readonly Error NullValue = new("Error.NullValue", "An unknown error occurred.");

    public static implicit operator string(Error error) => error.Code;
}

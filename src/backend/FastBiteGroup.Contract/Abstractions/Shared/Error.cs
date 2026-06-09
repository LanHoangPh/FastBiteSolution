namespace FastBiteGroup.Contract.Abstractions.Shared;

// rcord có thể thay thế cho class vì thằng ko pahỉ xứ lý kiểu so sánh tham chiếu nữa và complier sẽ tự động sinh ra các phương thức Equals và GetHashCode dựa trên các thuộc tính của record, giúp cho việc so sánh hai instance của Error trở nên dễ dàng và chính xác hơn
public sealed record Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);
    public static readonly Error NullValue = new("Error.NullValue", "An unknown error occurred.");

    public static implicit operator string(Error error) => error.Code;
}

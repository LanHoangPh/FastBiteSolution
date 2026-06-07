using System;

namespace FastBiteGroup.Desktop.Domain.Exceptions;

public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message = "Phiên đăng nhập đã hết hạn.") : base(message)
    {
    }
}

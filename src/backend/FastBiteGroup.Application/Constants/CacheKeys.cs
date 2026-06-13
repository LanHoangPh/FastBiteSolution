namespace FastBiteGroup.Application.Constants;

public static class CacheKeys
{

    // Access Token blacklist — TTL = remaining token lifetime
    public static string AccessTokenBlacklist(string jti) =>
        $"auth:blacklist:jti:{jti}";
    public static string UserProfileCacheKey(Guid userId) =>
        $"user:profile:{userId}";

    public static string CountKeyLogin(string email) =>
        $"OTP_RESET_COUNT_{email}";

    public static string UserWorkspaces(Guid userId) =>
        $"workspaces:user:{userId}";

    public static string ProductsCacheKey => "products:all";
}

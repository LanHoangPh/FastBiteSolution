namespace FastBiteGroup.Domain.Enums;

public enum SettingKeys
{
    // General Settings
    SiteName,
    MaintenanceMode,

    // User Management
    AllowNewRegistrations,
    RequireEmailConfirmation,
    DefaultRoleForNewUsers,

    // Content & Moderation
    ForbiddenKeywords,
    AutoLockAccountThreshold,

    // File Uploads
    MaxFileSizeMb,
    MaxAvatarSizeMb,
    AllowedFileTypes,
    AllowedAvatarTypes
}

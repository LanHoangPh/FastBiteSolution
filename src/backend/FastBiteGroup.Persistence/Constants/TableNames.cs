namespace FastBiteGroup.Persistence.Constants;

internal static class TableNames
{
    // ********** Plural Nouns **********
    //internal const string Actions = nameof(Actions);
    //internal const string Functions = nameof(Functions);
    //internal const string ActionInFunctions = nameof(ActionInFunctions);
    //internal const string Permissions = nameof(Permissions);

    internal const string AppUser = nameof(AppUser);
    internal const string AppRoles = nameof(AppRoles);
    internal const string AppUserRoles = nameof(AppUserRoles);

    internal const string AppUserClaims = nameof(AppUserClaims); // IdentityUserClaim
    internal const string AppRoleClaims = nameof(AppRoleClaims); // IdentityRoleClaim
    internal const string AppUserLogins = nameof(AppUserLogins); // IdentityRoleClaim
    internal const string AppUserTokens = nameof(AppUserTokens); // IdentityUserToken

    // ********** Singular Nouns **********
    internal const string Product = nameof(Product);
    internal const string RefreshTokens = nameof(RefreshTokens);
    internal const string AdminAuditLogs = nameof(AdminAuditLogs);
    internal const string AdminNotifications = nameof(AdminNotifications);
    internal const string ContentReports = nameof(ContentReports);
    internal const string ConversationParticipants = nameof(ConversationParticipants);
    internal const string Conversations = nameof(Conversations);
    internal const string GlobalSettings = nameof(GlobalSettings);
    internal const string Groups = nameof(Groups);
    internal const string GroupInvitations = nameof(GroupInvitations);
    internal const string GroupMembers = nameof(GroupMembers);
    internal const string LoginHistories = nameof(LoginHistories);
    internal const string PollOptions = nameof(PollOptions);
    internal const string Polls = nameof(Polls);
    internal const string PollVotes = nameof(PollVotes);
    internal const string PostAttachments = nameof(PostAttachments);
    internal const string PostComments = nameof(PostComments);
    internal const string PostLikes = nameof(PostLikes);
    internal const string Posts = nameof(Posts);
    internal const string SharedFiles = nameof(SharedFiles);
    internal const string UserGroupInvitations = nameof(UserGroupInvitations);
    internal const string VideoCallParticipants = nameof(VideoCallParticipants);
    internal const string VideoCallSessions = nameof(VideoCallSessions);
}

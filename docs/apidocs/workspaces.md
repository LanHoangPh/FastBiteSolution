# Workspace API

Base route:

```text
/api/v1/workspaces
```

Auth: all endpoints require `Authorization: Bearer <accessToken>`.

## Enums

Request enum values:

| Field | Value | Meaning |
|---|---:|---|
| `privacy` | `1` | `Public` |
| `privacy` | `2` | `Private` |

Response enum strings:

| Concept | Values |
|---|---|
| Privacy | `Public`, `Private` |
| Workspace role | `Member`, `Moderator`, `Admin`, `Owner` |
| Member status | `Pending`, `Active`, `Banned`, `Left` |

## Response Models

### WorkspaceResponse

```json
{
  "workspaceId": "0f5e0d4e-1b8f-4b3c-930f-9a8a68a8c7b1",
  "workspaceName": "Acme Team",
  "description": "Internal workspace",
  "isChatEnabled": true,
  "isFeedEnabled": true,
  "privacy": "Private",
  "workspaceAvatarUrl": "https://cdn.example.com/logo.png",
  "currentUserRole": "Owner",
  "createdAt": "2026-06-07T14:34:31Z",
  "isArchived": false,
  "memberCount": 1
}
```

### WorkspaceDetailResponse

Same fields as `WorkspaceResponse`.

### WorkspaceMemberResponse

```json
{
  "workspaceMemberId": 1,
  "userId": "a3dff0ca-7f0e-42bb-b0f4-70112fdc6ab0",
  "email": "owner@example.com",
  "fullName": "Owner User",
  "avatarUrl": "https://cdn.example.com/avatar.png",
  "role": "Owner",
  "status": "Active",
  "joinedAt": "2026-06-07T14:34:31Z"
}
```

### WorkspaceInvitationResponse

```json
{
  "invitationId": 12,
  "workspaceId": "0f5e0d4e-1b8f-4b3c-930f-9a8a68a8c7b1",
  "workspaceName": "Acme Team",
  "description": "Internal workspace",
  "workspaceAvatarUrl": "https://cdn.example.com/logo.png",
  "invitedByUserId": "a3dff0ca-7f0e-42bb-b0f4-70112fdc6ab0",
  "createdAt": "2026-06-07T14:34:31Z",
  "expiresAt": "2026-06-14T14:34:31Z"
}
```

### WorkspaceInviteLinkResponse

```json
{
  "invitationId": 7,
  "workspaceId": "0f5e0d4e-1b8f-4b3c-930f-9a8a68a8c7b1",
  "invitationCode": "9F2A8C1B0D3E4F5A",
  "createdAt": "2026-06-07T14:34:31Z",
  "expiresAt": "2026-06-14T14:34:31Z",
  "maxUses": 25,
  "currentUses": 0,
  "isActive": true
}
```

## Error Codes

Due to current `HandleFailure<T>` behavior, most workspace business errors return HTTP `400`, even when the code semantically represents forbidden, not found, or conflict.

| Code | Message | Current HTTP Status |
|---|---|---:|
| `Workspace.NotFound` | Workspace was not found. | `400` for `Result<T>`, `404` for archive |
| `Workspace.Forbidden` | You do not have permission to access this workspace. | `400` for `Result<T>`, `403` for archive |
| `Workspace.InvitationNotFound` | Workspace invitation was not found. | `400` |
| `Workspace.InvitationExpired` | Workspace invitation has expired. | `400` |
| `Workspace.InvitationInactive` | Workspace invitation is not active. | `400` |
| `Workspace.InvitationMaxUsesReached` | Workspace invitation has reached its usage limit. | `400` |
| `Workspace.InvitationConflict` | A pending invitation already exists for this email in the workspace. | `400` |
| `Workspace.MemberConflict` | User is already an active member of this workspace. | `400` |
| `Workspace.LastOwnerRequired` | Workspace must keep at least one active owner. | Not currently emitted by mapped handlers |

## POST /

Creates a workspace. The current user becomes `Owner`.

Request:

```json
{
  "workspaceName": "Acme Team",
  "description": "Internal workspace",
  "isChatEnabled": true,
  "isFeedEnabled": true,
  "privacy": 2,
  "workspaceAvatarUrl": "https://cdn.example.com/logo.png"
}
```

Success:

```text
201 Created
Location: /api/v1/workspaces/{workspaceId}
```

Response: `WorkspaceResponse`

Validation:

| Field | Rules |
|---|---|
| `workspaceName` | Required, max 255 chars. |
| `description` | Optional, max 1000 chars. |
| `privacy` | Must be `1` or `2`. |
| `workspaceAvatarUrl` | Optional, max 2048 chars. |

Business errors:

| Code | When |
|---|---|
| `Workspace.NotFound` | Workspace was created but summary projection cannot be loaded for the current user. |

## GET /me

Returns active workspaces for the current user. Used after login for onboarding and sidebar workspace switching.

Request body: none

Success:

```text
200 OK
```

Response:

```json
[
  {
    "workspaceId": "0f5e0d4e-1b8f-4b3c-930f-9a8a68a8c7b1",
    "workspaceName": "Acme Team",
    "description": "Internal workspace",
    "isChatEnabled": true,
    "isFeedEnabled": true,
    "privacy": "Private",
    "workspaceAvatarUrl": "https://cdn.example.com/logo.png",
    "currentUserRole": "Owner",
    "createdAt": "2026-06-07T14:34:31Z",
    "isArchived": false,
    "memberCount": 1
  }
]
```

Behavior:

- Returns only active memberships.
- Excludes archived/deleted workspaces.
- Returns `[]` for a new user with no workspace.
- Uses Redis cache for 5 minutes by current user id.

## GET /invitations/me

Returns pending workspace invitations for the current user's email.

Request body: none

Success:

```text
200 OK
```

Response: `WorkspaceInvitationResponse[]`

Behavior:

- Uses the authenticated user's email claim.
- Email is normalized before lookup.

## POST /invitations/{invitationId}/accept

Accepts a pending email invitation for the authenticated user's email.

Route parameters:

| Name | Type | Required |
|---|---|---:|
| `invitationId` | integer | Yes |

Request body: none

Success:

```text
200 OK
```

Response: `WorkspaceResponse`

Validation:

| Field | Rules |
|---|---|
| `invitationId` | Must be greater than `0`. |

Business errors:

| Code | When |
|---|---|
| `Workspace.InvitationNotFound` | Invitation does not exist, is not pending, or does not belong to current user's email. |
| `Workspace.NotFound` | Workspace is archived. |
| `Workspace.InvitationExpired` | Invitation expiry is in the past. |
| `Workspace.MemberConflict` | Current user is already an active member. |
| `Workspace.Forbidden` | Current user is banned from the workspace. |

## POST /join

Joins a workspace using a shared invitation code.

Request:

```json
{
  "invitationCode": "9F2A8C1B0D3E4F5A"
}
```

Success:

```text
200 OK
```

Response: `WorkspaceResponse`

Validation:

| Field | Rules |
|---|---|
| `invitationCode` | Required, max 50 chars. |

Business errors:

| Code | When |
|---|---|
| `Workspace.InvitationNotFound` | Code does not exist. |
| `Workspace.NotFound` | Workspace is archived. |
| `Workspace.InvitationInactive` | Invite link is inactive. |
| `Workspace.InvitationExpired` | Invite link has expired. |
| `Workspace.InvitationMaxUsesReached` | Invite link reached `maxUses`. |
| `Workspace.MemberConflict` | Current user is already an active member. |
| `Workspace.Forbidden` | Current user is banned from the workspace. |

## GET /{workspaceId}

Returns workspace detail for the selected workspace.

Route parameters:

| Name | Type | Required |
|---|---|---:|
| `workspaceId` | GUID | Yes |

Request body: none

Success:

```text
200 OK
```

Response: `WorkspaceDetailResponse`

Business errors:

| Code | When |
|---|---|
| `Workspace.Forbidden` | Current user is not an active member, or the workspace summary is not visible to the current user. |

## GET /{workspaceId}/members

Returns active members in a workspace.

Route parameters:

| Name | Type | Required |
|---|---|---:|
| `workspaceId` | GUID | Yes |

Request body: none

Success:

```text
200 OK
```

Response: `WorkspaceMemberResponse[]`

Business errors:

| Code | When |
|---|---|
| `Workspace.Forbidden` | Current user is not an active member of the workspace. |

## PATCH /{workspaceId}

Updates workspace profile/settings. Only `Owner` and `Admin` can update.

Route parameters:

| Name | Type | Required |
|---|---|---:|
| `workspaceId` | GUID | Yes |

Request:

```json
{
  "workspaceName": "Acme Team",
  "description": "Updated description",
  "isChatEnabled": true,
  "isFeedEnabled": true,
  "privacy": 2,
  "workspaceAvatarUrl": "https://cdn.example.com/new-logo.png"
}
```

Success:

```text
200 OK
```

Response: `WorkspaceResponse`

Validation:

| Field | Rules |
|---|---|
| `workspaceId` | Required route GUID. |
| `workspaceName` | Required, max 255 chars. |
| `description` | Optional, max 1000 chars. |
| `privacy` | Must be `1` or `2`. |
| `workspaceAvatarUrl` | Optional, max 2048 chars. |

Business errors:

| Code | When |
|---|---|
| `Workspace.Forbidden` | Current user is not an active member, or is not `Owner`/`Admin`. |
| `Workspace.NotFound` | Workspace does not exist or is archived. |

## DELETE /{workspaceId}

Archives a workspace. This is a soft archive, not a hard delete. Only `Owner` can archive.

Route parameters:

| Name | Type | Required |
|---|---|---:|
| `workspaceId` | GUID | Yes |

Request body: none

Success:

```text
204 No Content
```

Validation:

| Field | Rules |
|---|---|
| `workspaceId` | Required route GUID. |

Business errors:

| Code | HTTP Status | When |
|---|---:|---|
| `Workspace.Forbidden` | `403` | Current user is not an active owner. |
| `Workspace.NotFound` | `404` | Workspace does not exist or is already archived. |

## POST /{workspaceId}/invitations

Creates a pending email invitation. The invited user does not need to exist.

Only `Owner` and `Admin` can invite.

Route parameters:

| Name | Type | Required |
|---|---|---:|
| `workspaceId` | GUID | Yes |

Request:

```json
{
  "email": "member@example.com"
}
```

Success:

```text
201 Created
Location: /api/v1/workspaces/invitations/{invitationId}
```

Response: `WorkspaceInvitationResponse`

Validation:

| Field | Rules |
|---|---|
| `workspaceId` | Required route GUID. |
| `email` | Required, valid email, max 256 chars. |

Behavior:

- Normalizes the invited email.
- Invitation expiry defaults to 7 days.
- Existing user id is attached when the invited email already belongs to a user.
- The current implementation stores the invitation record; it does not send invitation email yet.

Business errors:

| Code | When |
|---|---|
| `Workspace.Forbidden` | Current user is not `Owner`/`Admin` in the workspace. |
| `Workspace.NotFound` | Workspace does not exist or is archived. |
| `Workspace.InvitationConflict` | A pending invitation already exists for the email and workspace. |
| `Workspace.MemberConflict` | Invited user already exists and is an active member. |

## POST /{workspaceId}/invite-links

Creates a shared invitation code/link. Only `Owner` and `Admin` can create invite links.

Route parameters:

| Name | Type | Required |
|---|---|---:|
| `workspaceId` | GUID | Yes |

Request:

```json
{
  "expiresAt": "2026-06-14T14:34:31Z",
  "maxUses": 25
}
```

Both fields are optional.

Success:

```text
201 Created
Location: /api/v1/workspaces/{workspaceId}/invite-links/{invitationId}
```

Response: `WorkspaceInviteLinkResponse`

Validation:

| Field | Rules |
|---|---|
| `workspaceId` | Required route GUID. |
| `expiresAt` | Optional, must be in the future. |
| `maxUses` | Optional, must be greater than `0`. |

Business errors:

| Code | When |
|---|---|
| `Workspace.Forbidden` | Current user is not `Owner`/`Admin` in the workspace. |
| `Workspace.NotFound` | Workspace does not exist or is archived. |


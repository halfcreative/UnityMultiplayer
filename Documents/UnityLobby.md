# Unity Lobby

## Overview

Unity Lobby is part of the **Multiplayer Services SDK** (`com.unity.services.multiplayer`). It provides a managed lobby system backed by Unity Gaming Services (UGS), allowing players to discover, create, and join game sessions without building your own matchmaking backend.

Key capabilities:
- **Public lobbies** — listed and joinable via quick-join or a browsable list
- **Private lobbies** — joinable only by sharing a short lobby code
- **Lobby data** — store arbitrary key/value metadata on the lobby or per-player
- **Host heartbeat** — keep a lobby alive while the host is connected
- **Player management** — kick players, transfer host, leave, or delete the lobby

The free tier is sufficient for prototyping and small projects. No billing setup is required to get started.

---

## Project Setup

### 1. Create a Unity Cloud Project

1. Go to [cloud.unity.com](https://cloud.unity.com) and sign in.
2. Click **Create project** and give it a name.
3. Note the **Project ID** shown in the dashboard — you will need it.

### 2. Link Your Unity Project to the Cloud Project

1. In the Unity Editor, open **Edit → Project Settings → Services**.
2. Click **Use an existing Unity project ID**.
3. Select your organization and the cloud project you just created.
4. Click **Link**.

Your project is now connected to UGS and the `ProjectSettings/ProjectSettings.asset` file will contain the linked project ID.

### 3. Enable the Lobby Service

1. In the Unity Dashboard, navigate to **Multiplayer → Lobby**.
2. If prompted, click **Enable** — the service activates on the free tier automatically.
3. No additional configuration is required for basic use.

### 4. Install the Multiplayer Services Package

1. Open **Window → Package Manager**.
2. Click **+** and choose **Add package by name**.
3. Enter `com.unity.services.multiplayer` and click **Add**.

This package includes the Lobby, Relay, and other multiplayer services in a single SDK.

---

## Initialization

The SDK must be initialized before any service call. Authentication is required — anonymous sign-in is the simplest approach for a tutorial context.

```csharp
private async void InitializeUnityAuthentication()
{
    if (UnityServices.State != ServicesInitializationState.Initialized)
    {
        InitializationOptions initializationOptions = new InitializationOptions();
        // Use different profiles in the editor so multiple instances get separate identities
        initializationOptions.SetProfile(Random.Range(0, 10000).ToString());

        await UnityServices.InitializeAsync(initializationOptions);
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }
}
```
*Implemented in [`KitchenGameLobby.cs:55`](../Assets/Scripts/KitchenGameLobby.cs#L55)*

> **Important:** Always pass your `InitializationOptions` into `InitializeAsync()`. If you call `InitializeAsync()` without the options, the random profile is ignored and all editor instances share the same player identity, causing 409 "already a member" conflicts when testing with multiple windows.

---

## Core Operations

### Create a Lobby

```csharp
Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, new CreateLobbyOptions
{
    IsPrivate = isPrivate,
});
// lobby.LobbyCode contains the short code players can share for private lobbies
```
*Implemented in [`KitchenGameLobby.cs:68`](../Assets/Scripts/KitchenGameLobby.cs#L68)*

### Join by Code (Private)

```csharp
Lobby lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode);
```
*Implemented in [`KitchenGameLobby.cs:104`](../Assets/Scripts/KitchenGameLobby.cs#L104)*

### Quick Join (Public)

```csharp
Lobby lobby = await LobbyService.Instance.QuickJoinLobbyAsync();
```
*Implemented in [`KitchenGameLobby.cs:88`](../Assets/Scripts/KitchenGameLobby.cs#L88)*

### Join by ID (from a lobby list)

```csharp
Lobby lobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId);
```
*Implemented in [`KitchenGameLobby.cs:119`](../Assets/Scripts/KitchenGameLobby.cs#L119)*

### List Public Lobbies

```csharp
QueryResponse response = await LobbyService.Instance.QueryLobbiesAsync(new QueryLobbiesOptions
{
    Filters = new List<QueryFilter>
    {
        new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
    }
});
List<Lobby> lobbies = response.Results;
```
*Implemented in [`KitchenGameLobby.cs:208`](../Assets/Scripts/KitchenGameLobby.cs#L208)*

### Host Heartbeat

Lobbies expire if the host does not send a heartbeat. Call this on a timer (every ~15 seconds) while the local player is the host:

```csharp
await LobbyService.Instance.SendHeartbeatPingAsync(lobby.Id);
```
*Implemented in [`KitchenGameLobby.cs:139`](../Assets/Scripts/KitchenGameLobby.cs#L139)*

### Leave, Kick, and Delete

```csharp
// Leave
await LobbyService.Instance.RemovePlayerAsync(lobby.Id, AuthenticationService.Instance.PlayerId);

// Kick a specific player (host only)
await LobbyService.Instance.RemovePlayerAsync(lobby.Id, targetPlayerId);

// Delete (host only)
await LobbyService.Instance.DeleteLobbyAsync(lobby.Id);
```
*Leave implemented in [`KitchenGameLobby.cs:177`](../Assets/Scripts/KitchenGameLobby.cs#L177) · Kick in [`KitchenGameLobby.cs:193`](../Assets/Scripts/KitchenGameLobby.cs#L193) · Delete in [`KitchenGameLobby.cs:160`](../Assets/Scripts/KitchenGameLobby.cs#L160)*

### Reconnect (already a member)

If the player is already in a lobby (e.g., they navigated back to the menu without leaving), `JoinLobbyByCodeAsync` returns a 409 conflict. Recover by finding and reconnecting to the existing session:

```csharp
var lobbyIds = await LobbyService.Instance.GetJoinedLobbiesAsync();
foreach (var id in lobbyIds)
{
    var existing = await LobbyService.Instance.GetLobbyAsync(id);
    if (existing.LobbyCode == lobbyCode)
    {
        var lobby = await LobbyService.Instance.ReconnectToLobbyAsync(id);
        break;
    }
}
```
*Not currently implemented — would live in [`KitchenGameLobby.cs:104`](../Assets/Scripts/KitchenGameLobby.cs#L104) as a catch branch inside `JoinWithCode`*

---

## Implementation in This Project

The lobby logic lives in `Assets/Scripts/KitchenGameLobby.cs`. It is a singleton `MonoBehaviour` that persists across scenes (`DontDestroyOnLoad`). Key behaviors:

| Feature | Detail |
|---|---|
| Heartbeat | Sent every 15 s when `IsLobbyHost()` is true |
| Lobby list refresh | Polls every 3 s when not yet in a lobby |
| Events | `OnCreateLobbyStarted/Failed`, `OnJoinStarted/Failed`, `OnLobbyListChanged` — consumed by UI scripts |
| Scene flow | Host calls `Loader.LoadNetwork(CharacterSelectScene)` after creating; clients call `StartClient()` after joining |

---

## Important Links

- [Unity Lobby Documentation](https://docs.unity.com/en-us/lobby)
- [Multiplayer Services SDK on Unity Docs](https://docs.unity.com/ugs/en-us/manual/lobby/manual/unity-lobby-service)
- [Unity Gaming Services Dashboard](https://cloud.unity.com)
- [CodeMonkey Lobby Tutorial (YouTube)](https://youtu.be/-KDlEBfCBiU?si=jrY9JT7EnA3KTGO3)
- [Kitchen Chaos Multiplayer Course](https://unitycodemonkey.com/kitchenchaosmultiplayercourse.php#intro)

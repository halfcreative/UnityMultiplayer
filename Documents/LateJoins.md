# Late Joins

This game is short, so we will not be implementing late joins into the game.

Doing so adds a bit of complexity due to issues of data desync.

## Data Syncing Methods and Late Joins

We have 2 main methods of syncing data in Unity Netcode for GameObjects:
- **RPCs (Remote Procedure Calls)**
- **Network Variables**

When a client joins late, they will automatically receive the current state of any `NetworkVariable`s. However, they will **not** receive the history of RPC calls that were made before they joined. This means that if game state is heavily reliant on a sequence of RPCs to reach a certain state, late-joining clients will be desynced from the server and other clients unless a specific mechanism is implemented to bring them up to speed upon joining.

## Preventing Late Joins

Because of this complexity, we have opted to prevent players from joining late once a game session has started. 

To achieve this, we have enabled and implemented the "Connection Approval" setting on the `NetworkManager`. This allows the server to evaluate incoming client connections and either approve or reject them based on our custom game logic.

### Implementation Details

In `KitchenGameMultiplayer.cs`, we subscribe to the `ConnectionApprovalCallback` when the host starts:

```csharp
public void StartHost()
{
    NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManager_ConnectionApprovalCallback;
    NetworkManager.Singleton.StartHost();
}
```

The actual approval logic resides in the `NetworkManager_ConnectionApprovalCallback` method. We check if the game is still in the `WaitingToStart` state using our `KitchenGameManager`. If the game is still waiting, we approve the connection. If the game has already started (meaning it's past the waiting phase), we reject the connection, effectively preventing any late joins.

```csharp
/// <summary>
/// Connection approval callback to approve or reject incoming client connections
/// This is called on the server when a client tries to connect
/// We will approve the connection if the game is in the WaitingToStart state, otherwise we will reject it
/// </summary>
private void NetworkManager_ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
{
    if (KitchenGameManager.Instance.isWaitingToStart())
    {
        response.Approved = true;
        response.CreatePlayerObject = true;
    }
    else
    {
        response.Approved = false;
    }
}
```
# Multiplayer Debug Tools

## Network Simulator

The tutorial references a **Debug Simulator** on the `UnityTransport` component (`Network Manager -> Unity Transport -> Debug Simulator`). This is **outdated** — `DebugSimulator` is marked obsolete in Netcode 2.x / Transport 2.x and has no effect.

The simulator has moved to the **Multiplayer Tools** package.

### Setup

1. In the Unity Package Manager, install **Multiplayer Tools** (`com.unity.multiplayer.tools`) — it is not included by default.
2. Once installed, open the simulator via **Window > Multiplayer > Network Simulator**.

### Usage

The Network Simulator window lets you simulate adverse network conditions at runtime in the Editor:

- **Packet Delay** — adds latency (ms) to all packets
- **Packet Jitter** — adds variance to the delay
- **Packet Loss** — drops a percentage of packets

Changes take effect immediately while in Play Mode without modifying any code or component settings.

---

## RpcDelivery — Reliable vs Unreliable

When testing with simulated jitter or packet loss, the delivery mode of each RPC determines how it behaves under those conditions.

### How it works

RPCs default to `RpcDelivery.Reliable`, which means the transport layer guarantees delivery and ordering — packets that are dropped in transit are automatically retransmitted. This is safe but adds latency overhead, especially under jitter.

`RpcDelivery.Unreliable` sends the packet once with no retransmission. If it's dropped, it's gone. This is lower latency but the receiving end must tolerate missing calls.

```csharp
// Reliable (default) — guaranteed delivery, higher latency under jitter
[ClientRpc]
private void ExampleClientRpc() { }

// Unreliable — fire and forget, lower latency, can be dropped
[ClientRpc(Delivery = RpcDelivery.Unreliable)]
private void ExampleClientRpc() { }
```

### When to use Unreliable

An RPC is a good candidate for `Unreliable` when **missing one call has no lasting consequence** — either because the effect is purely cosmetic (a sound or particle) and a dropped packet is imperceptible, or because subsequent calls naturally correct any missed state.

An RPC must stay `Reliable` when it carries **game state that clients must not miss** — spawning objects, updating scores, changing ownership, or modifying data that isn't re-sent every frame.

### RPCs in this project

| RPC | File | Delivery | Reasoning |
|-----|------|----------|-----------|
| `InteractLogicClientRpc` | `TrashCounter.cs` | Can be Unreliable | Fires `OnAnyObjectTrashed` (sound/visual). The object is already destroyed — a dropped packet just means one client misses the trash sound. |
| `CutObjectClientRpc` | `CuttingCounter.cs` | Must stay Reliable | Increments `cuttingProgress` on all clients. A dropped packet desyncs the progress bar across clients. |
| `PlaceObjectOnCounterClientRpc` | `CuttingCounter.cs` | Must stay Reliable | Resets `cuttingProgress` to 0 and fires `OnProgressChanged`. Dropping this desyncs client UI state. |
| `SpawnNewWaitingRecipeClientRpc` | `DeliveryManager.cs` | Must stay Reliable | Adds a recipe to the waiting list — a missed call means one client is missing an order entirely. |
| `DeliverCorrectRecipeClientRpc` | `DeliveryManager.cs` | Must stay Reliable | Updates score and removes a recipe from the list — cannot be missed. |
| `AddIngredientClientRpc` | `PlateKitchenObject.cs` | Must stay Reliable | Adds an ingredient to a plate — missed calls desync plate contents between clients. |
| `SpawnPlateClientRpc` | `PlatesCounter.cs` | Must stay Reliable | Spawns a plate object — must arrive on all clients. |
| `ClearKitchenObjectOnParentClientRpc` | `KitchenGameMultiplayer.cs` | Must stay Reliable | Clears object parent references — missing this causes ghost object references. |

---

## RuntimeNetStatsMonitor (RNSM)

The `RuntimeNetStatsMonitor` is a component from the **Multiplayer Tools** package (`com.unity.multiplayer.tools`) that renders a live network stats overlay on screen. Unlike the Editor-only Network Profiler, it works in both the Editor and in builds, making it useful for observing real network behaviour during playtesting.

It displays stats such as bytes/packets sent and received, RTT, packet loss, RPC counts, and active NetworkObject count. Stats can be shown as counters or graphs, and the layout is configurable via a `NetStatsMonitorConfiguration` ScriptableObject asset.

---

## Protocol Version

`ProtocolVersion` is a `ushort` field on the `NetworkManager`'s **NetworkConfig** (default `0`). It is a developer-controlled version number that gets baked into the config hash sent during the connection handshake.

### How it works

When a client attempts to connect, it sends a hash of its `NetworkConfig` to the server. The server computes its own hash and compares them. If they don't match, the server immediately disconnects the client with the log warning:

```
NetworkConfig mismatch. The configuration between the server and client does not match.
```

`ProtocolVersion` is one of the values included in that hash. Changing it on either side without changing the other guarantees a mismatch and a rejected connection.

### Why it's useful

It gives you a deliberate way to prevent version-mismatched clients from joining a session — useful when you ship an update that changes networked behaviour and old clients should no longer be able to connect. Rather than relying on incidental config differences to block them, you increment `ProtocolVersion` intentionally.

It's also useful during testing: setting a different `ProtocolVersion` on a client build is an easy way to verify that your mismatch handling and error UI work correctly without changing any actual game logic.

### When to increment it

Bump `ProtocolVersion` whenever a change would make an old client incompatible with a new server — for example: adding or removing `NetworkVariable`s, changing RPC signatures, or altering the `NetworkPrefabs` list. Leave it unchanged for patches that don't affect networked state.

---

### Note on `Unreliable` and packet size

`RpcDelivery.Unreliable` requires the serialized RPC payload to fit within a single MTU packet (~1400 bytes). RPCs that pass large structs or arrays may fail silently at runtime if they exceed this limit. All RPCs in this project pass only small value types (ints, `ulong`, `NetworkObjectReference`) so this is not a concern here.

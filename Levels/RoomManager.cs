using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class RoomManager : NetworkSingleton<RoomManager> {
    // Variables
    [SerializeField] private float spawnDelay = 1.5f;

    private NetworkRunner runner;

    public UnityEvent OnStartRespawn;
    public UnityEvent OnEndRespawn;

    // References
    [SerializeField] private NetworkPrefabRef _playerPrefab;
    [SerializeField] private NetworkPrefabRef playerHeadPrefab;

    [Networked] private PlayerRef hostPlayer { get; set; }

    public void SetHost() {
        if (runner.ActivePlayers.ToList().Count == 1) {
            Debug.Log("Host Player was set to id: " + runner.LocalPlayer);
            hostPlayer = runner.LocalPlayer;
        }
    }

    public void SetRunner(NetworkRunner runner) {
        this.runner = runner;
    }

    public void RespawnLocalPlayer() {
        StartCoroutine(RespawnLocalPlayerTimer());
    }

    private IEnumerator RespawnLocalPlayerTimer() {
        OnStartRespawn.Invoke();
        var localPlayer = runner.LocalPlayer;

        DespawnPlayer(localPlayer);

        yield return new WaitForSeconds(spawnDelay);

        SpawnPlayer(localPlayer);
        OnEndRespawn.Invoke();
    }

    public void SpawnPlayer(PlayerRef player) {
        var spawnPosition = LevelManager.instance.GetSpawnPosition(Team.ANY, runner.ActivePlayers.ToList().Count, player.RawEncoded + 1);
        NetworkObject networkPlayerObject = runner.Spawn(_playerPrefab, spawnPosition, Quaternion.identity, player);
    }

    public void SpawnLocalPlayer() {
        SpawnPlayer(runner.LocalPlayer);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void SpawnAllPlayersRpc() {
        var localPlayer = runner.LocalPlayer;

        var localPlayerObjects = GetPlayerObjects(localPlayer);
        if (localPlayerObjects.Count > 0) return;

        SpawnPlayer(localPlayer);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void DespawnAllPlayersRpc() {
        var localPlayer = runner.LocalPlayer;
        DespawnPlayer(localPlayer);
    }

    public void DespawnLocalPlayer() {
        DespawnPlayer(runner.LocalPlayer);
    }

    public NetworkObject SpawnObject(NetworkObject networkObject, Vector2 spawnPosition, Transform parent) {
        var spawnedObject = runner.Spawn(networkObject, spawnPosition, Quaternion.identity);
        var networkTransform = spawnedObject.GetComponent<NetworkTransform>();

        spawnedObject.transform.parent = parent;

        return spawnedObject;
    }

    public void KillLocalPlayer(GameObject player) {
        var playerRenderer = player.GetComponent<PlayerRenderer>();
        var headData = playerRenderer.GetHeadData();
        var localPlayer = runner.LocalPlayer;

        var spawnedHead = runner.Spawn(playerHeadPrefab, player.transform.position, Quaternion.identity);
        var headRenderer = spawnedHead.GetComponent<HeadRenderer>();
        headRenderer.InitializeRpc(headData.cosmeticIndex, spawnDelay);

        RespawnLocalPlayer();
    }

    public void DespawnPlayer(PlayerRef player) {
        var playerObjects = GetPlayerObjects(player);

        foreach (var playerObject in playerObjects) {
            runner.Despawn(playerObject);
        }
    }

    public void DespawnObject(NetworkObject networkObject) {
        if (networkObject == null) {
            Debug.Log("Was null for some reason");
            return;
        }
        if (runner == null) {
            Debug.Log("Runner is null");
            return;
        }
        runner.Despawn(networkObject);
    }

    private List<NetworkObject> GetPlayerObjects(PlayerRef player) {
        var networkObjects = FindObjectsOfType<NetworkObject>();
        var validObjects = networkObjects.Where((obj) => obj.InputAuthority == player).ToList();
        return validObjects;
    }

    public void LocalQuitSession() {
        var localPlayer = runner.LocalPlayer;

        DespawnPlayer(localPlayer);
        runner.Shutdown();
        SceneManager.LoadScene(0);
    }

    public void DestroyNetworkObject(NetworkObject networkObject) {
        runner.Despawn(networkObject);
    }

    public bool isLocalPlayerHost() {
        if (hostPlayer == null) SetHost();

        var localPlayer = runner.LocalPlayer;
        return localPlayer == hostPlayer;
    }

    public int GetActivePlayerCount() {
        return runner.ActivePlayers.ToArray().Length;
    }
}

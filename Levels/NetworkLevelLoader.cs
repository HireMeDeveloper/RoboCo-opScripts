using Fusion;
using System.Collections;
using UnityEngine;

public class NetworkLevelLoader : NetworkSingleton<NetworkLevelLoader> {

    [Networked] private LevelName currentHostLevel { get; set; }
    private const float loadingDelay = 1.5f;

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void LoadLevelWithStateRpc(LevelName levelName, GameState gameState) {
        if (!RoomManager.instance.isLocalPlayerHost()) return;

        currentHostLevel = levelName;
        LoadLevelWithState(levelName, gameState);
    }
    private void LoadLevelWithState(LevelName levelName, GameState newState) {
        GameStateManager.instance.SetStateRpc(GameState.LOADING);

        var hostState = GameStateManager.instance.GetCurrentHostState();

        LevelFormatter.instance.ImportLevel(levelName);
        StartCoroutine(SetGlobalStateAfterDelay(newState, loadingDelay));
    }

    public void SyncLevelWithHost() {
        GameStateManager.instance.SetState(GameState.LOADING);

        var hostLevel = this.currentHostLevel;
        var hostState = GameStateManager.instance.GetCurrentHostState();

        LevelFormatter.instance.LoadLocalLevel(hostLevel);
        StartCoroutine(SetStateAfterDelay(hostState, loadingDelay));
    }

    private IEnumerator SetStateAfterDelay(GameState state, float delay) {
        var remainingDelay = delay;
        while (remainingDelay > 0) {
            yield return new WaitForEndOfFrame();
            UIManager.instance.UpdateLoadingBarFill(1 - (remainingDelay / delay));
            remainingDelay -= Time.deltaTime;
        }

        GameStateManager.instance.SetState(state);
    }

    private IEnumerator SetGlobalStateAfterDelay(GameState state, float delay) {
        var remainingDelay = delay;
        while (remainingDelay > 0) {
            yield return new WaitForEndOfFrame();
            UIManager.instance.UpdateLoadingBarRpc(1 - (remainingDelay / delay));
            remainingDelay -= Time.deltaTime;
        }

        GameStateManager.instance.SetStateRpc(state);
    }

    public void LoadLobby() {
        LoadLevelWithStateRpc(LevelName.LOBBY_COOP, GameState.LOBBY_COOP);
    }

    public void LoadDevArena() {
        LoadLevelWithStateRpc(LevelName.COOP_DEVELOPMENT_ARENA, GameState.GAMEPLAY_COOP);
    }

    public LevelName GetCurrentHostLevel() {
        return this.currentHostLevel;
    }
}

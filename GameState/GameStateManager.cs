using Fusion;
using UnityEngine;

public class GameStateManager : NetworkSingleton<GameStateManager> {

    [Networked] private GameState currentHostState { get; set; }
    private GameState currentState = GameState.LOADING;

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void SetStateRpc(GameState newState) {
        SetState(newState);
    }
    public void SetState(GameState newState) {
        var lastState = currentState;
        TriggerState(lastState, false);

        currentState = newState;
        TriggerState(currentState, true);

        if (RoomManager.instance.isLocalPlayerHost()) currentHostState = currentState;
    }

    private void TriggerState(GameState state, bool isEntering) {
        if (isEntering) {
            Debug.Log("Entering State: " + state);
        } else {
            Debug.Log("Exiting State: " + state);
        }

        switch (state) {
            case GameState.LOBBY_COOP:
                if (isEntering) {
                    UIManager.instance.ShowUIGroup(UIGroup.HUD_COOP);
                    RoomManager.instance.SpawnLocalPlayer();
                } else {
                    RoomManager.instance.DespawnLocalPlayer();
                }
                break;
            case GameState.GAMEPLAY_COOP:
                if (isEntering) {
                    UIManager.instance.ShowUIGroup(UIGroup.HUD_COOP);
                    RoomManager.instance.SpawnLocalPlayer();
                } else {
                    RoomManager.instance.DespawnLocalPlayer();
                }
                break;
            case GameState.POSTGAME_COOP:
                if (isEntering) {

                } else {

                }
                break;
            case GameState.LOADING:
                if (isEntering) {
                    UIManager.instance.ShowUIGroup(UIGroup.LOADING);
                } else {

                }
                break;
            case GameState.SANDBOX_EDITOR:
                if (isEntering) {
                    UIManager.instance.ShowUIGroup(UIGroup.SANDBOX_EDITOR);
                    CameraManager.instance.SetCameraMode(false);
                    LevelEditorGridManager.instance.CreateTemplate();
                } else {
                    CameraManager.instance.SetCameraMode(true);
                }
                break;
            case GameState.SANDBOX_GAMEPLAY:
                if (isEntering) {
                    UIManager.instance.ShowUIGroup(UIGroup.HUD_COOP);
                    RoomManager.instance.SpawnLocalPlayer();
                } else {
                    RoomManager.instance.DespawnLocalPlayer();
                }
                break;
            default:
                break;
        }
    }

    public GameState GetCurrentState() {
        return currentState;
    }

    public GameState GetCurrentHostState() {
        return currentHostState;
    }
}

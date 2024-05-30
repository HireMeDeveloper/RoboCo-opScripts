using UnityEngine;
using UnityEngine.Events;

public class LocalLevelLoader : SingletonBehaviour<LocalLevelLoader> {
    private float progress = 0.0f;

    [HideInInspector] public UnityEvent onLoadComplete;

    public void LoadLevelWithState(LevelName levelName, GameState newState) {
        GameStateManager.instance.SetState(GameState.LOADING);

        ClearLoadingProgress();

        SetStateAfterLoadingRpc(newState);
        LevelFormatter.instance.ImportLevel(levelName);
    }

    public void LoadLobby() {
        LoadLevelWithState(LevelName.LOBBY_COOP, GameState.LOBBY_COOP);
    }

    public void LoadDevArena() {
        LoadLevelWithState(LevelName.COOP_DEVELOPMENT_ARENA, GameState.GAMEPLAY_COOP);
    }
    private void SetStateAfterLoadingRpc(GameState newState) {
        if (!RoomManager.instance.isLocalPlayerHost()) return;

        onLoadComplete.AddListener(() => {
            GameStateManager.instance.SetStateRpc(newState);
        });
    }

    public void SendLoadingProgress(float progress) {
        if (!RoomManager.instance.isLocalPlayerHost()) return;

        UIManager.instance.UpdateLoadingBarFill(progress);

        if (progress >= 1.0f) {
            OnLoadingProgressComplete();
        }
    }

    public void ClearLoadingProgress() {
        progress = 0.0f;
    }

    private void OnLoadingProgressComplete() {
        onLoadComplete.Invoke();
        ClearLoadingProgress();

        onLoadComplete.RemoveAllListeners();
    }
}

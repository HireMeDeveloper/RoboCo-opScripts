using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviour, INetworkRunnerCallbacks {
    // Networking
    private NetworkRunner _runner;

    // Input
    private InputActions inputActions;

    // Reference
    [SerializeField] private TMP_InputField roomNameInputField;

    public UnityEvent onConnectStart;
    public UnityEvent onConnectFail;

    private void Awake() {
        _runner = gameObject.AddComponent<NetworkRunner>();
        _runner.ProvideInput = true;

        inputActions = new InputActions();
        inputActions.Enable();
    }

    public void StartSinglePlayerGame() {
        StartGame(GameMode.Single, "local");
    }

    public void StartSharedGame() {
        StartGame(GameMode.Shared, roomNameInputField.text);
    }

    public void StartEditorGame() {
        StartGame(GameMode.Single, "editor");
    }

    private async void StartGame(GameMode mode, String roomName) {
        onConnectStart?.Invoke();

        var formatedRoomName = roomName.Trim().ToLower();

        // Create the NetworkSceneInfo from the current scene
        var scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex + 1);
        var editorScene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex + 2);

        if (formatedRoomName == "editor") scene = editorScene;

        var sceneInfo = new NetworkSceneInfo();
        if (scene.IsValid) {
            sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);
        }

        // Create the session properties
        var sessionProperties = new Dictionary<string, SessionProperty>();
        sessionProperties["is_editor"] = (formatedRoomName == "editor");


        // Start or join (depends on gamemode) a session with a specific name
        await _runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = formatedRoomName,
            Scene = scene,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>(),
            SessionProperties = sessionProperties
        });

    }

    public void OnConnectedToServer(NetworkRunner runner) {
        Debug.Log("Connected To Server");
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) {
        onConnectFail?.Invoke();
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }

    public void OnInput(NetworkRunner runner, NetworkInput input) {
        var data = new NetworkInputData();

        data.buttons.Set(MyButtons.Jump, inputActions.player.jump.IsPressed());
        data.movement = inputActions.player.move.ReadValue<Vector2>();

        input.Set(data);
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) {
        if (player == runner.LocalPlayer) {

            var roomManager = RoomManager.instance;
            var levelLoader = NetworkLevelLoader.instance;

            roomManager.SetRunner(runner);
            roomManager.SetHost();

            var isEditor = SceneManager.GetActiveScene() == SceneManager.GetSceneByName("Sandbox");

            Debug.Log("Is Editor: " + isEditor);
            LevelManager.instance.SetRoomName((runner.GameMode == GameMode.Single) ? " " : "Room: " + runner.SessionInfo.Name);

            if (roomManager.isLocalPlayerHost()) {
                if (isEditor) {
                    levelLoader.LoadLevelWithStateRpc(LevelName.EMPTY_EDITOR, GameState.SANDBOX_EDITOR);
                } else {
                    levelLoader.LoadLevelWithStateRpc(LevelName.LOBBY_COOP, GameState.LOBBY_COOP);
                }
            } else {
                levelLoader.SyncLevelWithHost();
            }
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) {
        RoomManager.instance.DespawnPlayer(player);
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }

    public void OnSceneLoadDone(NetworkRunner runner) { }

    public void OnSceneLoadStart(NetworkRunner runner) { }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
}

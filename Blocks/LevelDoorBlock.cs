using Fusion;
using TMPro;
using UnityEngine;

public class LevelDoorBlock : NetworkBlock, ISpawned {
    [SerializeField] private LevelName previousLevel;
    [SerializeField] private LevelName level;
    [SerializeField] private GameState levelState;

    private Animator anim;

    [Networked, Capacity(10)] private NetworkLinkedList<NetworkId> currentlyOverlapping { get; } = new NetworkLinkedList<NetworkId>();
    [Space, SerializeField] private GameObject goldPlaque;

    [Space]
    [SerializeField] private TMP_Text levelTextComponent;
    [SerializeField] private string levelText;

    private bool isLocked { get; set; }

    private void Awake() {
        anim = GetComponent<Animator>();

    }

    private void Start() {
        UpdateDoorDataRpc();
    }

    public override void Spawned() {

    }

    private void OnValidate() {
        levelTextComponent.text = levelText;
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        var player = collision.GetComponent<PlayerPhysics2D>();
        if (player == null) return;
        if (player.tag != "Player") return;
        var networkObject = player.GetComponent<NetworkObject>();

        currentlyOverlapping.Add(networkObject.Id);

        if (currentlyOverlapping.Count > 0) OnOpenRpc();
        if (!isLocked) player.OnJump.AddListener(UseDoor);
    }

    private void OnTriggerExit2D(Collider2D collision) {
        var player = collision.GetComponent<PlayerPhysics2D>();
        if (player == null) return;
        if (player.tag != "Player") return;
        var networkObject = player.GetComponent<NetworkObject>();

        if (currentlyOverlapping.Contains(networkObject.Id)) currentlyOverlapping.Remove(networkObject.Id);

        if (currentlyOverlapping.Count == 0) OnCloseRpc();
        player.OnJump.RemoveListener(UseDoor);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void OnOpenRpc() {
        anim.SetBool("open", true);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void OnCloseRpc() {
        anim.SetBool("open", false);
    }

    public void UseDoor() {
        NetworkLevelLoader.instance.LoadLevelWithStateRpc(level, levelState);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void UpdateDoorDataRpc() {
        if (!RoomManager.instance.isLocalPlayerHost()) return;

        int previousValue;

        if (previousLevel == level) {
            previousValue = 1;
        } else {
            previousValue = LocalDataManager.instance.GetLevelValue(previousLevel);
        }

        var previousPrint = LocalDataManager.instance.GetLevelValue(previousLevel);

        var levelValue = LocalDataManager.instance.GetLevelValue(level);
        UpdateDoorRpc(previousValue == 0, levelValue == 2);

        Debug.Log("Door Updated with: " + (previousPrint) + " and " + (levelValue));
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void UpdateDoorRpc(bool isLocked, bool isGold) {
        this.isLocked = isLocked;
        anim.SetBool("locked", isLocked);

        goldPlaque.SetActive(isGold);
    }
}

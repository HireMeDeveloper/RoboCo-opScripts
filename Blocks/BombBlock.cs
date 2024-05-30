using Fusion;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class BombBlock : NetworkBlock {

    private Animator anim;

    private bool canBeTriggered = true;
    private bool isBlocked = false;
    private bool isRespawning = false;

    [SerializeField] private float respawnDelay;
    private float currentRespawnDelay;

    [SerializeField] private float triggerDistance = 1.1f;

    [Networked, Capacity(10)] private NetworkLinkedList<NetworkId> currentlyPressing { get; } = new NetworkLinkedList<NetworkId>();

    [HideInInspector] public UnityEvent onPlayerExit;

    private void Awake() {
        anim = GetComponent<Animator>();
    }
    private void OnCollisionEnter2D(Collision2D collision) {
        var player = collision.gameObject.GetComponent<PlayerPhysics2D>();
        if (player == null) return;
        if (player.tag != "Player") return;

        TriggerBoomRpc();
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        var networkObject = collision.GetComponent<NetworkObject>();
        if (networkObject == null) return;
        if (networkObject.tag != "Player" && networkObject.tag != "Rock") return;

        currentlyPressing.Add(networkObject.Id);
        isBlocked = true;
    }

    private void OnTriggerExit2D(Collider2D collision) {
        var networkObject = collision.GetComponent<NetworkObject>();
        if (networkObject == null) return;
        if (networkObject.tag != "Player" && networkObject.tag != "Rock") return;

        if (currentlyPressing.Contains(networkObject.Id)) currentlyPressing.Remove(networkObject.Id);

        if (currentlyPressing.Count <= 0) {
            isBlocked = false;
            onPlayerExit.Invoke();
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void TriggerBoomRpc() {
        if (!canBeTriggered) return;
        canBeTriggered = false;

        anim.SetTrigger("boom");

        if (RoomManager.instance.isLocalPlayerHost()) {
            currentRespawnDelay = respawnDelay;
            isRespawning = true;
        }
    }

    private void Update() {
        var roomManager = RoomManager.instance;
        if (roomManager == null) return;

        if (roomManager.isLocalPlayerHost() && currentRespawnDelay > 0 && isRespawning) {
            currentRespawnDelay -= Time.deltaTime;
        } else if (roomManager.isLocalPlayerHost() && currentRespawnDelay <= 0 && isRespawning) {
            isRespawning = false;
            currentRespawnDelay = respawnDelay;
            RespawnRpc();
        }
    }

    public void FindAndTriggerNearbyBombBlocks() {
        var nearbyBombs = GameObject.FindObjectsOfType<BombBlock>();
        var validBombs = nearbyBombs.Where(bomb => Mathf.Abs(Vector3.Distance(transform.position, bomb.transform.position)) <= triggerDistance).ToList();

        foreach (var bomb in validBombs) {
            bomb.TriggerBoomRpc();
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RespawnRpc() {
        if (isBlocked) {
            onPlayerExit.AddListener(RespawnRpc);
            return;
        } else {
            onPlayerExit.RemoveListener(RespawnRpc);
        }

        anim.SetTrigger("spawn");
        canBeTriggered = true;
    }
}

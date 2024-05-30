using Fusion;
using UnityEngine;

public class Checkpoint : NetworkBlock {
    [SerializeField] private int number;
    [SerializeField] private Transform spawntransform;
    [SerializeField] private SpriteRenderer thumbnailRenderer;

    private Animator anim;
    private bool isRaised;

    private void Awake() {
        anim = GetComponent<Animator>();
    }

    private void Start() {
        thumbnailRenderer.enabled = false;
    }
    private void OnTriggerEnter2D(Collider2D collision) {
        var player = collision.GetComponent<PlayerController>();
        if (player == null) return;

        if (!isRaised) CollectCheckpointRpc();
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void CollectCheckpointRpc() {
        isRaised = true;
        LevelManager.instance.ActivateCheckpoint(this);

        anim.SetTrigger("Primary");
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void SetAsSecondaryRpc() {
        isRaised = true;
        anim.SetTrigger("Secondary");
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void DeactivateRpc() {
        isRaised = false;
        anim.SetTrigger("Reset");
    }

    public Vector2 GetSpawnPosition() {
        return spawntransform.position;
    }

    public int GetCheckpointNumber() {
        return number;
    }
}

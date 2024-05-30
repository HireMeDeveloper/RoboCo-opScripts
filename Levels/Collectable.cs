using Fusion;

public abstract class Collectable : NetworkBehaviour {

    protected abstract void OnCollect();

    private void OnTriggerEnter2D(UnityEngine.Collider2D collision) {
        var player = collision.gameObject.GetComponent<PlayerController>();
        if (player == null) return;

        // Get the player who collected it, so that we can apply effects later using an Rpc

        CollectRpc();

        var networkObject = GetComponent<NetworkObject>();
        RoomManager.instance.DestroyNetworkObject(networkObject);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void CollectRpc() {
        OnCollect();

    }
}

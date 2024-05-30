using Fusion;
using UnityEngine;

public class SecretBlock : NetworkBlock {
    [SerializeField] private SpriteRenderer thumbnailRenderer;

    [SerializeField] private GameObject hiddenBlock;
    [SerializeField] private GameObject revealedBlock;
    [Networked, Capacity(10)] private NetworkLinkedList<NetworkId> currentlyRevealing { get; } = new NetworkLinkedList<NetworkId>();

    private void Start() {
        thumbnailRenderer.enabled = false;
        hiddenBlock.SetActive(true);
        revealedBlock.SetActive(false);
    }
    private void OnTriggerEnter2D(Collider2D collision) {
        var networkObject = collision.GetComponent<NetworkObject>();
        if (networkObject == null) return;
        if (networkObject.tag != "Player") return;

        currentlyRevealing.Add(networkObject.Id);

        if (currentlyRevealing.Count > 0) OnRevealRpc();
    }

    private void OnTriggerExit2D(Collider2D collision) {
        var networkObject = collision.GetComponent<NetworkObject>();
        if (networkObject == null) return;
        if (networkObject.tag != "Player") return;

        if (currentlyRevealing.Contains(networkObject.Id)) currentlyRevealing.Remove(networkObject.Id);

        if (currentlyRevealing.Count == 0) OnHiddenRpc();
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void OnRevealRpc() {
        RevealBlock(true);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void OnHiddenRpc() {
        RevealBlock(false);
    }

    private void RevealBlock(bool isRevealed) {
        if (isRevealed) {
            hiddenBlock.SetActive(false);
            revealedBlock.SetActive(true);
        } else {
            hiddenBlock.SetActive(true);
            revealedBlock.SetActive(false);
        }
    }
}

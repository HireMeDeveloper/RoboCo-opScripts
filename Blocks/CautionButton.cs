using Fusion;
using System.Linq;
using UnityEngine;

public class CautionButton : NetworkBlock {

    [SerializeField] private BlockColor color;
    [Space]
    [SerializeField] private Animator anim;
    [Networked, Capacity(10)] private NetworkLinkedList<NetworkId> currentlyPressing { get; } = new NetworkLinkedList<NetworkId>();
    private void OnTriggerEnter2D(Collider2D collision) {
        var networkObject = collision.GetComponent<NetworkObject>();
        if (networkObject == null) return;
        if (networkObject.tag != "Player" && networkObject.tag != "Rock") return;

        currentlyPressing.Add(networkObject.Id);

        if (currentlyPressing.Count > 0) OnPressedRpc();
    }

    private void OnTriggerExit2D(Collider2D collision) {
        var networkObject = collision.GetComponent<NetworkObject>();
        if (networkObject == null) return;
        if (networkObject.tag != "Player" && networkObject.tag != "Rock") return;

        if (currentlyPressing.Contains(networkObject.Id)) currentlyPressing.Remove(networkObject.Id);

        if (currentlyPressing.Count == 0) OnReleasedRpc();
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void OnPressedRpc() {
        anim.SetBool("isDown", true);

        ToggleBlocks(false);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void OnReleasedRpc() {
        anim.SetBool("isDown", false);

        ToggleBlocks(true);
    }

    private void ToggleBlocks(bool on) {
        var cautionBlocks = GameObject.FindObjectsOfType<CautionBlock>();
        var validBlocks = cautionBlocks.Where((block) => block.color == this.color).ToList();

        foreach (var block in validBlocks) {
            block.SetIsABlock(on);
        }
    }
}

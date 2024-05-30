using UnityEngine;

public class IceBlock : Block {
    private void OnCollisionEnter2D(Collision2D collision) {
        var player = collision.gameObject.GetComponent<PlayerPhysics2D>();
        if (player == null) return;

        player.AddIceBlock(this.gameObject);
    }

    private void OnCollisionExit2D(Collision2D collision) {
        var player = collision.gameObject.GetComponent<PlayerPhysics2D>();
        if (player == null) return;

        player.RemoveIceBlock(this.gameObject);
    }
}

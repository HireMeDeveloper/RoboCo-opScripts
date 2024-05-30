using UnityEngine;

public class WaterBlock : Block {
    private void OnTriggerEnter2D(Collider2D collision) {
        var player = collision.gameObject.GetComponent<DeathManager>();
        if (player == null) return;

        player.KillPlayer();
    }
}

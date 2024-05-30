using UnityEngine;

public class SpikeBlock : Block {
    private Animator anim;

    private void Awake() {
        anim = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        var player = collision.gameObject.GetComponent<DeathManager>();
        if (player == null) return;

        anim.SetTrigger("Extend");
        player.KillPlayer();
    }
}

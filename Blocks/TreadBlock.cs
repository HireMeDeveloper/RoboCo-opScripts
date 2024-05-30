using UnityEngine;

public class TreadBlock : Block {
    [SerializeField] private bool movesRight;

    private void OnTriggerEnter2D(Collider2D collision) {
        var player = collision.gameObject.GetComponent<PlayerPhysics2D>();
        if (player == null) return;

        player.AddTreadBlock(this.gameObject, movesRight);
    }

    private void OnTriggerExit2D(Collider2D collision) {
        var player = collision.gameObject.GetComponent<PlayerPhysics2D>();
        if (player == null) return;

        player.RemoveTreadBlock(this.gameObject, movesRight);
    }
}

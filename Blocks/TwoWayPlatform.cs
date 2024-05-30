using System.Collections;
using UnityEngine;

public class TwoWayPlatform : Block {
    [SerializeField] private Collider2D twoWayCollider;
    private void OnCollisionEnter2D(Collision2D collision) {
        var player = collision.gameObject.GetComponent<PlayerPhysics2D>();
        if (player == null) return;

        //player.twoWayPlatformColliders.Add(twoWayCollider);
    }

    private void OnCollisionExit2D(Collision2D collision) {
        var player = collision.gameObject.GetComponent<PlayerPhysics2D>();
        if (player == null) return;

        //if (player.twoWayPlatformColliders.Contains(twoWayCollider)) player.RemoveTwoWayPlatfromCollider(twoWayCollider);
    }

    public void IgnoreCollisionForSeconds(Collider2D ignored, float seconds) {
        StartCoroutine(DropTimer(ignored, seconds));
    }

    private IEnumerator DropTimer(Collider2D ignored, float seconds) {
        Physics2D.IgnoreCollision(ignored, twoWayCollider, true);

        yield return new WaitForSeconds(seconds);

        Physics2D.IgnoreCollision(ignored, twoWayCollider, false);
    }
}

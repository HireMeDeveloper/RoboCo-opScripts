using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FireBlock : Block {

    private void OnTriggerEnter2D(Collider2D collision) {
        var player = collision.gameObject.GetComponent<PlayerPhysics2D>();
        if (player == null || player.tag != "Player") return;

        // Get nearby blocks
        var nearbyBlocks = getNearbyFireBlocks(1.1f);

        var normal = player.transform.position - transform.position;
        var direction = GetDirectionFromNormal(normal);

        // Apply force to player
        player.AddForce(direction.normalized, 12.0f);
    }

    private Vector2 GetDirectionFromNormal(Vector2 vector) {
        // Normalize the input vector to ensure it's a unit vector
        Vector2 normalizedVector = vector.normalized;

        // Reference vector (Vector2.up points upwards)
        Vector2 referenceVector = Vector2.up;

        // Calculate the dot product between the normalized vector and the reference vector
        float dotProduct = Vector2.Dot(normalizedVector, referenceVector);

        // Calculate the angle in radians between the two vectors
        float angleRadians = Mathf.Acos(dotProduct);

        // Convert the angle to degrees
        float angle = angleRadians * Mathf.Rad2Deg;

        // Calculate the cross product to determine the direction of the angle
        float crossProduct = normalizedVector.x * referenceVector.y - normalizedVector.y * referenceVector.x;

        // Adjust the angle based on the direction determined by the cross product
        if (crossProduct < 0) {
            angle = 360f - angle;
        }

        Vector2 direction;

        if (angle > 315.0f || angle < 45.0f) {
            // Up direction
            direction = Vector2.up;
        } else if (angle <= 135.0f) {
            // Right direction
            direction = Vector2.right;
        } else if (angle < 225.0f) {
            // Down direction
            direction = Vector2.down;
        } else {
            // Left direction
            direction = Vector2.left;
        }

        return direction;
    }

    private List<FireBlock> getNearbyFireBlocks(float maxDistance) {
        var fireBlocks = FindObjectsOfType<FireBlock>();
        var validBlocks = fireBlocks
            .Where(block => block != this)
            .Where(block => Mathf.Abs(Vector2.Distance(block.transform.position, transform.position)) < maxDistance)
            .ToList();
        return validBlocks;
    }
}

using UnityEngine;

public class PlayerSpawn : Block {
    public Team team;

    [SerializeField] private Transform spawnTransform;

    public Vector2 getSpawnPoint() {
        return spawnTransform.position;
    }
}

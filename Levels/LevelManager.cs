using System.Linq;
using TMPro;
using UnityEngine;

public class LevelManager : SingletonBehaviour<LevelManager> {

    [SerializeField] private float maxSpawnSpread = 0.75f;

    [SerializeField] private TMP_Text roomNameDisplay;

    private Checkpoint currentCheckpoint;
    public Vector2 GetSpawnPosition(Team team, int teamSize, int playerNumber) {
        // Create unique offset for player
        var startOffset = new Vector2((teamSize == 1)? 0 : -maxSpawnSpread, 0);
        var playerSpread = (teamSize > 1)
            ? (2.0f * maxSpawnSpread) / (teamSize - 1)
            : 0.0f;
        var uniqueOffset = new Vector2(startOffset.x + (playerSpread * playerNumber), 0);

        if (currentCheckpoint == null) {
            // Get available spawns
            var spawns = GameObject.FindObjectsOfType<PlayerSpawn>();
            var validSpawns = spawns.Where((spawn) => team == Team.ANY || team == spawn.team).ToList();

            // Return random point with offset
            var rand = Random.Range(0, validSpawns.Count);
            return validSpawns[rand].getSpawnPoint() + uniqueOffset;
        } else {
            return currentCheckpoint.GetSpawnPosition() + uniqueOffset;
        }
    }

    public void SetRoomName(string roomName) {
        roomNameDisplay.text = roomName;
    }

    public void CollectSilverCoin() {
        var exitBlocks = FindObjectsOfType<ExitBlock>();

        foreach (var exitBlock in exitBlocks) {
            exitBlock.AddCoinRpc();
        }
    }

    public void CollectGoldCoin() {

    }

    public void ActivateCheckpoint(Checkpoint newCheckpoint) {
        var previousCheckpoint = currentCheckpoint;

        int previousCheckpointNumber = 0;
        if (previousCheckpoint != null) {
            previousCheckpointNumber = previousCheckpoint.GetCheckpointNumber();
        }

        if (previousCheckpointNumber < newCheckpoint.GetCheckpointNumber()) {
            var checkpoints = GameObject.FindObjectsOfType<Checkpoint>();
            var lowerCheckpoints = checkpoints.Where((checkpoint) => newCheckpoint.GetCheckpointNumber() > checkpoint.GetCheckpointNumber()).ToList();
            Debug.Log("Found " + lowerCheckpoints.Count + " Checkpoints");
            foreach (var checkpoint in lowerCheckpoints) {
                checkpoint.SetAsSecondaryRpc();
            }
        } else if (previousCheckpointNumber == newCheckpoint.GetCheckpointNumber()) {
            previousCheckpoint.DeactivateRpc();
        }

        currentCheckpoint = newCheckpoint;
    }
}

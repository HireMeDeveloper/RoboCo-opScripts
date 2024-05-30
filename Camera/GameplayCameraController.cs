using System.Collections.Generic;
using UnityEngine;

public class GameplayCameraController : MonoBehaviour, ICameraResetable {
    [SerializeField] private List<Transform> players; // Array of player transforms
    //public Transform target;

    [SerializeField] private float minSize = 5f; // Minimum size of the camera orthographic size
    [SerializeField] private float maxSize = 10f; // Maximum size of the camera orthographic size
    [SerializeField] private float padding = 1f; // Padding around the players
    [SerializeField] private float smoothing = 3f;

    private Vector3 defaultCameraPosition = new Vector3(0.0f, 0.0f, -10.0f);

    private void Update() {
        // Iterate through all players
        for (int i = 0; i < players.Count; i++) {
            // Check if the transform is attached to a null object
            if (players[i] == null) {
                // If attached to a null object, remove the player
                RemovePlayer(i);
            }
        }
    }

    private void LateUpdate() {
        Bounds bounds = CalculateBounds();

        Vector3 center = bounds.center;
        center.z = transform.position.z; // Maintain the same z position

        if (players.Count == 0) {
            // Smoothly move camera towards the calculated center point
            transform.position = Vector3.Lerp(transform.position, defaultCameraPosition, Time.deltaTime * smoothing); ;

            // Calculate the required orthographic size
            float size = Mathf.Clamp(Mathf.Max(bounds.size.x / 2, bounds.size.y / 2) + padding, minSize, maxSize);
            Camera.main.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize, size, Time.deltaTime * smoothing);
        } else {
            // Smoothly move camera towards the calculated center point
            transform.position = Vector3.Lerp(transform.position, center, Time.deltaTime * smoothing);

            // Calculate the required orthographic size
            float size = Mathf.Clamp(Mathf.Max(bounds.size.x / 2, bounds.size.y / 2) + padding, minSize, maxSize);
            Camera.main.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize, size, Time.deltaTime * smoothing);
        }
    }

    public void ResetCamera() {

    }

    private void FixedUpdate() {

    }

    void RemovePlayer(int index) {
        // Remove the player at the given index from the list
        players.RemoveAt(index);
    }

    private Bounds CalculateBounds() {
        if (players.Count == 0) {
            return new Bounds(defaultCameraPosition, Vector3.zero);
        } else if (players.Count == 1) {
            return new Bounds(players[0].position, Vector3.zero);
        }

        Bounds bounds = new Bounds(players[0].position, Vector3.zero);
        for (int i = 1; i < players.Count; i++) {
            bounds.Encapsulate(players[i].position);
        }
        return bounds;
    }

    public void AddPlayer(Transform playerTransform) {
        players.Add(playerTransform);
    }

    public void RemovePlayer(Transform playerTransform) {
        players.Remove(playerTransform);
    }

}

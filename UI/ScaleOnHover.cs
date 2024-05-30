using UnityEngine;

public class ScaleOnHover : MonoBehaviour {
    [SerializeField] private float scaleAmount;
    public void OnHoverEnter() {
        var currentScale = transform.localScale;
        transform.localScale = new Vector3(currentScale.x * scaleAmount, currentScale.y * scaleAmount, 1.0f);
    }

    public void OnHoverExit() {
        var currentScale = transform.localScale;
        transform.localScale = new Vector3(currentScale.x / scaleAmount, currentScale.y / scaleAmount, 1.0f);
    }
}

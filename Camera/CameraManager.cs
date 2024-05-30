public class CameraManager : SingletonBehaviour<CameraManager> {
    private GameplayCameraController gameplayCameraController;
    private EditorCameraController editorCameraController;
    private void Awake() {
        base.Awake();
        gameplayCameraController = GetComponent<GameplayCameraController>();
        editorCameraController = GetComponent<EditorCameraController>();
    }
    public void SetCameraMode(bool isGameplay) {
        if (isGameplay) {
            gameplayCameraController.enabled = true;
            editorCameraController.enabled = false;

            gameplayCameraController.ResetCamera();
        } else {
            gameplayCameraController.enabled = false;
            editorCameraController.enabled = true;

            editorCameraController.ResetCamera();
        }
    }
}

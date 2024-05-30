using UnityEngine;

public class PauseMenu : MonoBehaviour {
    [SerializeField] private KeyCode pauseButton;
    private UIGroup lastUIGroup;

    private void Update() {
        if (Input.GetKeyDown(pauseButton)) {
            TogglePause();
        }
    }

    public void TogglePause() {
        var uiManager = UIManager.instance;

        if (uiManager.IsUIGroupActive(UIGroup.PAUSE)) {
            uiManager.ShowUIGroup(lastUIGroup);
        } else {
            lastUIGroup = uiManager.GetActiveGroup();
            uiManager.ShowUIGroup(UIGroup.PAUSE);
        }
    }
}

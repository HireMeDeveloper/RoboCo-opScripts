using UnityEngine;

public class ColorSelectionButton : MonoBehaviour {
    [SerializeField] private BlockColor colorSelection;
    [SerializeField] private GameObject selectionIndicator;

    private void Update() {
        var currentColor = LevelEditorGridManager.instance.GetCurrentColorSelection();
        selectionIndicator.SetActive(currentColor == colorSelection);
    }

    public void SetColorSelection() {
        var gridManager = LevelEditorGridManager.instance;

        gridManager.SetCurrentColorSelection(colorSelection);
    }
}

using UnityEngine;
using UnityEngine.UI;

public class ToolSelectionButton : MonoBehaviour {
    [SerializeField] private LevelEditorTool tool;

    [SerializeField] private Image thumbnailImage;
    [SerializeField] private Sprite thumbnailSprite;

    [SerializeField] private GameObject selectionIndicator;

    private void OnValidate() {
        thumbnailImage.sprite = thumbnailSprite;
    }

    private void Update() {
        var currentTool = LevelEditorGridManager.instance.GetCurrentEditorTool();
        selectionIndicator.SetActive(currentTool == this.tool);
    }

    public void SetCurrentTool() {
        LevelEditorGridManager.instance.SetCurrentEditorTool(this.tool);
    }
}

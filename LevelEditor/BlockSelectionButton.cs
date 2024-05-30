using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BlockSelectionButton : MonoBehaviour {
    [SerializeField] protected DrawableTile tileData;

    [SerializeField] private Image thumbnailImage;
    [SerializeField] protected GameObject selectionIndicator;

    public UnityEvent<DrawableTile> OnSelected;

    private void OnValidate() {
        UpdateThumbnail();
    }

    private void Update() {
        var currentBlock = LevelEditorGridManager.instance.GetCurrentTileData();
        if (tileData != null) {
            thumbnailImage.transform.localPosition = new Vector3(tileData.thumbnailOffset.x, tileData.thumbnailOffset.y, 0.0f);
            thumbnailImage.transform.localScale = new Vector3(tileData.thumbnailScale, tileData.thumbnailScale, 1.0f);
        }

        UpdateSelectionIndicator();
        UpdateThumbnail();
    }

    protected virtual void UpdateSelectionIndicator() {
        var currentBlock = LevelEditorGridManager.instance.GetCurrentTileData();
        if (tileData != null) {
            selectionIndicator.SetActive(currentBlock == this.tileData);
        } else {
            selectionIndicator.SetActive(false);
        }
    }

    private void UpdateThumbnail() {
        if (tileData == null) return;
        thumbnailImage.sprite = tileData.thumbnail;
    }

    public void SetCurrentBlock() {
        var gridManager = LevelEditorGridManager.instance;

        gridManager.SetCurrentTile(tileData);
        gridManager.SetCurrentEditorTool(LevelEditorTool.BRUSH);

        OnSelected.Invoke(tileData);
    }

    public void SetDataTile(DrawableTile tileData) {
        this.tileData = tileData;
    }
}

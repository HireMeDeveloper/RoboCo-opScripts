using AYellowpaper.SerializedCollections;
using UnityEngine;

public class ColorBasedBlockSelectionButton : BlockSelectionButton {
    [SerializedDictionary(keyName: "Color", valueName: "Variant")]
    public SerializedDictionary<BlockColor, DrawableTile> colorVariantDictionary = new SerializedDictionary<BlockColor, DrawableTile>();

    [SerializeField] private ColorVariantGroup colorVariantGroup;

    protected override void UpdateSelectionIndicator() {
        var gridManager = LevelEditorGridManager.instance;

        var currentColor = gridManager.GetCurrentColorSelection();
        var currentBlock = gridManager.GetCurrentTileData();

        if (colorVariantDictionary.ContainsKey(currentColor)) {
            var newBlock = colorVariantDictionary[currentColor];

            if (currentBlock.colorVariantGroup == this.colorVariantGroup) {
                if (currentBlock != newBlock && selectionIndicator.activeInHierarchy == true) {
                    gridManager.SetCurrentTile(newBlock);
                }
            }

            this.SetDataTile(newBlock);
            OnSelected.Invoke(this.tileData);
        }

        if (currentBlock.colorVariantGroup == this.colorVariantGroup) {
            selectionIndicator.SetActive(true);
        } else {
            selectionIndicator.SetActive(false);
        }
    }
}

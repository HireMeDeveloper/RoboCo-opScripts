using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BlockSelectionWatcher : MonoBehaviour {
    [SerializeField] private BlockSelectionButton previewButton;

    private List<BlockSelectionButton> blockSelectionButtons = new List<BlockSelectionButton>();

    private void Awake() {
        blockSelectionButtons = GetComponentsInChildren<BlockSelectionButton>().ToList();

        foreach (var button in blockSelectionButtons) {
            button.OnSelected.AddListener((newData) => previewButton.SetDataTile(newData));
        }
    }

    private void OnDestroy() {
        foreach (var button in blockSelectionButtons) {
            button.OnSelected.RemoveListener((newData) => previewButton.SetDataTile(newData));
        }
    }
}

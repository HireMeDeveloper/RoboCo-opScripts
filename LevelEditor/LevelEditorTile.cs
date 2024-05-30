using Fusion;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class LevelEditorTile : MonoBehaviour {

    private Dictionary<TilemapLayer, GameObject> currentTileObjects = new Dictionary<TilemapLayer, GameObject>();
    private Dictionary<TilemapLayer, NetworkObject> currentNetworkObjects = new Dictionary<TilemapLayer, NetworkObject>();

    LevelEditorGridManager gridManager;
    private int xPostion;
    private int yPostion;

    public void Init(LevelEditorGridManager gridManager, int xPosition, int yPosition) {
        this.gridManager = gridManager;
        this.xPostion = xPosition;
        this.yPostion = yPosition;
    }

    private void OnMouseDown() {
        if (IsPointerOverUIElement()) return;
        TriggerTool();
    }

    private void OnMouseEnter() {
        if (IsPointerOverUIElement()) return;

        var isMouseDown = Input.GetMouseButton(0);

        Debug.Log("Entered with mouse down: " + isMouseDown);
        if (isMouseDown) TriggerTool();
    }

    private void OnMouseExit() {

    }

    public bool IsPointerOverUIElement() {
        // Create a pointer event data with the current mouse position
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;

        // Create a list to hold all the raycast results
        List<RaycastResult> results = new List<RaycastResult>();

        // Perform the raycast using the EventSystem's current raycaster(s)
        EventSystem.current.RaycastAll(eventData, results);

        // Return true if there are any results, indicating the mouse is over a UI element
        return results.Count > 0;
    }

    private void TriggerTool() {
        Debug.Log("Mouse down");
        var currentTool = gridManager.GetCurrentEditorTool();

        switch (currentTool) {
            case LevelEditorTool.BRUSH:
                DrawTile();
                break;
            case LevelEditorTool.ERASER_FULL:
                RemoveTile(TilemapLayer.BACKGROUND);
                RemoveTile(TilemapLayer.MAIN);
                RemoveTile(TilemapLayer.FOREGROUND);
                break;
            case LevelEditorTool.ERASER_MAIN:
                RemoveTile(TilemapLayer.MAIN);
                break;
            case LevelEditorTool.ERASER_FOREGROUND:
                RemoveTile(TilemapLayer.FOREGROUND);
                break;
            case LevelEditorTool.ERASER_BACKGROUND:
                RemoveTile(TilemapLayer.BACKGROUND);
                break;
        }
    }

    private void DrawTile() {
        var currentBlock = gridManager.GetCurrentTileData();

        RemoveTile(currentBlock.tileLayer);

        switch (currentBlock.tileCategory) {
            case TileCategory.TILE:
                gridManager.DrawTile(xPostion, yPostion);
                break;
            case TileCategory.GAMEOBJECT:
                var tileObject = gridManager.DrawGameobject(xPostion, yPostion);
                currentTileObjects.Add(currentBlock.tileLayer, tileObject);
                break;
            case TileCategory.NETWORKOBJECT:
                var networkTile = gridManager.DrawNetworkObject(xPostion, yPostion);
                currentNetworkObjects.Add(currentBlock.tileLayer, networkTile);
                break;
        }
    }

    public void RemoveTile(TilemapLayer layer) {
        // Erase base tiles first
        gridManager.EraseTile(layer, xPostion, yPostion);

        // Then remove gameobject tiles
        if (currentTileObjects.ContainsKey(layer)) {
            var tileObject = currentTileObjects[layer];

            Destroy(tileObject);
            currentTileObjects.Remove(layer);
        }

        // Then despawn network object tiles
        if (currentNetworkObjects.ContainsKey(layer)) {
            var networkObject = currentNetworkObjects[layer];
            var roomManager = RoomManager.instance;

            roomManager.DespawnObject(networkObject);
            currentNetworkObjects.Remove(layer);

            Debug.Log("Despawned");
        }
    }
}

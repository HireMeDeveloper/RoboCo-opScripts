using Fusion;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelEditorGridManager : SingletonBehaviour<LevelEditorGridManager> {
    [SerializeField] private float tileSpread = 1.0f;
    [SerializeField] private Transform tileButtonParent;
    [SerializeField] private LevelEditorTile tilePrefab;
    [Space]
    [SerializeField] private Tilemap backdropTilemap;
    [SerializeField] private Tilemap backgroundTilemap;
    [SerializeField] private Tilemap mainTilemap;
    [SerializeField] private Tilemap foregroundTilemap;

    private Dictionary<TilemapLayer, Tilemap> tilemapDictionary = new Dictionary<TilemapLayer, Tilemap>();
    [Space]
    [SerializeField] private TileBase borderTile;
    [Space]
    [SerializeField] private DrawableTile currentTile;
    [SerializeField] private LevelEditorTool currentEditorTool = LevelEditorTool.BRUSH;
    [SerializeField] private BlockColor currentColorSelection = BlockColor.WHITE;

    private int borderThickness = 2;

    [SerializeField] private int height = 30;
    [SerializeField] private int width = 30;

    private void Awake() {
        base.Awake();

        tilemapDictionary.Add(TilemapLayer.BACKDROP, backgroundTilemap);
        tilemapDictionary.Add(TilemapLayer.BACKGROUND, backgroundTilemap);
        tilemapDictionary.Add(TilemapLayer.MAIN, mainTilemap);
        tilemapDictionary.Add(TilemapLayer.FOREGROUND, foregroundTilemap);
    }

    private void Start() {

    }

    public void SetCurrentTile(DrawableTile tile) {
        this.currentTile = tile;
    }

    public void SetCurrentEditorTool(LevelEditorTool editorTool) {
        this.currentEditorTool = editorTool;
    }

    public void SetCurrentColorSelection(BlockColor colorSelection) {
        this.currentColorSelection = colorSelection;
    }

    public void CreateTemplate() {
        SetDiminsions(width, height);
        CenterCamera();
    }

    public void SetDiminsions(int height, int width) {
        this.height = height;
        this.width = width;

        DrawGrid();
    }

    private void CenterCamera() {
        var camera = Camera.main;

        camera.transform.position = new Vector3(((float)width / 2) + (tileSpread / 2), ((float)height / 2) + (tileSpread / 2), -10.0f);
    }

    private void DrawGrid() {
        for (int x = 0 - borderThickness; x < width + borderThickness; x++) {
            for (int y = 0 - borderThickness; y < height + borderThickness; y++) {
                if (x < 0 || x >= width || y < 0 || y >= height) {
                    mainTilemap.SetTile(new Vector3Int(x, y, 0), borderTile);
                } else {
                    var gridTile = Instantiate(tilePrefab, new Vector3(x * tileSpread, y * tileSpread, -1.0f), Quaternion.identity, tileButtonParent);
                    gridTile.Init(this, x, y);
                    gridTile.name = "GridTile(" + x + ", " + y + ")";
                }
            }
        }
    }

    private Vector3 GridPointToWorld(int x, int y) {
        return new Vector3((x * tileSpread) + (tileSpread / 2), (y * tileSpread) + (tileSpread / 2), 0.0f);

    }

    public void DrawTile(int x, int y) {
        var position = new Vector3Int(x, y, 0);
        var tileBase = currentTile.tileBase;
        var layer = tilemapDictionary[currentTile.tileLayer];

        layer.SetTile(position, tileBase);
    }

    public GameObject DrawGameobject(int x, int y) {
        var tileObject = currentTile.prefab;
        var layer = tilemapDictionary[currentTile.tileLayer];
        var position = GridPointToWorld(x, y);

        return Instantiate(tileObject, position, Quaternion.identity, layer.transform);
    }

    public NetworkObject DrawNetworkObject(int x, int y) {
        var roomManager = RoomManager.instance;

        var networkObject = currentTile.networkPrefab;
        var layer = tilemapDictionary[currentTile.tileLayer];
        var position = GridPointToWorld(x, y);

        return roomManager.SpawnObject(networkObject, position, layer.transform);
    }

    public void EraseTile(TilemapLayer layer, int x, int y) {
        var position = new Vector3Int(x, y, 0);
        var tilemap = tilemapDictionary[layer];
        tilemap.SetTile(position, null);
    }

    public DrawableTile GetCurrentTileData() {
        return this.currentTile;
    }

    public LevelEditorTool GetCurrentEditorTool() {
        return this.currentEditorTool;
    }

    public BlockColor GetCurrentColorSelection() {
        return this.currentColorSelection;
    }
}

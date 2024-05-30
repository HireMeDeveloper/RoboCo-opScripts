using Fusion;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelFormatter : NetworkSingleton<LevelFormatter> {
    [SerializeField] private string levelName;
    [SerializeField] private string creatorName;
    [Space]
    [SerializeField] private Tilemap backdropTilemap;
    [SerializeField] private Tilemap backgroundTilemap;
    [SerializeField] private Tilemap mainTilemap;
    [SerializeField] private Tilemap foregroundTilemap;
    [Space]
    [SerializeField] private List<TileBase> tiles = new List<TileBase>();
    [SerializeField] private List<GameObject> tileObjects = new List<GameObject>();
    [Space]
    [SerializeField] private List<TextAsset> levelFiles = new List<TextAsset>();

    #region Exporting
    public void ExportCurrentLevelJSON() {
        // Convert the current level to a BlockMap object
        BlockMap level = GetLevelObject();

        // Only save if the level has a name
        if (String.IsNullOrEmpty(level.name)) {
            Debug.LogWarning("There name was empty, so it was not saved");
            return;
        }

        // Serialize the BlockMap to a JSON string
        string json = JsonUtility.ToJson(level);

        // Create a file path to the export folder within the project
        string relativeFilePath = "/FileExport/" + level.name + ".json";
        string absoluteFilePath = Application.dataPath + relativeFilePath;

        // Write the JSON string to a file
        File.WriteAllText(absoluteFilePath, json);
        Debug.Log("Data exported to JSON file: " + absoluteFilePath);
    }

    public BlockMap GetLevelObject() {
        // Create a new BlockMap object
        BlockMap blockMap = new BlockMap();

        // Format and add the name and creator for the BlockMap
        var formatedName = levelName.Trim().ToLower();
        blockMap.name = formatedName;
        blockMap.creator = creatorName;

        // Create a list of BlockLayer objects that will make up the BlockMap
        List<BlockLayerInfo> blockLayers = new List<BlockLayerInfo>();

        // Convert the backdrop layer into a BlockLayer object and add it to the list
        BlockLayerInfo backdrop = GetLayerObject(backdropTilemap, TilemapLayer.BACKDROP);
        blockLayers.Add(backdrop);

        // Convert the backdrop layer into a BlockLayer object and add it to the list
        BlockLayerInfo backgroundJSON = GetLayerObject(backgroundTilemap, TilemapLayer.BACKGROUND);
        blockLayers.Add(backgroundJSON);

        // Convert the backdrop layer into a BlockLayer object and add it to the list
        BlockLayerInfo mainJSON = GetLayerObject(mainTilemap, TilemapLayer.MAIN);
        blockLayers.Add(mainJSON);

        // Convert the backdrop layer into a BlockLayer object and add it to the list
        BlockLayerInfo foregroundJSON = GetLayerObject(foregroundTilemap, TilemapLayer.FOREGROUND);
        blockLayers.Add(foregroundJSON);

        // Add the layers to the BlockMap
        blockMap.layers = blockLayers.ToArray();

        return blockMap;
    }

    private BlockLayerInfo GetLayerObject(Tilemap tilemap, TilemapLayer layer) {
        // Create a new BlockLayer object
        BlockLayerInfo blockLayer = new BlockLayerInfo();

        // Set the current layer in the BlockLayer object
        blockLayer.tileLayer = layer;

        // Create a list of BlockInfo objects that will make up the current BlockLayer
        List<BlockInfo> blockList = new List<BlockInfo>();

        // Get the bounds of the tilemap
        BoundsInt bounds = tilemap.cellBounds;

        // Iterate over each cell in the tilemap and get the tiles drawn to each position and add them to the blockList
        foreach (Vector3Int position in bounds.allPositionsWithin) {
            // Get the tile at the current position
            TileBase tile = tilemap.GetTile(position);

            // Continue if there are no tiles at the given position
            if (tile == null) continue;

            // Create BlockInfo object for the current tile
            BlockInfo blockInfo = new BlockInfo();

            // Set the position to the BlockInfo
            blockInfo.position = new Vector2Int(position.x, position.y);

            // Set the category as TILE since this object will be draw to a tilemap
            blockInfo.TileCategory = TileCategory.TILE;

            // Assign an index and name to the BlockInfo
            blockInfo.index = GetTileIndex(tile);
            blockInfo.name = tile.name;

            // Add the BlockInfo to the blockList
            blockList.Add(blockInfo);
        }

        // Iterate all the NetworkBlocks in the current layer and get a list of the gameobjects
        List<GameObject> networkBlocks = FindObjectsOfType<NetworkBlock>()
            .Where(networkBlock => networkBlock.GetTileMapLayer() == layer)
            .Select((networkBlock) => networkBlock.gameObject)
            .ToList();

        // Iterate over all the (local) Blocks in the current layer and get a list of the gameobjects
        List<GameObject> blocks = FindObjectsOfType<Block>()
            .Where(block => block.GetTileMapLayer() == layer)
            .Select((block) => block.gameObject)
            .ToList();

        // Combine the lists
        List<GameObject> tileObjects = new List<GameObject>();
        tileObjects.AddRange(networkBlocks);
        tileObjects.AddRange(blocks);

        // Iterate over all of the tileObjects found in the current layer
        foreach (var tileObject in tileObjects) {

            // Skip tileObjects that are tagged to be ignored
            if (tileObject.tag == "Ignored") continue;

            // Create a BlockInfo for the current tile object
            BlockInfo blockInfo = new BlockInfo();

            // Set the position to the BlockInfo
            blockInfo.position = new Vector2(tileObject.transform.position.x, tileObject.transform.position.y);

            // Set the category as GAMEOBJECT since this tile will be instatiated within the scene
            blockInfo.TileCategory = TileCategory.GAMEOBJECT;

            // Assign an index and name to the blockInfo
            blockInfo.index = GetIndexFromTileObject(tileObject);
            blockInfo.name = tileObject.name;

            // Add the tile object to the blockList
            blockList.Add(blockInfo);
        }

        // Add all the block to the BlockLayer
        blockLayer.blocks = blockList.ToArray();

        return blockLayer;
    }
    #endregion
    #region Importing
    public void ImportLevel(LevelName levelName) {
        // Load the local blocks for the level on each client
        LoadLevelLocallyRpc(levelName);

        // Load the networked blocks for the level on the host client
        LoadNetworkLevelRpc(levelName);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void LoadLevelLocallyRpc(LevelName levelName) {
        // Called on all clients
        LoadLocalLevel(levelName);
    }

    #endregion
    #region Loading Levels
    public void LoadLocalLevel(LevelName levelName) {
        // Retreive the level json for the given levelName
        string json = levelFiles[(int)levelName].text;

        // Convert the json back into a BlockMap object
        BlockMap blockMap = JsonUtility.FromJson<BlockMap>(json);

        // Get the layers from the blockMap
        BlockLayerInfo[] blockLayers = blockMap.layers;

        // Clear all of the local layers
        foreach (var layer in blockLayers) {
            ClearLocalTilemap(layer.tileLayer);
        }

        // Load all the local layers from the blockMap
        foreach (var layer in blockLayers) {
            LoadLocalTilemap(layer);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void LoadNetworkLevelRpc(LevelName levelName) {
        // Only call this on the host client
        if (!RoomManager.instance.isLocalPlayerHost()) return;
        LoadNetworkLevel(levelName);
    }
    private void LoadNetworkLevel(LevelName levelName) {
        // Retreive the level json for the given levelName
        string json = levelFiles[(int)levelName].text;

        // Convert the json back into a BlockMap object
        BlockMap blockMap = JsonUtility.FromJson<BlockMap>(json);

        // Get the layers from the blockMap
        var blockLayers = blockMap.layers;

        // Clear all of the network layers
        foreach (var layer in blockLayers) {
            ClearNetworkTilemap(layer.tileLayer);
        }

        // Load all of the network layers
        foreach (var layer in blockLayers) {
            LoadNetworkTilemap(layer);
        }
    }

    #endregion
    #region Clearing Tilemaps
    private void ClearLocalTilemap(TilemapLayer layer) {
        // Get the tileMap for the layer
        Tilemap currentTilemap = GetTilemapFromLayer(layer);

        // Clear all of the tiles drawn to the currentTilemap
        currentTilemap.ClearAllTiles();

        // Iterate over all the (local) Blocks in the current layer and get a list of the gameobjects
        List<GameObject> tileObjects = FindObjectsOfType<Block>()
            .Where(block => block.GetTileMapLayer() == layer)
            .Select((block) => block.gameObject)
            .ToList();

        // Iterate over all of the tileObjects and destroy only the local ones
        foreach (var tileObject in tileObjects) {
            // Skip any tileObjects with a NetworkObject components, as they should only be destroyed by the host client
            var networkObject = tileObject.GetComponent<NetworkObject>();
            if (networkObject != null) continue;

            // Destroy the local tileObject
            Destroy(tileObject);
        }
    }

    private void ClearNetworkTilemap(TilemapLayer layer) {
        // Iterate all the NetworkBlocks in the current layer and get a list of the gameobjects
        List<GameObject> networkBlocks = FindObjectsOfType<NetworkBlock>()
            .Where(networkBlock => networkBlock.GetTileMapLayer() == layer)
            .Select((networkBlock) => networkBlock.gameObject)
            .ToList();

        // Iterate and despawn all of the network objects
        foreach (var networkBlock in networkBlocks) {
            // Skip any tileObjects that do not have a networkObject componenet
            var networkObject = networkBlock.GetComponent<NetworkObject>();
            if (networkObject == null) continue;

            // Despawn the networkObject using the networkObject component if it exists
            RoomManager.instance.DespawnObject(networkObject);
        }
    }
    #endregion
    #region Loading Tilemaps
    private void LoadLocalTilemap(BlockLayerInfo blockLayer) {
        // Get the layer from the BlockLayer
        TilemapLayer layer = blockLayer.tileLayer;

        // Get the tilemap from the current layer
        Tilemap currentTilemap = GetTilemapFromLayer(layer);

        // Iterate over the blocks found in the BlockLayer and load them
        foreach (var blockInfo in blockLayer.blocks) {
            // Get the JSON for the current blockInfo
            string json = JsonUtility.ToJson(blockInfo);

            // Load the local block into the level
            LoadLocalBlock(layer, json);
        }
    }

    private void LoadNetworkTilemap(BlockLayerInfo blockLayer) {
        // Get the layer from the BlockLayer
        TilemapLayer layer = blockLayer.tileLayer;

        // Get the tilemap from the current layer
        Tilemap currentTilemap = GetTilemapFromLayer(layer);

        // Iterate over the blocks found in the BlockLayer and load them
        foreach (var block in blockLayer.blocks) {
            // Get the JSON for the current blockInfo
            string json = JsonUtility.ToJson(block);

            // Load the local block into the level
            LoadNetworkBlock(layer, json);
        }
    }
    #endregion
    #region Loading Blocks
    private void LoadLocalBlock(TilemapLayer layer, string BlockInfoJSON) {
        // Get the BlockInfo from the JSON
        BlockInfo blockInfo = JsonUtility.FromJson<BlockInfo>(BlockInfoJSON);

        // Get the tilemap from the current layer
        Tilemap currentTilemap = GetTilemapFromLayer(layer);

        // If the block is considered a tile, then draw it to a tilemap, otherwise instatiate it
        if (blockInfo.TileCategory == TileCategory.TILE) {
            // Get the position to draw the tile
            Vector3Int position = new Vector3Int((int)blockInfo.position.x, (int)blockInfo.position.y, 0);

            // Get the tileBase object to use
            TileBase tile = tiles[blockInfo.index];

            // Draw the tile to the tilemap at the given position
            currentTilemap.SetTile(position, tile);
        } else {
            // Get the tile object prefab at the given index
            int index = GetIndexFromBlockInfo(blockInfo);
            GameObject tilePrefab = tileObjects[index];

            // Check to see if the gameobject has a NetworkObject component, if so skip 
            var networkObject = tilePrefab.GetComponent<NetworkObject>();
            if (networkObject != null) {
                return;
            } else {
                // Get the position to place the tileObject
                Vector2 position = blockInfo.position;

                // Instatiate the tileObject
                Instantiate(tilePrefab.gameObject, position, Quaternion.identity, currentTilemap.transform);
            }
        }
    }

    private void LoadNetworkBlock(TilemapLayer layer, string blockInfoJSON) {
        // Get the BlockInfo from the JSON
        BlockInfo blockInfo = JsonUtility.FromJson<BlockInfo>(blockInfoJSON);

        // Get the tilemap from the current layer
        Tilemap currentTilemap = GetTilemapFromLayer(layer);

        // Tiles cannot be network objects, so they are skipped
        if (blockInfo.TileCategory == TileCategory.TILE) return;

        // Get the tile object prefab at the given index
        int index = GetIndexFromBlockInfo(blockInfo);
        GameObject tileObject = tileObjects[index];

        // Check to see if the gameobject has a NetworkObject component, is so spawn it
        var networkObject = tileObject.GetComponent<NetworkObject>();
        if (networkObject != null) {
            // Get the position to place the tileObject
            var position = blockInfo.position;

            // Spawn the tileObject using the networkObject component
            RoomManager.instance.SpawnObject(networkObject, position, currentTilemap.transform);
        }
    }
    #endregion
    #region Utilties
    private Tilemap GetTilemapFromLayer(TilemapLayer layer) {
        // Return the tilemap that for the given layer
        switch (layer) {
            case TilemapLayer.BACKDROP:
                return backdropTilemap;
            case TilemapLayer.BACKGROUND:
                return backgroundTilemap;
            case TilemapLayer.MAIN:
                return mainTilemap;
            case TilemapLayer.FOREGROUND:
                return foregroundTilemap;
            default:
                return null;
        }
    }

    private int GetTileIndex(TileBase tileBase) {
        // Retrun the index from the tiles list
        var match = tiles.FindIndex(tileResource => tileResource == tileBase);
        return match;
    }

    private int GetIndexFromTileObject(GameObject tileObject) {
        // Return the index from the tileObjects list
        var firstMatch = tileObjects.FindIndex(original => tileObject.name == original.name || tileObject.name == original.name + "(Clone)");
        return firstMatch;
    }

    private int GetIndexFromBlockInfo(BlockInfo blockInfo) {
        // Return the index from the tileObjects list
        var firstMatch = tileObjects.FindIndex(original => blockInfo.name == original.name || blockInfo.name == original.name + "(Clone)");
        return firstMatch;
    }
    #endregion
}

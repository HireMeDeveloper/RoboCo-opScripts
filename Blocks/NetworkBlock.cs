using Fusion;
using UnityEngine;


public class NetworkBlock : NetworkBehaviour {
    [Header("NetworkBlock")]
    [SerializeField] private TilemapLayer tileLayer;
    public TilemapLayer GetTileMapLayer() {
        return this.tileLayer;
    }
}

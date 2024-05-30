using UnityEngine;

public class Block : MonoBehaviour {
    [Header("Block")]
    [SerializeField] private TilemapLayer tileLayer;
    public TilemapLayer GetTileMapLayer() {
        return this.tileLayer;
    }
}

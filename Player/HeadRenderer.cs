using Fusion;
using UnityEngine;

public class HeadRenderer : NetworkBehaviour, ISpawned, IDespawned {
    [SerializeField] private HeadData headData;
    [SerializeField] private SpriteRenderer headRenderer;
    [SerializeField] private float lifeTime = 10.0f;

    [Networked] private float remainingLifetime { get; set; }

    private Color defaultColor;
    private NetworkObject networkObject;
    private GameplayCameraController cameraController;

    private bool isGone = false;

    public override void Spawned() {
        networkObject = GetComponent<NetworkObject>();

        cameraController = Camera.main.GetComponent<GameplayCameraController>();
        if (cameraController != null) cameraController.AddPlayer(headRenderer.gameObject.transform);

        defaultColor = headRenderer.color;
    }

    public override void Despawned(NetworkRunner runner, bool hasState) {
        cameraController.RemovePlayer(headRenderer.gameObject.transform);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void InitializeRpc(int headIndex, float lifeTime) {
        var data = CosmeticsManager.instance.GetHead(headIndex);

        headData = data;
        headRenderer.sprite = headData.sprite;

        this.lifeTime = lifeTime;
        remainingLifetime = this.lifeTime;
    }

    public override void FixedUpdateNetwork() {
        remainingLifetime -= Runner.DeltaTime;

        UpdateAlpha();

        if (remainingLifetime <= 0.0f && !isGone) {
            isGone = true;
            RoomManager.instance.DespawnObject(networkObject);
        }
    }

    private void UpdateAlpha() {
        var newColor = defaultColor;
        newColor.a = remainingLifetime / lifeTime;

        headRenderer.color = newColor;
    }
}

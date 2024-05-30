using Fusion;
using UnityEngine;

public class ExitBlock : NetworkBlock {

    private Animator anim;
    private int currentCoins = 0;

    private void Awake() {
        anim = GetComponent<Animator>();
    }
    private void OnTriggerEnter2D(Collider2D collision) {
        var player = collision.GetComponent<PlayerPhysics2D>();
        if (player == null) return;
        if (player.tag != "Player") return;

        if (currentCoins >= 3) OnUse();
    }

    private void OnUse() {
        var levelLoader = NetworkLevelLoader.instance;

        var hostLevel = levelLoader.GetCurrentHostLevel();
        LocalDataManager.instance.SetLevelValue(hostLevel, 1);

        levelLoader.LoadLobby();
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void AddCoinRpc() {
        Debug.Log("Coin was collected");
        currentCoins++;
        currentCoins = Mathf.Clamp(currentCoins, 0, 3);

        anim.SetInteger("coins", currentCoins);
    }
}

public class GoldCoinCollectable : Collectable {
    protected override void OnCollect() {
        LevelManager.instance.CollectGoldCoin();
    }
}

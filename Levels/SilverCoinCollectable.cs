public class SilverCoinCollectable : Collectable {
    protected override void OnCollect() {
        LevelManager.instance.CollectSilverCoin();
    }
}

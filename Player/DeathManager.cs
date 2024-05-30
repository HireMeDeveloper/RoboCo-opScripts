using Fusion;

public class DeathManager : NetworkBehaviour {
    public void KillPlayer() {
        var roomManager = RoomManager.instance;
        roomManager.KillLocalPlayer(this.gameObject);
    }
}

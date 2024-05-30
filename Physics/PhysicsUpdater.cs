using Fusion;
using UnityEngine;

public class PhysicsUpdater : NetworkBehaviour {
    public override void FixedUpdateNetwork() {
        Physics2D.Simulate(Runner.DeltaTime);
    }
}

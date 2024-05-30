using Fusion;
using UnityEngine;

public class PlayerParticleSystem : NetworkBehaviour {
    [SerializeField] private ParticleSystem _dustParticleSystem;
    [SerializeField] private ParticleSystem _landParticleSystem;
    [SerializeField] private ParticleSystem _fireBounceParticleSystem;


    private float dustInterval = 0.15f;
    private float remainingDustInterval = 0.15f;

    private bool _isWalking;
    public bool isWalking {
        get {
            return _isWalking;
        }
        set {
            if (_isWalking != value) {
                remainingDustInterval = (value) ? 0 : dustInterval;
            }

            _isWalking = value;
        }
    }

    private void Start() {
        isWalking = false;
    }

    private void Update() {
        if (isWalking) {
            if (remainingDustInterval > 0) {
                remainingDustInterval -= Time.deltaTime;
            } else {
                remainingDustInterval = dustInterval;
                SpawnDustParticleRpc();
            }
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void SpawnDustParticleRpc() {
        _dustParticleSystem.Play();
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void SpawnLandParticleRpc() {
        _landParticleSystem.Play();
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void SpawnFireBounceParticlesRpc() {
        _fireBounceParticleSystem.Play();
    }
}

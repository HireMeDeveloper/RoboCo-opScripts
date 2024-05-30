using Fusion;

public abstract class NetworkSingleton<T> : NetworkBehaviour where T : NetworkSingleton<T> {
    public static T instance;

    protected virtual void Awake() {
        if (instance == null) {
            instance = this as T;
        } else {
            Destroy(gameObject);
        }
    }
}
using UnityEngine;

public abstract class SingletonBehaviour<T> : MonoBehaviour where T : SingletonBehaviour<T> {
    public static T instance;

    protected virtual void Awake() {
        if (instance == null) {
            instance = this as T;
        } else {
            Destroy(gameObject);
        }
    }
}
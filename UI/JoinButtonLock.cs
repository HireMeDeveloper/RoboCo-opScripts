using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class JoinButtonLock : MonoBehaviour {
    private TMP_InputField inputField;

    public UnityEvent onValidInput;
    public UnityEvent onInvalidInput;

    private void Awake() {
        inputField = GetComponent<TMP_InputField>();
        inputField.onValueChanged.AddListener((args) => OnValueChanged());
    }

    private void OnValueChanged() {
        if (inputField.text.Length == 0) {
            onInvalidInput.Invoke();
        } else {
            onValidInput.Invoke();
        }
    }

    private void OnDestroy() {
        inputField.onValueChanged.RemoveListener((args) => OnValueChanged());
    }
}

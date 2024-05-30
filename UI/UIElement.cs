
using UnityEngine;

public class UIElement : MonoBehaviour {
    [SerializeField] private UIGroup group;

    public UIGroup GetUIGroup() {
        return group;
    }
}

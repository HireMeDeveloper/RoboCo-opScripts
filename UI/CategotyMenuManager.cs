using AYellowpaper.SerializedCollections;
using System;
using UnityEngine;

public class CategotyMenuManager<T> : MonoBehaviour where T : Enum {
    [SerializedDictionary(keyName: "Category", valueName: "Menu")]
    public SerializedDictionary<T, GameObject> categoryMenus = new SerializedDictionary<T, GameObject>();

    private void Awake() {
        HideAllMenus();
    }

    public void ShowMenu(T category) {
        var menu = categoryMenus[category];
        if (menu != null) {
            menu.SetActive(true);
        }
    }

    public void HideAllMenus() {
        foreach (var menu in categoryMenus.Values) {
            menu?.SetActive(false);
        }
    }
}

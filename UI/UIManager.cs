using Fusion;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UIManager : NetworkSingleton<UIManager> {

    [SerializeField] private UIGroup showOnAwake;
    [SerializeField] private List<UIElement> uiElements = new List<UIElement>();
    private UIGroup currentActiveGroup;

    [SerializeField] private Image loadingBar;

    private void Awake() {
        base.Awake();
        ShowUIGroup(showOnAwake);
    }
    public bool IsUIGroupActive(UIGroup group) {
        return group == currentActiveGroup;
    }

    public UIGroup GetActiveGroup() {
        return currentActiveGroup;
    }

    public void ShowUIGroup(UIGroup group) {
        HideAllUIGroups();
        var element = GetUIElement(group);

        element.gameObject.SetActive(true);
        currentActiveGroup = group;
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void ShowUIGroupRpc(UIGroup group) {
        if (group == UIGroup.LOADING) UpdateLoadingBarFill(0);
        ShowUIGroup(group);
    }

    private UIElement GetUIElement(UIGroup group) {
        var element = uiElements.Find(element => element.GetUIGroup() == group);
        return element;
    }

    private void HideAllUIGroups() {
        foreach (UIElement element in uiElements) {
            element.gameObject.SetActive(false);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void UpdateLoadingBarRpc(float percent) {
        UpdateLoadingBarFill(percent);
    }

    public void UpdateLoadingBarFill(float amount) {
        if (loadingBar == null) return;
        loadingBar.fillAmount = amount;
    }
}

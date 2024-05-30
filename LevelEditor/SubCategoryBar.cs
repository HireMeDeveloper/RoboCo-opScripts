using UnityEngine;

public class SubCategoryBar : MonoBehaviour {

    [SerializeField] private GameObject content;
    [SerializeField] private GameObject menu;

    private void Start() {
        //content.SetActive(false);
    }

    private void OnValidate() {

    }

    private void Update() {
        var currentRect = GetComponent<RectTransform>();
        var menuRect = menu.GetComponent<RectTransform>();

        if (menuRect != null && currentRect != null) {
            var menuPosition = menuRect.localPosition;
            var xDifference = 250.0f - currentRect.localPosition.x;

            menuRect.localPosition = new Vector3(xDifference - 250.0f, menuPosition.y, menuPosition.z);
        }
    }
}

using UnityEngine;

public class ToggleBlock : Block {
    [SerializeField] private bool startOn;
    private bool isOn;

    [SerializeField] private SpriteRenderer thumbnailRenderer;
    [SerializeField] private GameObject visibleBlock;
    [SerializeField] private GameObject hiddenBlock;

    private void Start() {
        thumbnailRenderer.enabled = false;

        if (startOn) {
            visibleBlock.SetActive(true);
            hiddenBlock.SetActive(false);
        } else {
            visibleBlock.SetActive(false);
            hiddenBlock.SetActive(true);
        }

        isOn = startOn;
    }

    private void SetBlock(bool isOn) {
        this.isOn = isOn;

        if (isOn) {
            visibleBlock.SetActive(true);
            hiddenBlock.SetActive(false);
        } else {
            visibleBlock.SetActive(false);
            hiddenBlock.SetActive(true);
        }
    }

    private void OnJump() {
        if (isOn) {
            SetBlock(false);
        } else {
            SetBlock(true);
        }
    }

    public void SubscribeToJump(PlayerPhysics2D playerPhysics) {
        playerPhysics.OnJump.AddListener(OnJump);
    }

    public void UnsubscribeToJump(PlayerPhysics2D playerPhysics) {
        playerPhysics.OnJump.RemoveListener(OnJump);
    }

}

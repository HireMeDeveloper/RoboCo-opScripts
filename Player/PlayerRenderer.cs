using Fusion;
using UnityEngine;

public class PlayerRenderer : NetworkBehaviour {

    private const float pixelUnit = 1.0f / 16.0f;

    private float defaultFaceX;
    private float flippedFaceX;

    [Header("Sprites")]
    [SerializeField] private HeadData headData;
    [SerializeField] private FaceData faceData;
    [SerializeField] private BodyData bodyData;
    [Space, Header("References")]
    [SerializeField] private SpriteRenderer _headRenderer;
    [SerializeField] private SpriteRenderer _faceRenderer;
    [SerializeField] private SpriteRenderer _bodyRenderer;
    [SerializeField] private SpriteRenderer _legsRenderer;
    [SerializeField] private GameObject _face;

    private void Start() {
        defaultFaceX = _face.transform.localPosition.x;
    }

    private void OnValidate() {
        if (headData != null) _headRenderer.sprite = headData.sprite;
        if (faceData != null) _faceRenderer.sprite = faceData.sprite;
        if (bodyData != null) _bodyRenderer.sprite = bodyData.sprite;
    }
    public void SetFlip(bool isFlipped) {
        _headRenderer.flipX = isFlipped;
        _bodyRenderer.flipX = isFlipped;
        _legsRenderer.flipX = isFlipped;

        var flippedPixelOffset = headData.faceFlipPixelAmount;
        flippedFaceX = defaultFaceX + (pixelUnit * flippedPixelOffset);
        _face.transform.localPosition = new Vector2((isFlipped) ? flippedFaceX : defaultFaceX, _face.transform.localPosition.y);
    }

    public HeadData GetHeadData() {
        return this.headData;
    }
}

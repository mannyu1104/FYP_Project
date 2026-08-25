using UnityEngine;
using System.Collections.Generic;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class LookController : MonoBehaviour
{
    [System.Serializable]
    public class LookParameters
    {
        [Min(0f)]
        public float mouseSensitivity = 1f;
        [Min(0f)]
        public float centerDeadZone = 80f;
        [Min(0f)]
        public float smoothSpeed = 8f;
        public bool invertX = false;
        public bool showDebugInfo = true;
        public bool hideInactiveSceneImages = true;
    }

    [Header("References")]
    [HideInInspector]
    public RectTransform roomImage;
    public RectTransform canvasRect;

    [Header("Scene Images")]
    public List<RectTransform> sceneImages = new List<RectTransform>();
    [Min(0)]
    public int activeSceneIndex = 0;
    
    [Header("Look Parameters")]
    public LookParameters lookParameters = new LookParameters();

    private float centerX;
    private float maxMove;
    private float targetX;
    private float debugTimer;
    private string inputSource;
    private int currentSceneIndex = -1;

    void Start()
    {
        EnsureLookParameters();

        if (canvasRect == null)
        {
            Debug.LogError("Canvas Rect is not assigned!");
            enabled = false;
            return;
        }

        if (!SelectSceneImage(activeSceneIndex))
        {
            Debug.LogError("No scene image is assigned!");
            enabled = false;
            return;
        }
    }

    public void SetSceneImage(int sceneIndex)
    {
        SelectSceneImage(sceneIndex);
    }

    public void ShowNextSceneImage()
    {
        if (sceneImages.Count == 0)
        {
            return;
        }

        SelectSceneImage((activeSceneIndex + 1) % sceneImages.Count);
    }

    public void ShowPreviousSceneImage()
    {
        if (sceneImages.Count == 0)
        {
            return;
        }

        int previousIndex = activeSceneIndex - 1;

        if (previousIndex < 0)
        {
            previousIndex = sceneImages.Count - 1;
        }

        SelectSceneImage(previousIndex);
    }

    private bool SelectSceneImage(int sceneIndex)
    {
        if (sceneImages.Count > 0)
        {
            sceneIndex = Mathf.Clamp(sceneIndex, 0, sceneImages.Count - 1);
            roomImage = sceneImages[sceneIndex];
        }

        if (roomImage == null)
        {
            return false;
        }

        activeSceneIndex = sceneIndex;
        currentSceneIndex = sceneIndex;

        SetSceneImageVisibility();

        Canvas.ForceUpdateCanvases();
        CenterRoomInView();

        centerX = roomImage.anchoredPosition.x;
        targetX = centerX;

        RecalculateMoveLimit();
        return true;
    }

    void OnValidate()
    {
        EnsureLookParameters();

        lookParameters.mouseSensitivity = Mathf.Max(0f, lookParameters.mouseSensitivity);
        lookParameters.centerDeadZone = Mathf.Max(0f, lookParameters.centerDeadZone);
        lookParameters.smoothSpeed = Mathf.Max(0f, lookParameters.smoothSpeed);
    }

    void Update()
    {
        EnsureLookParameters();

        if (sceneImages.Count > 0 && activeSceneIndex != currentSceneIndex)
        {
            SelectSceneImage(activeSceneIndex);
        }

        RecalculateMoveLimit();
        HandleMouseLook();
        MoveRoom();
    }

    private void EnsureLookParameters()
    {
        if (lookParameters == null)
        {
            lookParameters = new LookParameters();
        }
    }

    private void SetSceneImageVisibility()
    {
        if (!lookParameters.hideInactiveSceneImages)
        {
            return;
        }

        for (int i = 0; i < sceneImages.Count; i++)
        {
            if (sceneImages[i] != null)
            {
                sceneImages[i].gameObject.SetActive(i == activeSceneIndex);
            }
        }
    }

    private void HandleMouseLook()
    {
        float normalizedLook = GetMousePositionFromScreenCenter();
        float direction = lookParameters.invertX ? -1f : 1f;

        targetX = centerX - normalizedLook * maxMove * lookParameters.mouseSensitivity * direction;
        targetX = Mathf.Clamp(targetX, centerX - maxMove, centerX + maxMove);

        LogDebugInfo();
    }

    private void MoveRoom()
    {
        if (lookParameters.smoothSpeed <= 0f)
        {
            roomImage.anchoredPosition = new Vector2(targetX, roomImage.anchoredPosition.y);
            return;
        }

        float newX = Mathf.Lerp(
            roomImage.anchoredPosition.x,
            targetX,
            Time.deltaTime * lookParameters.smoothSpeed
        );

        roomImage.anchoredPosition = new Vector2(newX, roomImage.anchoredPosition.y);
    }

    private float GetMousePositionFromScreenCenter()
    {
        if (Screen.width <= 0)
        {
            return 0f;
        }

        float halfScreenWidth = Screen.width / 2f;
        float mouseOffsetFromCenter = GetMousePosition().x - halfScreenWidth;

        if (Mathf.Abs(mouseOffsetFromCenter) <= lookParameters.centerDeadZone)
        {
            return 0f;
        }

        float usableHalfWidth = Mathf.Max(1f, halfScreenWidth - lookParameters.centerDeadZone);
        float normalizedLook = (Mathf.Abs(mouseOffsetFromCenter) - lookParameters.centerDeadZone) / usableHalfWidth;
        normalizedLook *= Mathf.Sign(mouseOffsetFromCenter);

        return Mathf.Clamp(normalizedLook, -1f, 1f);
    }

    private Vector2 GetMousePosition()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            inputSource = "New Input System";
            return Mouse.current.position.ReadValue();
        }
#endif

        inputSource = "Old Input Manager";
        return Input.mousePosition;
    }

    private void RecalculateMoveLimit()
    {
        Bounds roomBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(canvasRect, roomImage);
        float canvasWidth = canvasRect.rect.width;
        float roomWidth = roomBounds.size.x;

        maxMove = Mathf.Max(0f, (roomWidth - canvasWidth) / 2f);
        targetX = Mathf.Clamp(targetX, centerX - maxMove, centerX + maxMove);
    }

    private void CenterRoomInView()
    {
        Bounds roomBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(canvasRect, roomImage);
        float offsetFromCanvasCenter = roomBounds.center.x - canvasRect.rect.center.x;

        roomImage.anchoredPosition = new Vector2(
            roomImage.anchoredPosition.x - offsetFromCanvasCenter,
            roomImage.anchoredPosition.y
        );
    }

    private void LogDebugInfo()
    {
        if (!lookParameters.showDebugInfo)
        {
            return;
        }

        debugTimer -= Time.deltaTime;

        if (debugTimer > 0f)
        {
            return;
        }

        debugTimer = 0.5f;

        Vector2 mousePosition = GetMousePosition();

        Debug.Log(
            "LookController | Input Source: " + inputSource +
            " | Mouse X: " + mousePosition.x +
            " | Screen Center X: " + (Screen.width / 2f) +
            " | Normalized Look: " + GetMousePositionFromScreenCenter() +
            " | Room X: " + roomImage.anchoredPosition.x +
            " | Target X: " + targetX +
            " | Center X: " + centerX +
            " | Max Move: " + maxMove +
            " | Room Width: " + roomImage.rect.width +
            " | Canvas Width: " + canvasRect.rect.width
        );
    }
}

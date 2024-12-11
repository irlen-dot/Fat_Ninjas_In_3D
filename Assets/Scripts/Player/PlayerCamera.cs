using UnityEngine;
using Cinemachine;

public class PlayerCamera : MonoBehaviour
{
    [Header("Camera Reference")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera;

    [Header("Mouse Settings")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private bool invertMouseX = false;
    [SerializeField] private bool lockCursor = true;

    [Header("Rotation Settings")]
    [SerializeField] private bool enableWrapAround = true;
    [SerializeField] private float minBias = -180f;
    [SerializeField] private float maxBias = 180f;
    [SerializeField] private float smoothSpeed = 5f;

    private CinemachineOrbitalTransposer orbitalTransposer;
    private float currentBias = 0f;
    private float targetBias = 0f;
    private const float deadzone = 0.001f; // Prevent tiny movements

    private void Start()
    {
        if (virtualCamera == null)
        {
            virtualCamera = GetComponent<CinemachineVirtualCamera>();
        }

        orbitalTransposer = virtualCamera.GetCinemachineComponent<CinemachineOrbitalTransposer>();

        if (orbitalTransposer == null)
        {
            Debug.LogError("No CinemachineOrbitalTransposer found on the virtual camera!");
            enabled = false;
            return;
        }

        // Initialize both biases to the current camera bias
        currentBias = orbitalTransposer.m_Heading.m_Bias;
        targetBias = currentBias;

        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void Update()
    {
        if (orbitalTransposer == null) return;

        float mouseX = Input.GetAxis("Mouse X");

        // Only update if there's significant mouse movement
        if (Mathf.Abs(mouseX) > deadzone)
        {
            float deltaRotation = mouseX * mouseSensitivity;
            if (invertMouseX) deltaRotation *= -1;

            // Update target bias
            targetBias += deltaRotation;

            if (enableWrapAround)
            {
                // Handle wrap-around
                if (targetBias > maxBias)
                {
                    targetBias = minBias + (targetBias - maxBias);
                    currentBias = targetBias;
                }
                else if (targetBias < minBias)
                {
                    targetBias = maxBias - (minBias - targetBias);
                    currentBias = targetBias;
                }
            }
            else
            {
                targetBias = Mathf.Clamp(targetBias, minBias, maxBias);
            }

            // Only interpolate if there's a significant difference
            if (Mathf.Abs(targetBias - currentBias) > deadzone)
            {
                currentBias = Mathf.Lerp(currentBias, targetBias, smoothSpeed * Time.deltaTime);
            }
            else
            {
                currentBias = targetBias;
            }

            // Apply the bias
            orbitalTransposer.m_Heading.m_Bias = currentBias;
        }
    }

    private void OnDestroy()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
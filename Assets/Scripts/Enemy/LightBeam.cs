using UnityEngine;

public class LightBeam : MonoBehaviour
{
    [Tooltip("Input angle in degrees (0-180)")]
    [Range(0, 180)]
    public float inputAngle = 45f;

    [Tooltip("Normalized output value (0-1)")]
    [SerializeField]
    private float normalizedValue;

    [Tooltip("Mesh with blend shape to control")]
    public SkinnedMeshRenderer targetMesh;

    [Tooltip("Scale multiplier for beam")]
    [SerializeField]
    private float scaleMultiplier = 0.01f; // Since base scale is 100, we use 0.01 as default

    private const int BLEND_SHAPE_INDEX = 0;

    private void Start()
    {
        if (targetMesh == null)
        {
            targetMesh = GetComponent<SkinnedMeshRenderer>();
        }
        UpdateNormalizedValue();
    }

    private void OnValidate()
    {
        UpdateNormalizedValue();
    }

    private void UpdateNormalizedValue()
    {
        inputAngle = Mathf.Clamp(inputAngle, 0f, 180f);
        normalizedValue = 1f - (inputAngle / 180f);

        if (targetMesh != null)
        {
            float blendShapeValue = normalizedValue * 100f;
            targetMesh.SetBlendShapeWeight(BLEND_SHAPE_INDEX, blendShapeValue);
        }
    }

    public void SetAngle(float newAngle)
    {
        inputAngle = newAngle;
        UpdateNormalizedValue();
    }

    public void SetLength(float radius)
    {
        float scaledSize = radius * scaleMultiplier;
        transform.localScale = new Vector3(scaledSize, scaledSize, scaledSize) * 100f;
    }

    public float GetNormalizedValue()
    {
        return normalizedValue;
    }
}
using UnityEngine;

public class PlayerFatness : MonoBehaviour
{
    [SerializeField] [Range(0, 1)] private float fatnessLevel;
    [SerializeField] [Range(0, 1)] private float addFatness;
    [SerializeField] private float maxScaleMultiplier = 2f;
    
    private Vector3 originalScale;
    private float previousFatnessLevel;

    void Start()
    {
        originalScale = transform.localScale;
        UpdateScale();
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.F))
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                RaiseFatnessLevel();
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                LowerFatnessLevel();
            }
        }
    }

    private void UpdateScale()
    {
        float scaleMultiplier = 1 + (fatnessLevel * maxScaleMultiplier);
        transform.localScale = originalScale * scaleMultiplier;
        previousFatnessLevel = fatnessLevel;
    }

    public void RaiseFatnessLevel()
    {
        fatnessLevel = Mathf.Clamp01(fatnessLevel + addFatness);
        Debug.Log($"The new fatness level is: {fatnessLevel}");
        if (fatnessLevel != previousFatnessLevel)
        {
            UpdateScale();
        }
    }

    public void LowerFatnessLevel() 
    {
        fatnessLevel = Mathf.Clamp01(fatnessLevel - addFatness);
        if (fatnessLevel != previousFatnessLevel)
        {
            UpdateScale();
        }
    }
}
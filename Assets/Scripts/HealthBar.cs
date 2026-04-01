using UnityEngine;

public class HealthBar : MonoBehaviour
{
    [SerializeField]
    private Transform barTransform;

    private Vector3 originalScale;

    private float currentHealthPercentage;
    private float targetHealthPercentage;
    private float updateSpeed = 5f;

    private void Start()
    {
        if (barTransform == null)
        {
            Debug.LogError("Bar Transform is not assigned in the HealthBar script.");
        }
        else
        {
            originalScale = barTransform.localScale;
            currentHealthPercentage = 1f;
            targetHealthPercentage = 1f;
        }
    }

    private void Update()
    {
        if (barTransform != null)
        {
            currentHealthPercentage = Mathf.Lerp(currentHealthPercentage, targetHealthPercentage, Time.deltaTime * updateSpeed);

            // Update the X scale of the bar
            barTransform.localScale = new Vector3(originalScale.x * currentHealthPercentage, originalScale.y, originalScale.z);

            // Optionally update color based on currentHealthPercentage
            SpriteRenderer barRenderer = barTransform.GetComponent<SpriteRenderer>();
            if (barRenderer != null)
            {
                if (currentHealthPercentage > 0.5f)
                {
                    barRenderer.color = Color.green;
                }
                else if (currentHealthPercentage > 0.25f)
                {
                    barRenderer.color = Color.yellow;
                }
                else
                {
                    barRenderer.color = Color.red;
                }
            }
        }
    }

    public void SetHealth(float healthPercentage)
    {
        targetHealthPercentage = Mathf.Clamp01(healthPercentage);
    }
}

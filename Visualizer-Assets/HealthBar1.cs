using UnityEngine;
using UnityEngine.UI;

public class HealthBar1 : MonoBehaviour
{
    public Slider slider;

    public Gradient gradient;

    public Image Fill;

    // Create a method that initialized the slider
    public void SetMaxHealth(int maxHealth)
    {
        slider.maxValue = maxHealth;
        slider.value = maxHealth;

        // Set the fill colour based on the gradient and slider
        Fill.color = gradient.Evaluate(1f);
    }

    public void SetHealth(int health)
    {
        slider.value = health;

        // Set the fill colour based on the gradient and slider
        Fill.color = gradient.Evaluate(slider.normalizedValue);
    }
}

using UnityEngine;
using UnityEngine.UI;

public class Shield1 : MonoBehaviour
{
    public Slider slider;


    public Image fill;

    public void SetMaxStength(int maxStrength)
    {
        slider.maxValue = maxStrength;
        slider.value = maxStrength;

    }

    public void SetStrength(int strength)
    {
        slider.value = strength;

    }

}

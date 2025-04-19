using UnityEngine;
using System.Collections;

public class GunBehaviour : MonoBehaviour
{
    [Header("Recoil Settings")]
    public Transform barrelTransform;  // Assign "Weapon_01_2" here in the Inspector
    public float recoilDistance = 0.05f; // How much the barrel moves back
    public float recoilSpeed = 5f;   // How fast it moves

    private Vector3 originalBarrelPosition; // Store the barrel's original position
    private Vector3 recoilPosition;  // The target position for recoil
    private bool isRecoiling = false;
    private bool isReturning = false;

    void Start()
    {
        if (barrelTransform != null)
        {
            originalBarrelPosition = barrelTransform.localPosition; // Save the original position
            recoilPosition = originalBarrelPosition - new Vector3(0, 0, recoilDistance); // Recoil backward
        }
        else
        {
            Debug.LogError("❌ Barrel transform not assigned! Drag Weapon_01_2 into barrelTransform.");
        }
    }

    // Call this function when shooting
    public void ApplyRecoil()
    {
        if (barrelTransform != null && !isRecoiling && !isReturning)
        {
            isRecoiling = true; // Start recoil effect
        }
    }

    void Update()
    {
        if (isRecoiling)
        {
            // Move barrel back (simulate recoil)
            barrelTransform.localPosition = Vector3.Lerp(barrelTransform.localPosition, recoilPosition, Time.deltaTime * recoilSpeed);

            // If the barrel has moved far enough, start returning
            if (Vector3.Distance(barrelTransform.localPosition, recoilPosition) < 0.001f)
            {
                isRecoiling = false;
                isReturning = true;
            }
        }
        else if (isReturning)
        {
            // Move barrel back to original position
            barrelTransform.localPosition = Vector3.Lerp(barrelTransform.localPosition, originalBarrelPosition, Time.deltaTime * recoilSpeed);

            // If the barrel is back in place, stop moving
            if (Vector3.Distance(barrelTransform.localPosition, originalBarrelPosition) < 0.001f)
            {
                isReturning = false;
            }
        }
    }
}

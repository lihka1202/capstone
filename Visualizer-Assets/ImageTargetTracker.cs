using UnityEngine;
using Vuforia;
using TMPro;
using System.Collections.Generic;
public class ImageTargetTracker : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private ObserverBehaviour observerBehaviour;
    public TextMeshProUGUI statusText;

    public GameObject bulletHitExplosionPrefab;

    public GameObject bulletHitShieldExplosionPrefab;

    public GameObject bulletMissExplosionPrefab;

    // public Camera arCamera;
    public bool isTargetVisible { get; private set; } // Public read, private write

    public Vector3 revisedCoords { get; private set; } // Public read, private write

    public int snowStormCount;

    // Create snow storm holders in the same way as before
    // public bool inSnowStormOne { get; private set; }
    // public bool inSnowStormTwo { get; private set; }

    public TextMeshProUGUI collidingCylinderText;

    public HashSet<GameObject> collidingCylinders = new HashSet<GameObject>();

    // private Vector3 initialCameraCoordinates;
    void Start()
    {
        // Set the initial coordinates
        // initialCameraCoordinates = arCamera.transform.position;
        observerBehaviour = GetComponent<ObserverBehaviour>();

        if (observerBehaviour)
        {
            observerBehaviour.OnTargetStatusChanged += OnTrackingChanged;
        }
    }

    private void OnTrackingChanged(ObserverBehaviour behaviour, TargetStatus targetStatus)
    {
        // Check if the QR Code is currently visible
        isTargetVisible = targetStatus.Status == Status.TRACKED;

        // Get the vector3 position of the QR Code

        // if (isTargetVisible)
        // {
        //     statusText.text = $"<color=#FF0000>Enemy Visible\nPosition: {transform.position.ToString("F3")} </color>";
        // }
        // else
        // {
        //     statusText.text = "<color=#0000FF>Enemy Not Visible</color>";
        // }

        // if (isTargetVisible)
        // {
        //     UpdateStatusText();
        // }

        UpdateStatusText();

        Debug.Log("Image Target Visibility: " + isTargetVisible);
    }

    void UpdateStatusText()
    {
        if (statusText != null)
        {
            Vector3 relativePosition = Camera.main.transform.InverseTransformPoint(transform.position);
            if (isTargetVisible)
            {
                if (relativePosition.z > 0 && relativePosition.z <= 2f)
                {
                    statusText.text = $"<color=#00FF00>{relativePosition.z.ToString("F3")}</color>";
                }
                else if (relativePosition.z > 2 && relativePosition.z < 3)
                {
                    statusText.text = $"<color=#FFFF00>{relativePosition.z.ToString("F3")}</color>";
                }
                else
                {
                    statusText.text = $"<color=#FF0000>{relativePosition.z.ToString("F3")}</color>";
                }
            }
            else
            {
                statusText.text = "<color=#FFFFFF>N/A</color>";
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateStatusText();

        if (!isTargetVisible)
        {
            collidingCylinderText.text = $"00";
            snowStormCount = 0;
        }
        else if (isTargetVisible)
        {
            if (collidingCylinders.Count < 10)
            {
                collidingCylinderText.text = $"0{collidingCylinders.Count}";
            }
            else
            {
                collidingCylinderText.text = $"{collidingCylinders.Count}";
            }

            snowStormCount = collidingCylinders.Count;
        }
    }

    public void SpawnHit(bool guard, string damageType)
    {
        Debug.Log("From SpawnHit, type is: " + damageType);
        if (isTargetVisible)
        {
            if (guard && damageType == "bullet")
            {
                Vector3 spawnPosition = transform.position + new Vector3(0, 0.1f, 0);
                GameObject hitEffect = Instantiate(bulletHitShieldExplosionPrefab, spawnPosition, Quaternion.identity);
                // scale it down
                hitEffect.transform.localScale *= 0.5f;
                Destroy(hitEffect, 1f); // Destroy after 1 second

            }
            else if (damageType == "bullet")
            {

                // Not guarded
                Vector3 spawnPosition = transform.position;
                GameObject hitEffect = Instantiate(bulletHitExplosionPrefab, spawnPosition, Quaternion.identity);
                // scale it down
                hitEffect.transform.localScale *= 0.5f;
                Destroy(hitEffect, 1f); // Destroy after 1 second
            }
        }
        else
        {
            if (damageType == "bullet")
            {
                Debug.Log("Bullet missing");
                // Vector3 spawnPosition = new Vector3(0, 0, 10);
                // The depth is 2f, make sure to push this further in the off chance that you need this
                // Generate random screen coordinates within visible range(avoid edges)
                float randomX = Random.Range(0.2f, 0.8f); // Random X in viewport space
                float randomY = Random.Range(0.2f, 0.8f); // Random Y in viewport space
                float depth = Random.Range(15f, 20f);   // Random Z (distance from camera)

                // Convert random screen position to world space
                Vector3 spawnPosition = Camera.main.ViewportToWorldPoint(new Vector3(randomX, randomY, depth));
                Debug.Log("This is the central viewpoint: " + spawnPosition);
                GameObject hitEffect = Instantiate(bulletMissExplosionPrefab, spawnPosition, Quaternion.identity);
                Destroy(hitEffect, 1f);
            }

        }
    }

    public Transform GetImageTargetLocation()
    {
        return transform;
    }

    // private void OnTriggerEnter(Collider other)
    // {
    //     if (other.name.Contains("Cylinder")) // Or use Tag/Layer if you prefer
    //     {
    //         Debug.Log("🌪️ Opponent entered the SnowstormOne!");
    //         inSnowStormOne = true;
    //         // collisionStatus.text = "Inside";
    //     }
    //     // if (other.name.Contains("Snowstorm"))
    //     // {
    //     //     Debug.Log("🌪️ Opponent entered the SnowstormTwo!");
    //     //     inSnowStormTwo = true;
    //     // }
    // }

    // private void OnTriggerExit(Collider other)
    // {
    //     if (other.name.Contains("Cylinder"))
    //     {
    //         Debug.Log("🏃‍♂️ Opponent exited the SnowstormOne!");
    //         inSnowStormOne = false;
    //         // collisionStatus.text = "Outside";
    //     }

    //     // if (other.name.Contains("Snowstorm"))
    //     // {
    //     //     Debug.Log("🏃‍♂️ Opponent exited the SnowstormTwo!");
    //     //     inSnowStormTwo = false;
    //     // }
    // }

    private void OnTriggerEnter(Collider other)
    {
        if (other.name.Contains("Cylinder")) // Or use Tag/Layer if you prefer
        {
            Debug.Log("🌪️ Opponent entered the Snowstorm!");
            collidingCylinders.Add(other.gameObject);
            // collisionStatus.text = "Inside";
            // collidingCylinderText.text = $"{collidingCylinders.Count}";
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.name.Contains("Cylinder"))
        {
            Debug.Log("🏃‍♂️ Opponent exited the Snowstorm!");
            collidingCylinders.Remove(other.gameObject);
            // collisionStatus.text = "Outside";
            // collidingCylinderText.text = $"{collidingCylinders.Count}";
        }
    }



}

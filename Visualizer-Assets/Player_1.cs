using System.Collections;
using System.Collections.Generic;
// using System.Numerics;
using TMPro;
// using uPLibrary.Networking.M2Mqtt;
// using uPLibrary.Networking.M2Mqtt.Messages;
// using System.Text;
// using System.Diagnostics;
using UnityEngine;
// using UnityEngine.XR.ARFoundation;
// using UnityEngine.XR.ARSubsystems;
// using UnityEngine.Rendering;
// using UnityEngine.UI;
// using System.Runtime.CompilerServices;
using Unity.Mathematics;
using Vuforia;
// using System.Linq;
// using System.Numerics;

public class Player_1 : MonoBehaviour
{
    public int maxHealth = 100;
    public int maxAmmoCount = 6;

    public int maxBombCount = 2;
    public int currentHealth;

    public int shieldMaxCount = 3;

    public int currentShieldCount;

    public HealthBar1 healthBar;

    public Shield1 shield;

    public int currentShieldStrength;

    public bool shieldStatus = false;

    // Set up the shield UI
    public GameObject shieldIconPrefab;
    public Transform shieldContainer;

    private List<GameObject> shieldIcons = new List<GameObject>();

    // Set up the ammo UI
    public GameObject ammoIconPrefab;

    public Transform ammoContainer;

    private List<GameObject> ammoIcons = new List<GameObject>();

    public int currentAmmoCount;

    // Variables for the bombs

    public int currentBombCount;

    public GameObject bombIconPrefab;

    public Transform snowBombContainer;

    public List<GameObject> snowBombs = new List<GameObject>();


    // Initalize the scoreboard
    public GameObject playerTwoScoreboard;

    // Initialize a prefab for the gun
    public GameObject gunModelPrefab;

    private GameObject spawnedGun;

    public GameObject muzzleFlashPrefab;

    private bool isReloading = false;
    private float rotationSpeed = 720f; // Degrees per second
    private float rotationAmount = 0f;

    public GameObject shieldPrefab;

    public GameObject spawnedShieldPrefab;

    private bool shieldSpawnStatus;

    public GameObject golfBall;

    private GameObject golfBallInstance;

    public ImageTargetTracker imageTargetTracker;

    private Vector3 cachedTargetPosition;

    public GameObject golfBallHitExplosionPrefab;

    public GameObject golfBallMissExplosionPrefab;

    public Player_2 player_2;

    public float controlPointHeightOffset = 0.2f;
    public float travelDuration = 1f;

    public GameObject shuttleCockPrefab;

    public GameObject shuttleCockHitExplosionPrefab;

    public GameObject boxPrefab;

    public GameObject boxHitExplosionPrefab;

    public GameObject swordPrefab;

    public GameObject swordExplosionHitPrefab;

    public GameObject snowBallPrefab;

    public GameObject snowStormParticlePrefab;

    private GameObject snowStormOne;

    private Vector3 initCameraPos1;

    private Vector3 initSnowStormPos1;

    private GameObject snowStormTwo;

    private Vector3 initCameraPos2;

    private Vector3 initSnowStormPos2;

    private Vector3 initSnowStormUpdaterTwo;
    private bool inStorm = false;

    private bool impactReceived = false;

    private AnchorBehaviour anchorBehaviour;

    public bool isInsideSnowZoneOne = false;
    public bool isInsideSnowZoneTwo = false;


    public GameObject snowHitPrefab;

    private Vector3 initCameraPosition;

    private Vector3 initSnowStormUpdater;

    // Hold the number of snowStorms

    public List<GameObject> activeCylinders = new List<GameObject>();

    public TextMeshProUGUI activeSnowstormCountText;

    public GameObject cylinderPrefabOnMe;


    // private Vector3 initCamDeltaOne;

    // private float localPosZ;

    // public PlaneFinderBehaviour planeFinder;


    // public GameObject snowBallHitExplosionPrefab;

    // Get script for the gun

    // public bool shootController;



    public void InitializePlayerOne()
    {
        // Set the camera position
        initCameraPosition = Camera.main.transform.position;
        Debug.Log("Player 1 has been initialized");
        healthBar.SetMaxHealth(maxHealth);

        // Set the current health as the max health
        currentHealth = maxHealth;

        // Set the shields and shields count
        // Shield disappear
        shield.gameObject.SetActive(false);

        currentShieldCount = shieldMaxCount;

        UpdateShieldGUI();

        // Set ammo and update GUI

        currentAmmoCount = maxAmmoCount;

        UpdateAmmoGUI();

        //TODO: Update init number of bombs
        currentBombCount = maxBombCount;

        //TODO: Update the GUI
        UpdateSnowBombGUI();

        // Initialize score board
        playerTwoScoreboard.GetComponent<TextMeshProUGUI>().text = 0.ToString();

        // Add the gun prefab
        SpawnGunPrefab();

    }

    public void TakeDamage(int damage)
    {
        // check if you get pushed to zero
        if (shieldStatus)
        {
            // negate the damage
            currentShieldStrength -= damage;

            // TODO: Add code to approximate hit
            float shieldRecoilDistance = -0.05f;
            spawnedShieldPrefab.transform.localPosition += new Vector3(0, 0, shieldRecoilDistance);

            // Reset the position
            Invoke(nameof(ResetShieldPosition), 0.05f);


            if (currentShieldStrength <= 0)
            {
                shieldStatus = false;

                // Make the shield disappear
                shield.gameObject.SetActive(false);

                // Make sure to cancel the final invoke, avoid error while despawning
                CancelInvoke(nameof(ResetShieldPosition));

                // Despawn the shield
                DespawnShieldPrefab();

                // needa subtract from player
                int playerTransferredDamage = -1 * currentShieldStrength;

                if (currentHealth - playerTransferredDamage <= 0)
                {
                    // Replace the health and change the score counter
                    Debug.Log("Shield Transference caused, health set is 0, Rebirth invoked");

                    // Update the score counter
                    playerTwoScoreboard.GetComponent<TextMeshProUGUI>().text = (
                         int.Parse(playerTwoScoreboard.GetComponent<TextMeshProUGUI>().text) + 1
                         ).ToString();

                    // Replace health
                    currentHealth = maxHealth;
                    healthBar.SetMaxHealth(currentHealth);
                    Debug.Log("Health set to" + currentHealth);

                    //Update the shieldCount and the Shield Display
                    shield.gameObject.SetActive(false);
                    currentShieldCount = shieldMaxCount;
                    Debug.Log("Shield Count set to" + currentShieldCount);

                    // Update the GUI
                    UpdateShieldGUI();

                    // Update the ammo count
                    currentAmmoCount = maxAmmoCount;

                    //Update Ammo GUI
                    UpdateAmmoGUI();

                    //TODO: Replenish SnowBombs
                    currentBombCount = maxBombCount;

                    //TODO: Update the snowbomb GUI
                    UpdateSnowBombGUI();

                }
                else
                {
                    currentHealth -= playerTransferredDamage;
                    healthBar.SetHealth(currentHealth);
                }
            }
            else
            {
                shield.SetStrength(currentShieldStrength);
            }
        }
        else
        {
            if (currentHealth - damage <= 0)
            {
                // Replace the health and change the score counter
                Debug.Log("Health reduced to nothing, perform rebirth");

                // Update the score counter
                playerTwoScoreboard.GetComponent<TextMeshProUGUI>().text = (
                         int.Parse(playerTwoScoreboard.GetComponent<TextMeshProUGUI>().text) + 1
                         ).ToString();

                // Replace health
                currentHealth = maxHealth;
                healthBar.SetMaxHealth(currentHealth);
                Debug.Log("Health set to" + currentHealth);

                //Update the shieldCount and the Shield Display
                shield.gameObject.SetActive(false);
                currentShieldCount = shieldMaxCount;
                Debug.Log("Shield Count set to" + currentShieldCount);

                // Update the shield GUI
                UpdateShieldGUI();

                // Update the Ammo Counter
                currentAmmoCount = maxAmmoCount;

                // Update the Ammo GUI
                UpdateAmmoGUI();

                //TODO: Replenish SnowBombs
                currentBombCount = maxBombCount;

                //TODO: Update the snowbomb GUI
                UpdateSnowBombGUI();
            }
            else
            {
                currentHealth -= damage;
                healthBar.SetHealth(currentHealth);
            }
        }

    }

    public void MoveGun()
    {

        // Spawn Muzzle Flash
        // float xOffset = 0.76f;  // Moves the gun towards the right
        // float yOffset = 0.35f;  // Moves the gun slightly lower
        // float depth = 0.75f;    // Distance in front of the camera
        // Vector3 muzzlePosition = Camera.main.ViewportToWorldPoint(new Vector3(xOffset, yOffset, depth));
        // // Spawn muzzle flash at the approximated muzzle position
        // GameObject flash = Instantiate(muzzleFlashPrefab, muzzlePosition, spawnedGun.transform.rotation);
        // Debug.Log("HERE: " + spawnedGun.transform.position + " and MUZZLE: " + muzzlePosition);

        // Ensure to se the prefab as the parent
        // flash.transform.SetParent(spawnedGun.transform);
        float recoilDistance = -0.05f; // Move gun backward
                                       // float recoilAngle = -5f; // Recoil up a bit

        // Z and Y axis changes
        spawnedGun.transform.localPosition += new Vector3(0, 0, recoilDistance);
        // spawnedGun.transform.localRotation *= Quaternion.Euler(recoilAngle, 0, 0);

        // Restore the gun's position after a short delay
        Invoke(nameof(ResetGunPosition), 0.05f);
        // if (currentAmmoCount > 0)
        // {
        //     // Spawn some recoil
        //     // Apply Instant Recoil (Move Gun Back Slightly)


        //     // currentAmmoCount -= 1;

        //     // Update the Ammo GUI
        //     // UpdateAmmoGUI();

        //     // return true;
        // }
        // else
        // {
        //     // Trigger reload sequence
        //     // Debug.Log("Player 1 is out of bullets");

        //     // Return False
        //     // return false;
        // }
    }

    public bool ReloadGun()
    {
        if (currentAmmoCount == 0)
        {// Trigger the reload action
            isReloading = true;

            rotationAmount = 0f;

            // Reset the focus point to the front

            currentAmmoCount = maxAmmoCount;

            UpdateAmmoGUI();

            return true;
        }
        else
        {
            return false;
        }

    }

    // For the recoil
    private void ResetGunPosition()
    {
        spawnedGun.transform.localPosition -= new Vector3(0, 0, -0.05f); // Move gun forward again
        // spawnedGun.transform.localRotation *= Quaternion.Euler(5f, 0, 0); // Rotate back down
    }

    private void ResetShieldPosition()
    {
        // TODO: Move it forward again
        spawnedShieldPrefab.transform.localPosition -= new Vector3(0, 0, -0.05f);
    }

    // public bool UseSnowBomb()
    // {
    //     bool status;
    //     if (currentBombCount == 0)
    //     {
    //         Debug.Log("Player1 has no more snowbombs left");
    //         status = false;
    //     }
    //     else
    //     {
    //         if (imageTargetTracker.isTargetVisible)
    //         {
    //             currentBombCount -= 1;
    //             Debug.Log("Player1 has used a snowbomb");
    //             // Spawn the damage here for speed
    //             player_2.TakeDamage(5, "snowball");
    //             status = true;
    //         }
    //         else
    //         {
    //             currentBombCount -= 1;
    //             Debug.Log("Player1 has used a snowbomb, no opponent in sight");
    //             status = false;
    //         }

    //         // TODO: Show the impact of the snowball
    //         HitSnowBallBezier();
    //     }

    //     //TODO: Update the snowbomb GUI
    //     UpdateSnowBombGUI();
    //     return status;

    // }

    public void UseSnowBomb()
    {
        // bool status;
        // if (currentBombCount == 0)
        // {
        //     Debug.Log("Player1 has no more snowbombs left");
        //     status = false;
        // }
        // else
        // {
        //     if (imageTargetTracker.isTargetVisible)
        //     {
        //         currentBombCount -= 1;
        //         Debug.Log("Player1 has used a snowbomb");
        //         // Spawn the damage here for speed
        //         player_2.TakeDamage(5, "snowball");
        //         status = true;
        //     }
        //     else
        //     {
        //         currentBombCount -= 1;
        //         Debug.Log("Player1 has used a snowbomb, no opponent in sight");
        //         status = false;
        //     }

        //     // TODO: Show the impact of the snowball
        //     HitSnowBallBezier();
        // }

        // //TODO: Update the snowbomb GUI
        // UpdateSnowBombGUI();
        // return status;
        HitSnowBallBezier();

    }

    public bool UseShield()
    {
        // Check if you have enough shields to begin with
        if (shieldStatus)
        {
            Debug.Log("Shield is already active, " + currentShieldCount + " left");
            return false;
        }
        else if (currentShieldCount == 0)
        {
            Debug.Log("Player1 cannot use shields since no shields are left");
            return false;
        }
        else
        {
            Debug.Log("Player made use of a shield");
            currentShieldCount -= 1;
            shieldStatus = true;
            currentShieldStrength = 30;

            // Activate the slider to see this at first
            shield.SetMaxStength(30);

            // Make the shield appear
            shield.gameObject.SetActive(true);

            // Update the shield GUI
            UpdateShieldGUI();

            // Add the shield prefab at the users position.
            SpawnShieldPrefab();

            Debug.Log("This is the current ammo count: " + currentAmmoCount);
            return true;
        }
    }

    public void UpdateShieldGUI()
    {
        // Remove existing shield icons
        foreach (GameObject icon in shieldIcons)
        {
            Destroy(icon);
        }
        shieldIcons.Clear();

        // Spacing between shields (adjust as needed)
        float shieldSpacing = 200f; // Adjust to fine-tune spacing

        // Create new shield icons and position them correctly
        for (int i = 0; i < currentShieldCount; i++)
        {
            GameObject newShield = Instantiate(shieldIconPrefab, shieldContainer);
            shieldIcons.Add(newShield);

            // Offset each shield icon to the right
            RectTransform shieldTransform = newShield.GetComponent<RectTransform>();

            // Instead of `i * shieldSpacing`, center properly
            shieldTransform.anchoredPosition = new Vector2(i * shieldSpacing - ((currentShieldCount - 1) * shieldSpacing) / 2, 0);
        }
    }

    public void UpdateAmmoGUI()
    {
        foreach (GameObject ammoIcon in ammoIcons)
        {
            Destroy(ammoIcon);
        }
        ammoIcons.Clear();

        float ammoIconSpacing = 90f;

        // Create new ammo icons and place them appropriately.
        for (int i = 0; i < currentAmmoCount; i++)
        {
            GameObject newAmmo = Instantiate(ammoIconPrefab, ammoContainer);
            ammoIcons.Add(newAmmo); // Corrected from shieldIcons.Add(newShield)

            // Offset each ammo icon to the right
            RectTransform ammoTransform = newAmmo.GetComponent<RectTransform>();

            // Instead of `i * ammoIconSpacing`, center properly
            ammoTransform.anchoredPosition = new Vector2(i * ammoIconSpacing - ((currentAmmoCount - 1) * ammoIconSpacing) / 2, 0);
        }
    }

    public void UpdateSnowBombGUI()
    {
        // Destroy existing bomb icons
        foreach (GameObject bombIcon in snowBombs)
        {
            Destroy(bombIcon);
        }
        snowBombs.Clear();

        float bombIconSpacing = 200f; // Adjust spacing as needed

        // Create new bomb icons and place them appropriately
        for (int i = 0; i < currentBombCount; i++)
        {
            GameObject newBomb = Instantiate(bombIconPrefab, snowBombContainer);
            snowBombs.Add(newBomb);

            // Offset each bomb icon to the right
            RectTransform bombTransform = newBomb.GetComponent<RectTransform>();

            // Center the icons properly
            bombTransform.anchoredPosition = new Vector2(
                i * bombIconSpacing - ((currentBombCount - 1) * bombIconSpacing) / 2, 0
            );
        }
    }

    public void SpawnGunPrefab()
    {
        // Define a right-hand position (adjust as needed)
        //TODO: Move a little lower for mobile
        float xOffset = 0.65f;  // Moves the gun towards the right
        float yOffset = 0.25f;  // Moves the gun slightly lower
        float depth = 0.5f;    // Distance in front of the camera

        // Convert viewport point to world space
        Vector3 spawnPosition = Camera.main.ViewportToWorldPoint(new Vector3(xOffset, yOffset, depth));

        // Spawn the gun model
        spawnedGun = Instantiate(gunModelPrefab, spawnPosition, Quaternion.identity);

        // Adjust the rotation to make the gun face forward
        spawnedGun.transform.rotation = Camera.main.transform.rotation * Quaternion.Euler(0, 90, 0);

        // Parent the gun to the AR Camera so it moves with it
        spawnedGun.transform.SetParent(Camera.main.transform, worldPositionStays: true);

        Debug.Log($"🔫 Gun spawned at: {spawnPosition}");
    }

    // public void SpawnShieldPrefab()
    // {
    //     float xOffset = 0.5f;  // Centered in the viewport (adjust as needed)
    //     float yOffset = 0.2f;  // Slightly above center (adjust as needed)
    //     float depth = 0.5f;      // Closer to the camera for visibility
    //     float distanceFromCamera = 0.5f;

    //     // Convert viewport point to world space
    //     // Vector3 spawnPosition = Camera.main.ViewportToWorldPoint(new Vector3(xOffset, yOffset, depth));

    //     //FIXME: deal with position holding
    //     Vector3 spawnPosition = Camera.main.transform.position + Camera.main.transform.forward * distanceFromCamera;

    //     // Instantiate the shield
    //     spawnedShieldPrefab = Instantiate(shieldPrefab, spawnPosition, Quaternion.identity);

    //     //FIXME: Make sure it faces the same direction as the camera
    //     // spawnedShieldPrefab.transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);

    //     // Ensure the shield has a reasonable scale
    //     spawnedShieldPrefab.transform.localScale = Vector3.one * 0.2f; // Adjust if needed

    //     // Parent it to the camera, ensuring it maintains world position
    //     spawnedShieldPrefab.transform.SetParent(Camera.main.transform, worldPositionStays: true);

    //     spawnedShieldPrefab.SetActive(true);

    //     Debug.Log($"🛡️ Shield Spawned at: {spawnedShieldPrefab.transform.position}");
    //     Debug.Log($"🛡️ Shield Roation at: {spawnedShieldPrefab.transform.rotation}");
    // }

    public void SpawnShieldPrefab()
    {
        // Instantiate the shield
        spawnedShieldPrefab = Instantiate(shieldPrefab, Vector3.zero, Quaternion.identity);

        // Parent it to the camera first
        spawnedShieldPrefab.transform.SetParent(Camera.main.transform, worldPositionStays: false);

        // Set a fixed local position relative to the camera
        spawnedShieldPrefab.transform.localPosition = new Vector3(0.00f, -0.07f, 0.5f);

        // Scale it
        spawnedShieldPrefab.transform.localScale = Vector3.one * 0.2f;

        // Keep it facing forward
        spawnedShieldPrefab.transform.localRotation = Quaternion.identity;

        spawnedShieldPrefab.SetActive(true);

        Debug.Log($"🛡️ Shield Spawned at: {spawnedShieldPrefab.transform.position}");
    }

    public void DespawnShieldPrefab()
    {
        Debug.Log("Trying to despawn here: ");
        if (spawnedShieldPrefab != null)
        {
            Destroy(spawnedShieldPrefab);
        }

    }

    // public void HitGolfBall()
    // {
    //     if (imageTargetTracker.isTargetVisible)
    //     {
    //         Debug.Log($"Golf Ball needs to be hit this position: {imageTargetTracker.GetImageTargetLocation().position}");

    //         Vector3 golfBallSpawnPosition = Camera.main.transform.position +
    //                         (Camera.main.transform.forward * 0.5f) +
    //                         (Camera.main.transform.up * -0.2f);
    //         golfBallInstance = Instantiate(golfBall, golfBallSpawnPosition, Quaternion.identity);

    //         golfBallInstance.transform.localScale *= 3f;

    //         Rigidbody rb = golfBallInstance.GetComponent<Rigidbody>();

    //         if (rb != null)
    //         {
    //             Debug.Log("Rigid body is captured");
    //             Vector3 targetPos = imageTargetTracker.GetImageTargetLocation().position;
    //             Vector3 launchVelocity = CalculateProjectileVelocity(golfBallSpawnPosition, targetPos, 1.2f);

    //             rb.linearVelocity = launchVelocity;
    //         }

    //         Destroy(golfBallInstance, 2.0f);
    //     }
    //     else
    //     {
    //         Debug.Log("Target is not visible, cannot hit golf ball");
    //     }
    // }

    // public void HitGolfBall()
    // {
    //     if (imageTargetTracker.isTargetVisible)
    //     {
    //         Debug.Log($"Golf Ball needs to be hit to position: {imageTargetTracker.GetImageTargetLocation().position}");

    //         Vector3 golfBallSpawnPosition = Camera.main.transform.position +
    //                                         (Camera.main.transform.forward * 0.5f) +
    //                                         (Camera.main.transform.up * -0.2f);

    //         golfBallInstance = Instantiate(golfBall, golfBallSpawnPosition, Quaternion.identity);

    //         Rigidbody rb = golfBallInstance.GetComponent<Rigidbody>();

    //         if (rb != null)
    //         {
    //             Debug.Log("Rigid body is captured");

    //             Vector3 targetPos = imageTargetTracker.GetImageTargetLocation().position;

    //             // **STEP 1: Rotate the golf ball to face the target**
    //             Vector3 directionToTarget = (targetPos - golfBallSpawnPosition).normalized;
    //             golfBallInstance.transform.rotation = Quaternion.LookRotation(directionToTarget);

    //             // **STEP 2: Move the ball forward constantly (linear movement)**
    //             rb.isKinematic = true;  // Disable physics interactions
    //             StartCoroutine(MoveGolfBall(golfBallInstance.transform, directionToTarget, 5f));
    //         }
    //     }
    //     else
    //     {
    //         Debug.Log("Target is not visible, cannot hit golf ball");
    //     }
    // }

    // // **Coroutine to Move the Ball in a Straight Line**
    // private IEnumerator MoveGolfBall(Transform ball, Vector3 direction, float speed)
    // {
    //     float duration = 2f;  // Move for 2 seconds
    //     float timer = 0f;

    //     while (timer < duration)
    //     {
    //         ball.position += direction * speed * Time.deltaTime;
    //         timer += Time.deltaTime;
    //         yield return null;
    //     }

    //     // Destroy the ball after reaching its target
    //     Destroy(ball.gameObject);
    // }

    //FIXME: USE POSITION
    // public void HitGolfBall()
    // {
    //     if (imageTargetTracker.isTargetVisible)
    //     {
    //         Debug.Log($"Golf Ball needs to be hit to position: {imageTargetTracker.GetImageTargetLocation().position}");

    //         Debug.Log("Jitter caused");

    //         // **Spawn golf ball in front of camera**
    //         Vector3 golfBallSpawnPosition = Camera.main.transform.position +
    //                                         (Camera.main.transform.forward * 0.5f) +
    //                                         (Camera.main.transform.up * -0.2f);

    //         golfBallInstance = Instantiate(golfBall, golfBallSpawnPosition, Quaternion.identity);

    //         Rigidbody rb = golfBallInstance.GetComponent<Rigidbody>();

    //         if (rb != null)
    //         {
    //             Debug.Log("Rigid body is captured");

    //             Vector3 targetPos = imageTargetTracker.GetImageTargetLocation().position;

    //             // **STEP 1: Rotate the golf ball to face the target**
    //             Vector3 directionToTarget = (targetPos - golfBallSpawnPosition).normalized;
    //             golfBallInstance.transform.rotation = Quaternion.LookRotation(directionToTarget);

    //             // **STEP 2: Ensure Rigidbody is not kinematic (Enable physics)**
    //             rb.isKinematic = false;
    //             rb.useGravity = false;  // Disable gravity if you want a perfectly linear shot

    //             // **STEP 3: Use MovePosition for smooth movement instead of modifying transform.position**
    //             StartCoroutine(MoveGolfBall(rb, directionToTarget, 5f));
    //         }
    //     }
    //     else
    //     {
    //         Debug.Log("Target is not visible, cannot hit golf ball");
    //     }
    // }

    // // **Use MovePosition for Smooth Linear Movement**
    // private IEnumerator MoveGolfBall(Rigidbody rb, Vector3 direction, float speed)
    // {
    //     float duration = 2f;  // Move for 2 seconds
    //     float timer = 0f;

    //     while (timer < duration)
    //     {
    //         rb.MovePosition(rb.position + direction * speed * Time.fixedDeltaTime);
    //         timer += Time.fixedDeltaTime;
    //         yield return new WaitForFixedUpdate();  // Wait for next physics step
    //     }

    //     Destroy(rb.gameObject);
    // }


    // private Vector3 CalculateProjectileVelocity(Vector3 start, Vector3 target, float flightTime)
    // {
    //     Vector3 distance = target - start;

    //     // Solve for velocity
    //     float vx = distance.x / flightTime;
    //     float vz = distance.z / flightTime;
    //     float vy = (distance.y + 0.5f * Physics.gravity.y * flightTime * flightTime) / flightTime;

    //     return new Vector3(vx, vy, vz);
    // }

    // Try the bezier local version

    public void SwordBezier()
    {
        if (!imageTargetTracker.isTargetVisible)
        {
            Debug.Log("Target is not visible, box being spawned randomly");
            float randomX = UnityEngine.Random.Range(0.2f, 0.8f);
            float randomY = UnityEngine.Random.Range(0.2f, 0.8f);
            float depth = UnityEngine.Random.Range(15f, 20f);

            Vector3 targetPos = Camera.main.ViewportToWorldPoint(new Vector3(randomX, randomY, depth));

            Vector3 spawnPos = Camera.main.transform.TransformPoint(new Vector3(0f, -0.07f, 0.5f));
            GameObject sword = Instantiate(swordPrefab, spawnPos, Quaternion.identity);
            sword.transform.localScale *= 0.5f;

            // **Rotate to face the target**
            // Vector3 flatDir = targetPos - spawnPos;
            // flatDir.y = 0f;
            // if (flatDir != Vector3.zero)
            // {
            //     sword.transform.rotation = Quaternion.LookRotation(flatDir) * Quaternion.Euler(0, 90, 0);
            // }
            Vector3 directionToTarget = (targetPos - spawnPos).normalized;
            sword.transform.rotation = Quaternion.LookRotation(directionToTarget) * Quaternion.Euler(90, 0, 0);

            Rigidbody rb = sword.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.useGravity = false;
                rb.isKinematic = true;
            }



            StartCoroutine(MoveSwordStraight(sword, spawnPos, targetPos, travelDuration, false));
        }
        else
        {
            Vector3 targetPos = imageTargetTracker.GetImageTargetLocation().position;
            Debug.Log($"Box needs to be hit to position: {targetPos}");

            Vector3 spawnPos = Camera.main.transform.TransformPoint(new Vector3(0f, -0.07f, 0.1f));
            GameObject sword = Instantiate(swordPrefab, spawnPos, Quaternion.identity);
            sword.transform.localScale *= 0.2f;

            // **Rotate to face the target**
            // Vector3 flatDir = targetPos - spawnPos;
            // flatDir.y = 0f;
            // if (flatDir != Vector3.zero)
            // {
            //     sword.transform.rotation = Quaternion.LookRotation(flatDir) * Quaternion.Euler(0, 90, 0);
            // }
            Vector3 directionToTarget = (targetPos - spawnPos).normalized;
            sword.transform.rotation = Quaternion.LookRotation(directionToTarget) * Quaternion.Euler(90, 0, 0);

            Rigidbody rb = sword.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.useGravity = false;
                rb.isKinematic = true;
            }
            // Move damage here
            // player_2.TakeDamage(10, "sword");

            StartCoroutine(MoveSwordStraight(sword, spawnPos, targetPos, travelDuration, true));
        }
    }

    private IEnumerator MoveSwordStraight(GameObject sword, Vector3 startPos, Vector3 endPos, float duration, bool isVisible)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            sword.transform.position = Vector3.Lerp(startPos, endPos, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure it reaches the exact target position
        sword.transform.position = endPos;

        if (isVisible)
        {
            // Apply damage and destroy the box


            GameObject swordHitExplosion = Instantiate(swordExplosionHitPrefab, sword.transform.position, Quaternion.identity);
            Destroy(swordHitExplosion, 1f);
        }
        else
        {
            // Need to take some damage here so that MQ receives a report
            // player_2.TakeDamage(0, "sword");
            GameObject boxMissExplosion = Instantiate(golfBallMissExplosionPrefab, sword.transform.position, Quaternion.identity);
            Destroy(boxMissExplosion, 1f);
        }

        // Destroy the box object
        Destroy(sword);
    }

    public void BoxBezier()
    {
        if (!imageTargetTracker.isTargetVisible)
        {
            Debug.Log("Target is not visible, golf ball being spawned randomly");
            float randomX = UnityEngine.Random.Range(0.2f, 0.8f); // Random X in viewport space
            float randomY = UnityEngine.Random.Range(0.2f, 0.8f); // Random Y in viewport space
            float depth = UnityEngine.Random.Range(15f, 20f);   // Random Z (distance from camera)

            // Convert random screen position to world space
            Vector3 targetPos = Camera.main.ViewportToWorldPoint(new Vector3(randomX, randomY, depth));

            Vector3 spawnPos = Camera.main.transform.TransformPoint(new Vector3(0f, -0.07f, 0.5f));
            GameObject box = Instantiate(boxPrefab, spawnPos, Quaternion.identity);
            box.transform.localScale *= 0.05f;

            Vector3 flatDir = targetPos - spawnPos;
            flatDir.y = 0f;
            if (flatDir != Vector3.zero)
            {
                box.transform.rotation = Quaternion.LookRotation(flatDir) * Quaternion.Euler(90, -90, 0);
            }

            Rigidbody rb = box.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.useGravity = false;
                rb.isKinematic = true;
            }

            Vector3 midpoint = (spawnPos + targetPos) / 2f;
            Vector3 controlPoint = midpoint + Vector3.right * controlPointHeightOffset * 1.5f;



            StartCoroutine(MoveBallAlongBezierBox(box, spawnPos, controlPoint, targetPos, travelDuration, false));

        }
        else
        {
            // 1. Get the target position.
            Vector3 targetPos = imageTargetTracker.GetImageTargetLocation().position;
            Debug.Log($"Golf Ball needs to be hit to position: {targetPos}");

            // 2. Spawn the golf ball slightly in front of the camera.
            Vector3 spawnPos = Camera.main.transform.TransformPoint(new Vector3(0f, -0.07f, 0.5f));
            GameObject box = Instantiate(boxPrefab, spawnPos, Quaternion.identity);
            box.transform.localScale *= 0.02f;

            // 3. Rotate the ball to face the target horizontally.
            Vector3 flatDir = targetPos - spawnPos;
            flatDir.y = 0f;
            if (flatDir != Vector3.zero)
            {
                box.transform.rotation = Quaternion.LookRotation(flatDir) * Quaternion.Euler(90, -90, 0);
            }

            // 4. Optionally, disable physics control since we'll drive the motion via our coroutine.
            Rigidbody rb = box.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.useGravity = false;
                rb.isKinematic = true;
            }

            // 5. Compute the control point:
            // Use the midpoint between spawn and target, then add an upward offset.
            Vector3 midpoint = (spawnPos + targetPos) / 2f;

            // Make sure to go right for boxing
            Vector3 controlPoint = midpoint + Vector3.right * controlPointHeightOffset * 1.5f;

            // Move damage here:
            // player_2.TakeDamage(10, "box");

            // 6. Start moving the ball along the custom quadratic Bézier curve.
            StartCoroutine(MoveBallAlongBezierBox(box, spawnPos, controlPoint, targetPos, travelDuration, true));

        }

    }

    private IEnumerator MoveBallAlongBezierBox(GameObject box, Vector3 p0, Vector3 p1, Vector3 p2, float duration, bool isVisible)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            // Evaluate the quadratic Bézier curve.
            Vector3 pos = EvaluateQuadraticBezier(t, p0, p1, p2);
            box.transform.position = pos;
            Vector3 moveDirection = (pos - box.transform.position).normalized;
            // if (moveDirection != Vector3.zero)
            // {
            //     box.transform.rotation = Quaternion.LookRotation(moveDirection) * Quaternion.Euler(0, 180, 0);
            // }

            elapsed += Time.deltaTime;
            yield return null;
        }
        // Ensure the ball finishes exactly at the target.
        box.transform.position = p2;

        if (isVisible)
        {
            // At the end of the path, apply damage and destroy the ball.


            // Spawn a prefab at the location of the ball
            GameObject golfBallhitExplosion = Instantiate(boxHitExplosionPrefab, box.transform.position, quaternion.identity);

            Destroy(golfBallhitExplosion, 1f);
        }
        else
        {
            // Need to take some damage here so that MQ receives a report
            // player_2.TakeDamage(0, "box");
            GameObject golfBallMissExplosion = Instantiate(golfBallMissExplosionPrefab, box.transform.position, quaternion.identity);

            Destroy(golfBallMissExplosion, 1f);
        }

        // Destroy the ball object
        Destroy(box);

    }

    public void HitShuttleCockBezier()
    {
        if (!imageTargetTracker.isTargetVisible)
        {
            Debug.Log("Target is not visible, golf ball being spawned randomly");
            float randomX = UnityEngine.Random.Range(0.2f, 0.8f); // Random X in viewport space
            float randomY = UnityEngine.Random.Range(0.2f, 0.8f); // Random Y in viewport space
            float depth = UnityEngine.Random.Range(15f, 20f);   // Random Z (distance from camera)

            // Convert random screen position to world space
            Vector3 targetPos = Camera.main.ViewportToWorldPoint(new Vector3(randomX, randomY, depth));

            Vector3 spawnPos = Camera.main.transform.TransformPoint(new Vector3(0f, -0.07f, 0.5f));
            GameObject shuttleCock = Instantiate(shuttleCockPrefab, spawnPos, Quaternion.identity);
            shuttleCock.transform.localScale *= 3f;

            Vector3 flatDir = targetPos - spawnPos;
            flatDir.y = 0f;
            if (flatDir != Vector3.zero)
            {
                shuttleCock.transform.rotation = Quaternion.LookRotation(flatDir) * Quaternion.Euler(180, 0, 0);
            }

            Rigidbody rb = shuttleCock.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.useGravity = false;
                rb.isKinematic = true;
            }

            Vector3 midpoint = (spawnPos + targetPos) / 2f;
            Vector3 controlPoint = midpoint + Vector3.up * controlPointHeightOffset;


            StartCoroutine(MoveBallAlongBezierShuttleCock(shuttleCock, spawnPos, controlPoint, targetPos, travelDuration, false));

        }
        else
        {
            // 1. Get the target position.
            Vector3 targetPos = imageTargetTracker.GetImageTargetLocation().position;
            Debug.Log($"Golf Ball needs to be hit to position: {targetPos}");

            // 2. Spawn the golf ball slightly in front of the camera.
            Vector3 spawnPos = Camera.main.transform.TransformPoint(new Vector3(0f, -0.07f, 0.5f));
            GameObject shuttleCock = Instantiate(shuttleCockPrefab, spawnPos, Quaternion.identity);
            shuttleCock.transform.localScale *= 1.5f;

            // 3. Rotate the ball to face the target horizontally.
            Vector3 flatDir = targetPos - spawnPos;
            flatDir.y = 0f;
            if (flatDir != Vector3.zero)
            {
                shuttleCock.transform.rotation = Quaternion.LookRotation(flatDir) * Quaternion.Euler(180, 0, 0);
            }

            // 4. Optionally, disable physics control since we'll drive the motion via our coroutine.
            Rigidbody rb = shuttleCock.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.useGravity = false;
                rb.isKinematic = true;
            }

            // 5. Compute the control point:
            // Use the midpoint between spawn and target, then add an upward offset.
            Vector3 midpoint = (spawnPos + targetPos) / 2f;
            Vector3 controlPoint = midpoint + Vector3.up * controlPointHeightOffset;

            // Take Damage before coroutine spawn
            // player_2.TakeDamage(10, "badminton");

            // 6. Start moving the ball along the custom quadratic Bézier curve.
            StartCoroutine(MoveBallAlongBezierShuttleCock(shuttleCock, spawnPos, controlPoint, targetPos, travelDuration, true));

        }

    }

    private IEnumerator MoveBallAlongBezierShuttleCock(GameObject shuttle, Vector3 p0, Vector3 p1, Vector3 p2, float duration, bool isVisible)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            // Evaluate the quadratic Bézier curve.
            Vector3 pos = EvaluateQuadraticBezier(t, p0, p1, p2);
            shuttle.transform.position = pos;
            // Vector3 moveDirection = (pos - shuttle.transform.position).normalized;
            // if (moveDirection != Vector3.zero)
            // {
            //     shuttle.transform.rotation = Quaternion.LookRotation(moveDirection);
            // }

            elapsed += Time.deltaTime;
            yield return null;
        }
        // Ensure the ball finishes exactly at the target.
        shuttle.transform.position = p2;

        if (isVisible)
        {
            // At the end of the path, apply damage and destroy the ball.
            // Spawn a prefab at the location of the ball
            GameObject golfBallhitExplosion = Instantiate(shuttleCockHitExplosionPrefab, shuttle.transform.position, quaternion.identity);

            Destroy(golfBallhitExplosion, 1f);
        }
        else
        {
            // Need to take some damage here so that MQ receives a report
            // player_2.TakeDamage(0, "badminton");
            GameObject golfBallMissExplosion = Instantiate(golfBallMissExplosionPrefab, shuttle.transform.position, quaternion.identity);

            Destroy(golfBallMissExplosion, 1f);
        }

        // Destroy the ball object
        Destroy(shuttle);

    }
    public void HitSnowBallBezier()
    {
        if (!imageTargetTracker.isTargetVisible)
        {
            Debug.Log("Target is not visible, golf ball being spawned randomly");
            float randomX = UnityEngine.Random.Range(0.2f, 0.8f); // Random X in viewport space
            float randomY = UnityEngine.Random.Range(0.2f, 0.8f); // Random Y in viewport space
            float depth = UnityEngine.Random.Range(15f, 20f);   // Random Z (distance from camera)

            // Convert random screen position to world space
            Vector3 targetPos = Camera.main.ViewportToWorldPoint(new Vector3(randomX, randomY, depth));

            Vector3 spawnPos = Camera.main.transform.TransformPoint(new Vector3(0f, -0.07f, 0.5f));
            GameObject snowBall = Instantiate(snowBallPrefab, spawnPos, Quaternion.identity);
            snowBall.transform.localScale *= 1f;

            Vector3 flatDir = targetPos - spawnPos;
            flatDir.y = 0f;
            if (flatDir != Vector3.zero)
            {
                snowBall.transform.rotation = Quaternion.LookRotation(flatDir);
            }

            Rigidbody rb = snowBall.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.useGravity = false;
                rb.isKinematic = true;
            }

            Vector3 midpoint = (spawnPos + targetPos) / 2f;
            Vector3 controlPoint = midpoint + Vector3.up * controlPointHeightOffset;

            StartCoroutine(MoveBallAlongBezierSnowBall(snowBall, spawnPos, controlPoint, targetPos, travelDuration, false));

        }
        else
        {
            // 1. Get the target position.
            Vector3 targetPos = imageTargetTracker.GetImageTargetLocation().position;
            Debug.Log($"Golf Ball needs to be hit to position: {targetPos}");

            // 2. Spawn the golf ball slightly in front of the camera.
            Vector3 spawnPos = Camera.main.transform.TransformPoint(new Vector3(0f, -0.07f, 0.5f));
            GameObject snowBall = Instantiate(snowBallPrefab, spawnPos, Quaternion.identity);
            snowBall.transform.localScale *= 0.1f;

            // 3. Rotate the ball to face the target horizontally.
            Vector3 flatDir = targetPos - spawnPos;
            flatDir.y = 0f;
            if (flatDir != Vector3.zero)
            {
                snowBall.transform.rotation = Quaternion.LookRotation(flatDir);
            }

            // 4. Optionally, disable physics control since we'll drive the motion via our coroutine.
            Rigidbody rb = snowBall.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.useGravity = false;
                rb.isKinematic = true;
            }

            // 5. Compute the control point:
            // Use the midpoint between spawn and target, then add an upward offset.
            Vector3 midpoint = (spawnPos + targetPos) / 2f;
            Vector3 controlPoint = midpoint + Vector3.up * controlPointHeightOffset;

            // 6. Start moving the ball along the custom quadratic Bézier curve.
            StartCoroutine(MoveBallAlongBezierSnowBall(snowBall, spawnPos, controlPoint, targetPos, travelDuration, true));

        }
    }

    private IEnumerator MoveBallAlongBezierSnowBall(GameObject snowBall, Vector3 p0, Vector3 p1, Vector3 p2, float duration, bool isVisible)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            // Evaluate the quadratic Bézier curve.
            Vector3 pos = EvaluateQuadraticBezier(t, p0, p1, p2);
            snowBall.transform.position = pos;

            elapsed += Time.deltaTime;
            yield return null;
        }
        // Ensure the ball finishes exactly at the target.
        snowBall.transform.position = p2;

        if (isVisible)
        {
            // At the end of the path, apply damage and destroy the ball.


            // impactReceived = true;

            // Get a target position: you could use the image target's position,
            // or in your case, perhaps the final Bézier position (p2)
            // Vector3 targetPos = imageTargetTracker.transform.position;

            // // Option 1: Instantiate the object directly on the plane.
            // // You can decide on an offset relative to the plane if needed.
            // Vector3 spawnPos = targetPos;

            // // Instantiate your object at spawnPos.
            // snowStormOne.transform.localScale *= 1f;

            // snowStormOne = Instantiate(snowStormParticlePrefab);

            // StormBehavioour stormBehavior = snowStormOne.GetComponent<StormBehavioour>();
            // if (stormBehavior != null)
            // {
            //     Debug.Log("Storm behaviour present");
            //     Debug.Log($"SNOWSTORM1: {snowStormOne.activeInHierarchy}");
            //     Vector3 imageTargetScreenPos = Camera.main.WorldToScreenPoint(imageTargetTracker.transform.position);
            //     stormBehavior.PlaceInMidAir(imageTargetTracker.transform.position);
            //     snowStormOne.SetActive(true);
            //     Debug.Log($"SNOWSTORM1: {snowStormOne.activeInHierarchy}");
            // }
            // else
            // {
            //     Debug.LogError("Storm behaviour missing");
            // }

            // Vector3 viewportPos = Camera.main.WorldToViewportPoint(imageTargetTracker.transform.position);
            // // For debugging:
            // Debug.Log("Viewport coordinates: " + viewportPos);

            // planeFinder.PerformHitTest(viewportPos);
            // // Pose anchorPose = GetGroundPlanePose(imageTargetTracker.transform.position);
            // GameObject snowStormOne = Instantiate(snowStormParticlePrefab, imageTargetTracker.transform.position, Quaternion.identity);
            // snowStormOne.SetActive(true);
            // Optionally create an anchor GameObject and parent storm to it.


            // // Now, set the parent to the ground plane so that its position
            // // is relative to the plane.
            // snowStormOne.transform.SetParent(groundPlane.transform, worldPositionStays: true);

            // Pose anchorPose = GetGroundPlanePose(imageTargetTracker.transform.position);

            // Vector3 screenPoint = Camera.main.WorldToScreenPoint(anchorPose.position);

            // Debug.Log($"ANCHOR POSE: {anchorPose.position}");

            // List<HitTestResult> hits = planeFinder.PerformHitTest(screenPoint);

            // ContentPositioningBehaviour cpb = planeFinder.GetComponent<ContentPositioningBehaviour>();

            // if (cpb != null)
            // {
            //     // If you have a prefab (e.g., snowStormParticlePrefab),
            //     // you can let ContentPositioningBehaviour do the placement.
            //     // The method name may vary by Vuforia version. Common ones:
            //     //   - PositionContentAtPlaneAnchor(GameObject, Pose)
            //     //   - PositionContent(GameObject, HitTestResult)
            //     //   - PositionContent(GameObject, Pose)

            //     // Example: If you have a method that just needs a Pose:
            //     cpb.PositionContentAtPlaneAnchor(snowStormParticlePrefab, anchorPose);

            //     // Now, your prefab is placed on the ground plane at anchorPose.
            //     // You no longer need manual "Instantiate" or "parent = null" calls
            //     // because Vuforia’s content positioning does that for you.
            // }
            // else
            // {
            //     Debug.LogError("No ContentPositioningBehaviour found on planeFinder!");
            // }

            // Instead of spawning a hit, we ensure to spawn the snowStorm
            //FIXME:
            // Vector3 updatedPosition = Camera.main.transform.InverseTransformPoint(imageTargetTracker.transform.position);
            // if (snowStormOne == null)
            // {
            // Vector3 updatedPosition = imageTargetTracker.transform.position;
            // initSnowStormUpdater = updatedPosition;
            // float localPosZ = Camera.main.transform.InverseTransformPoint(updatedPosition).z;
            // Vector3 initCamDeltaOne = Camera.main.transform.position - initCameraPosition;
            // Move it back a little

            // TODO: no need to move back anymore
            // if (localPosZ > 0 && localPosZ <= 1)
            // {
            //     updatedPosition.z += 0.5f;
            // }
            // else if (localPosZ > 1 && localPosZ <= 2)
            // {
            //     updatedPosition.z += localPosZ;
            // }
            // updatedPosition = updatedPosition - initCamDelta;

            // Subtract this:
            // updatedPosition.z += localPosZ;

            // FIXME: During final add back initCamDelta

            // if (initCamDeltaOne.x < 0)
            // {
            //     Debug.Log("X is below 0");
            //     updatedPosition.x -= initCamDeltaOne.x;
            // }
            // else
            // {
            //     Debug.Log("X is above 0");
            //     updatedPosition.x -= initCamDeltaOne.x;
            // }

            // Drop the y position by a bit
            // updatedPosition.y -= 0.6f;
            // Account for changes from center of the device
            // updatedPosition = updatedPosition - initCamDelta;

            // Vector3 spawnPosition = Camera.main.transform.TransformPoint(updatedPosition);

            // snowStormOne = Instantiate(snowStormParticlePrefab, imageTargetTracker.transform.position, Quaternion.Euler(Vector3.up));
            // snowStormOne.transform.SetParent(null);
            // Make it larger to track
            // snowStormOne.transform.localScale *= 2f;

            // initCameraPos1 = Camera.main.transform.position;
            // initSnowStormPos1 = snowStormOne.transform.position;
            // }
            // else
            // {
            // Vector3 updatedPosition = imageTargetTracker.transform.position;
            // initSnowStormUpdaterTwo = updatedPosition;
            // float localPosZ = Camera.main.transform.InverseTransformPoint(updatedPosition).z;
            // Vector3 initCamDeltaTwo = Camera.main.transform.position - initCameraPosition;
            // Move it back a little

            // TODO: no need to move back anymore
            // if (localPosZ > 0 && localPosZ <= 1)
            // {
            //     updatedPosition.z += 0.5f;
            // }
            // else if (localPosZ > 1 && localPosZ <= 2)
            // {
            //     updatedPosition.z += localPosZ;
            // }
            // updatedPosition = updatedPosition - initCamDelta;

            // Subtract this:
            // updatedPosition.z += localPosZ;

            // FIXME: During final add back initCamDelta

            // if (initCamDeltaTwo.x < 0)
            // {
            //     Debug.Log("X is below 0");
            //     updatedPosition.x -= initCamDeltaTwo.x;
            // }
            // else
            // {
            //     Debug.Log("X is above 0");
            //     updatedPosition.x -= initCamDeltaTwo.x;
            // }

            // Account for changes from center of the device
            // updatedPosition = updatedPosition - initCamDelta;

            // Vector3 spawnPosition = Camera.main.transform.TransformPoint(updatedPosition);

            // updatedPosition.y -= 0.6f;

            // snowStormTwo = Instantiate(snowStormParticlePrefab, imageTargetTracker.transform.position, Quaternion.Euler(Vector3.up));
            // snowStormTwo.transform.SetParent(null);
            // Make it larger to track
            // snowStormTwo.transform.localScale *= f;

            // initCameraPos2 = Camera.main.transform.position;
            // initSnowStormPos2 = snowStormTwo.transform.position;
            // }
            Vector3 currPos = imageTargetTracker.transform.position;
            currPos.y = 1;
            GameObject snowStorm = Instantiate(snowStormParticlePrefab, currPos, Quaternion.Euler(Vector3.up));
            snowStorm.transform.SetParent(null);
            activeCylinders.Add(snowStorm);

            // Update the text
            if (activeCylinders.Count < 10)
            {
                activeSnowstormCountText.text = $"0{activeCylinders.Count}";
            }
            else
            {
                activeSnowstormCountText.text = $"{activeCylinders.Count}";
            }


            // // Ensure that the parent is null
            // snowStormOne.transform.parent = null;

            // GameObject anchor = new("Anchor");
            // anchor.transform.position = imageTargetTracker.transform.position;
            // anchor.transform.rotation = Quaternion.identity;
            // anchor.AddComponent<ARAnchor>();
            // snowStormOne.transform.SetParent(anchor.transform);

            // // Create an AR Foundation anchor
            // var createAnchorTask = arAnchorManager.TryAddAnchorAsync(
            //     new Pose(imageTargetTracker.transform.position, Quaternion.identity)
            // );
            // yield return createAnchorTask; // Wait for the async task to complete

            // anchorBehaviour = snowStormOne.AddComponent<AnchorBehaviour>();

            // if (anchorBehaviour != null)
            // {
            //     Debug.Log("ANCHOR BEHAVIOUR HAS BEEN FOUND");
            //     anchorBehaviour.ConfigureAnchor("tester", imageTargetTracker.transform.position, Quaternion.identity);

            // }
            // else
            // {
            //     Debug.Log("IN TROUBLE");
            // }

            // Destroy(snowBall, 1f);

            // anchorBehaviourOne.ConfigureAnchor("test", imageTargetTracker.transform.position, Quaternion.identity);
        }
        else
        {
            // Need to take some damage here so that MQ receives a report
            // player_2.TakeDamage(0, "snowball");
            GameObject golfBallMissExplosion = Instantiate(golfBallMissExplosionPrefab, snowBall.transform.position, quaternion.identity);

            Destroy(golfBallMissExplosion, 1f);
        }

        // Destroy the ball object
        Destroy(snowBall);
    }

    // public void HandleHitTestResult(HitTestResult hitTestResult)
    // {
    //     // Get the ContentPositioningBehaviour component from the Plane Finder
    //     ContentPositioningBehaviour cpb = planeFinder.GetComponent<ContentPositioningBehaviour>();
    //     if (cpb != null)
    //     {
    //         // Place the content using the hit test result.
    //         // This method typically instantiates or repositions your prefab onto the detected plane.
    //         cpb.PositionContentAtPlaneAnchor(hitTestResult);
    //         Debug.Log("Content positioned using hit test result: " + hitTestResult.Position);
    //     }
    //     else
    //     {
    //         Debug.LogError("ContentPositioningBehaviour not found on the Plane Finder!");
    //     }
    // }

    // public void anchorSnowstorm(Transform transform)
    // {
    //     snowStormOne.transform.SetParent(transform);
    // }

    // // Returns a Pose for the ground plane at the image target's position.
    // private Pose GetGroundPlanePose(Vector3 referencePos)
    // {
    //     Ray ray = new Ray(referencePos, Vector3.down);
    //     if (Physics.Raycast(ray, out RaycastHit hit, 50f))
    //     {
    //         Quaternion rot = Quaternion.LookRotation(
    //             Vector3.ProjectOnPlane(Camera.main.transform.forward, hit.normal),
    //             hit.normal
    //         );
    //         return new Pose(hit.point, rot);
    //     }
    //     return new Pose(referencePos, Quaternion.identity);
    // }
    public void HitGolfBallBezier()
    {
        if (!imageTargetTracker.isTargetVisible)
        {
            Debug.Log("Target is not visible, golf ball being spawned randomly");
            float randomX = UnityEngine.Random.Range(0.2f, 0.8f); // Random X in viewport space
            float randomY = UnityEngine.Random.Range(0.2f, 0.8f); // Random Y in viewport space
            float depth = UnityEngine.Random.Range(15f, 20f);   // Random Z (distance from camera)

            // Convert random screen position to world space
            Vector3 targetPos = Camera.main.ViewportToWorldPoint(new Vector3(randomX, randomY, depth));

            Vector3 spawnPos = Camera.main.transform.TransformPoint(new Vector3(0f, -0.07f, 0.5f));
            GameObject ballInstance = Instantiate(golfBall, spawnPos, Quaternion.identity);
            ballInstance.transform.localScale *= 3f;

            Vector3 flatDir = targetPos - spawnPos;
            flatDir.y = 0f;
            if (flatDir != Vector3.zero)
            {
                ballInstance.transform.rotation = Quaternion.LookRotation(flatDir);
            }

            Rigidbody rb = ballInstance.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.useGravity = false;
                rb.isKinematic = true;
            }

            Vector3 midpoint = (spawnPos + targetPos) / 2f;
            Vector3 controlPoint = midpoint + Vector3.up * controlPointHeightOffset;



            StartCoroutine(MoveBallAlongBezierGolf(ballInstance, spawnPos, controlPoint, targetPos, travelDuration, false));

        }
        else
        {
            // 1. Get the target position.
            Vector3 targetPos = imageTargetTracker.GetImageTargetLocation().position;
            Debug.Log($"Golf Ball needs to be hit to position: {targetPos}");

            // 2. Spawn the golf ball slightly in front of the camera.
            Vector3 spawnPos = Camera.main.transform.TransformPoint(new Vector3(0f, -0.07f, 0.5f));
            GameObject ballInstance = Instantiate(golfBall, spawnPos, Quaternion.identity);
            ballInstance.transform.localScale *= 1.5f;

            // 3. Rotate the ball to face the target horizontally.
            Vector3 flatDir = targetPos - spawnPos;
            flatDir.y = 0f;
            if (flatDir != Vector3.zero)
            {
                ballInstance.transform.rotation = Quaternion.LookRotation(flatDir);
            }

            // 4. Optionally, disable physics control since we'll drive the motion via our coroutine.
            Rigidbody rb = ballInstance.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.useGravity = false;
                rb.isKinematic = true;
            }

            // 5. Compute the control point:
            // Use the midpoint between spawn and target, then add an upward offset.
            Vector3 midpoint = (spawnPos + targetPos) / 2f;
            Vector3 controlPoint = midpoint + Vector3.up * controlPointHeightOffset;

            // Move golf ball damage here
            // player_2.TakeDamage(10, "golf");

            // 6. Start moving the ball along the custom quadratic Bézier curve.
            StartCoroutine(MoveBallAlongBezierGolf(ballInstance, spawnPos, controlPoint, targetPos, travelDuration, true));

        }


    }

    private IEnumerator MoveBallAlongBezierGolf(GameObject ball, Vector3 p0, Vector3 p1, Vector3 p2, float duration, bool isVisible)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            // Evaluate the quadratic Bézier curve.
            Vector3 pos = EvaluateQuadraticBezier(t, p0, p1, p2);
            ball.transform.position = pos;

            elapsed += Time.deltaTime;
            yield return null;
        }
        // Ensure the ball finishes exactly at the target.
        ball.transform.position = p2;

        if (isVisible)
        {
            // At the end of the path, apply damage and destroy the ball.


            // Spawn a prefab at the location of the ball
            GameObject golfBallhitExplosion = Instantiate(golfBallHitExplosionPrefab, ball.transform.position, quaternion.identity);

            Destroy(golfBallhitExplosion, 1f);
        }
        else
        {
            // Need to take some damage here so that MQ receives a report
            // player_2.TakeDamage(0, "golf");
            GameObject golfBallMissExplosion = Instantiate(golfBallMissExplosionPrefab, ball.transform.position, quaternion.identity);

            Destroy(golfBallMissExplosion, 1f);
        }

        // Destroy the ball object
        Destroy(ball);
    }

    private Vector3 EvaluateQuadraticBezier(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        return u * u * p0 + 2 * u * t * p1 + t * t * p2;
    }

    // public void HitGolfBallStraightLine()
    // {
    //     if (imageTargetTracker.isTargetVisible)
    //     {
    //         // Cache the target position.
    //         cachedTargetPosition = imageTargetTracker.GetImageTargetLocation().position;
    //         Debug.Log($"Golf Ball needs to be hit to position: {cachedTargetPosition}");

    //         // STEP 1: Spawn golf ball slightly in front of the camera.
    //         Vector3 golfBallSpawnPosition = Camera.main.transform.TransformPoint(new Vector3(0.0f, -0.07f, 0.5f));
    //         golfBallInstance = Instantiate(golfBall, golfBallSpawnPosition, Quaternion.identity);

    //         Rigidbody rb = golfBallInstance.GetComponent<Rigidbody>();

    //         if (rb != null)
    //         {
    //             Debug.Log("Rigid body is captured");

    //             // Get the (current) target position.
    //             Vector3 targetPos = imageTargetTracker.GetImageTargetLocation().position;

    //             // STEP 2: Rotate the golf ball to face the target horizontally.
    //             Vector3 directionToTarget = (cachedTargetPosition - golfBallSpawnPosition).normalized;
    //             golfBallInstance.transform.rotation = Quaternion.LookRotation(directionToTarget);

    //             // STEP 3: Configure the Rigidbody.
    //             rb.mass = 1f;
    //             rb.useGravity = true;
    //             rb.isKinematic = false;
    //             rb.interpolation = RigidbodyInterpolation.Interpolate;
    //             rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

    //             // STEP 4: Apply force in the direction of the target.
    //             float forceMagnitude = 600f; // Adjust as needed.
    //             rb.AddForce(directionToTarget * forceMagnitude, ForceMode.Impulse);
    //             Debug.Log("Launch velocity: " + directionToTarget * forceMagnitude);

    //             // STEP 5: Instead of destroying after 2 seconds, start a coroutine to destroy when the ball crosses the target's z-plane.
    //             StartCoroutine(CheckForZPlaneCollision(golfBallInstance, targetPos, golfBallSpawnPosition));
    //         }
    //     }
    //     else
    //     {
    //         Debug.Log("Target is not visible, cannot hit golf ball");
    //     }
    // }

    // public void HitGolfBallLob()
    // {
    //     if (!imageTargetTracker.isTargetVisible)
    //     {
    //         Debug.Log("Target is not visible, cannot hit golf ball");
    //         return;
    //     }

    //     // 1. Cache the target position.
    //     cachedTargetPosition = imageTargetTracker.GetImageTargetLocation().position;
    //     Debug.Log($"Golf Ball needs to be hit to position: {cachedTargetPosition}");

    //     // 2. Spawn the golf ball slightly in front of the camera.
    //     Vector3 golfBallSpawnPosition = Camera.main.transform.TransformPoint(new Vector3(0f, -0.07f, 0.5f));
    //     GameObject ballInstance = Instantiate(golfBall, golfBallSpawnPosition, Quaternion.identity);

    //     // Optionally scale the ball.
    //     ballInstance.transform.localScale *= 3f;

    //     Rigidbody rb = ballInstance.GetComponent<Rigidbody>();
    //     if (rb == null)
    //     {
    //         Debug.LogWarning("Golf ball is missing a Rigidbody!");
    //         return;
    //     }

    //     // 3. Compute horizontal direction (flat projection) from spawn to target.
    //     Vector3 targetPos = imageTargetTracker.GetImageTargetLocation().position;
    //     Vector3 flatDir = targetPos - golfBallSpawnPosition;
    //     flatDir.y = 0f;
    //     Vector3 horizontalDir = flatDir.normalized;

    //     // 4. Determine our desired launch angle.
    //     // We want the ball to be launched with an upward angle. Although the instruction says "rotate 45° higher than direct line-of-sight",
    //     // we clamp the final launch angle to 40° above horizontal.
    //     float launchAngleDeg = 40f;
    //     float launchAngleRad = launchAngleDeg * Mathf.Deg2Rad;

    //     // 5. Rotate the ball so it is aimed along the horizontal direction but pitched upward.
    //     // First, get the base horizontal rotation.
    //     Quaternion baseRotation = Quaternion.LookRotation(horizontalDir);
    //     // Then pitch it upward by the desired angle.
    //     Quaternion lobRotation = baseRotation * Quaternion.Euler(launchAngleDeg, 0f, 0f);
    //     ballInstance.transform.rotation = lobRotation;

    //     // 6. Compute the required initial speed using the range formula.
    //     // We use only the z coordinate difference (absolute) as the horizontal range.
    //     float distanceZ = Mathf.Abs(targetPos.z - golfBallSpawnPosition.z);
    //     float gravity = Mathf.Abs(Physics.gravity.y);
    //     // For level ground: Range = (v^2 * sin(2θ)) / g, so:
    //     float initialSpeed = Mathf.Sqrt(distanceZ * gravity / Mathf.Sin(2 * launchAngleRad));

    //     float horizontalBoost = 1.5f;

    //     // 7. Decompose the initial speed into horizontal and vertical components.
    //     Vector3 launchVelocity = horizontalDir * (initialSpeed * Mathf.Cos(launchAngleRad) * horizontalBoost)
    //                                + Vector3.up * (initialSpeed * Mathf.Sin(launchAngleRad));

    //     // 8. Configure Rigidbody properties.
    //     rb.mass = 1f;
    //     rb.useGravity = true;
    //     rb.isKinematic = false;
    //     rb.interpolation = RigidbodyInterpolation.Interpolate;
    //     rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

    //     // 9. Apply the launch velocity using an impulse (so mass is factored in).
    //     rb.AddForce(launchVelocity, ForceMode.Impulse);
    //     Debug.Log("Launch velocity: " + launchVelocity);

    //     // 10. Start a coroutine to check when the ball crosses the target's z-plane.
    //     StartCoroutine(CheckForZPlaneCollision(ballInstance, targetPos, golfBallSpawnPosition));
    // }

    // /// <summary>
    // /// Checks if the ball has crossed the target's z-plane (with a tolerance), then applies damage and destroys the ball.
    // /// </summary>
    // private IEnumerator CheckForZPlaneCollision(GameObject ball, Vector3 targetPos, Vector3 spawnPos)
    // {
    //     // Determine whether we're moving in the positive or negative z direction.
    //     bool movingPositive = targetPos.z >= spawnPos.z;
    //     // Set a small tolerance so the check doesn't require an exact match.
    //     float tolerance = 0.1f;

    //     while (ball != null)
    //     {
    //         float ballZ = ball.transform.position.z;

    //         if (movingPositive)
    //         {
    //             if (ballZ >= targetPos.z - tolerance)
    //             {
    //                 // The ball has crossed (or is near) the target's z-plane.
    //                 player_2.TakeDamage(10, "golf");
    //                 Destroy(ball);
    //                 yield break;
    //             }
    //         }
    //         else
    //         {
    //             if (ballZ <= targetPos.z + tolerance)
    //             {
    //                 player_2.TakeDamage(10, "golf");
    //                 Destroy(ball);
    //                 yield break;
    //             }
    //         }
    //         yield return null;
    //     }
    // }

    // public void HitGolfBallNoDirection()
    // {
    //     if (imageTargetTracker.isTargetVisible)
    //     {
    //         cachedTargetPosition = imageTargetTracker.GetImageTargetLocation().position;
    //         Debug.Log($"Golf Ball needs to be hit to position: {cachedTargetPosition}");

    //         // **Spawn at a Fixed Local Position (Similar to Shield)**
    //         Vector3 fixedSpawnPosition = Camera.main.transform.TransformPoint(new Vector3(0.00f, -0.07f, 0.5f));
    //         golfBallInstance = Instantiate(golfBall, fixedSpawnPosition, Quaternion.identity);
    //         golfBallInstance.transform.localScale *= 2f;

    //         Rigidbody rb = golfBallInstance.GetComponent<Rigidbody>();

    //         if (rb != null)
    //         {
    //             Debug.Log("Rigid body is captured");

    //             // **Calculate Direction to Target**
    //             Vector3 directionToTarget = (cachedTargetPosition - fixedSpawnPosition).normalized;

    //             // **Modify to Add Upward Component for a Lob**
    //             Vector3 lobDirection = directionToTarget + Vector3.up * 0.5f; // Adjust 0.5f for different lob heights
    //             lobDirection.Normalize(); // Keep vector length consistent

    //             // **Optimize Rigidbody Settings for Smooth Motion**
    //             rb.mass = 1f;
    //             rb.useGravity = true;  // Enable gravity for natural descent
    //             rb.isKinematic = false;
    //             rb.interpolation = RigidbodyInterpolation.Interpolate;
    //             rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

    //             // **Apply Force for a Lob Effect**
    //             float forceMagnitude = 10f; // Adjust based on lob strength
    //             rb.AddForce(lobDirection * forceMagnitude, ForceMode.Impulse);

    //             // **Destroy ball when it crosses the target Z-plane**
    //             // StartCoroutine(CheckBallPosition(rb));
    //             Destroy(golfBallInstance, 2f);
    //         }
    //     }
    //     else
    //     {
    //         Debug.Log("Target is not visible, cannot hit golf ball");
    //     }
    // }

    // public void HitGolfBallFailedMotion()
    // {
    //     if (imageTargetTracker.isTargetVisible)
    //     {
    //         cachedTargetPosition = imageTargetTracker.GetImageTargetLocation().position;
    //         Debug.Log($"Golf Ball needs to be hit to position: {cachedTargetPosition}");

    //         // **Spawn at a Fixed Local Position (Similar to Shield)**
    //         Vector3 fixedSpawnPosition = Camera.main.transform.TransformPoint(new Vector3(0.00f, -0.07f, 0.5f));
    //         golfBallInstance = Instantiate(golfBall, fixedSpawnPosition, Quaternion.identity);

    //         Rigidbody rb = golfBallInstance.GetComponent<Rigidbody>();

    //         if (rb != null)
    //         {
    //             Debug.Log("Rigid body is captured");

    //             // **STEP 1: Calculate Forward Direction to Target**
    //             Vector3 directionToTarget = (cachedTargetPosition - fixedSpawnPosition).normalized;
    //             directionToTarget.y = 0;
    //             directionToTarget.Normalize();

    //             // **STEP 2: Rotate the Ball to Face the Target**
    //             golfBallInstance.transform.rotation = Quaternion.LookRotation(directionToTarget);

    //             // **STEP 3: Apply Force (Forward + Upward in One Step)**
    //             float forceMagnitude = 10f; // Adjust shot power
    //             float lobFactor = 0.3f;  // Adjust how much "lift" is added

    //             // **Create a single launch vector that includes upward force**
    //             Vector3 launchVector = (golfBallInstance.transform.forward + Vector3.up * lobFactor).normalized * forceMagnitude;

    //             // **Optimize Rigidbody Settings**
    //             rb.useGravity = true;  // Enable gravity for natural descent
    //             rb.isKinematic = false;
    //             rb.interpolation = RigidbodyInterpolation.Interpolate;
    //             rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

    //             // **Apply Force in One Go**
    //             rb.AddForce(launchVector, ForceMode.Impulse);

    //             // **Destroy ball after 2 seconds**
    //             Destroy(golfBallInstance, 2f);
    //         }
    //     }
    //     else
    //     {
    //         Debug.Log("Target is not visible, cannot hit golf ball");
    //     }
    // }

    // public void HitGolfBallO3Failed()
    // {
    //     if (!imageTargetTracker.isTargetVisible)
    //     {
    //         Debug.Log("Target is not visible, cannot hit golf ball");
    //         return;
    //     }

    //     // Get the target position (assumed to be in world space)
    //     Vector3 targetPosition = imageTargetTracker.GetImageTargetLocation().position;
    //     Debug.Log($"Golf Ball needs to be hit to position: {targetPosition}");

    //     // **Spawn the ball at a fixed local position relative to the camera**
    //     // (You may need to adjust this spawn point to be stable in your AR scene.)
    //     Vector3 fixedSpawnPosition = Camera.main.transform.TransformPoint(new Vector3(0.0f, -0.07f, 0.5f));
    //     GameObject golfBallInstance = Instantiate(golfBall, fixedSpawnPosition, Quaternion.identity);

    //     Rigidbody rb = golfBallInstance.GetComponent<Rigidbody>();
    //     if (rb == null)
    //     {
    //         Debug.LogWarning("Golf ball has no Rigidbody!");
    //         return;
    //     }

    //     // Enable proper physics settings
    //     rb.useGravity = true;
    //     rb.isKinematic = false;
    //     rb.interpolation = RigidbodyInterpolation.Interpolate;
    //     rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

    //     // **Calculate displacement and desired flight time**
    //     Vector3 displacement = targetPosition - fixedSpawnPosition;

    //     // Choose a flight time that makes sense for your shot (in seconds)
    //     float flightTime = 1.5f; // Adjust based on desired shot distance & style

    //     // Separate horizontal and vertical components.
    //     Vector3 horizontalDisplacement = new Vector3(displacement.x, 0, displacement.z);
    //     float horizontalDistance = horizontalDisplacement.magnitude;
    //     float verticalDistance = displacement.y;

    //     // Calculate horizontal velocity (in the direction of the target)
    //     Vector3 horizontalDirection = horizontalDisplacement.normalized;
    //     float horizontalSpeed = horizontalDistance / flightTime;

    //     // Calculate vertical velocity to overcome gravity and cover verticalDistance.
    //     // (Using Physics.gravity.y which is negative; take its absolute value.)
    //     float gravity = Mathf.Abs(Physics.gravity.y);
    //     float verticalSpeed = verticalDistance / flightTime + 0.5f * gravity * flightTime;

    //     // Combine into the initial velocity vector
    //     Vector3 launchVelocity = horizontalDirection * horizontalSpeed + Vector3.up * verticalSpeed;

    //     // Rotate the ball to face the launch direction (optional visualization)
    //     if (horizontalDirection != Vector3.zero)
    //     {
    //         golfBallInstance.transform.rotation = Quaternion.LookRotation(horizontalDirection);
    //     }

    //     // **Apply the computed velocity**
    //     Vector3 velocityChange = launchVelocity - rb.linearVelocity;
    //     rb.AddForce(velocityChange, ForceMode.VelocityChange);

    //     Debug.Log($"Launching ball with velocity: {launchVelocity}");

    //     // Optionally, destroy the ball after enough time for it to complete its flight.
    //     Destroy(golfBallInstance, flightTime + 1f);
    // }

    // public void HitGolfBallRandomUnpredictable()
    // {
    //     if (imageTargetTracker.isTargetVisible)
    //     {
    //         // STEP 1: Get the target position and spawn the ball
    //         cachedTargetPosition = imageTargetTracker.GetImageTargetLocation().position;
    //         Debug.Log($"Golf Ball needs to be hit to position: {cachedTargetPosition}");

    //         // Spawn golf ball slightly in front of the camera
    //         Vector3 golfBallSpawnPosition = Camera.main.transform.TransformPoint(new Vector3(0.0f, -0.07f, 0.5f));
    //         golfBallInstance = Instantiate(golfBall, golfBallSpawnPosition, Quaternion.identity);

    //         Rigidbody rb = golfBallInstance.GetComponent<Rigidbody>();
    //         if (rb != null)
    //         {
    //             Debug.Log("Rigid body is captured");

    //             // STEP 2: Calculate the direction to the target
    //             // Use the full direction for rotation (if needed)
    //             Vector3 directionToTarget = (cachedTargetPosition - golfBallSpawnPosition).normalized;
    //             golfBallInstance.transform.rotation = Quaternion.LookRotation(directionToTarget);

    //             // STEP 3: Prepare the Rigidbody for a lob shot
    //             rb.mass = 1f;
    //             rb.useGravity = true;  // Enable gravity to allow for a parabolic (lob) arc
    //             rb.isKinematic = false;
    //             rb.interpolation = RigidbodyInterpolation.Interpolate;
    //             rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

    //             // STEP 4: Separate horizontal and vertical (lob) components
    //             // Get the horizontal (forward) direction by ignoring the y component
    //             Vector3 horizontalDirection = new Vector3(directionToTarget.x, 0, directionToTarget.z).normalized;

    //             // Define your force magnitudes (tweak these values until the arc looks right)
    //             float forwardForceMagnitude = 60f; // Force to propel the ball toward the target
    //             float upwardForceMagnitude = 20f;  // Additional force to give the ball a lob

    //             // Combine the forces
    //             Vector3 force = horizontalDirection * forwardForceMagnitude + Vector3.up * upwardForceMagnitude;

    //             // STEP 5: Apply the force
    //             rb.AddForce(force, ForceMode.Acceleration);
    //             Debug.Log($"Applied force: {force}");

    //             // STEP 6: Clean up
    //             Destroy(golfBallInstance, 2f); // Adjust the lifetime as needed
    //         }
    //     }
    //     else
    //     {
    //         Debug.Log("Target is not visible, cannot hit golf ball");
    //     }
    // }

    // public void HitGolfBallRoughFailure()
    // {
    //     if (!imageTargetTracker.isTargetVisible)
    //     {
    //         Debug.Log("Target is not visible, cannot hit golf ball");
    //         return;
    //     }

    //     // STEP 1: Get the target position
    //     cachedTargetPosition = imageTargetTracker.GetImageTargetLocation().position;
    //     Debug.Log($"Golf Ball needs to be hit to position: {cachedTargetPosition}");

    //     // STEP 2: Spawn the ball in front of the camera
    //     Vector3 golfBallSpawnPosition = Camera.main.transform.TransformPoint(new Vector3(0.0f, -0.07f, 0.5f));
    //     GameObject golfBallInstance = Instantiate(golfBall, golfBallSpawnPosition, Quaternion.identity);
    //     Rigidbody rb = golfBallInstance.GetComponent<Rigidbody>();
    //     if (rb == null)
    //     {
    //         Debug.LogWarning("No Rigidbody found on golf ball!");
    //         return;
    //     }

    //     // STEP 3: Set up Rigidbody for physics
    //     rb.mass = 1f;
    //     rb.useGravity = true;
    //     rb.isKinematic = false;
    //     rb.interpolation = RigidbodyInterpolation.Interpolate;
    //     rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

    //     // STEP 4: Compute displacement and desired flight time
    //     Vector3 displacement = cachedTargetPosition - golfBallSpawnPosition;
    //     float flightTime = 2f; // Adjust this to get the desired arc

    //     // Compute horizontal velocity (x and z)
    //     Vector3 horizontalDisplacement = new Vector3(displacement.x, 0, displacement.z);
    //     Vector3 horizontalVelocity = horizontalDisplacement / flightTime;

    //     // Compute vertical velocity (y) with gravity compensation
    //     float gravity = Mathf.Abs(Physics.gravity.y);
    //     float verticalVelocity = (displacement.y / flightTime) + (0.5f * gravity * flightTime);

    //     // Combine into the initial velocity
    //     Vector3 initialVelocity = horizontalVelocity + Vector3.up * verticalVelocity;
    //     Debug.Log($"Computed initial velocity: {initialVelocity}");

    //     // STEP 5: Optionally rotate the ball to face the target
    //     Vector3 lookDirection = displacement.normalized;
    //     golfBallInstance.transform.rotation = Quaternion.LookRotation(lookDirection);

    //     // STEP 6: Apply the velocity change
    //     rb.AddForce(initialVelocity - rb.linearVelocity, ForceMode.VelocityChange);

    //     // STEP 7: Clean up (destroy the ball after its flight plus a buffer)
    //     Destroy(golfBallInstance, flightTime + 1f);
    // }

    // public void HitGolfBallPreBrezier()
    // {
    //     // Ensure that the image target is visible.
    //     if (!imageTargetTracker.isTargetVisible)
    //     {
    //         Debug.Log("Target is not visible, cannot hit golf ball");
    //         return;
    //     }

    //     // 1. Get the target's world position.
    //     Vector3 targetPos = imageTargetTracker.GetImageTargetLocation().position;
    //     Debug.Log("Target position: " + targetPos);

    //     // 2. Spawn the golf ball at the desired position.
    //     // (This is relative to the camera—but once instantiated, the ball is independent.)
    //     Vector3 spawnPos = Camera.main.transform.TransformPoint(new Vector3(0.0f, -0.07f, 0.5f));
    //     GameObject ballInstance = Instantiate(golfBall, spawnPos, Quaternion.identity);

    //     // Get the ball's Rigidbody.
    //     Rigidbody rb = ballInstance.GetComponent<Rigidbody>();
    //     if (rb == null)
    //     {
    //         Debug.LogWarning("Golf ball is missing a Rigidbody!");
    //         return;
    //     }

    //     // 3. Face the image target—but only adjust horizontal rotation.
    //     // Compute the flat (horizontal) direction from the ball's spawn to the target.
    //     Vector3 flatDirection = targetPos - spawnPos;
    //     flatDirection.y = 0;  // ignore vertical differences
    //     if (flatDirection != Vector3.zero)
    //     {
    //         ballInstance.transform.rotation = Quaternion.LookRotation(flatDirection);
    //     }

    //     // 4. Calculate how much upward force to add based on the z-distance.
    //     // For this example, we use the absolute difference along z (you can modify this as needed).
    //     float zDistance = Mathf.Abs(targetPos.z - spawnPos.z);
    //     // Define a factor to convert z distance into upward force.
    //     float upwardForceFactor = 0.5f; // tweak this factor until the arc is what you want
    //     float yForce = upwardForceFactor * zDistance;

    //     // 5. Build a unified force vector.
    //     // Start with the forward direction (which now points toward the target in x/z).
    //     Vector3 force = ballInstance.transform.forward;
    //     // Replace the y component with our calculated upward force.
    //     force.y = yForce;

    //     // Optionally, scale the overall impulse.
    //     float overallForceMagnitude = 10f;  // adjust this value as needed
    //     force = force.normalized * overallForceMagnitude;

    //     // 6. Apply the force as an impulse so that the ball immediately gains this velocity.
    //     rb.AddForce(force, ForceMode.Impulse);
    //     Debug.Log("Applied impulse: " + force);

    //     // At this point the ball is independent of the image target and camera.
    //     // Its physics simulation (and thus its motion) is solely governed by the applied force and gravity.
    // }

    // public void HitGolfBallPrefCamOffset()
    // {

    //     float controlPointHeightOffset = 1f;
    //     float travelDuration = 1f;
    //     if (!imageTargetTracker.isTargetVisible)
    //     {
    //         Debug.Log("Target is not visible, cannot hit golf ball");
    //         return;
    //     }

    //     // 1. Get the target's world position.
    //     Vector3 targetPos = imageTargetTracker.GetImageTargetLocation().position;
    //     Debug.Log("Golf Ball needs to be hit to position: " + targetPos);

    //     // 2. Spawn the golf ball slightly in front of the camera.
    //     Vector3 spawnPos = Camera.main.transform.TransformPoint(new Vector3(0f, -0.07f, 0.5f));
    //     GameObject ballInstance = Instantiate(golfBall, spawnPos, Quaternion.identity);

    //     ballInstance.transform.localScale *= 3f;

    //     // 3. Face the ball toward the target horizontally.
    //     Vector3 flatDir = targetPos - spawnPos;
    //     flatDir.y = 0;
    //     if (flatDir != Vector3.zero)
    //         ballInstance.transform.rotation = Quaternion.LookRotation(flatDir);

    //     // 4. Create a spline with two knots (start and end).

    //     // Create a new spline (which implements ISpline)
    //     Spline spline = new Spline();

    //     // Create the start knot.
    //     BezierKnot knotStart = new BezierKnot();
    //     knotStart.Position = spawnPos;
    //     // Set the outbound tangent (relative to the knot position) to point upward.
    //     // Here we want a tangent of length equal to controlPointHeightOffset in the up direction.
    //     knotStart.TangentOut = Vector3.forward * controlPointHeightOffset;
    //     spline.Add(knotStart);

    //     // Create the end knot.
    //     BezierKnot knotEnd = new BezierKnot();
    //     knotEnd.Position = targetPos;
    //     // Set the inbound tangent (relative to the knot position) to point upward.
    //     knotEnd.TangentIn = Vector3.forward * controlPointHeightOffset;
    //     spline.Add(knotEnd);

    //     // 5. Start moving the ball along the spline.
    //     StartCoroutine(MoveBallAlongSplinePreCamOffset(ballInstance, spline, travelDuration));
    // }

    // private IEnumerator MoveBallAlongSplinePreCamOffset(GameObject ball, Spline spline, float duration)
    // {
    //     float elapsed = 0f;
    //     while (elapsed < duration)
    //     {
    //         float t = elapsed / duration;
    //         // Evaluate the spline at parameter t.
    //         float3 pos, tangent, up;
    //         SplineUtility.Evaluate(spline, t, out pos, out tangent, out up);
    //         ball.transform.position = (Vector3)pos;

    //         elapsed += Time.deltaTime;
    //         yield return null;
    //     }
    //     // Ensure the ball finishes exactly at the target.
    //     float3 finalPos, finalTangent, finalUp;
    //     SplineUtility.Evaluate(spline, 1f, out finalPos, out finalTangent, out finalUp);
    //     ball.transform.position = (Vector3)finalPos;
    //     Destroy(ball);

    //     //TODO: Cause a hit
    // }


    // public void HitGolfBallBrezier()
    // {
    //     if (!imageTargetTracker.isTargetVisible)
    //     {
    //         Debug.Log("Target is not visible, cannot hit golf ball");
    //         return;
    //     }

    //     // 1. Get the target's world position.
    //     Vector3 targetPos = imageTargetTracker.GetImageTargetLocation().position;
    //     Debug.Log("Golf Ball needs to be hit to position: " + targetPos);

    //     // Cache the initial camera position at the moment of the shot.
    //     Vector3 initialCamPos = Camera.main.transform.position;

    //     // 2. Spawn the golf ball slightly in front of the camera.
    //     Vector3 spawnPos = Camera.main.transform.TransformPoint(new Vector3(0f, -0.07f, 0.5f));
    //     GameObject ballInstance = Instantiate(golfBall, spawnPos, Quaternion.identity);
    //     ballInstance.transform.localScale *= 3f;

    //     // 3. Face the ball toward the target horizontally.
    //     Vector3 flatDir = targetPos - spawnPos;
    //     flatDir.y = 0;
    //     if (flatDir != Vector3.zero)
    //         ballInstance.transform.rotation = Quaternion.LookRotation(flatDir);

    //     // 4. Create a spline (Bezier curve) with two knots (start and end).
    //     Spline spline = new Spline();

    //     // Create the start knot.
    //     BezierKnot knotStart = new BezierKnot();
    //     knotStart.Position = spawnPos;
    //     // Setting TangentOut to point upward (relative to the start knot)
    //     knotStart.TangentOut = Vector3.up * controlPointHeightOffset;
    //     spline.Add(knotStart);

    //     // Create the end knot.
    //     BezierKnot knotEnd = new BezierKnot();
    //     knotEnd.Position = targetPos;
    //     // Setting TangentIn to point upward (relative to the end knot)
    //     knotEnd.TangentIn = Vector3.up * controlPointHeightOffset;
    //     spline.Add(knotEnd);

    //     // 5. Start moving the ball along the spline, passing in the initial camera position.
    //     StartCoroutine(MoveBallAlongSpline(ballInstance, spline, travelDuration, initialCamPos));
    // }

    // private IEnumerator MoveBallAlongSplineOneLevelSmooth(GameObject ball, Spline spline, float duration, Vector3 initialCamPos)
    // {
    //     float elapsed = 0f;
    //     // We'll maintain a velocity vector for SmoothDamp.
    //     Vector3 velocity = Vector3.zero;
    //     // Define a smoothTime; lower values react faster, higher values smooth out more.
    //     float smoothTime = 0.05f;  // Adjust this value as needed.

    //     while (elapsed < duration)
    //     {
    //         float t = elapsed / duration;
    //         // Evaluate the spline at parameter t.
    //         float3 pos, tangent, up;
    //         SplineUtility.Evaluate(spline, t, out pos, out tangent, out up);

    //         // Calculate the desired position by offsetting the evaluated position with the camera's movement.
    //         Vector3 desiredPos = (Vector3)pos - (Camera.main.transform.position - initialCamPos);

    //         // Smoothly move from the current ball position toward the desired position.
    //         ball.transform.position = Vector3.SmoothDamp(ball.transform.position, desiredPos, ref velocity, smoothTime);

    //         elapsed += Time.deltaTime;
    //         yield return null;
    //     }



    //     // Ensure the ball finishes exactly at the target.
    //     float3 finalPos, finalTangent, finalUp;
    //     SplineUtility.Evaluate(spline, 1f, out finalPos, out finalTangent, out finalUp);
    //     Vector3 finalDesiredPos = (Vector3)finalPos - (Camera.main.transform.position - initialCamPos);
    //     ball.transform.position = finalDesiredPos;
    //     Destroy(ball);
    // }

    // private IEnumerator MoveBallAlongSplineBrezierSingleAvg(GameObject ball, Spline spline, float duration, Vector3 initialCamPos)
    // {
    //     float elapsed = 0f;
    //     // Variables for smoothing the ball's position.
    //     Vector3 ballVelocity = Vector3.zero;
    //     float ballSmoothTime = 0.05f; // Adjust as needed

    //     // Variables for smoothing the camera offset.
    //     Vector3 smoothedCamOffset = Vector3.zero;
    //     Vector3 offsetVelocity = Vector3.zero;
    //     float offsetSmoothTime = 0.05f; // Adjust as needed

    //     while (elapsed < duration)
    //     {
    //         float t = elapsed / duration;
    //         // Evaluate the spline at parameter t.
    //         float3 pos, tangent, up;
    //         SplineUtility.Evaluate(spline, t, out pos, out tangent, out up);

    //         // Calculate the raw camera offset from the cached (initial) position.
    //         Vector3 rawCamOffset = Camera.main.transform.position - initialCamPos;
    //         // Smooth the camera offset over time.
    //         smoothedCamOffset = Vector3.SmoothDamp(smoothedCamOffset, rawCamOffset, ref offsetVelocity, offsetSmoothTime);

    //         // The desired position for the ball is the evaluated spline position
    //         // adjusted by the smoothed camera offset.
    //         Vector3 desiredPos = (Vector3)pos - smoothedCamOffset;

    //         // Smoothly move the ball from its current position toward the desired position.
    //         ball.transform.position = Vector3.SmoothDamp(ball.transform.position, desiredPos, ref ballVelocity, ballSmoothTime);

    //         elapsed += Time.deltaTime;
    //         yield return null;
    //     }

    //     // Ensure the ball finishes exactly at the target.
    //     float3 finalPos, finalTangent, finalUp;
    //     SplineUtility.Evaluate(spline, 1f, out finalPos, out finalTangent, out finalUp);
    //     Vector3 finalRawOffset = Camera.main.transform.position - initialCamPos;
    //     // Smooth one final time.
    //     smoothedCamOffset = Vector3.SmoothDamp(smoothedCamOffset, finalRawOffset, ref offsetVelocity, offsetSmoothTime);
    //     ball.transform.position = (Vector3)finalPos - smoothedCamOffset;
    //     Destroy(ball);
    // }

    // private IEnumerator MoveBallAlongSpline(GameObject ball, Spline spline, float duration, Vector3 initialCamPos)
    // {
    //     float elapsed = 0f;

    //     // Variables for smoothing the ball's position.
    //     Vector3 ballVelocity = Vector3.zero;
    //     float ballSmoothTime = 0.05f; // Adjust as needed

    //     // Variables for smoothing the camera offset.
    //     Vector3 smoothedCamOffset = Vector3.zero;
    //     Vector3 offsetVelocity = Vector3.zero;
    //     float offsetSmoothTime = 0.05f; // Adjust as needed

    //     // Buffer for averaging evaluated spline positions.
    //     const int bufferSize = 5; // Number of samples to average
    //     Queue<Vector3> positionBuffer = new Queue<Vector3>();

    //     while (elapsed < duration)
    //     {
    //         float t = elapsed / duration;
    //         // Evaluate the spline at parameter t.
    //         float3 pos, tangent, up;
    //         SplineUtility.Evaluate(spline, t, out pos, out tangent, out up);
    //         Vector3 evaluatedPos = (Vector3)pos;

    //         // Add the evaluated position to the buffer.
    //         positionBuffer.Enqueue(evaluatedPos);
    //         if (positionBuffer.Count > bufferSize)
    //         {
    //             positionBuffer.Dequeue();
    //         }

    //         // Compute the average (moving average) of the buffer.
    //         Vector3 averagedPos = Vector3.zero;
    //         foreach (Vector3 sample in positionBuffer)
    //         {
    //             averagedPos += sample;
    //         }
    //         averagedPos /= positionBuffer.Count;

    //         // Calculate the raw camera offset from the cached (initial) position.
    //         Vector3 rawCamOffset = Camera.main.transform.position - initialCamPos;
    //         // Smooth the camera offset over time.
    //         smoothedCamOffset = Vector3.SmoothDamp(smoothedCamOffset, rawCamOffset, ref offsetVelocity, offsetSmoothTime);

    //         // The desired position for the ball is the averaged evaluated position adjusted by the smoothed camera offset.
    //         Vector3 desiredPos = averagedPos - smoothedCamOffset;

    //         // Smoothly move the ball from its current position toward the desired position.
    //         ball.transform.position = Vector3.SmoothDamp(ball.transform.position, desiredPos, ref ballVelocity, ballSmoothTime);

    //         elapsed += Time.deltaTime;
    //         yield return null;
    //     }

    //     // Final evaluation at t=1.
    //     float3 finalPos, finalTangent, finalUp;
    //     SplineUtility.Evaluate(spline, 1f, out finalPos, out finalTangent, out finalUp);
    //     Vector3 finalRawOffset = Camera.main.transform.position - initialCamPos;
    //     smoothedCamOffset = Vector3.SmoothDamp(smoothedCamOffset, finalRawOffset, ref offsetVelocity, offsetSmoothTime);
    //     ball.transform.position = (Vector3)finalPos - smoothedCamOffset;
    //     player_2.TakeDamage(10, "golf");
    //     Destroy(ball);
    // }

    // public void HitGolfBallTooHigh()
    // {
    //     // Check if the target is visible
    //     if (!imageTargetTracker.isTargetVisible)
    //     {
    //         Debug.Log("Target is not visible, cannot hit golf ball");
    //         return;
    //     }

    //     // 1. Get the target's world position.
    //     Vector3 targetPos = imageTargetTracker.GetImageTargetLocation().position;
    //     Debug.Log("Target position: " + targetPos);

    //     // 2. Spawn the golf ball slightly in front of the camera.
    //     Vector3 spawnPos = Camera.main.transform.TransformPoint(new Vector3(0f, -0.07f, 0.5f));
    //     GameObject ballInstance = Instantiate(golfBall, spawnPos, Quaternion.identity);

    //     // Optionally, scale the ball if needed.
    //     ballInstance.transform.localScale *= 3f;

    //     Rigidbody rb = ballInstance.GetComponent<Rigidbody>();
    //     if (rb == null)
    //     {
    //         Debug.LogWarning("Golf ball is missing a Rigidbody!");
    //         return;
    //     }

    //     // 3. Rotate the ball to face the target horizontally.
    //     Vector3 flatDir = targetPos - spawnPos;
    //     flatDir.y = 0f; // ignore vertical difference
    //     if (flatDir != Vector3.zero)
    //     {
    //         ballInstance.transform.rotation = Quaternion.LookRotation(flatDir);
    //     }

    //     // 4. Prepare the Rigidbody settings for projectile motion.
    //     rb.useGravity = true;
    //     rb.isKinematic = false;
    //     rb.interpolation = RigidbodyInterpolation.Interpolate;
    //     rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

    //     // 5. Compute the projectile motion parameters.
    //     // Set a flight time (in seconds) for the ball to reach the target.
    //     float flightTime = 2f; // Tweak as needed

    //     // Displacement from spawn to target.
    //     Vector3 displacement = targetPos - spawnPos;

    //     // Horizontal displacement (ignore vertical component).
    //     Vector3 horizontalDisplacement = new Vector3(displacement.x, 0f, displacement.z);
    //     float horizontalDistance = horizontalDisplacement.magnitude;

    //     // Vertical displacement.
    //     float verticalDistance = displacement.y;

    //     // Get the magnitude of gravity (positive value).
    //     float gravity = Mathf.Abs(Physics.gravity.y);

    //     // Compute required initial vertical velocity:
    //     float initialVy = verticalDistance / flightTime + 0.5f * gravity * flightTime;

    //     // Compute required horizontal speed:
    //     float horizontalSpeed = horizontalDistance / flightTime;

    //     // Determine horizontal direction.
    //     Vector3 horizontalDir = horizontalDisplacement.normalized;

    //     // Combine horizontal and vertical components to get launch velocity.
    //     Vector3 launchVelocity = horizontalDir * horizontalSpeed + Vector3.up * initialVy;

    //     // 6. Apply the launch velocity using AddForce (instant change in velocity).
    //     rb.AddForce(launchVelocity, ForceMode.VelocityChange);
    //     Debug.Log("Launched ball with velocity: " + launchVelocity);

    //     // 7. Optionally, destroy the ball after a set time.
    //     Destroy(ballInstance, flightTime + 1f);
    // }

    // public void HitGolfBall()
    // {
    //     if (!imageTargetTracker.isTargetVisible)
    //     {
    //         Debug.Log("Target is not visible, cannot hit golf ball");
    //         return;
    //     }

    //     // 1. Get the target's world position.
    //     Vector3 targetPos = imageTargetTracker.GetImageTargetLocation().position;
    //     Debug.Log("Target position: " + targetPos);

    //     // 2. Spawn the golf ball slightly in front of the camera.
    //     Vector3 spawnPos = Camera.main.transform.TransformPoint(new Vector3(0f, -0.07f, 0.5f));
    //     GameObject ballInstance = Instantiate(golfBall, spawnPos, Quaternion.identity);
    //     ballInstance.transform.localScale *= 3f;

    //     // 3. Rotate the ball to face the target horizontally.
    //     Vector3 flatDir = targetPos - spawnPos;
    //     flatDir.y = 0f; // ignore vertical component for rotation
    //     if (flatDir != Vector3.zero)
    //         ballInstance.transform.rotation = Quaternion.LookRotation(flatDir);

    //     // 4. Get the Rigidbody and configure it.
    //     Rigidbody rb = ballInstance.GetComponent<Rigidbody>();
    //     if (rb == null)
    //     {
    //         Debug.LogWarning("Golf ball is missing a Rigidbody!");
    //         return;
    //     }
    //     rb.useGravity = true;
    //     rb.isKinematic = false;
    //     rb.interpolation = RigidbodyInterpolation.Interpolate;
    //     rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

    //     // 5. Compute projectile parameters.
    //     // Choose a flight time (the time in the air if unhindered).
    //     float flightTime = 2f; // tweak as needed

    //     // Displacement from spawn to target.
    //     Vector3 displacement = targetPos - spawnPos;

    //     // Horizontal displacement (ignoring vertical).
    //     Vector3 horizontalDisplacement = new Vector3(displacement.x, 0f, displacement.z);
    //     float horizontalDistance = horizontalDisplacement.magnitude;
    //     float horizontalSpeed = horizontalDistance / flightTime;

    //     // Vertical displacement.
    //     float verticalDistance = displacement.y;
    //     float gravity = Mathf.Abs(Physics.gravity.y);

    //     // Normally, vertical velocity would be:
    //     //   v_y = verticalDistance/flightTime + 0.5 * gravity * flightTime
    //     // We'll reduce it by an arcFactor so that most velocity goes horizontally.
    //     float arcFactor = 0.5f; // lower than 1 means less upward velocity.
    //     float initialVy = arcFactor * (verticalDistance / flightTime + 0.5f * gravity * flightTime);

    //     // Horizontal direction.
    //     Vector3 horizontalDir = horizontalDisplacement.normalized;

    //     // Combine components.
    //     Vector3 launchVelocity = horizontalDir * horizontalSpeed + Vector3.up * initialVy;

    //     // 6. Apply the calculated velocity.
    //     rb.AddForce(launchVelocity, ForceMode.VelocityChange);
    //     Debug.Log("Launch velocity: " + launchVelocity);

    //     // 7. Start a coroutine to destroy the ball when it crosses the target's z plane.
    //     StartCoroutine(CheckForZPlaneCollision(ballInstance, targetPos, spawnPos));
    // }

    // private IEnumerator CheckForZPlaneCollision(GameObject ball, Vector3 targetPos, Vector3 spawnPos)
    // {
    //     // Determine if we're moving in the positive or negative z direction.
    //     bool movingPositive = targetPos.z >= spawnPos.z;

    //     while (ball != null)
    //     {
    //         float ballZ = ball.transform.position.z;
    //         if (movingPositive)
    //         {
    //             if (ballZ >= targetPos.z)
    //             {
    //                 player_2.TakeDamage(10, "golf");
    //                 Destroy(ball);
    //                 yield break;
    //             }
    //         }
    //         else
    //         {
    //             if (ballZ <= targetPos.z)
    //             {
    //                 player_2.TakeDamage(10, "golf");
    //                 Destroy(ball);
    //                 yield break;
    //             }
    //         }
    //         yield return null;
    //     }
    // }

    // public void HitGolfBall()
    // {
    //     if (!imageTargetTracker.isTargetVisible)
    //     {
    //         Debug.Log("Target is not visible, cannot hit golf ball");
    //         return;
    //     }

    //     // 1. Get the target's world position.
    //     Vector3 targetPos = imageTargetTracker.GetImageTargetLocation().position;
    //     Debug.Log("Target position: " + targetPos);

    //     // 2. Spawn the golf ball slightly in front of the camera.
    //     Vector3 spawnPos = Camera.main.transform.TransformPoint(new Vector3(0f, -0.07f, 0.5f));
    //     GameObject ballInstance = Instantiate(golfBall, spawnPos, Quaternion.identity);
    //     ballInstance.transform.localScale *= 3f;

    //     // 3. Rotate the ball to face the target horizontally.
    //     Vector3 flatDir = targetPos - spawnPos;
    //     flatDir.y = 0f; // ignore vertical component for rotation
    //     if (flatDir != Vector3.zero)
    //         ballInstance.transform.rotation = Quaternion.LookRotation(flatDir);

    //     // 4. Get the Rigidbody and configure it.
    //     Rigidbody rb = ballInstance.GetComponent<Rigidbody>();
    //     if (rb == null)
    //     {
    //         Debug.LogWarning("Golf ball is missing a Rigidbody!");
    //         return;
    //     }
    //     rb.useGravity = true;
    //     rb.isKinematic = false;
    //     rb.interpolation = RigidbodyInterpolation.Interpolate;
    //     rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

    //     // 5. Compute projectile parameters.
    //     float flightTime = 2f; // tweak as needed

    //     // Displacement from spawn to target.
    //     Vector3 displacement = targetPos - spawnPos;

    //     // Horizontal displacement (ignoring vertical).
    //     Vector3 horizontalDisplacement = new Vector3(displacement.x, 0f, displacement.z);
    //     float horizontalDistance = horizontalDisplacement.magnitude;
    //     float horizontalSpeed = horizontalDistance / flightTime;

    //     // Vertical displacement.
    //     float verticalDistance = displacement.y;
    //     float gravity = Mathf.Abs(Physics.gravity.y);

    //     // Normally, vertical velocity would be:
    //     //   v_y = verticalDistance/flightTime + 0.5 * gravity * flightTime
    //     // We'll reduce it by an arcFactor so that most velocity goes horizontally.
    //     float arcFactor = 0.5f; // lower than 1 means less upward velocity.
    //     float initialVy = arcFactor * (verticalDistance / flightTime + 0.5f * gravity * flightTime);

    //     // Horizontal direction.
    //     Vector3 horizontalDir = horizontalDisplacement.normalized;

    //     // Combine components.
    //     Vector3 launchVelocity = horizontalDir * horizontalSpeed + Vector3.up * initialVy;

    //     // 6. Apply the calculated velocity.
    //     rb.AddForce(launchVelocity, ForceMode.VelocityChange);
    //     Debug.Log("Launch velocity: " + launchVelocity);

    //     // 7. Start a coroutine to detect when the ball is near the target.
    //     // Here we use a distance threshold instead of a z-plane.
    //     StartCoroutine(CheckForTargetCollision(ballInstance, targetPos, 0.5f)); // threshold can be adjusted
    // }

    // /// <summary>
    // /// Checks every frame if the ball is within a specified distance of the target position.
    // /// When it is, the target takes damage, and the ball is destroyed.
    // /// </summary>
    // private IEnumerator CheckForTargetCollision(GameObject ball, Vector3 targetPos, float threshold)
    // {
    //     while (ball != null)
    //     {
    //         float distance = Vector3.Distance(ball.transform.position, targetPos);
    //         if (distance <= threshold)
    //         {
    //             player_2.TakeDamage(10, "golf");
    //             Destroy(ball);
    //             yield break;
    //         }
    //         yield return null;
    //     }
    // }

    // public void HitGolfBall()
    // {
    //     if (!imageTargetTracker.isTargetVisible)
    //     {
    //         Debug.Log("Target is not visible, cannot hit golf ball");
    //         return;
    //     }

    //     // 1. Get the target's world position.
    //     Vector3 targetPos = imageTargetTracker.GetImageTargetLocation().position;
    //     Debug.Log("Golf Ball needs to be hit to position: " + targetPos);

    //     // Cache the initial camera position.
    //     Vector3 initialCamPos = Camera.main.transform.position;

    //     // 2. Spawn the golf ball slightly in front of the camera.
    //     Vector3 spawnPos = Camera.main.transform.TransformPoint(new Vector3(0f, -0.07f, 0.5f));
    //     GameObject ballInstance = Instantiate(golfBall, spawnPos, Quaternion.identity);
    //     ballInstance.transform.localScale *= 3f;

    //     // 3. Face the ball toward the target horizontally.
    //     Vector3 flatDir = targetPos - spawnPos;
    //     flatDir.y = 0;
    //     if (flatDir != Vector3.zero)
    //         ballInstance.transform.rotation = Quaternion.LookRotation(flatDir);

    //     // 4. Create a spline (Bezier curve) with two knots (start and end).
    //     Spline spline = new Spline();

    //     // Start knot
    //     BezierKnot knotStart = new BezierKnot();
    //     knotStart.Position = spawnPos;
    //     // Use upward tangent to generate a lob (you can adjust this vector if needed)
    //     knotStart.TangentOut = Vector3.up * controlPointHeightOffset;
    //     spline.Add(knotStart);

    //     // End knot
    //     BezierKnot knotEnd = new BezierKnot();
    //     knotEnd.Position = targetPos;
    //     knotEnd.TangentIn = Vector3.up * controlPointHeightOffset;
    //     spline.Add(knotEnd);

    //     // 5. Use DOTween to tween a parameter t from 0 to 1 over the travel duration.
    //     float t = 0f;
    //     Vector3 smoothedCamOffset = Vector3.zero;
    //     float offsetSmoothTime = 0.05f; // Adjust to control the smoothing of the camera offset

    //     DOTween.To(() => t, x => t = x, 1f, travelDuration)
    //            .SetEase(Ease.Linear)
    //            .OnUpdate(() =>
    //            {
    //                // Evaluate the spline at parameter t.
    //                float3 pos, tangent, up;
    //                SplineUtility.Evaluate(spline, t, out pos, out tangent, out up);

    //                // Calculate the raw camera offset from the cached position.
    //                Vector3 rawCamOffset = Camera.main.transform.position - initialCamPos;
    //                // Smooth the camera offset.
    //                smoothedCamOffset = Vector3.Lerp(smoothedCamOffset, rawCamOffset, Time.deltaTime / offsetSmoothTime);

    //                // Update ball position: subtract the smoothed camera offset.
    //                ballInstance.transform.position = (Vector3)pos - smoothedCamOffset;
    //            })
    //            .OnComplete(() =>
    //            {
    //                // Final evaluation to ensure the ball reaches the target.
    //                float3 finalPos, finalTangent, finalUp;
    //                SplineUtility.Evaluate(spline, 1f, out finalPos, out finalTangent, out finalUp);
    //                Vector3 finalRawOffset = Camera.main.transform.position - initialCamPos;
    //                smoothedCamOffset = Vector3.Lerp(smoothedCamOffset, finalRawOffset, Time.deltaTime / offsetSmoothTime);
    //                ballInstance.transform.position = (Vector3)finalPos - smoothedCamOffset;

    //                // Optionally, trigger a hit or cleanup.
    //                Destroy(ballInstance);
    //            });
    // }

    // public void HitGolfBall()
    // {
    //     if (!imageTargetTracker.isTargetVisible)
    //     {
    //         Debug.Log("Target not visible");
    //         return;
    //     }
    //     else
    //     {
    //         Debug.Log($"This is the camera position {Camera.main.transform.position}");
    //         Debug.Log($"This is the target position {imageTargetTracker.GetImageTargetLocation().position}");
    //     }
    // }

    // public void HitGolfBallLob()
    // {
    //     if (!imageTargetTracker.isTargetVisible)
    //     {
    //         Debug.Log("Target is not visible, cannot hit golf ball");
    //         return;
    //     }

    //     // 1. Get the target's world position.
    //     Vector3 targetPos = imageTargetTracker.GetImageTargetLocation().position;
    //     Debug.Log("Target position: " + targetPos);

    //     // 2. Spawn the golf ball slightly in front of the camera.
    //     Vector3 spawnPos = Camera.main.transform.TransformPoint(new Vector3(0f, -0.07f, 0.5f));
    //     GameObject ballInstance = Instantiate(golfBall, spawnPos, Quaternion.identity);
    //     ballInstance.transform.localScale *= 2.5f;

    //     // 3. Get the Rigidbody.
    //     Rigidbody rb = ballInstance.GetComponent<Rigidbody>();
    //     if (rb == null)
    //     {
    //         Debug.LogWarning("Golf ball is missing a Rigidbody!");
    //         return;
    //     }

    //     // 4. Determine projectile motion parameters.
    //     // Set the desired flight time (how long the ball should be in the air).
    //     float flightTime = 2f; // Adjust this value as needed.

    //     // Calculate displacement from spawn to target.
    //     Vector3 displacement = targetPos - spawnPos;

    //     // Horizontal displacement: ignore the vertical component.
    //     Vector3 horizontalDisplacement = new Vector3(displacement.x, 0f, displacement.z);
    //     float horizontalDistance = horizontalDisplacement.magnitude;
    //     Vector3 horizontalDir = horizontalDisplacement.normalized;

    //     // Horizontal speed required to cover the distance in flightTime.
    //     float horizontalSpeed = horizontalDistance / flightTime;

    //     // Gravity (positive value).
    //     float gravity = Mathf.Abs(Physics.gravity.y);

    //     // Compute the required initial vertical velocity using the equation:
    //     // v_y = (verticalDistance / flightTime) + (0.5 * gravity * flightTime)
    //     float initialVy = (displacement.y / flightTime) + (0.5f * gravity * flightTime);

    //     // 5. Combine horizontal and vertical components into the launch velocity.
    //     Vector3 launchVelocity = horizontalDir * horizontalSpeed + Vector3.up * initialVy;

    //     // 6. Rotate the ball to face the target horizontally.
    //     Vector3 flatDir = targetPos - spawnPos;
    //     flatDir.y = 0f; // ignore vertical component for rotation
    //     if (flatDir != Vector3.zero)
    //     {
    //         ballInstance.transform.rotation = Quaternion.LookRotation(flatDir);
    //     }

    //     // 7. Configure the Rigidbody.
    //     rb.useGravity = true;
    //     rb.isKinematic = false;
    //     rb.interpolation = RigidbodyInterpolation.Interpolate;
    //     rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

    //     // 8. Apply the launch velocity.
    //     // Using VelocityChange instantly sets the velocity regardless of mass.
    //     rb.AddForce(launchVelocity, ForceMode.Impulse);
    //     Debug.Log("Launch velocity: " + launchVelocity);

    //     // 9. Destroy the ball after it's had time to land (or optionally on impact).
    //     Destroy(ballInstance, flightTime + 1f);
    // }

    void Update()
    {
        // if (snowStormOne != null)
        // {
        //     // Vector3 currPos = snowStormOne.transform.position;
        //     // Debug.Log($"This is the original object position {initSnowStormPos1}");
        //     // Debug.Log($"This is the original camera position {initCameraPos1}");
        //     Vector3 camDelta = Camera.main.transform.position - initCameraPos1;

        //     // float boostFactor = 1.5f;
        //     // if (Mathf.Abs(camDelta.x) < 1f)
        //     // {
        //     //     camDelta.x *= boostFactor;
        //     // }
        //     // // Similarly for y
        //     // if (Mathf.Abs(camDelta.y) < 1f)
        //     // {
        //     //     camDelta.y *= boostFactor;
        //     // }
        //     // camDelta *= 5f;

        //     snowStormOne.transform.position = initSnowStormPos1 - camDelta;
        //     // Debug.Log($"This is the current position {snowStormOne.transform.position}");
        //     // Debug.Log($"This is the current camera position {Camera.main.transform.position}");
        // }
        // else
        // {

        // }
        // if (snowStormTwo != null)
        // {
        //     // Vector3 currPos = snowStormOne.transform.position;
        //     // Debug.Log($"This is the original object position {initSnowStormPos1}");
        //     // Debug.Log($"This is the original camera position {initCameraPos1}");
        //     Vector3 camDelta = Camera.main.transform.position - initCameraPos2;

        //     // float boostFactor = 1.5f;
        //     // if (Mathf.Abs(camDelta.x) < 1f)
        //     // {
        //     //     camDelta.x *= boostFactor;
        //     // }
        //     // // Similarly for y
        //     // if (Mathf.Abs(camDelta.y) < 1f)
        //     // {
        //     //     camDelta.y *= boostFactor;
        //     // }
        //     // camDelta *= 5f;

        //     snowStormTwo.transform.position = initSnowStormPos2 - camDelta;
        //     // Debug.Log($"This is the current position {snowStormOne.transform.position}");
        //     // Debug.Log($"This is the current camera position {Camera.main.transform.position}");
        // }
        // else
        // {

        // }

        if (isReloading)
        {
            float rotationStep = rotationSpeed * Time.deltaTime; // Amount to rotate this frame
            spawnedGun.transform.Rotate(rotationStep, 0, 0); // Rotate along X-axis
            rotationAmount += rotationStep;

            // Stop rotation when 360 degrees is reached
            if (rotationAmount >= 360f)
            {
                isReloading = false;

                // Ensure that the gun faces the front
                spawnedGun.transform.rotation = Camera.main.transform.rotation * Quaternion.Euler(0, 90, 0);
            }
        }
        else
        {


        }

        // Debug.Log($"Distance: {Vector3.Distance(imageTargetTracker.transform.position, initSnowStormPos1)}");
        // TODO: Change this to dynamic image tracking systems
        // initSnowStormPos1.x += initCamDeltaOne.x;
        // initSnowStormPos1.z -= localPosZ;
        // if (snowStormOne != null)
        // {
        //     Debug.Log($"Distance SS1: {Vector3.Distance(imageTargetTracker.transform.position, initSnowStormUpdater)}");
        // }
        // else
        // {

        // }
        // if (imageTargetTracker.isTargetVisible && snowStormOne != null && Vector3.Distance(imageTargetTracker.transform.position, initSnowStormUpdater) < 1.55)
        // {
        //     Debug.Log($"Distance: {Vector3.Distance(imageTargetTracker.transform.position, initSnowStormPos1)}");
        //     if (!isInsideSnowZoneOne)
        //     {
        //         isInsideSnowZoneOne = true;


        //     }

        // }
        // else
        // {
        //     isInsideSnowZoneOne = false;
        // }

        // if (snowStormTwo != null)
        // {
        //     Debug.Log($"Distance SS2: {Vector3.Distance(imageTargetTracker.transform.position, initSnowStormUpdaterTwo)}");
        // }
        // else
        // {

        // }

        // if (imageTargetTracker.isTargetVisible && snowStormTwo != null && Vector3.Distance(imageTargetTracker.transform.position, initSnowStormUpdaterTwo) < 1.55)
        // {
        //     Debug.Log($"Distance: {Vector3.Distance(imageTargetTracker.transform.position, initSnowStormUpdaterTwo)}");
        //     if (!isInsideSnowZoneTwo)
        //     {
        //         isInsideSnowZoneTwo = true;


        //     }

        // }
        // else
        // {
        //     isInsideSnowZoneTwo = false;
        // }

    }

    public int playerOneDeaths()
    {
        return int.Parse(playerTwoScoreboard.GetComponent<TextMeshProUGUI>().text);
    }

    public void updatePlayerOneDeaths(int deaths)
    {
        playerTwoScoreboard.GetComponent<TextMeshProUGUI>().text = (deaths).ToString();
    }

    public void causeSnowStormOneDamage()
    {
        player_2.TakeDamage(5, "snow");
        Debug.Log($"User entered snow zone, applying {5} damage.");
        // isInsideSnowZoneOne = true;

        GameObject snowHitEffect = Instantiate(snowHitPrefab, imageTargetTracker.transform.position, Quaternion.identity);
        Destroy(snowHitEffect, 1f);

    }

    public void causeSnowStormMultiEffect(int countOfStorm)
    {
        player_2.TakeDamage(countOfStorm * 5, "snow");
        Debug.Log($"User entered snow, applying {countOfStorm * 5} damage.");
        GameObject snowHitEffect = Instantiate(snowHitPrefab, imageTargetTracker.transform.position, Quaternion.identity);
        Destroy(snowHitEffect, 1f);
    }

    public void causeSnowStormTwoDamage()
    {
        player_2.TakeDamage(5, "snow");
        Debug.Log($"User entered snow zone, applying {5} damage.");
        // isInsideSnowZoneTwo = true;

        GameObject snowHitEffect = Instantiate(snowHitPrefab, imageTargetTracker.transform.position, Quaternion.identity);
        Destroy(snowHitEffect, 1f);

    }

    public void spawnSnowStormOnMe()
    {
        // Spawn on the main camera
        Vector3 currentPos = Camera.main.transform.position;


        // Set this to 0
        // currentPos.y = Camera.main.transform.position.y - 1;
        currentPos.y = 1;

        GameObject spawnedCylinderPrefabOnMe = Instantiate(cylinderPrefabOnMe, currentPos, Quaternion.Euler(Vector3.up));

        spawnedCylinderPrefabOnMe.transform.SetParent(null);


    }



}

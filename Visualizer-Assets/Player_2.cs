using UnityEngine;
using TMPro;
using Vuforia;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using System.Text;
using System;

public class Player_2 : MonoBehaviour
{

    public int maxHealth = 100;

    public int shieldMaxCount = 3;
    public int currentHealth;

    public HealthBar2 healthBar;

    public Shield2 shield;

    public int currentShieldCount;
    public int currentShieldStrength;

    public bool shieldStatus;

    public GameObject playerOneScoreboard;

    public GameObject shieldPrefab;

    public ImageTargetTracker imageTargetTracker;

    // Viz to Eval MQ
    // private MqttClient client;
    // private string MQTT_BROKER = "armadillo.rmq.cloudamqp.com";
    // private int MQTT_PORT = 1883;  // Default MQTT Port
    // private string MQTT_TOPIC = "topic/visibility";  // Topic to subscribe to
    // private string MQTT_USER = "swnviqwj:swnviqwj"; // Extracted from "swnviqwj:swnviqwj"
    // private string MQTT_PW = "ZOCHf82N8XcNHID-oGHtJUe8gkfxFfqN";


    // Initialize code for the shield to be shown on the QRCode
    // public GameObject qrCode;
    // private GameObject activeShield;
    // public GameObject shieldPrefab;

    public void InititlizePlayerTwo()
    {
        Debug.Log("Player 2 has been initialized");
        healthBar.SetMaxHealth(maxHealth);

        /// Set the current health as the max health
        currentHealth = maxHealth;

        // Set the shields and shields count
        // Shield disappear
        shield.gameObject.SetActive(false);

        currentShieldCount = shieldMaxCount;

        // Set the scoreboard
        playerOneScoreboard.GetComponent<TextMeshProUGUI>().text = 0.ToString();

        // Ensure that the shield is initially inactive
        shieldPrefab.SetActive(false);

        //TODO: Make sure MQ initialized here
        // InitVizToEvalMQ();

        // Debug.Log($"📩 MQ has been init");

    }

    // TakeDamage will have to change to account for other cases
    // public void TakeDamage(int damage, string damageType)
    // {
    //     // Check if the opponent is visible to begin with
    //     if (imageTargetTracker != null && imageTargetTracker.isTargetVisible)
    //     {
    //         //TODO: Return True for the MQ here
    //         // client.Publish(MQTT_TOPIC, Encoding.UTF8.GetBytes("True"), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
    //         // Debug.Log($"📩 True Sent to MQ");

    //         // check if you get pushed to zero
    //         if (shieldStatus)
    //         {
    //             // negate the damage
    //             currentShieldStrength -= damage;

    //             // Set the shield explosion here
    //             imageTargetTracker.SpawnHit(true, damageType);

    //             if (currentShieldStrength <= 0)
    //             {
    //                 shieldStatus = false;


    //                 // Make the shield disappear
    //                 shield.gameObject.SetActive(false);

    //                 // Make the shieldPrefab disapear
    //                 shieldPrefab.SetActive(false);

    //                 // needa subtract from player
    //                 int playerTransferredDamage = -1 * currentShieldStrength;

    //                 if (currentHealth - playerTransferredDamage <= 0)
    //                 {
    //                     // Replace the health and change the score counter
    //                     Debug.Log("Shield Transference caused, health set is 0, Rebirth invoked");

    //                     // Update the score counter
    //                     playerOneScoreboard.GetComponent<TextMeshProUGUI>().text = (
    //                          int.Parse(playerOneScoreboard.GetComponent<TextMeshProUGUI>().text) + 1
    //                          ).ToString();

    //                     // Replace health
    //                     currentHealth = maxHealth;
    //                     healthBar.SetMaxHealth(currentHealth);
    //                     Debug.Log("Health set to" + currentHealth);

    //                     //Update the shieldCount and the Shield Display
    //                     shield.gameObject.SetActive(false);

    //                     // Set the shield prefab to invisible
    //                     shieldPrefab.SetActive(false);
    //                     currentShieldCount = shieldMaxCount;
    //                     Debug.Log("Shield Count set to" + currentShieldCount);

    //                 }
    //                 else
    //                 {
    //                     currentHealth -= playerTransferredDamage;
    //                     healthBar.SetHealth(currentHealth);
    //                 }
    //             }
    //             else
    //             {
    //                 shield.SetStrength(currentShieldStrength);
    //             }
    //         }
    //         else
    //         {
    //             // Spawn the hit effect
    //             imageTargetTracker.SpawnHit(false, damageType);

    //             // Check the current health and damage correlation
    //             if (currentHealth - damage <= 0)
    //             {
    //                 // Replace the health and change the score counter
    //                 Debug.Log("Health reduced to nothing, perform rebirth");

    //                 // Update the score counter
    //                 playerOneScoreboard.GetComponent<TextMeshProUGUI>().text = (
    //                          int.Parse(playerOneScoreboard.GetComponent<TextMeshProUGUI>().text) + 1
    //                          ).ToString();

    //                 // Replace health
    //                 currentHealth = maxHealth;
    //                 healthBar.SetMaxHealth(currentHealth);
    //                 Debug.Log("Health set to" + currentHealth);

    //                 //Update the shieldCount and the Shield Display
    //                 shield.gameObject.SetActive(false);
    //                 shieldPrefab.SetActive(false);
    //                 currentShieldCount = shieldMaxCount;
    //                 Debug.Log("Shield Count set to" + currentShieldCount);

    //             }
    //             else
    //             {
    //                 currentHealth -= damage;
    //                 healthBar.SetHealth(currentHealth);
    //             }
    //         }
    //     }
    //     else
    //     {
    //         //TODO: Send False to the MQ here:
    //         // client.Publish(MQTT_TOPIC, Encoding.UTF8.GetBytes("False"), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
    //         // Debug.Log($"📩 False Sent to MQ");
    //         // Spawn the miss shot somewhere else

    //         imageTargetTracker.SpawnHit(false, damageType);
    //         Debug.Log("Player2 not visible, damage negated");
    //         Debug.Log("imageTargetStatus: " + imageTargetTracker);
    //         Debug.Log("image visibility status" + imageTargetTracker.isTargetVisible);
    //     }
    // }

    public void TakeDamage(int damage, string damageType)
    {
        // Check if the opponent is visible to begin with
        if (imageTargetTracker != null && imageTargetTracker.isTargetVisible)
        {

            // check if you get pushed to zero
            if (shieldStatus)
            {

                // Set the shield explosion here
                imageTargetTracker.SpawnHit(true, damageType);

            }
            else
            {
                // Spawn the hit effect
                imageTargetTracker.SpawnHit(false, damageType);
            }
        }
        else
        {
            imageTargetTracker.SpawnHit(false, damageType);
        }
    }

    public void UseShield()
    {
        // Check if player 2 is even visible or not
        if (imageTargetTracker != null && imageTargetTracker.isTargetVisible)
        {
            // Check if you have enough shields to begin with
            if (shieldStatus)
            {
                Debug.Log("Shield is already active, " + currentShieldCount + " left");
            }
            else if (currentShieldCount == 0)
            {
                Debug.Log("Player1 cannot use shields since no shields are left");
            }
            else
            {
                Debug.Log("Player made use of a shield");
                currentShieldCount -= 1;
                shieldStatus = true;
                currentShieldStrength = 30;

                // Activate the slider to see this at first
                shield.SetMaxStength(30);

                // Make the shield bar appear
                shield.gameObject.SetActive(true);

                // Make use of Shield Display
                shieldPrefab.SetActive(true);


            }
        }
        else
        {
            Debug.Log("Player2 not visible, hence cannot draw shield");
            Debug.Log("imageTargetStatus: " + imageTargetTracker);
            Debug.Log("image visibility status" + imageTargetTracker.isTargetVisible);
        }
    }

    // public void InitVizToEvalMQ()
    // {
    //     try
    //     {
    //         Debug.Log("📡 Connecting to RabbitMQ via MQTT...");

    //         // 1️⃣ Create MQTT Client
    //         client = new MqttClient(MQTT_BROKER, MQTT_PORT, false, null, null, MqttSslProtocols.None);

    //         // 2️⃣ Connect to MQTT Broker
    //         client.Connect(Guid.NewGuid().ToString(), MQTT_USER, MQTT_PW);

    //         if (client.IsConnected)
    //         {
    //             Debug.Log("✅ MQTT Connected Successfully!");

    //             // // 3️⃣ Subscribe to a Topic
    //             // client.MqttMsgPublishReceived += OnMessageReceived;
    //             client.Subscribe(new string[] { MQTT_TOPIC }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE });
    //             Debug.Log($"📩 Subscribed to: {MQTT_TOPIC}");

    //         }
    //         else
    //         {
    //             Debug.LogError("❌ Failed to connect to MQTT broker.");
    //         }
    //     }
    //     catch (Exception ex)
    //     {
    //         Debug.LogError($"❌ MQTT Connection Error: {ex.Message}");
    //     }
    // }

    public int getPlayerTwoDeaths()
    {
        return int.Parse(playerOneScoreboard.GetComponent<TextMeshProUGUI>().text);
    }

    public void updatePlayerTwoDeath(int deaths)
    {
        playerOneScoreboard.GetComponent<TextMeshProUGUI>().text = (deaths).ToString();
    }

    // public void shieldDamageCreator() {

    // }


}

using UnityEngine;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using System.Text;
using System;
using System.Collections.Concurrent;
using TMPro;
using UnityEngine.Rendering;
using UnityEngine.UI;
using System.Linq;
using JetBrains.Annotations;
using System.Collections;
using UnityEngine.Rendering.Universal;



public class GameStateMachine : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public const float thresholdScore = 0.7f;
    public Player_1 player_1;

    public Player_2 player_2;
    public GameObject p1ShootButton;

    public GameObject p2ShootButton;
    public GameObject p1ShieldButton;
    public GameObject p2ShieldButton;
    public GameObject p1SnowBombButton;
    public GameObject p2SnowBombButton;

    public GameObject p1GolfSwingButton;

    public GameObject p2ActionButton;

    public GameObject p1BadmintonSwingButton;

    public GameObject p1BoxButton;

    public GameObject p1SwordSwingButton;

    public GameObject GNumberP1Reload;
    public GameObject LaunchStereoModeButton;

    public GameObject playerSelectorOne;

    public GameObject playerSelectorTwo;

    public GameObject numberOneIndicator;

    public GameObject numberTwoIndicator;

    // Set up the informer
    public GameObject actionQualityInformer;
    public TextMeshProUGUI action;
    // public TextMeshProUGUI status;
    // public TextMeshProUGUI confidenceScoreDisplay;

    // Set up the queue informer
    public GameObject noLaserTagIcon;
    public GameObject laserTagIcon;
    public GameObject noBcastIcon;
    public GameObject BcastIcon;
    public GameObject noSnowListener;
    public GameObject snowListener;
    public GameObject noSnowSender;
    public GameObject snowSender;
    public GameObject noVisSender;
    public GameObject visSender;
    private ConcurrentQueue<Action> _mainThreadQueue = new ConcurrentQueue<Action>();
    // private ConcurrentQueue<string> actionQueue = new ConcurrentQueue<string>();
    private ConcurrentQueue<GameStateWrapper> stateQueue = new ConcurrentQueue<GameStateWrapper>();

    private ConcurrentQueue<string> snowqueue = new ConcurrentQueue<string>();


    // public TextMeshProUGUI worldCenterIndicator;

    // Anchor Status images
    public GameObject worldPositionNotSetAnchor;

    public GameObject worldPositionSetAnchor;

    // Hold onto snow storm converters
    public TextMeshProUGUI collidingSnowStormCountText;

    // Settle the player id
    public int playerId;

    // SET UP MQTT
    private MqttClient listener;
    private MqttClient listener_broadcast;

    private MqttClient listener_snow_bomb;
    private string MQTT_BROKER = "armadillo.rmq.cloudamqp.com";
    private int MQTT_PORT = 1883;  // Default MQTT Port

    // Note this is changing later when subscribing
    private string MQTT_TOPIC = "topic/lasertag";  // Topic to subscribe to
    private string MQTT_BCAST_TOPIC = "topic/broadcast"; // broadcast tester
    private string MQTT_TOPIC_SNOWBOMB_LISTENER = "topic/snowlistener";
    private string MQTT_USER = "jzzkaznp:jzzkaznp"; // Extracted from "swnviqwj:swnviqwj"
    private string MQTT_PW = "qsM-cpATwPPXkferAG2MjPEeMgk1tvZC";

    [System.Serializable]
    public class PlayerState
    {
        public int hp;
        public int bullets;
        public int bombs;
        public int shield_hp;
        public int death;
        public int shields;

        public PlayerState(int hp, int bullets, int bombs, int shield_hp, int death, int shields)
        {
            this.hp = hp;
            this.bullets = bullets;
            this.bombs = bombs;
            this.shield_hp = shield_hp;
            this.death = death;
            this.shields = shields;
        }
    }

    [System.Serializable]
    public class GameState
    {
        public PlayerState p1;
        public PlayerState p2;

        public GameState(PlayerState p1, PlayerState p2)
        {
            this.p1 = p1;
            this.p2 = p2;
        }
    }

    [System.Serializable]
    public class GameStateWrapper
    {
        public GameState game_state;
    }

    [System.Serializable]
    public class GameData
    {
        public int player_id;
        public string action;
        public bool isVisible;
        public GameState game_state;

        public GameData(int player_id, string move, bool isVisible, GameState game_state)
        {
            this.player_id = player_id;
            this.action = move;
            this.isVisible = isVisible;
            this.game_state = game_state;
        }
    }

    private MqttClient client_sender;
    private MqttClient snow_bomb_client;
    private string MQTT_BROKER_SENDER = "armadillo.rmq.cloudamqp.com";
    private int MQTT_PORT_SENDER = 1883;  // Default MQTT Port
    private string MQTT_TOPIC_SENDER = "topic/visibility";  // Topic to subscribe to
    private string MQTT_TOPIC_SNOWBOMB_SENDER = "topic/snowsender";
    private string MQTT_USER_SENDER = "swnviqwj:swnviqwj"; // Extracted from "swnviqwj:swnviqwj"
    private string MQTT_PW_SENDER = "ZOCHf82N8XcNHID-oGHtJUe8gkfxFfqN";


    void Start()
    {
        // Init players
        player_1.InitializePlayerOne();
        player_2.InititlizePlayerTwo();

        // Initialize world center status
        // worldCenterIndicator.text = $"<color=#FF0000>WC</color>";
        worldPositionNotSetAnchor.SetActive(true);
        worldPositionSetAnchor.SetActive(false);

        // Set action quality informer to disappear
        actionQualityInformer.SetActive(false);

        // Ensure that the buttons are off at the start
        p1ShootButton.SetActive(false);
        p2ShootButton.SetActive(false);
        p1ShieldButton.SetActive(false);
        p2ShieldButton.SetActive(false);
        p1SnowBombButton.SetActive(false);
        p2SnowBombButton.SetActive(false);
        p1GolfSwingButton.SetActive(false);
        p2ActionButton.SetActive(false);
        p1BadmintonSwingButton.SetActive(false);
        p1SwordSwingButton.SetActive(false);
        p1BoxButton.SetActive(false);
        GNumberP1Reload.SetActive(false);
        LaunchStereoModeButton.SetActive(false);
        playerSelectorOne.SetActive(false);
        playerSelectorTwo.SetActive(false);
        numberOneIndicator.SetActive(false);
        numberTwoIndicator.SetActive(false);

    }

    // 4️⃣ Handle Incoming Messages
    // void OnMessageReceived(object sender, MqttMsgPublishEventArgs e)
    // {
    //     string message = Encoding.UTF8.GetString(e.Message);
    //     Debug.Log($"📥 Received MQTT Message: {message} on {e.Topic}");

    //     if (message == "SHOOT")
    //     {
    //         PlayerOneShoot();
    //     }
    // }

    void OnMessageReceived(object sender, MqttMsgPublishEventArgs e)
    {
        string messageComposite = Encoding.UTF8.GetString(e.Message);
        Debug.Log($"📥 Received MQTT Message: {messageComposite} on {e.Topic}");

        string[] parts = messageComposite.Split(',');
        string message = parts[0];
        float confidenceScore = 0f;
        if (parts.Count() > 1)
        {
            confidenceScore = float.Parse(parts[1]);
        }

        Debug.Log($"This is conf: {confidenceScore}");
        // string score = parts[1];
        // Debug.Log($"Conf Score: {score}");

        // Queue the function call to be executed in Update()
        _mainThreadQueue.Enqueue(() =>
        {
            if (message == "gun")
            {
                PlayerOneShoot(confidenceScore);
            }
            else if (message == "boxing")
            {
                PlayerOneBox(confidenceScore);
            }
            else if (message == "golf")
            {
                PlayerOneUseGolfSwing(confidenceScore);
            }
            else if (message == "fencing")
            {
                PlayerOneSwingSword(confidenceScore);
            }
            else if (message == "badminton")
            {
                PlayerOneUseBadmintonSwing(confidenceScore);
            }
            else if (message == "snowbomb")
            {
                PlayerOneUseSnowBomb(confidenceScore);
            }
            else if (message == "shield")
            {
                PlayerOneUseShield(confidenceScore);
            }
            else if (message == "reload")
            {
                PlayerOneReload(confidenceScore);
            }
            else if (message == "logout")
            {
                PlayerOneLogout();
            }
            else
            {
                Debug.LogWarning($"{message} feature does not exist");
            }
        });
    }

    void OnBroadcastReceived(object sender, MqttMsgPublishEventArgs e)
    {
        string messageComposite = Encoding.UTF8.GetString(e.Message);
        Debug.Log($"📥 Received MQTT Message: {messageComposite} on {e.Topic}");

        try
        {
            GameStateWrapper stateWrapper = JsonUtility.FromJson<GameStateWrapper>(messageComposite);
            stateQueue.Enqueue(stateWrapper);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("❌ Failed to parse JSON: " + ex.Message);
        }
    }
    void OnSnowInfoReceived(object sender, MqttMsgPublishEventArgs e)
    {
        string messageComposite = Encoding.UTF8.GetString(e.Message);
        Debug.Log($"📥 Received MQTT Message: {messageComposite} on {e.Topic}");
        snowqueue.Enqueue("test");
        // player_1.spawnSnowStormOnMe();
    }

    // Update is called once per frame
    void Update()
    {
        while (_mainThreadQueue.TryDequeue(out Action action))
        {
            action.Invoke();
        }

        while (stateQueue.TryDequeue(out GameStateWrapper stateWrapper))
        {
            // Process all queued game states one-by-one

            // player_1 processing

            // Health Processing
            player_1.currentHealth = stateWrapper.game_state.p1.hp;
            player_1.healthBar.SetHealth(player_1.currentHealth);

            // Shield Processing (Count + hp)
            player_1.currentShieldCount = stateWrapper.game_state.p1.shields;
            player_1.UpdateShieldGUI();
            player_1.currentShieldStrength = stateWrapper.game_state.p1.shield_hp;
            if (player_1.currentShieldStrength > 0)
            {
                if (player_1.spawnedShieldPrefab != null)
                {
                    Debug.Log("Shield already present, dont add random");
                }
                else
                {
                    player_1.SpawnShieldPrefab();
                    player_1.shield.gameObject.SetActive(true);
                }
                player_1.shield.SetStrength(player_1.currentShieldStrength);
            }
            else
            {
                player_1.DespawnShieldPrefab();
                player_1.shield.gameObject.SetActive(false);
            }

            // BUllet Processing
            player_1.currentAmmoCount = stateWrapper.game_state.p1.bullets;
            player_1.UpdateAmmoGUI();

            // Snowbomb processing
            player_1.currentBombCount = stateWrapper.game_state.p1.bombs;
            player_1.UpdateSnowBombGUI();

            // player_1 deaths
            player_1.updatePlayerOneDeaths(stateWrapper.game_state.p1.death);

            // Player 2 side of things

            // player 2 health
            player_2.currentHealth = stateWrapper.game_state.p2.hp;
            player_2.healthBar.SetHealth(player_2.currentHealth);

            // player 2 shield
            player_2.currentShieldStrength = stateWrapper.game_state.p2.shield_hp;
            player_2.shield.SetStrength(player_2.currentShieldStrength);
            if (player_2.currentShieldStrength > 0)
            {
                player_2.shield.gameObject.SetActive(true);
                // Check if even visible or not
                // if (player_1.imageTargetTracker.isTargetVisible
                player_2.shieldPrefab.SetActive(true);
                player_2.shieldStatus = true;
            }
            else
            {
                player_2.shield.gameObject.SetActive(false);
                player_2.shieldPrefab.SetActive(false);
                player_2.shieldStatus = false;
            }

            // player 2 deaths
            player_2.updatePlayerTwoDeath(stateWrapper.game_state.p2.death);

            // Debug.Log($"✅ Game state updated for P1 HP: {player_1.currentHealth}, P2 HP: {player_2.currentHealth}");
        }

        while (snowqueue.TryDequeue(out string str))
        {
            player_1.spawnSnowStormOnMe();
        }
    }


    // Run this on fixed update to ensure that this is updated fairly often
    void FixedUpdate()
    {
        // Debug.Log(listener);
        if (listener != null && listener.IsConnected)
        {
            noLaserTagIcon.SetActive(false);
            laserTagIcon.SetActive(true);
        }
        else
        {
            noLaserTagIcon.SetActive(true);
            laserTagIcon.SetActive(false);
        }

        if (listener_broadcast != null && listener_broadcast.IsConnected)
        {
            noBcastIcon.SetActive(false);
            BcastIcon.SetActive(true);
        }
        else
        {
            noBcastIcon.SetActive(true);
            BcastIcon.SetActive(false);
        }

        if (listener_snow_bomb != null && listener_snow_bomb.IsConnected)
        {
            noSnowListener.SetActive(false);
            snowListener.SetActive(true);
        }
        else
        {
            noSnowListener.SetActive(true);
            snowListener.SetActive(false);
        }

        // Set the clients up
        if (client_sender != null && client_sender.IsConnected)
        {
            noVisSender.SetActive(false);
            visSender.SetActive(true);
        }
        else
        {
            noVisSender.SetActive(true);
            visSender.SetActive(false);
        }

        if (snow_bomb_client != null && snow_bomb_client.IsConnected)
        {
            snowSender.SetActive(true);
            noSnowSender.SetActive(false);
        }
        else
        {
            snowSender.SetActive(false);
            noSnowSender.SetActive(true);
        }

    }

    public void PlayerOneLogout()
    {
        // Set the coroutine
        StartCoroutine(ShowActionInformer("logout", "BYE", 0.0f));

        // Send data to the server
        string payload = $"{playerId},logout,{player_1.imageTargetTracker.isTargetVisible},{int.Parse(collidingSnowStormCountText.text)}";
        client_sender.Publish(MQTT_TOPIC_SENDER, Encoding.UTF8.GetBytes(payload), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
        Debug.Log($"📤 Sending CSV payload from gun: {payload}");

    }

    public void PlayerOneShoot(float confidenceScore)
    {

        // Needs to able to have bullets to fire and target needs to be visible

        bool res = player_1.currentAmmoCount > 0;
        if (res)
        {
            // Cause damage animation
            player_1.MoveGun();
            if (player_1.imageTargetTracker.isTargetVisible)
            {
                player_2.TakeDamage(5, "bullet");
            }
            else
            {
                player_2.TakeDamage(0, "bullet");
            }
            // Check for snow storm system debuf
            if (int.Parse(collidingSnowStormCountText.text) > 0)
            {
                player_1.causeSnowStormMultiEffect(int.Parse(collidingSnowStormCountText.text));
            }
        }

        string actionStatus;

        if (confidenceScore < thresholdScore)
        {
            actionStatus = $"<color=#FF0000>not okay</color>";
        }
        else
        {
            actionStatus = $"<color=#00FF00>okay</color>";
        }

        StartCoroutine(ShowActionInformer("gun", actionStatus, confidenceScore));



        // PlayerState p1 = new PlayerState(player_1.currentHealth, player_1.currentAmmoCount, player_1.currentBombCount, player_1.currentShieldStrength, player_1.playerOneDeaths(), player_1.currentShieldCount);
        // PlayerState p2 = new PlayerState(player_2.currentHealth, 6, 2, player_2.currentShieldStrength, player_2.getPlayerTwoDeaths(), player_2.currentShieldCount);

        // GameState gameState = new GameState(p1, p2);

        // GameData gameData = new GameData(1, "gun", res, gameState);
        // string jsonPayload = JsonUtility.ToJson(gameData);
        string payload = $"{playerId},gun,{player_1.imageTargetTracker.isTargetVisible},{int.Parse(collidingSnowStormCountText.text)}";
        client_sender.Publish(MQTT_TOPIC_SENDER, Encoding.UTF8.GetBytes(payload), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
        Debug.Log($"📤 Sending CSV payload from gun: {payload}");
    }

    public void PlayerOneShootButton()
    {
        // Needs to able to have bullets to fire and target needs to be visible
        player_1.MoveGun();
        bool res = player_1.imageTargetTracker.isTargetVisible;
        if (res)
        {
            // Cause damage animation
            player_2.TakeDamage(5, "bullet");

            // Check for snow storm system debuf
            if (int.Parse(collidingSnowStormCountText.text) > 0)
            {
                player_1.causeSnowStormMultiEffect(int.Parse(collidingSnowStormCountText.text));
            }
        }
        else
        {
            player_2.TakeDamage(0, "bullet");
        }

        // PlayerState p1 = new PlayerState(player_1.currentHealth, player_1.currentAmmoCount, player_1.currentBombCount, player_1.currentShieldStrength, player_1.playerOneDeaths(), player_1.currentShieldCount);
        // PlayerState p2 = new PlayerState(player_2.currentHealth, 6, 2, player_2.currentShieldStrength, player_2.getPlayerTwoDeaths(), player_2.currentShieldCount);

        // GameState gameState = new GameState(p1, p2);

        // GameData gameData = new GameData(1, "gun", res, gameState);
        // string jsonPayload = JsonUtility.ToJson(gameData);
        // client_sender.Publish(MQTT_TOPIC_SENDER, Encoding.UTF8.GetBytes(jsonPayload), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
        // Debug.Log($"📤 Sending JSON from shoot: {jsonPayload}");
    }

    public void PlayerOneReload(float confidenceScore)
    {
        bool res = player_1.ReloadGun();
        if (res)
        {
            // Check for snow storm system debuf
            if (int.Parse(collidingSnowStormCountText.text) > 0)
            {
                player_1.causeSnowStormMultiEffect(int.Parse(collidingSnowStormCountText.text));
            }
        }
        string actionStatus;

        if (confidenceScore < thresholdScore)
        {
            actionStatus = $"<color=#FF0000>not okay</color>";
        }
        else
        {
            actionStatus = $"<color=#00FF00>okay</color>";
        }

        StartCoroutine(ShowActionInformer("reload", actionStatus, confidenceScore));

        // PlayerState p1 = new PlayerState(player_1.currentHealth, player_1.currentAmmoCount, player_1.currentBombCount, player_1.currentShieldStrength, player_1.playerOneDeaths(), player_1.currentShieldCount);
        // PlayerState p2 = new PlayerState(player_2.currentHealth, 6, 2, player_2.currentShieldStrength, player_2.getPlayerTwoDeaths(), player_2.currentShieldCount);

        // GameState gameState = new GameState(p1, p2);

        // GameData gameData = new GameData(1, "reload", res, gameState);
        // string jsonPayload = JsonUtility.ToJson(gameData);
        // client_sender.Publish(MQTT_TOPIC_SENDER, Encoding.UTF8.GetBytes(jsonPayload), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
        // Debug.Log($"📤 Sending JSON from reloading: {jsonPayload}");
        string payload = $"{playerId},reload,{player_1.imageTargetTracker.isTargetVisible},{int.Parse(collidingSnowStormCountText.text)}";
        client_sender.Publish(MQTT_TOPIC_SENDER, Encoding.UTF8.GetBytes(payload), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
        Debug.Log($"📤 Sending CSV payload from reload: {payload}");
    }

    public void PlayerOneReloadButton()
    {
        bool res = player_1.ReloadGun();
        if (res)
        {
            // Check for snow storm system debuf
            if (int.Parse(collidingSnowStormCountText.text) > 0)
            {
                player_1.causeSnowStormMultiEffect(int.Parse(collidingSnowStormCountText.text));
            }
        }

        // PlayerState p1 = new PlayerState(player_1.currentHealth, player_1.currentAmmoCount, player_1.currentBombCount, player_1.currentShieldStrength, player_1.playerOneDeaths(), player_1.currentShieldCount);
        // PlayerState p2 = new PlayerState(player_2.currentHealth, 6, 2, player_2.currentShieldStrength, player_2.getPlayerTwoDeaths(), player_2.currentShieldCount);

        // GameState gameState = new GameState(p1, p2);

        // GameData gameData = new GameData(1, "reload", res, gameState);
        // string jsonPayload = JsonUtility.ToJson(gameData);
        // client_sender.Publish(MQTT_TOPIC_SENDER, Encoding.UTF8.GetBytes(jsonPayload), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
        // Debug.Log($"📤 Sending JSON from reloading: {jsonPayload}");
    }

    public void PlayerTwoShoot()
    {
        player_1.TakeDamage(5);
    }

    public void PlayerOneUseShield(float confidenceScore)
    {
        bool res = player_1.UseShield();
        if (res)
        {
            // if (player_1.isInsideSnowZoneOne)
            // {
            //     player_1.causeSnowStormOneDamage();
            // }
            // if (player_1.isInsideSnowZoneTwo)
            // {
            //     player_1.causeSnowStormTwoDamage();
            // }

            // Check for snow storm system debuf
            if (int.Parse(collidingSnowStormCountText.text) > 0)
            {
                player_1.causeSnowStormMultiEffect(int.Parse(collidingSnowStormCountText.text));
            }
        }

        string actionStatus;

        if (confidenceScore < thresholdScore)
        {
            actionStatus = $"<color=#FF0000>not okay</color>";
        }
        else
        {
            actionStatus = $"<color=#00FF00>okay</color>";
        }

        StartCoroutine(ShowActionInformer("shield", actionStatus, confidenceScore));

        // PlayerState p1 = new PlayerState(player_1.currentHealth, player_1.currentAmmoCount, player_1.currentBombCount, player_1.currentShieldStrength, player_1.playerOneDeaths(), player_1.currentShieldCount);
        // PlayerState p2 = new PlayerState(player_2.currentHealth, 6, 2, player_2.currentShieldStrength, player_2.getPlayerTwoDeaths(), player_2.currentShieldCount);

        // GameState gameState = new GameState(p1, p2);

        // GameData gameData = new GameData(1, "shield", res, gameState);
        // string jsonPayload = JsonUtility.ToJson(gameData);
        // client_sender.Publish(MQTT_TOPIC_SENDER, Encoding.UTF8.GetBytes(jsonPayload), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
        // Debug.Log($"📤 Sending JSON from shield: {jsonPayload}");
        string payload = $"{playerId},shield,{player_1.imageTargetTracker.isTargetVisible},{int.Parse(collidingSnowStormCountText.text)}";
        client_sender.Publish(MQTT_TOPIC_SENDER, Encoding.UTF8.GetBytes(payload), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
        Debug.Log($"📤 Sending CSV payload from shield: {payload}");


    }
    public void PlayerOneUseShieldButton()
    {
        bool res = player_1.UseShield();
        if (res)
        {
            // if (player_1.isInsideSnowZoneOne)
            // {
            //     player_1.causeSnowStormOneDamage();
            // }
            // if (player_1.isInsideSnowZoneTwo)
            // {
            //     player_1.causeSnowStormTwoDamage();
            // }

            // Check for snow storm system debuf
            if (int.Parse(collidingSnowStormCountText.text) > 0)
            {
                player_1.causeSnowStormMultiEffect(int.Parse(collidingSnowStormCountText.text));
            }
        }

        // PlayerState p1 = new PlayerState(player_1.currentHealth, player_1.currentAmmoCount, player_1.currentBombCount, player_1.currentShieldStrength, player_1.playerOneDeaths(), player_1.currentShieldCount);
        // PlayerState p2 = new PlayerState(player_2.currentHealth, 6, 2, player_2.currentShieldStrength, player_2.getPlayerTwoDeaths(), player_2.currentShieldCount);

        // GameState gameState = new GameState(p1, p2);

        // GameData gameData = new GameData(1, "shield", res, gameState);
        // string jsonPayload = JsonUtility.ToJson(gameData);
        // client_sender.Publish(MQTT_TOPIC_SENDER, Encoding.UTF8.GetBytes(jsonPayload), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
        // Debug.Log($"📤 Sending JSON from shield: {jsonPayload}");

    }

    public void PlayerTwoUseShield()
    {
        player_2.UseShield();
    }

    public void PlayerOneUseSnowBombButton()
    {
        // Assuming a max of 2 snowbombs
        // bool res = player_1.UseSnowBomb();
        player_1.HitSnowBallBezier();
        bool res = player_1.imageTargetTracker.isTargetVisible;
        if (res)
        {
            if (int.Parse(collidingSnowStormCountText.text) > 0)
            {
                player_1.causeSnowStormMultiEffect(int.Parse(collidingSnowStormCountText.text));
            }
        }

        // PlayerState p1 = new PlayerState(player_1.currentHealth, player_1.currentAmmoCount, player_1.currentBombCount, player_1.currentShieldStrength, player_1.playerOneDeaths(), player_1.currentShieldCount);
        // PlayerState p2 = new PlayerState(player_2.currentHealth, 6, 2, player_2.currentShieldStrength, player_2.getPlayerTwoDeaths(), player_2.currentShieldCount);

        // GameState gameState = new GameState(p1, p2);

        // GameData gameData = new GameData(1, "bomb", res, gameState);
        // string jsonPayload = JsonUtility.ToJson(gameData);
        // client_sender.Publish(MQTT_TOPIC_SENDER, Encoding.UTF8.GetBytes(jsonPayload), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
        // Debug.Log($"📤 Sending JSON from snowbomb: {jsonPayload}");
    }

    public void PlayerOneUseSnowBomb(float confidenceScore)
    {
        // Assuming a max of 2 snowbombs
        // bool res = player_1.UseSnowBomb();
        // if (res)
        // {
        //     if (int.Parse(collidingSnowStormCountText.text) > 0)
        //     {
        //         player_1.causeSnowStormMultiEffect(int.Parse(collidingSnowStormCountText.text));
        //     }
        // }
        if (player_1.currentBombCount > 0)
        {
            player_1.HitSnowBallBezier();

            if (int.Parse(collidingSnowStormCountText.text) > 0)
            {
                player_1.causeSnowStormMultiEffect(int.Parse(collidingSnowStormCountText.text));
            }

            if (player_1.imageTargetTracker.isTargetVisible)
            {
                // Send to the other topic
                string localPayload = "spawn";
                snow_bomb_client.Publish(MQTT_TOPIC_SNOWBOMB_SENDER, Encoding.UTF8.GetBytes(localPayload), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
                Debug.Log($"Sending data to {MQTT_TOPIC_SNOWBOMB_SENDER}");

            }
        }

        string actionStatus;

        if (confidenceScore < thresholdScore)
        {
            actionStatus = $"<color=#FF0000>not okay</color>";
        }
        else
        {
            actionStatus = $"<color=#00FF00>okay</color>";
        }

        StartCoroutine(ShowActionInformer("snowbomb", actionStatus, confidenceScore));

        // PlayerState p1 = new PlayerState(player_1.currentHealth, player_1.currentAmmoCount, player_1.currentBombCount, player_1.currentShieldStrength, player_1.playerOneDeaths(), player_1.currentShieldCount);
        // PlayerState p2 = new PlayerState(player_2.currentHealth, 6, 2, player_2.currentShieldStrength, player_2.getPlayerTwoDeaths(), player_2.currentShieldCount);

        // GameState gameState = new GameState(p1, p2);

        // GameData gameData = new GameData(1, "bomb", res, gameState);
        // string jsonPayload = JsonUtility.ToJson(gameData);
        // client_sender.Publish(MQTT_TOPIC_SENDER, Encoding.UTF8.GetBytes(jsonPayload), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
        // Debug.Log($"📤 Sending JSON from snowbomb: {jsonPayload}");
        string payload = $"{playerId},snowbomb,{player_1.imageTargetTracker.isTargetVisible},{int.Parse(collidingSnowStormCountText.text)}";
        client_sender.Publish(MQTT_TOPIC_SENDER, Encoding.UTF8.GetBytes(payload), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
        Debug.Log($"📤 Sending CSV payload from snowbomb: {payload}");
    }

    public void PlayerTwoUseSnowBomb()
    {
        player_1.spawnSnowStormOnMe();
        player_1.TakeDamage(5);
    }

    // Create a badminton swing
    // This is tagged to Action
    public void PlayerTwoUseBadmintonSwing()
    {
        player_1.TakeDamage(10);
    }

    public void PlayerOneUseBadmintonSwing(float confidenceScore)
    {
        bool res = player_1.imageTargetTracker.isTargetVisible;
        player_1.HitShuttleCockBezier();
        if (res)
        {
            if (int.Parse(collidingSnowStormCountText.text) > 0)
            {
                player_1.causeSnowStormMultiEffect(int.Parse(collidingSnowStormCountText.text));
            }
        }

        string actionStatus;

        if (confidenceScore < thresholdScore)
        {
            actionStatus = $"<color=#FF0000>not okay</color>";
        }
        else
        {
            actionStatus = $"<color=#00FF00>okay</color>";
        }

        StartCoroutine(ShowActionInformer("badminton", actionStatus, confidenceScore));
        // PlayerState p1 = new PlayerState(player_1.currentHealth, player_1.currentAmmoCount, player_1.currentBombCount, player_1.currentShieldStrength, player_1.playerOneDeaths(), player_1.currentShieldCount);
        // PlayerState p2 = new PlayerState(player_2.currentHealth, 6, 2, player_2.currentShieldStrength, player_2.getPlayerTwoDeaths(), player_2.currentShieldCount);

        // GameState gameState = new GameState(p1, p2);

        // GameData gameData = new GameData(1, "badminton", res, gameState);
        // string jsonPayload = JsonUtility.ToJson(gameData);
        // client_sender.Publish(MQTT_TOPIC_SENDER, Encoding.UTF8.GetBytes(jsonPayload), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
        // Debug.Log($"📤 Sending JSON from bamdinton: {jsonPayload}");

        string payload = $"{playerId},badminton,{player_1.imageTargetTracker.isTargetVisible},{int.Parse(collidingSnowStormCountText.text)}";
        client_sender.Publish(MQTT_TOPIC_SENDER, Encoding.UTF8.GetBytes(payload), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
        Debug.Log($"📤 Sending CSV payload from badminton: {payload}");
    }

    public void PlayerOneUseBadmintonSwingButton()
    {
        bool res = player_1.imageTargetTracker.isTargetVisible;
        player_1.HitShuttleCockBezier();
        if (res)
        {
            if (int.Parse(collidingSnowStormCountText.text) > 0)
            {
                player_1.causeSnowStormMultiEffect(int.Parse(collidingSnowStormCountText.text));
            }
        }


        // PlayerState p1 = new PlayerState(player_1.currentHealth, player_1.currentAmmoCount, player_1.currentBombCount, player_1.currentShieldStrength, player_1.playerOneDeaths(), player_1.currentShieldCount);
        // PlayerState p2 = new PlayerState(player_2.currentHealth, 6, 2, player_2.currentShieldStrength, player_2.getPlayerTwoDeaths(), player_2.currentShieldCount);

        // GameState gameState = new GameState(p1, p2);

        // GameData gameData = new GameData(1, "badminton", res, gameState);
        // string jsonPayload = JsonUtility.ToJson(gameData);
        // client_sender.Publish(MQTT_TOPIC_SENDER, Encoding.UTF8.GetBytes(jsonPayload), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
        // Debug.Log($"📤 Sending JSON from bamdinton: {jsonPayload}");
    }

    public void PlayerOneBox(float confidenceScore)
    {
        bool res = player_1.imageTargetTracker.isTargetVisible;
        player_1.BoxBezier();
        if (res)
        {
            if (int.Parse(collidingSnowStormCountText.text) > 0)
            {
                player_1.causeSnowStormMultiEffect(int.Parse(collidingSnowStormCountText.text));
            }
        }

        string actionStatus;

        if (confidenceScore < thresholdScore)
        {
            actionStatus = $"<color=#FF0000>not okay</color>";
        }
        else
        {
            actionStatus = $"<color=#00FF00>okay</color>";
        }

        StartCoroutine(ShowActionInformer("boxing", actionStatus, confidenceScore));
        // PlayerState p1 = new PlayerState(player_1.currentHealth, player_1.currentAmmoCount, player_1.currentBombCount, player_1.currentShieldStrength, player_1.playerOneDeaths(), player_1.currentShieldCount);
        // PlayerState p2 = new PlayerState(player_2.currentHealth, 6, 2, player_2.currentShieldStrength, player_2.getPlayerTwoDeaths(), player_2.currentShieldCount);

        // GameState gameState = new GameState(p1, p2);

        // GameData gameData = new GameData(1, "boxing", res, gameState);
        // string jsonPayload = JsonUtility.ToJson(gameData);
        // client_sender.Publish(MQTT_TOPIC_SENDER, Encoding.UTF8.GetBytes(jsonPayload), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
        // Debug.Log($"📤 Sending JSON from box: {jsonPayload}");
        string payload = $"{playerId},boxing,{player_1.imageTargetTracker.isTargetVisible},{int.Parse(collidingSnowStormCountText.text)}";
        client_sender.Publish(MQTT_TOPIC_SENDER, Encoding.UTF8.GetBytes(payload), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
        Debug.Log($"📤 Sending CSV payload from boxing: {payload}");
    }

    public void PlayerOneBoxButton()
    {
        bool res = player_1.imageTargetTracker.isTargetVisible;
        player_1.BoxBezier();
        if (res)
        {
            if (int.Parse(collidingSnowStormCountText.text) > 0)
            {
                player_1.causeSnowStormMultiEffect(int.Parse(collidingSnowStormCountText.text));
            }
        }


        // PlayerState p1 = new PlayerState(player_1.currentHealth, player_1.currentAmmoCount, player_1.currentBombCount, player_1.currentShieldStrength, player_1.playerOneDeaths(), player_1.currentShieldCount);
        // PlayerState p2 = new PlayerState(player_2.currentHealth, 6, 2, player_2.currentShieldStrength, player_2.getPlayerTwoDeaths(), player_2.currentShieldCount);

        // GameState gameState = new GameState(p1, p2);

        // GameData gameData = new GameData(1, "boxing", res, gameState);
        // string jsonPayload = JsonUtility.ToJson(gameData);
        // client_sender.Publish(MQTT_TOPIC_SENDER, Encoding.UTF8.GetBytes(jsonPayload), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
        // Debug.Log($"📤 Sending JSON from box: {jsonPayload}");
    }

    public void PlayerOneSwingSword(float confidenceScore)
    {
        bool res = player_1.imageTargetTracker.isTargetVisible;
        player_1.SwordBezier();
        if (res)
        {
            if (int.Parse(collidingSnowStormCountText.text) > 0)
            {
                player_1.causeSnowStormMultiEffect(int.Parse(collidingSnowStormCountText.text));
            }
        }

        string actionStatus;

        if (confidenceScore < thresholdScore)
        {
            actionStatus = $"<color=#FF0000>not okay</color>";
        }
        else
        {
            actionStatus = $"<color=#00FF00>okay</color>";
        }

        StartCoroutine(ShowActionInformer("fencing", actionStatus, confidenceScore));
        // PlayerState p1 = new PlayerState(player_1.currentHealth, player_1.currentAmmoCount, player_1.currentBombCount, player_1.currentShieldStrength, player_1.playerOneDeaths(), player_1.currentShieldCount);
        // PlayerState p2 = new PlayerState(player_2.currentHealth, 6, 2, player_2.currentShieldStrength, player_2.getPlayerTwoDeaths(), player_2.currentShieldCount);

        // GameState gameState = new GameState(p1, p2);

        // GameData gameData = new GameData(1, "fencing", res, gameState);
        // string jsonPayload = JsonUtility.ToJson(gameData);
        // client_sender.Publish(MQTT_TOPIC_SENDER, Encoding.UTF8.GetBytes(jsonPayload), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
        // Debug.Log($"📤 Sending JSON from sword: {jsonPayload}");
        string payload = $"{playerId},fencing,{player_1.imageTargetTracker.isTargetVisible},{int.Parse(collidingSnowStormCountText.text)}";
        client_sender.Publish(MQTT_TOPIC_SENDER, Encoding.UTF8.GetBytes(payload), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
        Debug.Log($"📤 Sending CSV payload from fencing: {payload}");
    }

    public void PlayerOneSwingSwordButton()
    {
        bool res = player_1.imageTargetTracker.isTargetVisible;
        player_1.SwordBezier();
        if (res)
        {
            if (int.Parse(collidingSnowStormCountText.text) > 0)
            {
                player_1.causeSnowStormMultiEffect(int.Parse(collidingSnowStormCountText.text));
            }
        }
        // PlayerState p1 = new PlayerState(player_1.currentHealth, player_1.currentAmmoCount, player_1.currentBombCount, player_1.currentShieldStrength, player_1.playerOneDeaths(), player_1.currentShieldCount);
        // PlayerState p2 = new PlayerState(player_2.currentHealth, 6, 2, player_2.currentShieldStrength, player_2.getPlayerTwoDeaths(), player_2.currentShieldCount);

        // GameState gameState = new GameState(p1, p2);

        // GameData gameData = new GameData(1, "fencing", res, gameState);
        // string jsonPayload = JsonUtility.ToJson(gameData);
        // client_sender.Publish(MQTT_TOPIC_SENDER, Encoding.UTF8.GetBytes(jsonPayload), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
        // Debug.Log($"📤 Sending JSON from sword: {jsonPayload}");
    }


    public void PlayerOneUseGolfSwing(float confidenceScore)
    {
        bool res = player_1.imageTargetTracker.isTargetVisible;
        player_1.HitGolfBallBezier();
        if (res)
        {
            if (int.Parse(collidingSnowStormCountText.text) > 0)
            {
                player_1.causeSnowStormMultiEffect(int.Parse(collidingSnowStormCountText.text));
            }
        }

        string actionStatus;

        if (confidenceScore < thresholdScore)
        {
            actionStatus = $"<color=#FF0000>not okay</color>";
        }
        else
        {
            actionStatus = $"<color=#00FF00>okay</color>";
        }

        StartCoroutine(ShowActionInformer("golf", actionStatus, confidenceScore));
        // PlayerState p1 = new PlayerState(player_1.currentHealth, player_1.currentAmmoCount, player_1.currentBombCount, player_1.currentShieldStrength, player_1.playerOneDeaths(), player_1.currentShieldCount);
        // PlayerState p2 = new PlayerState(player_2.currentHealth, 6, 2, player_2.currentShieldStrength, player_2.getPlayerTwoDeaths(), player_2.currentShieldCount);

        // GameState gameState = new GameState(p1, p2);

        // GameData gameData = new GameData(1, "golf", res, gameState);
        // string jsonPayload = JsonUtility.ToJson(gameData);
        // client_sender.Publish(MQTT_TOPIC_SENDER, Encoding.UTF8.GetBytes(jsonPayload), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
        // Debug.Log($"📤 Sending JSON from golf: {jsonPayload}");
        string payload = $"{playerId},golf,{player_1.imageTargetTracker.isTargetVisible},{int.Parse(collidingSnowStormCountText.text)}";
        client_sender.Publish(MQTT_TOPIC_SENDER, Encoding.UTF8.GetBytes(payload), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
        Debug.Log($"📤 Sending CSV payload from golf: {payload}");
    }

    public void PlayerOneUseGolfSwingButton()
    {
        bool res = player_1.imageTargetTracker.isTargetVisible;
        player_1.HitGolfBallBezier();
        if (res)
        {
            if (int.Parse(collidingSnowStormCountText.text) > 0)
            {
                player_1.causeSnowStormMultiEffect(int.Parse(collidingSnowStormCountText.text));
            }
        }
        // PlayerState p1 = new PlayerState(player_1.currentHealth, player_1.currentAmmoCount, player_1.currentBombCount, player_1.currentShieldStrength, player_1.playerOneDeaths(), player_1.currentShieldCount);
        // PlayerState p2 = new PlayerState(player_2.currentHealth, 6, 2, player_2.currentShieldStrength, player_2.getPlayerTwoDeaths(), player_2.currentShieldCount);

        // GameState gameState = new GameState(p1, p2);

        // GameData gameData = new GameData(1, "golf", res, gameState);
        // string jsonPayload = JsonUtility.ToJson(gameData);
        // client_sender.Publish(MQTT_TOPIC_SENDER, Encoding.UTF8.GetBytes(jsonPayload), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
        // Debug.Log($"📤 Sending JSON from golf: {jsonPayload}");
    }

    public void PlayerTwoUseGolfSwing()
    {
        player_1.TakeDamage(10);
    }
    public void ToggleDebug()
    {
        p1ShootButton.SetActive(!p1ShootButton.activeSelf);
        p2ShootButton.SetActive(!p2ShootButton.activeSelf);
        p1ShieldButton.SetActive(!p1ShieldButton.activeSelf);
        p2ShieldButton.SetActive(!p2ShieldButton.activeSelf);
        p1SnowBombButton.SetActive(!p1SnowBombButton.activeSelf);
        p2SnowBombButton.SetActive(!p2SnowBombButton.activeSelf);
        p1GolfSwingButton.SetActive(!p1GolfSwingButton.activeSelf);
        p2ActionButton.SetActive(!p2ActionButton.activeSelf);
        p1BadmintonSwingButton.SetActive(!p1BadmintonSwingButton.activeSelf);
        p1SwordSwingButton.SetActive(!p1SwordSwingButton.activeSelf);
        p1BoxButton.SetActive(!p1BoxButton.activeSelf);
        GNumberP1Reload.SetActive(!GNumberP1Reload.activeSelf);
        LaunchStereoModeButton.SetActive(!LaunchStereoModeButton.activeSelf);
        if (playerId == 0)
        {
            playerSelectorOne.SetActive(!playerSelectorOne.activeSelf);
            playerSelectorTwo.SetActive(!playerSelectorTwo.activeSelf);
        }


    }

    public void InitVizToEvalMQ()
    {
        try
        {
            MQTT_TOPIC_SENDER = $"topic/p{playerId}-visibility";
            Debug.Log("📡 Connecting to RabbitMQ via MQTT...");

            // 1️⃣ Create MQTT Client
            client_sender = new MqttClient(MQTT_BROKER_SENDER, MQTT_PORT_SENDER, false, null, null, MqttSslProtocols.None);

            // 2️⃣ Connect to MQTT Broker
            client_sender.Connect(Guid.NewGuid().ToString(), MQTT_USER_SENDER, MQTT_PW_SENDER);

            if (client_sender.IsConnected)
            {
                Debug.Log("✅ MQTT (JSON SENDER) Connected Successfully!");

                // // 3️⃣ Subscribe to a Topic
                // listener.MqttMsgPublishReceived += OnMessageReceived;
                client_sender.Subscribe(new string[] { MQTT_TOPIC_SENDER }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE });
                Debug.Log($"📩 Subscribed to: {MQTT_TOPIC_SENDER}");

            }
            else
            {
                Debug.LogError("❌ Failed to connect to MQTT broker.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ MQTT Connection Error: {ex.Message}");
        }

        try
        {
            int opp_id = 0;
            if (playerId == 1)
            {
                opp_id = 2;
            }
            else
            {
                opp_id = 1;
            }

            // Send to the opposite one
            MQTT_TOPIC_SNOWBOMB_SENDER = $"topic/p{opp_id}-snowsender";
            Debug.Log("📡 Connecting to RabbitMQ via MQTT...");

            // 1️⃣ Create MQTT Client
            snow_bomb_client = new MqttClient(MQTT_BROKER_SENDER, MQTT_PORT_SENDER, false, null, null, MqttSslProtocols.None);

            // 2️⃣ Connect to MQTT Broker
            snow_bomb_client.Connect(Guid.NewGuid().ToString(), MQTT_USER_SENDER, MQTT_PW_SENDER);

            if (snow_bomb_client.IsConnected)
            {
                Debug.Log("✅ MQTT (SNOW) Connected Successfully!");

                // // 3️⃣ Subscribe to a Topic
                // listener.MqttMsgPublishReceived += OnMessageReceived;
                client_sender.Subscribe(new string[] { MQTT_TOPIC_SNOWBOMB_SENDER }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE });
                Debug.Log($"📩 Subscribed to: {MQTT_TOPIC_SNOWBOMB_SENDER}, this is the sender");

            }
            else
            {
                Debug.LogError("❌ Failed to connect to MQTT broker.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ MQTT Connection Error: {ex.Message}");
        }
    }

    public void SetWorldCenterPosition()
    {
        // worldCenterIndicator.text = $"<color=#00FF00>WC</color>";
        worldPositionNotSetAnchor.SetActive(false);
        worldPositionSetAnchor.SetActive(true);
    }

    public void setPlayerOne()
    {
        playerId = 1;
        numberOneIndicator.SetActive(true);
        blockSelectionAfterInitSelection();

        // init MQ Listener, since we now know where to send data
        MQTT_TOPIC = "topic/p1-lasertag";
        MQTT_BCAST_TOPIC = "topic/p1-broadcast";
        MQTT_TOPIC_SNOWBOMB_LISTENER = "topic/p1-snowsender";
        try
        {
            Debug.Log("📡 Connecting to RabbitMQ via MQTT...");

            // 1️⃣ Initialize MQTT Client
            listener = new MqttClient(MQTT_BROKER, MQTT_PORT, false, null, null, MqttSslProtocols.None);

            // 2️⃣ Connect to the Broker
            listener.Connect(Guid.NewGuid().ToString(), MQTT_USER, MQTT_PW);

            if (listener.IsConnected)
            {
                Debug.Log("✅ MQTT LISTENER Connected Successfully!");

                // 3️⃣ Subscribe to the Topic
                listener.MqttMsgPublishReceived += OnMessageReceived;
                listener.Subscribe(new string[] { MQTT_TOPIC }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE });

                Debug.Log($"📩 Subscribed to: {MQTT_TOPIC}");
            }
            else
            {
                Debug.LogError("❌ Failed to connect to MQTT broker.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ MQTT Connection Error: {ex.Message}");
        }

        // Settle broadcast side of things
        try
        {
            Debug.Log("📡 Connecting to RabbitMQ via MQTT... (BCAST)");

            // 1️⃣ Initialize MQTT Client
            listener_broadcast = new MqttClient(MQTT_BROKER, MQTT_PORT, false, null, null, MqttSslProtocols.None);

            // 2️⃣ Connect to the Broker
            listener_broadcast.Connect(Guid.NewGuid().ToString(), MQTT_USER, MQTT_PW);

            if (listener_broadcast.IsConnected)
            {
                Debug.Log("✅ MQTT LISTENER BROADCAST Connected Successfully!");

                // 3️⃣ Subscribe to the Topic
                listener_broadcast.MqttMsgPublishReceived += OnBroadcastReceived;
                listener_broadcast.Subscribe(new string[] { MQTT_BCAST_TOPIC }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE });

                Debug.Log($"📩 Subscribed to: {MQTT_BCAST_TOPIC}");
            }
            else
            {
                Debug.LogError("❌ Failed to connect to MQTT broker.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ MQTT Connection Error: {ex.Message}");
        }

        // Settle the snow storm side of things
        try
        {
            Debug.Log("📡 Connecting to RabbitMQ via MQTT... (SNOW)");

            // 1️⃣ Initialize MQTT Client
            listener_snow_bomb = new MqttClient(MQTT_BROKER_SENDER, MQTT_PORT_SENDER, false, null, null, MqttSslProtocols.None);

            // 2️⃣ Connect to the Broker
            listener_snow_bomb.Connect(Guid.NewGuid().ToString(), MQTT_USER_SENDER, MQTT_PW_SENDER);

            if (listener_snow_bomb.IsConnected)
            {
                Debug.Log("✅ MQTT LISTENER SNOW Connected Successfully!");

                // 3️⃣ Subscribe to the Topic
                listener_snow_bomb.MqttMsgPublishReceived += OnSnowInfoReceived;
                listener_snow_bomb.Subscribe(new string[] { MQTT_TOPIC_SNOWBOMB_LISTENER }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE });

                Debug.Log($"📩 Subscribed to: {MQTT_TOPIC_SNOWBOMB_LISTENER}");
            }
            else
            {
                Debug.LogError("❌ Failed to connect to MQTT broker.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ MQTT Connection Error: {ex.Message}");
        }

        // init MQ Sender
        InitVizToEvalMQ();

        // Listen to the broadcast
    }

    public void setPlayerTwo()
    {
        playerId = 2;
        numberTwoIndicator.SetActive(true);
        blockSelectionAfterInitSelection();

        // init MQ Listener, since we now know where to send data
        MQTT_TOPIC = "topic/p2-lasertag";
        MQTT_BCAST_TOPIC = "topic/p2-broadcast";
        MQTT_TOPIC_SNOWBOMB_LISTENER = "topic/p2-snowsender";
        try
        {
            Debug.Log("📡 Connecting to RabbitMQ via MQTT...");

            // 1️⃣ Initialize MQTT Client
            listener = new MqttClient(MQTT_BROKER, MQTT_PORT, false, null, null, MqttSslProtocols.None);

            // 2️⃣ Connect to the Broker
            listener.Connect(Guid.NewGuid().ToString(), MQTT_USER, MQTT_PW);

            if (listener.IsConnected)
            {
                Debug.Log("✅ MQTT LISTENER Connected Successfully!");

                // 3️⃣ Subscribe to the Topic
                listener.MqttMsgPublishReceived += OnMessageReceived;
                listener.Subscribe(new string[] { MQTT_TOPIC }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE });

                Debug.Log($"📩 Subscribed to: {MQTT_TOPIC}");
            }
            else
            {
                Debug.LogError("❌ Failed to connect to MQTT broker.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ MQTT Connection Error: {ex.Message}");
        }

        // Settle broadcast side of things
        try
        {
            Debug.Log("📡 Connecting to RabbitMQ via MQTT... (BCAST)");

            // 1️⃣ Initialize MQTT Client
            listener_broadcast = new MqttClient(MQTT_BROKER, MQTT_PORT, false, null, null, MqttSslProtocols.None);

            // 2️⃣ Connect to the Broker
            listener_broadcast.Connect(Guid.NewGuid().ToString(), MQTT_USER, MQTT_PW);

            if (listener_broadcast.IsConnected)
            {
                Debug.Log("✅ MQTT LISTENER BROADCAST Connected Successfully!");

                // 3️⃣ Subscribe to the Topic
                listener_broadcast.MqttMsgPublishReceived += OnBroadcastReceived;
                listener_broadcast.Subscribe(new string[] { MQTT_BCAST_TOPIC }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE });

                Debug.Log($"📩 Subscribed to: {MQTT_BCAST_TOPIC}");
            }
            else
            {
                Debug.LogError("❌ Failed to connect to MQTT broker.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ MQTT Connection Error: {ex.Message}");
        }

        // Settle the other side of the MQ
        try
        {
            Debug.Log("📡 Connecting to RabbitMQ via MQTT... (SNOW)");

            // 1️⃣ Initialize MQTT Client
            listener_snow_bomb = new MqttClient(MQTT_BROKER_SENDER, MQTT_PORT_SENDER, false, null, null, MqttSslProtocols.None);

            // 2️⃣ Connect to the Broker
            listener_snow_bomb.Connect(Guid.NewGuid().ToString(), MQTT_USER_SENDER, MQTT_PW_SENDER);

            if (listener_snow_bomb.IsConnected)
            {
                Debug.Log("✅ MQTT LISTENER SNOW Connected Successfully!");

                // 3️⃣ Subscribe to the Topic
                listener_snow_bomb.MqttMsgPublishReceived += OnSnowInfoReceived;
                listener_snow_bomb.Subscribe(new string[] { MQTT_TOPIC_SNOWBOMB_LISTENER }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE });

                Debug.Log($"📩 Subscribed to: {MQTT_TOPIC_SNOWBOMB_LISTENER}");
            }
            else
            {
                Debug.LogError("❌ Failed to connect to MQTT broker.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ MQTT Connection Error: {ex.Message}");
        }

        InitVizToEvalMQ();
    }

    public void blockSelectionAfterInitSelection()
    {
        playerSelectorOne.SetActive(false);
        playerSelectorTwo.SetActive(false);

    }

    IEnumerator ShowActionInformer(string localAction, string localStatus, float localConfidenceScore)
    {
        action.text = localAction;
        // status.text = localStatus;
        // confidenceScoreDisplay.text = $"{localConfidenceScore}";

        actionQualityInformer.SetActive(true);

        yield return new WaitForSeconds(5f);

        actionQualityInformer.SetActive(false);

    }
}



using UnityEngine;
using UnityEngine.UI;
using Google.XR.Cardboard;

public class CardboardManager : MonoBehaviour
{
    private Google.XR.Cardboard.XRLoader cardboardLoader;

    public GameStateMachine gameStateMachine;

    void Start()
    {
        cardboardLoader = ScriptableObject.CreateInstance<Google.XR.Cardboard.XRLoader>();
    }

    public void LaunchStereoMode()
    {
        var status = cardboardLoader.Initialize();
        Debug.Log($"Initialized? : {status}");
        var started = cardboardLoader.Start();
        Debug.Log($"Started? : {started}");
        gameStateMachine.ToggleDebug();

    }

    void Update()
    {
        if (Google.XR.Cardboard.Api.IsCloseButtonPressed)
        {
            cardboardLoader.Stop();
            cardboardLoader.Deinitialize();
            gameStateMachine.ToggleDebug();
        }
    }
}
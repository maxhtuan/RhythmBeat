using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

public class FirebaseManager : MonoBehaviour, IService
{
    [SerializeField] GameSettings gameSettings;
    [System.Serializable]
    public class RemoteConfigData
    {
        public float BPM_Base = 60f;
        public float BPM_Increase = 5f;
        public float BPM_Increase_Max = 100f;
        public float Hit_Window = 0.2f;
    }

    private RemoteConfigData remoteConfig;
    public bool isInitialized = false;

    void Awake()
    {
        StartAsync();
        SendMessageToReact("FirebaseManager", "Message testing");
    }

    // Send any message to React
    public void SendMessageToReact(string messageType, string data)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        Application.ExternalCall("receiveMessageFromUnity", messageType, data);
#endif
    }

    async Task StartAsync()
    {
#if UNITY_EDITOR

        isInitialized = true;
#endif

        await Task.Delay(10000); //10s
        isInitialized = true;
    }

    public void OnRemoteConfigReceived(string jsonString)
    {
        Debug.Log("FirebaseManager: Remote config received: " + jsonString);

        try
        {
            // Parse the JSON data
            remoteConfig = JsonUtility.FromJson<RemoteConfigData>(jsonString);
            Debug.Log($"FirebaseManager: Parsed config - BPM_Base: {remoteConfig.BPM_Base}, BPM_Increase: {remoteConfig.BPM_Increase}, BPM_Increase_Max: {remoteConfig.BPM_Increase_Max}, Hit_Window: {remoteConfig.Hit_Window}");

            // Apply the remote config to game settings
            ApplyRemoteConfigToGame();

            isInitialized = true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"FirebaseManager: Failed to parse remote config: {e.Message}");
            // Use default values if parsing fails
            remoteConfig = new RemoteConfigData();
            isInitialized = true;
        }
    }

    private void ApplyRemoteConfigToGame()
    {
        // Apply BPM settings to SongHandler
        // apply to game settings manager
        var gametsettingmanager = ServiceLocator.Instance.GetService<GameSettingsManager>();
        gametsettingmanager.ApplyFirebaseSettings(remoteConfig.BPM_Increase, remoteConfig.BPM_Increase_Max, remoteConfig.Hit_Window, remoteConfig.BPM_Base);
    }

    public void Initialize()
    {
        Debug.Log("FirebaseManager initialized");
        // Initialize with default values
        remoteConfig = new RemoteConfigData();
    }

    // Getter methods for remote config data
    public float GetBPMBase() => remoteConfig?.BPM_Base ?? 60f;
    public float GetBPMIncrease() => remoteConfig?.BPM_Increase ?? 5f;
    public float GetBPMIncreaseMax() => remoteConfig?.BPM_Increase_Max ?? 100f;
    public float GetHitWindow() => remoteConfig?.Hit_Window ?? 0.2f;

    // Get the full remote config data
    public RemoteConfigData GetRemoteConfig() => remoteConfig;

    public void Cleanup()
    {
        Debug.Log("FirebaseManager cleaned up");
    }
}

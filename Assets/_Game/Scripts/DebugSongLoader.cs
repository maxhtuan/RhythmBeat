using UnityEngine;

public class DebugSongLoader : MonoBehaviour
{
    void Start()
    {
        Debug.Log("=== Debug Song Loader ===");

        // Test 1: Try to load with extension
        TextAsset xmlFile1 = Resources.Load<TextAsset>("song.xml");
        Debug.Log($"Load with extension: {(xmlFile1 != null ? "SUCCESS" : "FAILED")}");

        // Test 2: Try to load without extension
        TextAsset xmlFile2 = Resources.Load<TextAsset>("song");
        Debug.Log($"Load without extension: {(xmlFile2 != null ? "SUCCESS" : "FAILED")}");

        // Test 3: List all TextAssets in Resources
        TextAsset[] allTextAssets = Resources.LoadAll<TextAsset>("");
        Debug.Log($"Total TextAssets in Resources: {allTextAssets.Length}");
        foreach (var asset in allTextAssets)
        {
            Debug.Log($"Found TextAsset: {asset.name}");
        }

        // Test 4: Try to load as generic Object
        Object[] allObjects = Resources.LoadAll("");
        Debug.Log($"Total Objects in Resources: {allObjects.Length}");
        foreach (var obj in allObjects)
        {
            Debug.Log($"Found Object: {obj.name} ({obj.GetType()})");
        }
    }
}

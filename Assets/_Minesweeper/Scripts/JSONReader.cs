using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class JSONReader : MonoBehaviour
{
    public string mapFileName;
    private string mapFilePath;


    private void Awake()
    {
        mapFilePath = Path.Combine(Application.streamingAssetsPath, mapFileName);
    }

    public MapArray ReadMapDataFromJSON()
    {
        if (!JSONFileExist())
        {
            Debug.LogError("JSON file does not exist: " + mapFilePath);
            return null;
        }

        string jsonString = File.ReadAllText(mapFilePath);

        return Newtonsoft.Json.JsonConvert.DeserializeObject<MapArray>(jsonString);
    }

    public bool JSONFileExist()
    {
        return File.Exists(mapFilePath);
    }
}

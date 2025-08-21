using UnityEngine;
using System;
using System.IO;
using System.Collections;
using UnityEngine.Networking;

//// Shape data class
//[System.Serializable]
//public class Shape
//{
//    public string Name { get; set; }
//    public int Sides { get; set; }
//    public double Area { get; set; }
//    public bool IsRegular { get; set; }
//    public string Color { get; set; }
//    public override string ToString()
//    {
//        return $"Shape: {Name}, Sides: {Sides}, Area: {Area}, Regular: {IsRegular}, Color: {Color}";
//    }
//}

public class SettingsDataHandler
{
    private static SettingsDataHandler _instance;

    // File paths and keys
    private const string SETTINGS_DATA_KEY = "SettingsData";
    private const string JSON_FILE_NAME = "shapeData.json";
    private string jsonFilePath;

    // Server configuration
    public string serverUploadURL = "https://your-server.com/api/upload";
    private Date currentSettingsData;
    //private TableSettings currentSettingsData;
    private bool dataModified = false;

    public bool HasSettingsData => currentSettingsData != null;

    public static SettingsDataHandler Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new SettingsDataHandler();
            }
            return _instance;
        }
    }

    private SettingsDataHandler()
    {
      //Singleton
    }

    // Initialize the data handler
    public void Init()
    {
        if(!PlayerPrefs.HasKey(SETTINGS_DATA_KEY))
        {
            string defaultData = Resources.Load<TextAsset>("DefaultTableData").text;
            PlayerPrefs.SetString(SETTINGS_DATA_KEY, defaultData);
            PlayerPrefs.Save();
            Debug.LogError("Default Loaded");
        }
        if (PlayerPrefs.HasKey(SETTINGS_DATA_KEY))
        {
            string savedJson = PlayerPrefs.GetString(SETTINGS_DATA_KEY);
            try
            {
                currentSettingsData = JsonUtility.FromJson<Date>(savedJson);
                Debug.Log("Shape data loaded from PlayerPrefs: " + currentSettingsData.ToString());
                //SaveToJsonFile();
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to deserialize shape data from PlayerPrefs: " + e.Message);
            }
        }
    }

    // Set shape data from JSON string
    public void SaveSettingsData(Date tableSettings)
    {
        try
        {
            string date = JsonUtility.ToJson(tableSettings);

            if (date != null)
            {
                currentSettingsData = tableSettings;
                dataModified = true;

                // Save to both PlayerPrefs and JSON file
                PlayerPrefs.SetString(SETTINGS_DATA_KEY, date);
                PlayerPrefs.Save();

                Debug.Log("Shape data updated: " + date);
            }
            else
            {
                Debug.LogError("Invalid JSON data provided");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to process JSON data: " + e.Message);
        }
    }

    // Get current shape data
    public Date GetSettingsData()
    {
        if (currentSettingsData == null)
        {
            Debug.LogWarning("No shape data available. Initializing...");
        }
        return currentSettingsData;
    }

    // Convenience methods
    //public string GetShapeName() => currentSettingsData?.Name ?? "Unknown";
    //public int GetShapeSides() => currentSettingsData?.Sides ?? 0;
    //public double GetShapeArea() => currentSettingsData?.Area ?? 0.0;
    //public bool IsShapeRegular() => currentSettingsData?.IsRegular ?? false;
    //public string GetShapeColor() => currentSettingsData?.Color ?? "Unknown";

    // Get JSON file path for external access
    //public string GetJsonFilePath() => jsonFilePath;
}

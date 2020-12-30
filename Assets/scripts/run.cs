using System.Collections.Generic;
using System.Runtime.CompilerServices;
using SFB;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class run : MonoBehaviour
{
    
    public GameObject prefab_ac;
    public Slider timeNumberSlider;
    public GameObject plane;
    
    // Start is called before the first frame update
    private Dictionary<int, Dictionary<string, Dictionary<string, float>>> dataDict;


    private List<GameObject> acObjectList = new List<GameObject>();
    void Start()
    {
        Debug.Log("start method reached");
        var fileFilterExtensions = new []
        {
            new ExtensionFilter("txt", "txt"),
        };
        Debug.Log("init file extensions filter");
        string esSecnarioTextFilepath = StandaloneFileBrowser.OpenFilePanel("Open Euroscope Secnario File", "", fileFilterExtensions, false)[0];
        DataFeeder feeder = new DataFeeder(esSecnarioTextFilepath);
        
        dataDict = feeder._AcDataDict;
        int maxTimeNumber = feeder._MaxTimeNumber;
        
        // calculate size of plane
        float deltaLat = feeder._DeltaLat;
        float deltaLon = feeder._DeltaLon;
        plane.transform.localScale = new Vector3(deltaLat, 100, deltaLon);
        
        timeNumberSlider.maxValue = maxTimeNumber;
        timeNumberSlider.value = 0;
    }

    // Update is called once per frame
    void Update()
    {
        foreach (var ac in acObjectList)
        {
            Destroy(ac);
        }
        acObjectList.Clear();
        int currentTimeSelected = (int)timeNumberSlider.value;
        
        var acDataDict = dataDict[currentTimeSelected];
        foreach (var acEntry in acDataDict)
        {
            var acEntryValue = acEntry.Value;
            GameObject aircraft = Instantiate(prefab_ac, new Vector3(acEntryValue["latitude"], acEntryValue["altitude"], acEntryValue["longitude"]),
                Quaternion.identity);
            aircraft.name = acEntry.Key;
            acObjectList.Add(aircraft);
        }

    }
}

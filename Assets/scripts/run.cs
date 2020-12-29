using System.Collections.Generic;
using SFB;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class run : MonoBehaviour
{
    
    public GameObject prefab_ac;
    public Slider timeNumberSlider;
    
    // Start is called before the first frame update
    private Dictionary<int, Dictionary<string, Dictionary<string, float>>> dataDict;


    private List<GameObject> acObjectList = new List<GameObject>();
    void Start()
    {
        var fileFilterExtensions = new []
        {
            new ExtensionFilter("txt", "txt"),
        };
        string esSecnarioTextFilepath = StandaloneFileBrowser.OpenFilePanel("Open Euroscope Secnario File", "", fileFilterExtensions, false)[0];
        Debug.Log(esSecnarioTextFilepath.ToString());
        DataFeeder feeder = new DataFeeder(esSecnarioTextFilepath.ToString()); 
        dataDict = feeder.acDataDict;
        int maxTimeNumber = feeder.maxTimeNumber;

        timeNumberSlider.maxValue = maxTimeNumber;
        timeNumberSlider.value = 0;
        
        // // create from prefab
        // var acDataDict = dataDict[0];
        // foreach (var acEntry in acDataDict)
        // {
        //     var acEntryValue = acEntry.Value;
        //     GameObject aircraft = Instantiate(prefab_ac, new Vector3(acEntryValue["latitude"], acEntryValue["altitude"], acEntryValue["longitude"]),
        //         Quaternion.identity);
        //     aircraft.name = acEntry.Key;
        // }
        
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

using System;
using System.Collections.Generic;
using SFB;
using UnityEngine;
using UnityEngine.UI;

public class run : MonoBehaviour
{
    private int _scaleFactor = 1000;
    
    public GameObject _prefabAc;
    public Slider timeNumberSlider;
    public GameObject plane;
    
    // Start is called before the first frame update
    private Dictionary<int, Dictionary<string, Dictionary<string, float>>> _dataDict;

    private float _areaMaxLat;
    private float _areaMaxLon;
    private int _maxTimeValue;
    void Start()
    {
        var fileFilterExtensions = new []
        {
            new ExtensionFilter("txt", "txt"),
        };
        string esSecnarioTextFilepath = StandaloneFileBrowser.OpenFilePanel("Open Euroscope Secnario File", "", fileFilterExtensions, false)[0];
        DataFeeder feeder = new DataFeeder(esSecnarioTextFilepath, _scaleFactor);
        
        _dataDict = feeder._AcDataDict;
        int maxTimeNumber = feeder._MaxTimeNumber;
        _maxTimeValue = maxTimeNumber;
        
        // set size of plane
        float deltaLat = feeder._DeltaLat;
        float deltaLon = feeder._DeltaLon;
        _areaMaxLat = feeder._maxLat;
        _areaMaxLon = feeder._maxLon;
        plane.transform.localScale = new Vector3(deltaLat, 100, deltaLon);
        
        timeNumberSlider.maxValue = maxTimeNumber;
        timeNumberSlider.value = 0;
    }

    // Update is called once per frame
    void Update()
    {
        int currentTimeSelected = (int)timeNumberSlider.value;

        
        var acDataDict = _dataDict[currentTimeSelected];
        foreach (var acEntry in acDataDict)
        {
            var acCallsign = acEntry.Key;
            var acLatitude = acEntry.Value["latitude"];
            var acLongitude = acEntry.Value["longitude"];
            var acAltitude = acEntry.Value["altitude"];
            bool isLastAppearance;
            if (acEntry.Value["isLastAppearance"] == 1)
            {
                isLastAppearance = true;
            }
            else
            {
                isLastAppearance = false;
            }

            try
            {
                var ac = GameObject.Find(acEntry.Key);
                ac.transform.localPosition = new Vector3(acLatitude, acAltitude, acLongitude);

                if (isLastAppearance)
                {
                    Destroy(ac);
                }
            }
            catch (NullReferenceException)
            {
                var acEntryValue = acEntry.Value;
                GameObject ac = Instantiate(_prefabAc, new Vector3(acLatitude, acAltitude, acLongitude),
                    Quaternion.identity);
                ac.name = acCallsign;
                
                if (isLastAppearance)
                {
                    Destroy(ac);
                }
            }
        }
    }
}

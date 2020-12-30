using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
public class DataFeeder
{
    public Dictionary<int, Dictionary<string, Dictionary<string, float>>> _AcDataDict;
    public int _MaxTimeNumber;
    public float _DeltaLat;
    public float _DeltaLon;
    public DataFeeder(string filePath)
    {
        string rxPattern = @"\[(?<timestamp>.*?)\s.*\].*\n@N:(?<callsign>.*?):(?<squakcode>\d{4}):1:(?<latitude>.*?):(?<longitude>.*?):(?<altitude>\d*?|-\d*?):";
        string esSecnarioText = System.IO.File.ReadAllText(filePath);
        
        Regex rx = new Regex(rxPattern);
        MatchCollection matches = rx.Matches(esSecnarioText);
        
        // preprocessing
        List<string> timestampList = new List<string>();
        List<string> callsignList = new List<string>();
        List<float> latitudeList = new List<float>();
        List<float> longitudeList = new List<float>();
        List<float> altitudeList = new List<float>();
        // custom time list
        List<int> timeNumberList = new List<int>();
        int timeNumber = 0;

        int loopCount = 0;
        foreach (Match match in matches)
        {
            GroupCollection matchGroups = match.Groups;
            
            timestampList.Add(matchGroups["timestamp"].Value);
            callsignList.Add(matchGroups["callsign"].Value);
            latitudeList.Add(float.Parse(matchGroups["latitude"].Value));
            longitudeList.Add(float.Parse(matchGroups["longitude"].Value));
            altitudeList.Add((float)(Double.Parse(matchGroups["altitude"].Value) * 3.281));

            if (loopCount > 0 && timestampList[loopCount] != timestampList[loopCount - 1])
            {
                timeNumber++;
            }
            timeNumberList.Add(timeNumber);

            loopCount++;
        }
        float maxLat = latitudeList.Max();
        float minLat = latitudeList.Min();
        float avgLat = latitudeList.Average();

        float maxLon = longitudeList.Max();
        float minLon = longitudeList.Min();
        float avgLon = longitudeList.Average();

        float maxAlt = altitudeList.Max();
        
        // set oob variables
        for (int i=0; i<timestampList.Count; i++)
        {
            latitudeList[i] = (latitudeList[i] - avgLat) * 100;
            longitudeList[i] = (longitudeList[i] - avgLon) * 100;
            altitudeList[i] = altitudeList[i] / 100;
        }

        var dataDict = new Dictionary<int, Dictionary<string, Dictionary<string, float>>>();
        for (int i = 0; i<timestampList.Count; i++)
        {
            try
            {
                dataDict[timeNumberList[i]][callsignList[i]] =
                    new Dictionary<string, float>()
                    {
                        {"latitude", latitudeList[i]},
                        {"longitude", longitudeList[i]},
                        {"altitude", altitudeList[i]},
                    };
            }
            catch (KeyNotFoundException)
            {
                dataDict[timeNumberList[i]] = new Dictionary<string, Dictionary<string, float>>()
                {
                    {
                        callsignList[i],
                        new Dictionary<string, float>()
                        {
                            {"latitude", latitudeList[i]},
                            {"longitude", longitudeList[i]},
                            {"altitude", altitudeList[i]},
                        }
                    }
                };
            }
        }
        
        // create dict for ac data
        _DeltaLat = (maxLat - minLat) * 10;
        _DeltaLon = (maxLon - minLon) * 10;
        _MaxTimeNumber = timeNumberList.Max();
        _AcDataDict = dataDict;
        
    }
}

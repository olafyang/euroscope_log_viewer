using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
public class DataFeeder
{
    public Dictionary<int, Dictionary<string, Dictionary<string, float>>> _AcDataDict;
    public int _MaxTimeNumber;
    public float _DeltaLat;
    public float _DeltaLon;
    public float _maxLat;
    public float _maxLon;
    public DataFeeder(string filePath, int scaleFactor)
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
            altitudeList.Add((float)(Double.Parse(matchGroups["altitude"].Value) / 0.3048));

            if (loopCount > 0 && timestampList[loopCount] != timestampList[loopCount - 1])
            {
                timeNumber++;
            }
            timeNumberList.Add(timeNumber);

            loopCount++;
        }

        int listLength = callsignList.Count; 
        
        List<float> lastAppearanceList = new List<float>();
        
        for (int i = 0; i < listLength; i++)
        {
            string currentCallsign = callsignList[i];
            float lastAppearance;
            var slicedList = callsignList.Skip(i + 1);
            if (slicedList.Contains(currentCallsign))
            {
                lastAppearance = 0;
            }
            else
            {
                lastAppearance = 1;
            }
            lastAppearanceList.Add(lastAppearance);
        }
        
        float maxLat = latitudeList.Max();
        float minLat = latitudeList.Min();
        float avgLat = latitudeList.Average();

        float maxLon = longitudeList.Max();
        float minLon = longitudeList.Min();
        float avgLon = longitudeList.Average();

        float maxAlt = altitudeList.Max();
        
        // calculate model values
        for (int i=0; i<listLength; i++)
        {
            latitudeList[i] = (latitudeList[i] - avgLat) * scaleFactor;
            longitudeList[i] = (longitudeList[i] - avgLon) * scaleFactor;
            altitudeList[i] = altitudeList[i] / (scaleFactor / 10);
        }

        var dataDict = new Dictionary<int, Dictionary<string, Dictionary<string, float>>>();
        for (int i = 0; i<listLength; i++)
        {
            try
            {
                dataDict[timeNumberList[i]][callsignList[i]] =
                    new Dictionary<string, float>()
                    {
                        {"isLastAppearance", lastAppearanceList[i]},
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
                            {"isLastAppearance", lastAppearanceList[i]},
                            {"latitude", latitudeList[i]},
                            {"longitude", longitudeList[i]},
                            {"altitude", altitudeList[i]},
                        }
                    }
                };
            }
        }
        
        // set oob variables
        _DeltaLat = (maxLat - minLat) * scaleFactor / 10;
        _DeltaLon = (maxLon - minLon) * scaleFactor / 10;
        _MaxTimeNumber = timeNumberList.Max();
        _AcDataDict = dataDict;
        _maxLat = maxLat;
        _maxLon = maxLon;

    }
}

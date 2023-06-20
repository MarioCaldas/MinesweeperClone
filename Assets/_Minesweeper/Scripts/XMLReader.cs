using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using UnityEngine;

public class XMLReader : MonoBehaviour
{
    public string mapFileName;
    private string mapFilePath;

    private void Awake()
    {
        mapFilePath = Path.Combine(Application.streamingAssetsPath, mapFileName);
    }

    public MapArray ReadMapDataFromXml()
    {
        if (!XMLFileExist())
        {
            Debug.LogError("XML file does not exist: " + mapFilePath);
            return null;
        }

        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(mapFilePath);

        MapArray map = new MapArray();
        map.rows = int.Parse(xmlDoc.SelectSingleNode("/root/rows").InnerText);
        map.columns = int.Parse(xmlDoc.SelectSingleNode("/root/columns").InnerText);
        map.mines = int.Parse(xmlDoc.SelectSingleNode("/root/mines").InnerText);

        map.tiles = new List<List<int>>();
        XmlNodeList rowNodes = xmlDoc.SelectNodes("/root/tiles");
        foreach (XmlNode rowNode in rowNodes)
        {
            List<int> row = new List<int>();
            foreach (XmlNode tileNode in rowNode.ChildNodes)
            {
                int tileValue = int.Parse(tileNode.InnerText);
                row.Add(tileValue);
            }
            map.tiles.Add(row);
        }
        return map;
    }

    public bool XMLFileExist()
    {
        return File.Exists(mapFilePath);
    }

}
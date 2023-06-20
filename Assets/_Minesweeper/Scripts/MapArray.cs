using System.Collections.Generic;

[System.Serializable]
public class MapArray
{
    public int rows { get; set; }
    public int columns { get; set; }
    public int mines { get; set; }
    public List<List<int>> tiles { get; set; }
}
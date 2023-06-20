using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private GridBoard gridBoard;

    [SerializeField] private XMLReader xmlReader;
    [SerializeField] private JSONReader jsonReader;

    public bool isRandomMap;
    public bool isAutoPlay; // set to true on AutoPlay
    public bool canPlay;

    public int numberPlays;

    [Header("Random Map Settings")]
    [SerializeField] public Vector2 gridSize;
    [SerializeField] public int minesAmount;
    [SerializeField] public int flagsAmount;

    public event Action GameWon;
    public event Action GameLost;
    public event Action OnStartAutoPlay;
    public event Action OnStopAutoPlay;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    private void Start()
    {
        canPlay = true;

        GenerateMap();
    }
    private void GenerateMap()
    {
        if(isRandomMap)
        {
            gridBoard.InitializeBoard((int)gridSize.x, (int)gridSize.y, null);
        }
        else
        {
            if (xmlReader.XMLFileExist())
            {
                // Read XML file and generate map using the extracted data
                gridBoard.GenerateMapFromXML(xmlReader.ReadMapDataFromXml());
            }
            else if (jsonReader.JSONFileExist())
            {
                // Read JSON file and generate map using the extracted data
                gridBoard.GenerateMapFromJSON(jsonReader.ReadMapDataFromJSON());
            }
            else
            {
                isRandomMap = true;
                GenerateMap();
            }
        }
    }

    public void RestartGame()
    {
        canPlay = true;

        isAutoPlay = false;

        AutoPlay.Instance.ResetAutoPlay();

        GenerateMap();

        numberPlays = 0;

        flagsAmount = 0;

        UIManager.Instance.ResetUI();
    }

    private bool IsGameWon()
    {  
        Cell[,] cells = GridBoard.Instance.GetCells();

        foreach (Cell cell in cells)
        {
            if (!cell.IsUnlocked() && cell.GetCellValue() != -1)
            {
                return false;
            }
        }

        return true;      
    }

    private bool IsGameLost()
    {
        Cell[,] cells = GridBoard.Instance.GetCells();

        for (int i = 0; i < cells.GetLength(0); i++)
        {
            for (int j = 0; j < cells.GetLength(1); j++)
            {
                Cell cell = cells[i, j];

                if (cell.GetCellValue() == -1 && cell.IsUnlocked())
                {
                    return true;
                }
            }
        }

        return false; 
    }

    public void CheckGameState()
    {
        if (IsGameWon())
        {
            OnGameWon();
        }
        else if (IsGameLost())
        {
            OnGameLost();
        }
    }

    private void OnGameWon()
    {
        canPlay = false;
        GameWon?.Invoke();
    }

    private void OnGameLost()
    {
        canPlay = false;
        GameLost?.Invoke();
    }

    public void StartAutoPlay()
    {
        canPlay = false;
        isAutoPlay = true;
        OnStartAutoPlay?.Invoke();
    }
    
    public void StopAutoPlay()
    {
        canPlay = true;
        isAutoPlay = false;
        OnStopAutoPlay?.Invoke();
    }

}

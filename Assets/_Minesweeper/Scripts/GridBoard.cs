using UnityEngine;

public class GridBoard : MonoBehaviour
{
    public static GridBoard Instance { get; private set; }

    private System.Random random = new System.Random();

    private Cell[,] board;

    [Header("Cell Info")]
    [SerializeField] private Cell cellObj;
    private float cellSize = 10f; 
    private float cellSpacing = 1f;


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

    public void GenerateMapFromXML(MapArray xmlContent)
    {
        InitializeBoard(xmlContent.columns, xmlContent.rows, xmlContent);
    }

    public void GenerateMapFromJSON(MapArray JSONContent)
    {
        InitializeBoard(JSONContent.columns, JSONContent.rows, JSONContent);
    }

    public void InitializeBoard(int _sizeX, int _sizeY, MapArray _readArray)
    {
        ResetGrid();

        board = new Cell[_sizeX, _sizeY];

        for (int col = 0; col < _sizeX; col++) 
        {
            for (int row = 0; row < _sizeY; row++) 
            {
                Vector3 position = new Vector3(col * (cellSpacing + cellSize), 0f, row * (cellSpacing + cellSize));

                Cell newCell = CreateCell(position);
                InitializeCell(newCell, _readArray, row, col);
            }
        }

        if (_readArray != null)   
            GameManager.Instance.minesAmount = _readArray.mines;

        PlaceCameraAtCenter();
        SetCellsNeighbors();
    }

    private Cell CreateCell(Vector3 position)
    {
        Cell newCell = Instantiate(cellObj, position, Quaternion.identity, transform);
        return newCell;
    }

    private void InitializeCell(Cell cell, MapArray readArray, int row, int col)
    {
        board[col, row] = cell;

        if (readArray == null)
            cell.SetCellType(0);
        else
            cell.SetCellType(readArray.tiles[row][col]);

        cell.name = "Cell " + col + "," + row + ")";

        //Data for autoplay feature
        AutoPlay.Instance.coveredCells.Add(cell);
    }

    public Cell[,] GetCells()
    {
        return board;
    }

    private void SetCellsNeighbors()
    {
        for (int row = 0; row < board.GetLength(0); row++)
        {
            for (int col = 0; col < board.GetLength(1); col++)
            {
                SetNeighborsForCell(row, col);
            }
        }
    }

    private void SetNeighborsForCell(int row, int col)
    {
        const int MinRow = -1;
        const int MaxRow = 1;
        const int MinCol = -1;
        const int MaxCol = 1;

        Cell currentCell = board[row, col];

        //Iterate through cells square around parameter cell
        for (int neighborRow = row + MinRow; neighborRow <= row + MaxRow; neighborRow++)
        {
            for (int neighborCol = col + MinCol; neighborCol <= col + MaxCol; neighborCol++)
            {
                if (neighborRow == row && neighborCol == col)
                    continue;

                if (IsValidCell(neighborRow, neighborCol))
                {
                    Cell neighborCell = board[neighborRow, neighborCol];
                    if (neighborCell != null)
                    {
                        currentCell.UpdateNearCells(neighborCell);
                    }
                }
            }
        }
    }

    private bool IsValidCell(int row, int col)
    {
        return row >= 0 && row < board.GetLength(0) && col >= 0 && col < board.GetLength(1);
    }

    public void PlaceMines(Cell firstClickCell)
    {
        if(GameManager.Instance.minesAmount >= (board.GetLength(0) * board.GetLength(1)))//safety for user input, more mines than cells
        {
            GameManager.Instance.minesAmount = (board.GetLength(0) * board.GetLength(1)) - 1;
        }

        // Randomly place mines on the board
        int minesPlaced = 0;
        while (minesPlaced < GameManager.Instance.minesAmount)
        {
            int row = random.Next(0, board.GetLength(0));
            int col = random.Next(0, board.GetLength(1));

            // If the cell is already assigned a mine or is equal to the player first clicked cell, skip this iteration
            if (board[row, col].GetCellValue() == -1 || board[row, col] == firstClickCell)
            {
                continue;
            }

            board[row, col].SetCellType(-1);

            // Update the neighbors
            foreach (var item in board[row, col].GetNeighbours())
            {
                if (item.GetCellValue() != -1)
                {
                    item.UpdateNearMinesAmount();
                }
            }

            minesPlaced++;
        }
    }

    void PlaceCameraAtCenter()
    {
        // Place the camera at the center of the grid, taking in account, spacing and cellsize offset, and camera height based on grid size
        float biggerSide = board.GetLength(0) >= board.GetLength(1) ? board.GetLength(0) : board.GetLength(1);
        float totalSize = biggerSide * (cellSpacing + cellSize);
        float heightOffset = 10;

        float cameraHeight = totalSize / 2f;

        float offset = (cellSpacing + cellSize) / 2f;

        Vector3 centerPosition = new Vector3((board.GetLength(0) * (cellSpacing + cellSize)) / 2f - offset, cameraHeight, (board.GetLength(1) * (cellSpacing + cellSize)) / 2f - offset);

        Camera.main.orthographic = true;
        Camera.main.orthographicSize = (totalSize / 2f) + heightOffset;

        Camera.main.transform.position = centerPosition;
    }

    void ResetGrid()
    {
        if(board != null)
        {
            for (int i = 0; i < board.GetLength(0); i++)
            {
                for (int j = 0; j < board.GetLength(1); j++)
                {
                    Destroy(board[i, j].gameObject);
                }
            }
        }
    }


}

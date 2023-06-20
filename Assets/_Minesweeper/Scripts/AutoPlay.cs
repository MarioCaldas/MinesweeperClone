using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoPlay : MonoBehaviour
{
    public static AutoPlay Instance { get; private set; }

    private System.Random random = new System.Random();

    [SerializeField] private float pauseDuration;

    public List<Cell> coveredCells = new List<Cell>();

    private void Awake()
    {
        if(Instance == null)
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
        GameManager.Instance.OnStartAutoPlay += StartAutoPlay;
        GameManager.Instance.OnStopAutoPlay += StopAutoPlay;
    }

    private IEnumerator PlaySequence(float pauseDuration)
    {
        yield return new WaitForSeconds(pauseDuration);

        Cell cell = GetLowestProbability();

        if(cell != null)
        {
            //Place mines on auto play first click
            if (GameManager.Instance.numberPlays < 1 && GameManager.Instance.isRandomMap)//Event
            {
                GridBoard.Instance.PlaceMines(cell);
            }
            GameManager.Instance.numberPlays++;

            Play(cell);
        }
        else//Next moves are luck based
        {
            GameManager.Instance.StopAutoPlay();
        }
    }

    public void Play(Cell cell)
    {
        if (!cell.IsUnlocked())
        {
            cell.AutoPlayCellAnimation();

            cell.UnlockCell();

            GameManager.Instance.CheckGameState();

            if (cell.GetCellValue() == -1)
            {
                return;
            }

            StartCoroutine(PlaySequence(pauseDuration));
        }
    }

    private Cell GetLowestProbability()
    {
        List<Cell> lowProbCells = new List<Cell>();
        
        float maxProbability = 0.65f;
        float minProbability = 0.25f;
        // Find cells with a probability less than or equal to 2/8 (0.25)
        foreach (var item in coveredCells)
        {
            if (item.isFlag)
            {
                continue;
            }

            if (item.mineProbability > 0f && item.mineProbability <= minProbability)
            {
                lowProbCells.Add(item);
            }
        }

        Cell selectedCell = null;

        if (lowProbCells.Count > 0)
        {
            // Select a cell with a low probability
            selectedCell = lowProbCells[random.Next(0, lowProbCells.Count)];
        }
        else
        {
            // No cells with a probability less than or equal to 2/8 (0.25) found, choose the cell with the lowest probability
            minProbability = float.MaxValue;

            foreach (var item in coveredCells)
            {
                if (item.isFlag)
                {
                    continue;
                }

                if (item.mineProbability < minProbability && item.mineProbability < maxProbability)
                {
                    minProbability = item.mineProbability;
                    selectedCell = item;
                }
            }
        }

        lowProbCells.Clear(); // Clear the lowProbCells list

        return selectedCell;
    }

    public void StartAutoPlay()
    {
        StartCoroutine(PlaySequence(pauseDuration));
    }

    public void StopAutoPlay()
    {
        StopAllCoroutines();
    }

    public void ResetAutoPlay()
    {
        coveredCells.Clear();
        StopAllCoroutines();
    }

}

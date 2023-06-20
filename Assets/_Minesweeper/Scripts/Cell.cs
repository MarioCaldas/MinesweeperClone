using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Cell : MonoBehaviour
{
    [Header("Properties")]
    [SerializeField] private int cellValue; // 0 = empty / -1 = mine / >= 1 = number of neighbours mines
    private bool mineChecked;//Flag for mine search recursion
    private bool unlocked;
    [SerializeField] private List<Cell> neighbourCells = new List<Cell>();
    public bool isFlag;
    [SerializeField]public float mineProbability;

    [Header("Display")]
    [SerializeField] private Image unlockedImage;
    [SerializeField] private List<Image> numbersImages;
    [SerializeField] private Image mineImage;
    [SerializeField] private Image flagImage;
    [SerializeField] private Image autoPlayImage;

    private void Start()
    {
        StopAllCoroutines();

        SetUnlockStateImage(false);
        SetNumberImage(false);
        SetMineImage(false);
        SetFlagImage(false);
    }

    public void SetCellType(int value)
    {
        cellValue = value;
    }

    public int GetCellValue()
    {
        return cellValue;
    }

    public List<Cell> GetNeighbours()
    {
        return neighbourCells;
    }

    public void UpdateNearMinesAmount()
    {
        cellValue++;
    }

    public void UpdateNearCells(Cell nearCell)
    {
        neighbourCells.Add(nearCell);
    }

    public void UnlockCell()
    {
        unlocked = true;

        //Data for autoplay feature
        AutoPlay.Instance.coveredCells.Remove(this);

        if (GetCellValue() == 0)
        {
            //Open all neighbours empty cells
            foreach (var item in neighbourCells)
            {
                if (!item.unlocked && item.GetCellValue() != -1)
                {
                    item.UnlockCell();
                }
            }
            SetUnlockStateImage(true);
        }
        else if(GetCellValue() == -1)
        {           
            //Open all mine cells
            UnlockMine();
        }
        else
        {
            //Show adjacent mines number
            SetNumberImage(true);
        }

        //data for the auto play feature
        if (GameManager.Instance.isAutoPlay && GetCellValue() != -1)
            CalculateNeighboursMineProbability(this);

    }

    public void UnlockMine()
    {
        if(GetCellValue() == -1)
        {
            SetMineImage(true);
        }

        mineChecked = true;

        //Open all mine cells
        foreach (var item in neighbourCells)
        {
            if (!item.mineChecked)
            {
                item.UnlockMine();
            }
        }
    }

    public void SetFlag()
    {
        if (!isFlag)
        {
            GameManager.Instance.flagsAmount++;
            isFlag = true;
        }
        else
        {    
            GameManager.Instance.flagsAmount--;
            isFlag = false;
        }

        UIManager.Instance.UpdateMinesAmountText();

        SetFlagImage(isFlag);
    }

    public bool IsUnlocked()
    {
        return unlocked;
    }


    //Player Input
    private void OnMouseOver()
    {
        if (GameManager.Instance.isAutoPlay || !GameManager.Instance.canPlay)
        {
            // Auto play mode is enabled, skip player input handling
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if(!unlocked)
            {
                //Place mines on player first click
                if(GameManager.Instance.numberPlays < 1 && GameManager.Instance.isRandomMap)
                {
                    GridBoard.Instance.PlaceMines(this);
                }
                GameManager.Instance.numberPlays++;

                UnlockCell();

                GameManager.Instance.CheckGameState();
            }
        }
        else if(Input.GetMouseButtonDown(1))
        {
            SetFlag();
        }
    }


    private void CalculateNeighboursMineProbability(Cell cell)
    {
        int unexploredCount = 0;
        int flaggedCount = 0;

        foreach (var item in cell.GetNeighbours())
        {

            if (!item.IsUnlocked() && !item.isFlag)
            {
                unexploredCount++;
            }

            // Check if the neighboring cell is flagged
            if (item.isFlag)
            {
                flaggedCount++;
            }
        }

        int adjacentMines = cell.GetCellValue();
        int remainingMines = adjacentMines - flaggedCount;


        if (unexploredCount > 0 && remainingMines > 0)
        {        
            float flagThreshold = (float)remainingMines / unexploredCount;

            foreach (var item in cell.GetNeighbours())
            {
                if (!item.IsUnlocked() && !item.isFlag)
                {
                    item.mineProbability += (float)remainingMines / unexploredCount;

                    if(item.mineProbability > flagThreshold)
                    {
                        item.SetFlag();
                    }
                }
            }

        }
    }

    #region Display
    public void SetUnlockStateImage(bool value)
    {
        unlockedImage.gameObject.SetActive(value);
    }
    public void SetNumberImage(bool value)
    {
        if (cellValue > 0)
            numbersImages[cellValue - 1].gameObject.SetActive(value);
    }
    public void SetMineImage(bool value)
    {
        mineImage.gameObject.SetActive(value);
        SetFlagImage(!value);
    }
    public void SetFlagImage(bool value)
    {
        flagImage.gameObject.SetActive(value);
    }

    public void AutoPlayCellAnimation()
    {
        //Auto play cell highlight
        StartCoroutine(AutoPlayCellAnimationSequence());
    }

    private IEnumerator AutoPlayCellAnimationSequence()
    {
        float elapsedTime = 0f;
        Color currentColor = autoPlayImage.color;

        float targetAlpha = 0.5f;
        float lerpDuration = 0.1f;
        float initialAlpha = currentColor.a;

        while (elapsedTime < lerpDuration)
        {
            float normalizedProgress = elapsedTime / lerpDuration;

            float lerpedAlpha = Mathf.Lerp(initialAlpha, targetAlpha, normalizedProgress);

            currentColor.a = lerpedAlpha;

            autoPlayImage.color = currentColor;

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        currentColor.a = targetAlpha;
        autoPlayImage.color = currentColor;

        yield return new WaitForSeconds(0.3f);

        elapsedTime = 0f;

        while (elapsedTime < lerpDuration)
        {
            float normalizedProgress = elapsedTime / lerpDuration;

            float lerpedAlpha = Mathf.Lerp(targetAlpha, initialAlpha, normalizedProgress);

            currentColor.a = lerpedAlpha;

            autoPlayImage.color = currentColor;

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        currentColor.a = initialAlpha;
        autoPlayImage.color = currentColor;
    }
    #endregion
}

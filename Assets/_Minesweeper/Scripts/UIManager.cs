using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI remainingMines;

    [Header("Buttons")]
    [SerializeField] private Button restartBtt;
    [SerializeField] private Button startAutoPlayBtt;
    [SerializeField] private Button pauseAutoPlayBtt;
    [SerializeField] private Button configBtt;
    [SerializeField] private Button infoBtt;

    [Header("Config Window Properties")]
    [SerializeField] private TMP_InputField gridSizeXInput;
    [SerializeField] private TMP_InputField gridSizeYInput;
    [SerializeField] private TMP_InputField minesAmountInput;
    [SerializeField] private Button generateRandomMapBtt;
    [SerializeField] private Button generateDatabaseBtt;

    [Header("Game Screens")]
    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private GameObject winScreen;
    [SerializeField] private GameObject configScreen;
    [SerializeField] private GameObject infoScreen;

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
        GameManager.Instance.GameWon += GameWon;
        GameManager.Instance.GameLost += GameLost;
        GameManager.Instance.OnStopAutoPlay += ResetAutoPlayBtt;

        startAutoPlayBtt.onClick.AddListener(() =>
        {
            if (!GameManager.Instance.isAutoPlay && GameManager.Instance.canPlay)
            {
                GameManager.Instance.StartAutoPlay();

                pauseAutoPlayBtt.gameObject.SetActive(true);
            }
        });

        pauseAutoPlayBtt.onClick.AddListener(() =>
        {
            if (GameManager.Instance.isAutoPlay)
            {
                GameManager.Instance.StopAutoPlay();

                pauseAutoPlayBtt.gameObject.SetActive(false);
            }
        });

        restartBtt.onClick.AddListener(() =>
        {
            GameManager.Instance.RestartGame();
        });

        configBtt.onClick.AddListener(() =>
        {
            if (!configScreen.activeSelf)
            {
                configScreen.SetActive(true);
                infoScreen.SetActive(false);
                winScreen.SetActive(false);
                gameOverScreen.SetActive(false);

                GameManager.Instance.canPlay = false;
            }
            else
            {
                configScreen.SetActive(false);
                GameManager.Instance.canPlay = true;
            }
        });

        generateRandomMapBtt.onClick.AddListener(() =>
        {
            GameManager.Instance.isRandomMap = true;
            GameManager.Instance.RestartGame();
            configScreen.SetActive(false);

        });

        generateDatabaseBtt.onClick.AddListener(() =>
        {
            GameManager.Instance.isRandomMap = false;
            GameManager.Instance.RestartGame();
            configScreen.SetActive(false);

        }); 

        infoBtt.onClick.AddListener(() =>
        {
            if(!infoScreen.activeSelf)
            {
                infoScreen.SetActive(true);
                configScreen.SetActive(false);
                winScreen.SetActive(false);
                gameOverScreen.SetActive(false);
                GameManager.Instance.canPlay = false;
            }
            else
            {
                infoScreen.SetActive(false);
                GameManager.Instance.canPlay = true;
            }
        });

        gridSizeXInput.onValueChanged.AddListener(OnInputGridSizeXUpdate);
        gridSizeYInput.onValueChanged.AddListener(OnInputGridSizeYUpdate);
        minesAmountInput.onValueChanged.AddListener(OnInputMinesAmountUpdate);

        UpdateMinesAmountText();
    }

    private void OnInputGridSizeXUpdate(string value)
    {
        GameManager.Instance.gridSize.x = int.Parse(value);
    }

    private void OnInputGridSizeYUpdate(string value)
    {
        GameManager.Instance.gridSize.y = int.Parse(value);
    }

    private void OnInputMinesAmountUpdate(string value)
    {
        GameManager.Instance.minesAmount = int.Parse(value);
    }
  
    public void UpdateMinesAmountText()
    {
        int value = GameManager.Instance.minesAmount - GameManager.Instance.flagsAmount;
        if(value >= 0)
        remainingMines.text = value.ToString();
    }

    private void GameWon()
    {
        winScreen.SetActive(true);
    }

    private void GameLost()
    {
        gameOverScreen.SetActive(true);
    }

    private void ResetAutoPlayBtt()
    {
        pauseAutoPlayBtt.gameObject.SetActive(false);
        startAutoPlayBtt.gameObject.SetActive(true);
    }

    public void ResetUI()
    {
        UpdateMinesAmountText();
        winScreen.SetActive(false);
        gameOverScreen.SetActive(false);
        pauseAutoPlayBtt.gameObject.SetActive(false);
        configScreen.SetActive(false);
        infoScreen.SetActive(false);
    }
}

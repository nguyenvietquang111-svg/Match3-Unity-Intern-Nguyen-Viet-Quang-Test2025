using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public event Action<eStateGame> StateChangedAction = delegate { };

    public enum eLevelMode
    {
        NORMAL,
        AUTOPLAY,
        AUTO_LOSE,
        TIME_ATTACK,
    }

    public enum eStateGame
    {
        SETUP,
        MAIN_MENU,
        GAME_STARTED,
        PAUSE,
        GAME_OVER,
    }

    public enum eGameResult
    {
        NONE,
        WIN,
        LOSE,
    }

    private eStateGame m_state;

    public eStateGame State
    {
        get { return m_state; }
        private set
        {
            m_state = value;
            StateChangedAction(m_state);
        }
    }

    public eGameResult Result { get; private set; } = eGameResult.NONE;

    public eLevelMode CurrentMode { get; private set; } = eLevelMode.NORMAL;

    public int CurrentRound { get; private set; } = 1;
    public int CurrentBoardSizeX { get; private set; } = 4;
    public int CurrentBoardSizeY { get; private set; } = 6;

    private GameSettings m_gameSettings;
    private BoardController m_boardController;
    private UIMainManager m_uiMenu;

    private Coroutine m_gameOverRoutine;

    private void Awake()
    {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;

        State = eStateGame.SETUP;

        m_gameSettings = Resources.Load<GameSettings>(Constants.GAME_SETTINGS_PATH);

        m_uiMenu = FindObjectOfType<UIMainManager>();
        if (m_uiMenu != null)
        {
            m_uiMenu.Setup(this);
        }
    }

    private void Start()
    {
        State = eStateGame.MAIN_MENU;
    }

    internal void SetState(eStateGame state)
    {
        State = state;

        if (State == eStateGame.PAUSE)
        {
            DOTween.PauseAll();
        }
        else
        {
            DOTween.PlayAll();
        }
    }

    public void LoadLevel(eLevelMode mode)
    {
        // Start fresh run at base round and dimensions
        CurrentRound = 1;
        CurrentBoardSizeX = m_gameSettings != null ? m_gameSettings.BoardSizeX : 4;
        CurrentBoardSizeY = m_gameSettings != null ? m_gameSettings.BoardSizeY : 6;

        LoadLevelInternal(mode);
    }

    public void ContinueToNextRound()
    {
        if (CurrentBoardSizeX < 32 || CurrentBoardSizeY < 32)
        {
            // Alternating progression: Odd round increases column, Even round increases row
            if (CurrentRound % 2 == 1)
            {
                CurrentBoardSizeX = Mathf.Min(32, CurrentBoardSizeX + 1);
            }
            else
            {
                CurrentBoardSizeY = Mathf.Min(32, CurrentBoardSizeY + 1);
            }

            CurrentRound++;
        }

        LoadLevelInternal(CurrentMode);
    }

    public void GetNextRoundDimensions(out int nextX, out int nextY)
    {
        nextX = CurrentBoardSizeX;
        nextY = CurrentBoardSizeY;

        if (CurrentBoardSizeX >= 32 && CurrentBoardSizeY >= 32)
        {
            return;
        }

        if (CurrentRound % 2 == 1)
        {
            nextX = Mathf.Min(32, nextX + 1);
        }
        else
        {
            nextY = Mathf.Min(32, nextY + 1);
        }
    }

    private void LoadLevelInternal(eLevelMode mode)
    {
        ClearLevel();

        Result = eGameResult.NONE;
        CurrentMode = mode;

        m_boardController = new GameObject("BoardController").AddComponent<BoardController>();
        m_boardController.StartGame(
            this,
            m_gameSettings,
            mode,
            m_uiMenu != null ? m_uiMenu.GetLevelConditionView() : null,
            CurrentBoardSizeX,
            CurrentBoardSizeY,
            CurrentRound);

        SetState(eStateGame.GAME_STARTED);
    }

    public void GameOver(bool win)
    {
        if (Result != eGameResult.NONE)
        {
            return;
        }

        Result = win ? eGameResult.WIN : eGameResult.LOSE;

        if (m_gameOverRoutine != null)
        {
            StopCoroutine(m_gameOverRoutine);
        }

        m_gameOverRoutine = StartCoroutine(WaitBoardController());
    }

    internal void ClearLevel()
    {
        if (m_gameOverRoutine != null)
        {
            StopCoroutine(m_gameOverRoutine);
            m_gameOverRoutine = null;
        }

        if (m_boardController != null)
        {
            m_boardController.Clear();
            Destroy(m_boardController.gameObject);
            m_boardController = null;
        }
    }

    private IEnumerator WaitBoardController()
    {
        while (m_boardController != null && m_boardController.IsBusy)
        {
            yield return null;
        }

        yield return new WaitForSeconds(0.35f);

        SetState(eStateGame.GAME_OVER);
    }
}

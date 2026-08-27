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

    private GameSettings m_gameSettings;
    private BoardController m_boardController;
    private UIMainManager m_uiMenu;

    private Coroutine m_gameOverRoutine;

    private void Awake()
    {
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
        ClearLevel();

        Result = eGameResult.NONE;
        CurrentMode = mode;

        m_boardController = new GameObject("BoardController").AddComponent<BoardController>();
        m_boardController.StartGame(this, m_gameSettings, mode, m_uiMenu != null ? m_uiMenu.GetLevelConditionView() : null);

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

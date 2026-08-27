using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UIMainManager : MonoBehaviour
{
    private IMenu[] m_menuList;

    private GameManager m_gameManager;

    private void Awake()
    {
        m_menuList = GetComponentsInChildren<IMenu>(true);
    }

    void Start()
    {
        for (int i = 0; i < m_menuList.Length; i++)
        {
            m_menuList[i].Setup(this);
        }
    }

    internal void ShowMainMenu()
    {
        if (m_gameManager == null)
        {
            return;
        }

        m_gameManager.ClearLevel();
        m_gameManager.SetState(GameManager.eStateGame.MAIN_MENU);
    }

    void Update()
    {
        if (m_gameManager == null)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (m_gameManager.State == GameManager.eStateGame.GAME_STARTED)
            {
                m_gameManager.SetState(GameManager.eStateGame.PAUSE);
            }
            else if (m_gameManager.State == GameManager.eStateGame.PAUSE)
            {
                m_gameManager.SetState(GameManager.eStateGame.GAME_STARTED);
            }
        }
    }

    internal void Setup(GameManager gameManager)
    {
        m_gameManager = gameManager;
        m_gameManager.StateChangedAction += OnGameStateChange;
    }

    private void OnGameStateChange(GameManager.eStateGame state)
    {
        switch (state)
        {
            case GameManager.eStateGame.SETUP:
                break;
            case GameManager.eStateGame.MAIN_MENU:
                ShowMenu<UIPanelMain>();
                break;
            case GameManager.eStateGame.GAME_STARTED:
                ShowMenu<UIPanelGame>();
                break;
            case GameManager.eStateGame.PAUSE:
                ShowMenu<UIPanelPause>();
                break;
            case GameManager.eStateGame.GAME_OVER:
                ShowGameResult();
                break;
        }
    }

    private void ShowMenu<T>() where T : IMenu
    {
        for (int i = 0; i < m_menuList.Length; i++)
        {
            IMenu menu = m_menuList[i];
            if (menu is T)
            {
                menu.Show();
            }
            else
            {
                menu.Hide();
            }
        }
    }

    internal Text GetLevelConditionView()
    {
        UIPanelGame game = m_menuList.Where(x => x is UIPanelGame).Cast<UIPanelGame>().FirstOrDefault();
        if (game)
        {
            return game.LevelConditionView;
        }

        return null;
    }

    internal void ShowPauseMenu()
    {
        if (m_gameManager == null)
        {
            return;
        }

        m_gameManager.SetState(GameManager.eStateGame.PAUSE);
    }

    internal void LoadLevelMoves()
    {
        LoadAutoplayLevel();
    }

    internal void LoadLevelTimer()
    {
        m_gameManager.LoadLevel(GameManager.eLevelMode.TIME_ATTACK);
    }

    internal void LoadAutoLoseLevel()
    {
        m_gameManager.LoadLevel(GameManager.eLevelMode.AUTO_LOSE);
    }

    internal void LoadNormalLevel()
    {
        m_gameManager.LoadLevel(GameManager.eLevelMode.NORMAL);
    }

    internal void LoadAutoplayLevel()
    {
        m_gameManager.LoadLevel(GameManager.eLevelMode.AUTOPLAY);
    }

    internal void ShowGameMenu()
    {
        if (m_gameManager == null)
        {
            return;
        }

        m_gameManager.SetState(GameManager.eStateGame.GAME_STARTED);
    }

    private void ShowGameResult()
    {
        string panelName = m_gameManager != null && m_gameManager.Result == GameManager.eGameResult.WIN
            ? "PanelWin"
            : "PanelGameOver";

        ShowMenuByName(panelName);
    }

    private void ShowMenuByName(string menuName)
    {
        for (int i = 0; i < m_menuList.Length; i++)
        {
            IMenu menu = m_menuList[i];
            MonoBehaviour behaviour = menu as MonoBehaviour;
            if (behaviour != null && behaviour.gameObject.name == menuName)
            {
                menu.Show();
            }
            else
            {
                menu.Hide();
            }
        }
    }
}

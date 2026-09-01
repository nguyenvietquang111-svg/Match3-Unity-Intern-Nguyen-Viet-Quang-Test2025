using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class BoardController : MonoBehaviour
{
    public event Action OnMoveEvent = delegate { };

    public bool IsBusy { get; private set; }

    private Board m_board;
    private GameManager m_gameManager;
    private GameSettings m_gameSettings;
    private Camera m_cam;
    private Text m_statusText;
    private GameManager.eLevelMode m_mode;
    private bool m_gameOver;
    private float m_timeLeft;
    private Queue<Cell> m_autoplayPlan = new Queue<Cell>();
    private Coroutine m_autoCoroutine;
    private int m_sizeX;
    private int m_sizeY;
    private int m_round;

    public void StartGame(GameManager gameManager, GameSettings gameSettings, GameManager.eLevelMode mode, Text statusText, int sizeX = 0, int sizeY = 0, int round = 1)
    {
        m_gameManager = gameManager;
        m_gameSettings = gameSettings;
        m_mode = mode;
        m_statusText = statusText;
        m_cam = Camera.main;

        m_sizeX = sizeX > 0 ? sizeX : (gameSettings != null ? gameSettings.BoardSizeX : 4);
        m_sizeY = sizeY > 0 ? sizeY : (gameSettings != null ? gameSettings.BoardSizeY : 6);
        m_round = round;

        m_board = new Board(transform, gameSettings, m_sizeX, m_sizeY);
        m_autoplayPlan = BuildAutoplayPlan();

        if (m_mode == GameManager.eLevelMode.TIME_ATTACK)
        {
            int totalTiles = m_sizeX * m_sizeY;
            float baseTime = gameSettings.LevelTime > 0f ? gameSettings.LevelTime : 60f;
            m_timeLeft = totalTiles > 24 ? baseTime + (totalTiles - 24) * 1.5f : baseTime;
        }

        UpdateStatusText();

        if (m_mode == GameManager.eLevelMode.AUTOPLAY || m_mode == GameManager.eLevelMode.AUTO_LOSE)
        {
            m_autoCoroutine = StartCoroutine(AutoPlayCoroutine());
        }
    }

    public void Update()
    {
        if (m_board == null || m_gameOver)
        {
            return;
        }

        if (m_gameManager == null || m_gameManager.State != GameManager.eStateGame.GAME_STARTED)
        {
            return;
        }

        if (m_mode == GameManager.eLevelMode.TIME_ATTACK)
        {
            m_timeLeft -= Time.deltaTime;
            if (m_timeLeft <= 0f)
            {
                m_timeLeft = 0f;
                UpdateStatusText();
                LoseGame();
                return;
            }

            UpdateStatusText();
        }

        if (m_mode == GameManager.eLevelMode.AUTOPLAY || m_mode == GameManager.eLevelMode.AUTO_LOSE)
        {
            return;
        }

        if (IsBusy)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            var hit = Physics2D.Raycast(m_cam.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (hit.collider != null)
            {
                Cell cell = hit.collider.GetComponent<Cell>();
                if (cell != null)
                {
                    HandleCellTap(cell);
                }
            }
        }
    }

    private IEnumerator AutoPlayCoroutine()
    {
        while (!m_gameOver)
        {
            if (m_gameManager == null || m_gameManager.State != GameManager.eStateGame.GAME_STARTED)
            {
                yield return null;
                continue;
            }

            if (IsBusy)
            {
                yield return null;
                continue;
            }

            if (m_autoplayPlan.Count == 0)
            {
                yield return null;
                continue;
            }

            Cell next = m_autoplayPlan.Dequeue();
            if (next == null || next.Item == null)
            {
                continue;
            }

            HandleBoardCellTap(next);
            yield return new WaitForSeconds(0.5f);
        }
    }

    private Queue<Cell> BuildAutoplayPlan()
    {
        Queue<Cell> plan = new Queue<Cell>();

        if (m_board == null)
        {
            return plan;
        }

        List<Cell> boardCells = m_board.BoardCells.ToList();

        if (m_mode == GameManager.eLevelMode.AUTO_LOSE)
        {
            HashSet<NormalItem.eNormalType> usedTypes = new HashSet<NormalItem.eNormalType>();
            foreach (Cell cell in boardCells)
            {
                if (cell?.Item is NormalItem normal && !usedTypes.Contains(normal.ItemType))
                {
                    usedTypes.Add(normal.ItemType);
                    plan.Enqueue(cell);
                }

                if (plan.Count >= 5)
                {
                    break;
                }
            }

            return plan;
        }

        var grouped = boardCells
            .Where(x => x != null && x.Item is NormalItem)
            .Select(x => new { Cell = x, Type = ((NormalItem)x.Item).ItemType })
            .GroupBy(x => x.Type)
            .OrderBy(x => x.Key)
            .ToList();

        foreach (var group in grouped)
        {
            List<Cell> cells = group.Select(x => x.Cell).ToList();
            global::Utils.Shuffle(cells);

            foreach (Cell cell in cells)
            {
                plan.Enqueue(cell);
            }
        }

        return plan;
    }

    private void HandleCellTap(Cell cell)
    {
        if (cell == null || cell.Item == null)
        {
            return;
        }

        if (cell.IsBottomCell)
        {
            if (m_mode == GameManager.eLevelMode.TIME_ATTACK)
            {
                HandleBottomCellTap(cell);
            }

            return;
        }

        HandleBoardCellTap(cell);
    }

    private void HandleBoardCellTap(Cell boardCell)
    {
        if (boardCell == null || boardCell.Item == null)
        {
            return;
        }

        Cell bottomCell = m_board.GetFirstFreeBottomCell();
        if (bottomCell == null)
        {
            if (m_mode != GameManager.eLevelMode.TIME_ATTACK)
            {
                LoseGame();
            }

            return;
        }

        NormalItem item = boardCell.Item as NormalItem;
        if (item == null)
        {
            return;
        }

        IsBusy = true;
        OnMoveEvent();

        m_board.MoveBoardItemToBottom(boardCell, bottomCell, 0.28f, () =>
        {
            StartCoroutine(ResolveAfterMoveCoroutine(item.ItemType));
        });
    }

    private void HandleBottomCellTap(Cell bottomCell)
    {
        Cell homeCell = m_board.GetHomeCellForBottomItem(bottomCell);
        if (homeCell == null || !homeCell.IsEmpty)
        {
            return;
        }

        IsBusy = true;
        OnMoveEvent();

        m_board.MoveBottomItemToBoard(bottomCell, homeCell, 0.28f, () =>
        {
            m_board.CompactBottomTray(0.18f, () =>
            {
                StartCoroutine(ResolveAfterMoveCoroutine(null));
            });
        });
    }

    private IEnumerator ResolveAfterMoveCoroutine(NormalItem.eNormalType? movedType)
    {
        yield return new WaitForSeconds(0.02f);

        if (movedType.HasValue)
        {
            var matchingCells = m_board.BottomCells
                .Where(x => x.Item is NormalItem normal && normal.ItemType == movedType.Value)
                .ToList();

            if (matchingCells.Count >= 3)
            {
                bool clearFinished = false;
                m_board.ClearBottomCellsOfType(movedType.Value, () =>
                {
                    clearFinished = true;
                });

                while (!clearFinished)
                {
                    yield return null;
                }
            }
        }
        else
        {
            foreach (NormalItem.eNormalType type in Enum.GetValues(typeof(NormalItem.eNormalType)))
            {
                var matchingCells = m_board.BottomCells
                    .Where(x => x.Item is NormalItem normal && normal.ItemType == type)
                    .ToList();

                if (matchingCells.Count >= 3)
                {
                    bool clearFinished = false;
                    m_board.ClearBottomCellsOfType(type, () =>
                    {
                        clearFinished = true;
                    });

                    while (!clearFinished)
                    {
                        yield return null;
                    }
                    break;
                }
            }
        }

        IsBusy = false;
        UpdateStatusText();
        EvaluateEndConditions();
    }

    private void EvaluateEndConditions()
    {
        if (m_gameOver)
        {
            return;
        }

        if (m_board.IsBoardClear())
        {
            WinGame();
            return;
        }

        if (m_mode != GameManager.eLevelMode.TIME_ATTACK && m_board.IsBottomFull())
        {
            LoseGame();
        }
    }

    private void WinGame()
    {
        if (m_gameOver)
        {
            return;
        }

        m_gameOver = true;
        StopAutoRoutine();
        m_gameManager.GameOver(true);
    }

    private void LoseGame()
    {
        if (m_gameOver)
        {
            return;
        }

        m_gameOver = true;
        StopAutoRoutine();
        m_gameManager.GameOver(false);
    }

    private void StopAutoRoutine()
    {
        if (m_autoCoroutine != null)
        {
            StopCoroutine(m_autoCoroutine);
            m_autoCoroutine = null;
        }
    }

    private void UpdateStatusText()
    {
        if (m_statusText == null)
        {
            return;
        }

        string modeName = "FISH MODE";
        switch (m_mode)
        {
            case GameManager.eLevelMode.NORMAL:
                modeName = "NORMAL";
                break;
            case GameManager.eLevelMode.AUTOPLAY:
                modeName = "AUTOPLAY";
                break;
            case GameManager.eLevelMode.AUTO_LOSE:
                modeName = "AUTO LOSE";
                break;
            case GameManager.eLevelMode.TIME_ATTACK:
                modeName = "TIME ATTACK";
                break;
        }

        int maxBoardItems = m_sizeX * m_sizeY;
        int activeSpawns = maxBoardItems - (maxBoardItems % 3);

        if (m_mode == GameManager.eLevelMode.TIME_ATTACK)
        {
            m_statusText.text = string.Format("{0} (R{1}: {4}x{5})\nTime: {2:00}\nBoard: {3}/{6}\nBottom: {7}/5",
                modeName,
                m_round,
                Mathf.CeilToInt(m_timeLeft),
                m_board.GetBoardItemCount(),
                m_sizeX,
                m_sizeY,
                activeSpawns,
                m_board.GetBottomItemCount());
        }
        else
        {
            m_statusText.text = string.Format("{0} (R{1}: {3}x{4})\nBoard: {2}/{5}\nBottom: {6}/5",
                modeName,
                m_round,
                m_board.GetBoardItemCount(),
                m_sizeX,
                m_sizeY,
                activeSpawns,
                m_board.GetBottomItemCount());
        }
    }

    internal void Clear()
    {
        m_gameOver = true;
        IsBusy = false;
        StopAllCoroutines();
        StopAutoRoutine();

        if (m_board != null)
        {
            m_board.Clear();
            m_board = null;
        }
    }
}

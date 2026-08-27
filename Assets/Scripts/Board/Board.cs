using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Board
{
    private const int BottomCellCount = 5;

    private readonly int m_boardSizeX;
    private readonly int m_boardSizeY;
    private readonly Transform m_root;
    private readonly GameSettings m_gameSettings;

    private readonly Cell[,] m_boardCells;
    private readonly List<Cell> m_bottomCells = new List<Cell>();

    public IReadOnlyList<Cell> BoardCells => m_boardCells.Cast<Cell>().ToList();

    public IReadOnlyList<Cell> BottomCells => m_bottomCells;

    public Board(Transform transform, GameSettings gameSettings)
    {
        m_root = transform;
        m_gameSettings = gameSettings;
        m_boardSizeX = gameSettings.BoardSizeX;
        m_boardSizeY = gameSettings.BoardSizeY;
        m_boardCells = new Cell[m_boardSizeX, m_boardSizeY];

        CreateBoardCells();
        CreateBottomCells();
        FillInitialItems();
    }

    private void CreateBoardCells()
    {
        GameObject prefabBG = Resources.Load<GameObject>(Constants.PREFAB_CELL_BACKGROUND);
        Vector3 origin = new Vector3(-(m_boardSizeX - 1) * 0.5f, (m_boardSizeY - 1) * 0.5f, 0f);

        for (int x = 0; x < m_boardSizeX; x++)
        {
            for (int y = 0; y < m_boardSizeY; y++)
            {
                GameObject go = GameObject.Instantiate(prefabBG, m_root);
                go.transform.position = origin + new Vector3(x, -y, 0f);

                Cell cell = go.GetComponent<Cell>();
                cell.Setup(x, y, false);
                m_boardCells[x, y] = cell;
            }
        }
    }

    private void CreateBottomCells()
    {
        GameObject prefabBG = Resources.Load<GameObject>(Constants.PREFAB_CELL_BACKGROUND);
        Vector3 origin = new Vector3(-(BottomCellCount - 1) * 0.5f, -((m_boardSizeY - 1) * 0.5f) - 1.75f, 0f);

        for (int i = 0; i < BottomCellCount; i++)
        {
            GameObject go = GameObject.Instantiate(prefabBG, m_root);
            go.transform.position = origin + new Vector3(i, 0f, 0f);

            Cell cell = go.GetComponent<Cell>();
            cell.Setup(i, -1, true);
            m_bottomCells.Add(cell);
        }
    }

    private void FillInitialItems()
    {
        List<NormalItem.eNormalType> types = global::Utils.GetBalancedInitialNormalTypes(m_boardSizeX * m_boardSizeY);
        int index = 0;

        for (int y = 0; y < m_boardSizeY; y++)
        {
            for (int x = 0; x < m_boardSizeX; x++)
            {
                SpawnItemOnBoard(m_boardCells[x, y], types[index]);
                index++;
            }
        }
    }

    public void SpawnItemOnBoard(Cell cell, NormalItem.eNormalType type)
    {
        if (cell == null)
        {
            return;
        }

        NormalItem item = new NormalItem();
        item.SetType(type);
        item.SetView();
        item.SetViewRoot(m_root);

        cell.AssignAsHome(item);
        cell.ApplyItemPosition(true);
    }

    public IEnumerable<NormalItem.eNormalType> GetBoardItemTypes()
    {
        foreach (Cell cell in m_boardCells)
        {
            if (cell == null || cell.Item == null)
            {
                continue;
            }

            NormalItem normal = cell.Item as NormalItem;
            if (normal != null)
            {
                yield return normal.ItemType;
            }
        }
    }

    public List<Cell> GetCellsWithType(NormalItem.eNormalType type)
    {
        List<Cell> result = new List<Cell>();

        foreach (Cell cell in m_boardCells)
        {
            NormalItem normal = cell.Item as NormalItem;
            if (normal != null && normal.ItemType == type)
            {
                result.Add(cell);
            }
        }

        return result;
    }

    public Cell GetFirstFreeBottomCell()
    {
        return m_bottomCells.FirstOrDefault(x => x.IsEmpty);
    }

    public Cell GetBottomCellWithItem(Item item)
    {
        return m_bottomCells.FirstOrDefault(x => x.Item == item);
    }

    public Cell GetHomeCellForBottomItem(Cell bottomCell)
    {
        if (bottomCell?.Item == null)
        {
            return null;
        }

        return bottomCell.Item.HomeCell;
    }

    public int GetBoardItemCount()
    {
        int count = 0;
        foreach (Cell cell in m_boardCells)
        {
            if (cell != null && cell.Item != null)
            {
                count++;
            }
        }

        return count;
    }

    public int GetBottomItemCount()
    {
        int count = 0;
        foreach (Cell cell in m_bottomCells)
        {
            if (cell != null && cell.Item != null)
            {
                count++;
            }
        }

        return count;
    }

    public bool IsBoardClear()
    {
        return GetBoardItemCount() == 0;
    }

    public bool IsBottomFull()
    {
        return GetBottomItemCount() >= BottomCellCount;
    }

    public Cell GetCellFromCollider(Collider2D collider)
    {
        if (collider == null)
        {
            return null;
        }

        return collider.GetComponent<Cell>();
    }

    public void MoveBoardItemToBottom(Cell boardCell, Cell bottomCell, float duration, Action onComplete = null)
    {
        if (boardCell == null || bottomCell == null || boardCell.Item == null || !bottomCell.IsEmpty)
        {
            onComplete?.Invoke();
            return;
        }

        Item item = boardCell.Item;
        boardCell.Free();
        bottomCell.Assign(item);
        item.MoveToPosition(bottomCell.transform.position, duration, onComplete);
    }

    public void MoveBottomItemToBoard(Cell bottomCell, Cell boardCell, float duration, Action onComplete = null)
    {
        if (bottomCell == null || boardCell == null || bottomCell.Item == null || !boardCell.IsEmpty)
        {
            onComplete?.Invoke();
            return;
        }

        Item item = bottomCell.Item;
        bottomCell.Free();
        boardCell.Assign(item);
        item.MoveToPosition(boardCell.transform.position, duration, onComplete);
    }

    public void ClearBottomCellsOfType(NormalItem.eNormalType type, Action onComplete = null)
    {
        List<Cell> matches = m_bottomCells
            .Where(x => x.Item is NormalItem normal && normal.ItemType == type)
            .ToList();

        if (matches.Count < 3)
        {
            onComplete?.Invoke();
            return;
        }

        foreach (Cell cell in matches)
        {
            cell.ExplodeItem();
        }

        onComplete?.Invoke();
    }

    public void Clear()
    {
        for (int x = 0; x < m_boardSizeX; x++)
        {
            for (int y = 0; y < m_boardSizeY; y++)
            {
                Cell cell = m_boardCells[x, y];
                if (cell != null)
                {
                    cell.Clear();
                    GameObject.Destroy(cell.gameObject);
                    m_boardCells[x, y] = null;
                }
            }
        }

        for (int i = 0; i < m_bottomCells.Count; i++)
        {
            Cell cell = m_bottomCells[i];
            if (cell != null)
            {
                cell.Clear();
                GameObject.Destroy(cell.gameObject);
            }
        }

        m_bottomCells.Clear();
    }
}

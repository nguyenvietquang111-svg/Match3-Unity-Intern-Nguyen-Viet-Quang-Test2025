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
    private readonly float m_cellSpacing;
    private readonly float m_tileScale;

    private readonly Cell[,] m_boardCells;
    private readonly List<Cell> m_bottomCells = new List<Cell>();
    private readonly List<GameObject> m_backplateObjects = new List<GameObject>();

    private const float BoardCenterY = 0.55f;
    private const float TrayCenterY = -3.75f;
    private const float BottomTrayCellSpacing = 1.0f;

    public IReadOnlyList<Cell> BoardCells => m_boardCells.Cast<Cell>().ToList();

    public IReadOnlyList<Cell> BottomCells => m_bottomCells;

    public Board(Transform transform, GameSettings gameSettings, int boardSizeX = 0, int boardSizeY = 0)
    {
        m_root = transform;
        m_gameSettings = gameSettings;
        m_boardSizeX = boardSizeX > 0 ? boardSizeX : (gameSettings != null ? gameSettings.BoardSizeX : 4);
        m_boardSizeY = boardSizeY > 0 ? boardSizeY : (gameSettings != null ? gameSettings.BoardSizeY : 6);
        m_boardCells = new Cell[m_boardSizeX, m_boardSizeY];

        float maxPlayfieldWidth = 4.4f;
        float maxPlayfieldHeight = 6.2f;
        m_cellSpacing = Mathf.Min(maxPlayfieldWidth / m_boardSizeX, maxPlayfieldHeight / m_boardSizeY, 1.0f);
        m_tileScale = m_cellSpacing * 0.92f;

        CreateBoardBackplate();
        CreateTrayBackplate();
        CreateBoardCells();
        CreateBottomCells();
        FillInitialItems();
    }

    private void CreateBoardBackplate()
    {
        Sprite panelSprite = Resources.Load<Sprite>("textures/panel_blue");
        if (panelSprite == null)
        {
            GameObject prefabBG = Resources.Load<GameObject>(Constants.PREFAB_CELL_BACKGROUND);
            if (prefabBG != null)
            {
                SpriteRenderer sr = prefabBG.GetComponent<SpriteRenderer>();
                if (sr != null) panelSprite = sr.sprite;
            }
        }

        GameObject backplateRoot = new GameObject("BoardBackplate");
        backplateRoot.transform.SetParent(m_root, false);
        backplateRoot.transform.position = new Vector3(0f, BoardCenterY, 0f);
        m_backplateObjects.Add(backplateRoot);

        float boardWidth = m_boardSizeX * m_cellSpacing + 0.35f * m_cellSpacing + 0.10f;
        float boardHeight = m_boardSizeY * m_cellSpacing + 0.35f * m_cellSpacing + 0.10f;

        // Shadow layer
        GameObject shadowGO = new GameObject("BoardShadow");
        shadowGO.transform.SetParent(backplateRoot.transform, false);
        shadowGO.transform.localPosition = new Vector3(0.06f * m_cellSpacing, -0.08f * m_cellSpacing, 0f);
        SpriteRenderer shadowSR = shadowGO.AddComponent<SpriteRenderer>();
        shadowSR.sprite = panelSprite;
        shadowSR.drawMode = SpriteDrawMode.Sliced;
        shadowSR.size = new Vector2(boardWidth + 0.05f, boardHeight + 0.05f);
        shadowSR.color = new Color(0f, 0f, 0f, 0.45f);
        shadowSR.sortingOrder = -2;

        // Main glass frame
        GameObject frameGO = new GameObject("BoardFrame");
        frameGO.transform.SetParent(backplateRoot.transform, false);
        frameGO.transform.localPosition = Vector3.zero;
        SpriteRenderer frameSR = frameGO.AddComponent<SpriteRenderer>();
        frameSR.sprite = panelSprite;
        frameSR.drawMode = SpriteDrawMode.Sliced;
        frameSR.size = new Vector2(boardWidth, boardHeight);
        frameSR.color = new Color(0.02f, 0.08f, 0.16f, 0.85f);
        frameSR.sortingOrder = -1;

        // Inner neon rim
        GameObject rimGO = new GameObject("BoardRim");
        rimGO.transform.SetParent(backplateRoot.transform, false);
        rimGO.transform.localPosition = Vector3.zero;
        SpriteRenderer rimSR = rimGO.AddComponent<SpriteRenderer>();
        rimSR.sprite = panelSprite;
        rimSR.drawMode = SpriteDrawMode.Sliced;
        rimSR.size = new Vector2(boardWidth - 0.10f * m_cellSpacing, boardHeight - 0.10f * m_cellSpacing);
        rimSR.color = new Color(0.12f, 0.45f, 0.70f, 0.30f);
        rimSR.sortingOrder = 0;
    }

    private void CreateTrayBackplate()
    {
        Sprite panelSprite = Resources.Load<Sprite>("textures/panel_blue");
        if (panelSprite == null)
        {
            GameObject prefabBG = Resources.Load<GameObject>(Constants.PREFAB_CELL_BACKGROUND);
            if (prefabBG != null)
            {
                SpriteRenderer sr = prefabBG.GetComponent<SpriteRenderer>();
                if (sr != null) panelSprite = sr.sprite;
            }
        }

        GameObject dockRoot = new GameObject("TrayDockBackplate");
        dockRoot.transform.SetParent(m_root, false);
        dockRoot.transform.position = new Vector3(0f, TrayCenterY, 0f);
        m_backplateObjects.Add(dockRoot);

        float dockWidth = BottomCellCount * BottomTrayCellSpacing + 0.45f;
        float dockHeight = BottomTrayCellSpacing + 0.35f;

        // Shadow layer
        GameObject shadowGO = new GameObject("DockShadow");
        shadowGO.transform.SetParent(dockRoot.transform, false);
        shadowGO.transform.localPosition = new Vector3(0.06f, -0.08f, 0f);
        SpriteRenderer shadowSR = shadowGO.AddComponent<SpriteRenderer>();
        shadowSR.sprite = panelSprite;
        shadowSR.drawMode = SpriteDrawMode.Sliced;
        shadowSR.size = new Vector2(dockWidth + 0.05f, dockHeight + 0.05f);
        shadowSR.color = new Color(0f, 0f, 0f, 0.50f);
        shadowSR.sortingOrder = -2;

        // Main floating dock bar
        GameObject dockGO = new GameObject("DockFrame");
        dockGO.transform.SetParent(dockRoot.transform, false);
        dockGO.transform.localPosition = Vector3.zero;
        SpriteRenderer dockSR = dockGO.AddComponent<SpriteRenderer>();
        dockSR.sprite = panelSprite;
        dockSR.drawMode = SpriteDrawMode.Sliced;
        dockSR.size = new Vector2(dockWidth, dockHeight);
        dockSR.color = new Color(0.02f, 0.07f, 0.14f, 0.92f);
        dockSR.sortingOrder = -1;

        // Inner neon accent rim
        GameObject rimGO = new GameObject("DockRim");
        rimGO.transform.SetParent(dockRoot.transform, false);
        rimGO.transform.localPosition = Vector3.zero;
        SpriteRenderer rimSR = rimGO.AddComponent<SpriteRenderer>();
        rimSR.sprite = panelSprite;
        rimSR.drawMode = SpriteDrawMode.Sliced;
        rimSR.size = new Vector2(dockWidth - 0.10f, dockHeight - 0.10f);
        rimSR.color = new Color(0.15f, 0.55f, 0.80f, 0.40f);
        rimSR.sortingOrder = 0;
    }

    private void CreateBoardCells()
    {
        GameObject prefabBG = Resources.Load<GameObject>(Constants.PREFAB_CELL_BACKGROUND);
        Vector3 origin = new Vector3(
            -(m_boardSizeX - 1) * 0.5f * m_cellSpacing,
            BoardCenterY + (m_boardSizeY - 1) * 0.5f * m_cellSpacing,
            0f);

        for (int x = 0; x < m_boardSizeX; x++)
        {
            for (int y = 0; y < m_boardSizeY; y++)
            {
                GameObject go = GameObject.Instantiate(prefabBG, m_root);
                go.transform.position = origin + new Vector3(x * m_cellSpacing, -y * m_cellSpacing, 0f);

                Cell cell = go.GetComponent<Cell>();
                cell.Setup(x, y, false, m_tileScale);
                m_boardCells[x, y] = cell;
            }
        }
    }

    private void CreateBottomCells()
    {
        GameObject prefabBG = Resources.Load<GameObject>(Constants.PREFAB_CELL_BACKGROUND);
        Vector3 origin = new Vector3(
            -(BottomCellCount - 1) * 0.5f * BottomTrayCellSpacing,
            TrayCenterY,
            0f);

        for (int i = 0; i < BottomCellCount; i++)
        {
            GameObject go = GameObject.Instantiate(prefabBG, m_root);
            go.transform.position = origin + new Vector3(i * BottomTrayCellSpacing, 0f, 0f);

            Cell cell = go.GetComponent<Cell>();
            cell.Setup(i, -1, true, 0.92f);
            m_bottomCells.Add(cell);
        }
    }

    private void FillInitialItems()
    {
        int totalCells = m_boardSizeX * m_boardSizeY;
        int activeCount = totalCells - (totalCells % 3);

        List<NormalItem.eNormalType> types = global::Utils.GetBalancedInitialNormalTypes(activeCount);
        int index = 0;

        for (int y = 0; y < m_boardSizeY; y++)
        {
            for (int x = 0; x < m_boardSizeX; x++)
            {
                if (index < types.Count)
                {
                    float delay = Mathf.Min(0.5f, (x + y) * (0.15f / Mathf.Max(1, m_boardSizeX + m_boardSizeY)));
                    SpawnItemOnBoard(m_boardCells[x, y], types[index], delay);
                    index++;
                }
            }
        }
    }

    public void SpawnItemOnBoard(Cell cell, NormalItem.eNormalType type, float delay = 0f)
    {
        if (cell == null)
        {
            return;
        }

        NormalItem item = new NormalItem();
        item.SetType(type);
        item.SetView();
        item.SetViewRoot(m_root);
        item.SetCustomScale(m_cellSpacing * 0.86f);

        cell.AssignAsHome(item);
        cell.ApplyItemPosition(true, delay);
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
        item.FlyToPosition(bottomCell.transform.position, duration, 0.45f, 0.86f, onComplete);
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
        item.FlyToPosition(boardCell.transform.position, duration, 0.45f, m_cellSpacing * 0.86f, onComplete);
    }

    public void CompactBottomTray(float duration, Action onComplete = null)
    {
        List<Item> currentItems = new List<Item>();
        for (int i = 0; i < m_bottomCells.Count; i++)
        {
            if (m_bottomCells[i].Item != null)
            {
                currentItems.Add(m_bottomCells[i].Item);
            }
        }

        for (int i = 0; i < m_bottomCells.Count; i++)
        {
            m_bottomCells[i].Free();
        }

        if (currentItems.Count == 0)
        {
            onComplete?.Invoke();
            return;
        }

        int movingCount = 0;
        Action checkDone = () =>
        {
            movingCount--;
            if (movingCount <= 0)
            {
                onComplete?.Invoke();
            }
        };

        for (int i = 0; i < currentItems.Count; i++)
        {
            Cell targetCell = m_bottomCells[i];
            Item item = currentItems[i];
            targetCell.Assign(item);

            if (item.View != null && Vector3.Distance(item.View.position, targetCell.transform.position) > 0.01f)
            {
                movingCount++;
                item.SlideToPosition(targetCell.transform.position, duration, checkDone);
            }
        }

        if (movingCount == 0)
        {
            onComplete?.Invoke();
        }
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

        int explodingCount = matches.Count;
        Action checkExploded = () =>
        {
            explodingCount--;
            if (explodingCount <= 0)
            {
                CompactBottomTray(0.18f, onComplete);
            }
        };

        foreach (Cell cell in matches)
        {
            cell.ExplodeItem(checkExploded);
        }
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

        for (int i = 0; i < m_backplateObjects.Count; i++)
        {
            if (m_backplateObjects[i] != null)
            {
                GameObject.Destroy(m_backplateObjects[i]);
            }
        }
        m_backplateObjects.Clear();
    }
}

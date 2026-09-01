using System;
using UnityEngine;

public class Cell : MonoBehaviour
{
    public int BoardX { get; private set; }

    public int BoardY { get; private set; }

    public bool IsBottomCell { get; private set; }

    public Item Item { get; private set; }

    public Cell NeighbourUp { get; set; }

    public Cell NeighbourRight { get; set; }

    public Cell NeighbourBottom { get; set; }

    public Cell NeighbourLeft { get; set; }

    public bool IsEmpty => Item == null;

    private SpriteRenderer m_spriteRenderer;
    private GameObject m_shadowObj;
    private GameObject m_glowObj;

    public void Setup(int cellX, int cellY, bool isBottomCell = false, float tileSize = 0.92f)
    {
        this.BoardX = cellX;
        this.BoardY = cellY;
        IsBottomCell = isBottomCell;

        ConfigureVisuals(tileSize);
    }

    private void ConfigureVisuals(float tileSize)
    {
        m_spriteRenderer = GetComponent<SpriteRenderer>();
        Sprite panelSprite = Resources.Load<Sprite>("textures/panel_blue");
        if (panelSprite == null && m_spriteRenderer != null)
        {
            panelSprite = m_spriteRenderer.sprite;
        }

        if (m_spriteRenderer != null)
        {
            if (panelSprite != null)
            {
                m_spriteRenderer.sprite = panelSprite;
            }
            m_spriteRenderer.drawMode = SpriteDrawMode.Sliced;
            m_spriteRenderer.size = new Vector2(tileSize, tileSize);
            m_spriteRenderer.sortingOrder = 1;

            if (IsBottomCell)
            {
                // Deep recessed dark glass socket for bottom tray
                m_spriteRenderer.color = new Color(0.03f, 0.10f, 0.18f, 0.88f);
            }
            else
            {
                // Sleek translucent ocean/cyan glass tile for board
                m_spriteRenderer.color = new Color(0.10f, 0.32f, 0.48f, 0.82f);
            }
        }

        BoxCollider2D box = GetComponent<BoxCollider2D>();
        if (box != null)
        {
            box.size = new Vector2(tileSize, tileSize);
        }

        // Soft drop shadow
        if (m_shadowObj == null)
        {
            m_shadowObj = new GameObject("TileShadow");
            m_shadowObj.transform.SetParent(this.transform, false);
            m_shadowObj.transform.localPosition = new Vector3(tileSize * 0.04f, -tileSize * 0.05f, 0f);
            SpriteRenderer shadowSR = m_shadowObj.AddComponent<SpriteRenderer>();
            shadowSR.sprite = panelSprite;
            shadowSR.drawMode = SpriteDrawMode.Sliced;
            shadowSR.size = new Vector2(tileSize, tileSize);
            shadowSR.color = new Color(0f, 0f, 0f, 0.40f);
            shadowSR.sortingOrder = 0;
        }

        // Inner rim / glow highlight
        if (m_glowObj == null)
        {
            m_glowObj = new GameObject("TileGlow");
            m_glowObj.transform.SetParent(this.transform, false);
            m_glowObj.transform.localPosition = Vector3.zero;
            SpriteRenderer glowSR = m_glowObj.AddComponent<SpriteRenderer>();
            glowSR.sprite = panelSprite;
            glowSR.drawMode = SpriteDrawMode.Sliced;
            glowSR.size = new Vector2(tileSize * 0.93f, tileSize * 0.93f);
            glowSR.sortingOrder = 2;

            if (IsBottomCell)
            {
                glowSR.color = new Color(0.12f, 0.45f, 0.70f, 0.25f);
            }
            else
            {
                glowSR.color = new Color(0.25f, 0.65f, 0.90f, 0.35f);
            }
        }
    }

    public bool IsNeighbour(Cell other)
    {
        return BoardX == other.BoardX && Mathf.Abs(BoardY - other.BoardY) == 1 ||
            BoardY == other.BoardY && Mathf.Abs(BoardX - other.BoardX) == 1;
    }

    public void Free()
    {
        Item = null;
    }

    public void Assign(Item item)
    {
        Item = item;
        Item.SetCell(this);
    }

    public void AssignAsHome(Item item)
    {
        Assign(item);
        item.SetHomeCell(this);
    }

    public void ApplyItemPosition(bool withAppearAnimation, float delay = 0f)
    {
        Item.SetViewPosition(this.transform.position);

        if (withAppearAnimation)
        {
            Item.ShowAppearAnimation(delay);
        }
    }

    internal void Clear()
    {
        if (Item != null)
        {
            Item.Clear();
            Item = null;
        }
    }

    internal bool IsSameType(Cell other)
    {
        return Item != null && other.Item != null && Item.IsSameType(other.Item);
    }

    internal void ExplodeItem(Action onComplete = null)
    {
        if (Item == null)
        {
            onComplete?.Invoke();
            return;
        }

        Item itemToExplode = Item;
        Item = null;
        itemToExplode.ExplodeView(onComplete);
    }

    internal void AnimateItemForHint()
    {
        Item?.AnimateForHint();
    }

    internal void StopHintAnimation()
    {
        Item?.StopAnimateForHint();
    }

    internal void ApplyItemMoveToPosition()
    {
        Item?.AnimationMoveToPosition();
    }
}

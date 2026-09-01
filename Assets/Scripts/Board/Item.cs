using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[Serializable]
public class Item
{
    public Cell Cell { get; private set; }

    public Cell HomeCell { get; private set; }

    public Transform View { get; private set; }

    public float TargetScale { get; private set; } = 0.86f;

    public virtual void SetView()
    {
        string prefabname = GetPrefabName();

        if (!string.IsNullOrEmpty(prefabname))
        {
            GameObject prefab = Resources.Load<GameObject>(prefabname);
            if (prefab)
            {
                View = GameObject.Instantiate(prefab).transform;
                View.localScale = new Vector3(TargetScale, TargetScale, 1f);
            }
        }
    }

    public void SetCustomScale(float scale)
    {
        TargetScale = scale;
        if (View != null)
        {
            View.localScale = new Vector3(scale, scale, 1f);
        }
    }

    protected virtual string GetPrefabName() { return string.Empty; }

    public virtual void SetCell(Cell cell)
    {
        Cell = cell;
    }

    public void SetHomeCell(Cell cell)
    {
        if (HomeCell == null)
        {
            HomeCell = cell;
        }
    }

    internal void MoveToPosition(Vector3 targetPosition, float duration, Action onComplete = null)
    {
        if (View == null)
        {
            onComplete?.Invoke();
            return;
        }

        View.DOMove(targetPosition, duration).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            onComplete?.Invoke();
        });
    }

    internal void SlideToPosition(Vector3 targetPosition, float duration = 0.18f, Action onComplete = null)
    {
        if (View == null)
        {
            onComplete?.Invoke();
            return;
        }

        View.DOKill();
        View.DOMove(targetPosition, duration).SetEase(Ease.OutCubic).OnComplete(() =>
        {
            onComplete?.Invoke();
        });
    }

    internal void FlyToPosition(Vector3 targetPosition, float duration = 0.28f, float jumpPower = 0.45f, float endScale = 0.86f, Action onComplete = null)
    {
        if (View == null)
        {
            onComplete?.Invoke();
            return;
        }

        View.DOKill();
        SetSortingLayerHigher();

        View.DOScale(new Vector3(endScale, endScale, 1f), duration).SetEase(Ease.OutQuad);
        View.DOJump(targetPosition, jumpPower, 1, duration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                SetSortingLayerLower();
                if (View != null)
                {
                    float punchMagnitude = 0.14f * (endScale / 0.86f);
                    View.DOPunchScale(new Vector3(punchMagnitude, -punchMagnitude, 0f), 0.16f, 5, 0.5f);
                }
                onComplete?.Invoke();
            });
    }

    internal void AnimationMoveToPosition()
    {
        if (Cell == null)
        {
            return;
        }

        MoveToPosition(Cell.transform.position, 0.2f);
    }

    public void SetViewPosition(Vector3 pos)
    {
        if (View)
        {
            View.position = pos;
        }
    }

    public void SetViewRoot(Transform root)
    {
        if (View)
        {
            View.SetParent(root);
        }
    }

    public void SetSortingLayerHigher()
    {
        if (View == null) return;

        SpriteRenderer[] renderers = View.GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].sortingOrder = 15;
        }
    }

    public void SetSortingLayerLower()
    {
        if (View == null) return;

        SpriteRenderer[] renderers = View.GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].sortingOrder = 3;
        }
    }

    internal void ShowAppearAnimation(float delay = 0f)
    {
        if (View == null) return;

        Vector3 targetScale = new Vector3(TargetScale, TargetScale, 1f);
        View.localScale = Vector3.zero;
        View.DOScale(targetScale, 0.25f).SetEase(Ease.OutBack).SetDelay(delay);
    }

    internal virtual bool IsSameType(Item other)
    {
        return false;
    }

    internal virtual void ExplodeView(Action onComplete = null)
    {
        if (View == null)
        {
            onComplete?.Invoke();
            return;
        }

        View.DOKill();
        SetSortingLayerHigher();
        Vector3 currentScale = View.localScale;

        Sequence seq = DOTween.Sequence();
        seq.Append(View.DOScale(currentScale * 1.25f, 0.1f).SetEase(Ease.OutBack));
        seq.Append(View.DOScale(Vector3.zero, 0.15f).SetEase(Ease.InBack));
        seq.OnComplete(() =>
        {
            if (View)
            {
                GameObject.Destroy(View.gameObject);
                View = null;
            }
            onComplete?.Invoke();
        });
    }

    internal void AnimateForHint()
    {
        if (View)
        {
            View.DOPunchScale(View.localScale * 0.1f, 0.1f).SetLoops(-1);
        }
    }

    internal void StopAnimateForHint()
    {
        if (View)
        {
            View.DOKill();
        }
    }

    internal void Clear()
    {
        Cell = null;

        if (View)
        {
            View.DOKill();
            GameObject.Destroy(View.gameObject);
            View = null;
        }
    }
}

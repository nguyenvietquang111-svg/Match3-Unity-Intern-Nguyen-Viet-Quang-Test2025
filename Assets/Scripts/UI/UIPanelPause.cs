using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIPanelPause : MonoBehaviour, IMenu
{
    [SerializeField] private Button btnClose;

    private UIMainManager m_mngr;
    private CanvasGroup m_canvasGroup;
    private Transform m_dialogPanel;

    private void Awake()
    {
        m_canvasGroup = GetComponent<CanvasGroup>();
        if (m_canvasGroup == null)
        {
            m_canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        m_dialogPanel = transform.Find("Panel");
        if (m_dialogPanel == null && transform.childCount > 0)
        {
            m_dialogPanel = transform.GetChild(0);
        }

        if (btnClose != null)
        {
            btnClose.onClick.AddListener(OnClickClose);
        }
    }

    private void OnDestroy()
    {
        if (btnClose != null)
        {
            btnClose.onClick.RemoveListener(OnClickClose);
        }
    }

    public void Setup(UIMainManager mngr)
    {
        m_mngr = mngr;
    }

    private void OnClickClose()
    {
        if (m_dialogPanel != null)
        {
            m_dialogPanel.DOKill();
            m_dialogPanel.DOScale(Vector3.zero, 0.18f)
                .SetEase(Ease.InBack)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    if (m_mngr != null)
                    {
                        m_mngr.ShowGameMenu();
                    }
                });
        }
        else
        {
            if (m_mngr != null)
            {
                m_mngr.ShowGameMenu();
            }
        }
    }

    public void Show()
    {
        this.gameObject.SetActive(true);
        PlayPopUpAnimation();
    }

    private void PlayPopUpAnimation()
    {
        if (m_canvasGroup != null)
        {
            m_canvasGroup.DOKill();
            m_canvasGroup.alpha = 0f;
            m_canvasGroup.DOFade(1f, 0.25f).SetUpdate(true);
        }

        if (m_dialogPanel != null)
        {
            m_dialogPanel.DOKill();
            m_dialogPanel.localScale = Vector3.zero;
            m_dialogPanel.DOScale(Vector3.one, 0.38f)
                .SetEase(Ease.OutBack)
                .SetUpdate(true);
        }

        if (btnClose != null)
        {
            btnClose.transform.DOKill();
            btnClose.transform.localScale = Vector3.zero;
            btnClose.transform.DOScale(Vector3.one, 0.28f)
                .SetEase(Ease.OutBack)
                .SetDelay(0.12f)
                .SetUpdate(true);
        }
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
    }
}

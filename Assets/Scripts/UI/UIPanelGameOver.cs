using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIPanelGameOver : MonoBehaviour, IMenu
{
    [SerializeField] private Button btnClose;

    private UIMainManager m_mngr;
    private CanvasGroup m_canvasGroup;
    private Transform m_dialogPanel;
    private Button m_btnContinue;

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

        if (m_btnContinue != null)
        {
            m_btnContinue.onClick.RemoveListener(OnClickContinue);
        }
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
                        m_mngr.ShowMainMenu();
                    }
                });
        }
        else
        {
            if (m_mngr != null)
            {
                m_mngr.ShowMainMenu();
            }
        }
    }

    private void OnClickContinue()
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
                        m_mngr.ContinueToNextRound();
                    }
                });
        }
        else
        {
            if (m_mngr != null)
            {
                m_mngr.ContinueToNextRound();
            }
        }
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
    }

    public void Setup(UIMainManager mngr)
    {
        m_mngr = mngr;
    }

    public void Show()
    {
        this.gameObject.SetActive(true);
        ConfigureDialogContent();
        PlayPopUpAnimation();
    }

    private void ConfigureDialogContent()
    {
        bool isWinPanel = gameObject.name.Contains("Win");

        if (isWinPanel && m_mngr != null)
        {
            int currentRound = m_mngr.GetCurrentRound();
            m_mngr.GetCurrentDimensions(out int curX, out int curY);
            m_mngr.GetNextRoundDimensions(out int nextX, out int nextY);

            Text titleText = m_dialogPanel != null ? m_dialogPanel.GetComponentInChildren<Text>() : null;
            if (titleText != null)
            {
                titleText.text = string.Format("ROUND {0} WIN!\n({1}x{2})", currentRound, curX, curY);
            }

            if (m_btnContinue == null && btnClose != null)
            {
                GameObject continueGO = Instantiate(btnClose.gameObject, m_dialogPanel);
                continueGO.name = "btnContinue";
                m_btnContinue = continueGO.GetComponent<Button>();
                m_btnContinue.onClick.RemoveAllListeners();
                m_btnContinue.onClick.AddListener(OnClickContinue);
            }

            if (btnClose != null && m_btnContinue != null)
            {
                RectTransform closeRect = btnClose.GetComponent<RectTransform>();
                if (closeRect != null)
                {
                    closeRect.anchoredPosition = new Vector2(-125f, -157.2f);
                    closeRect.sizeDelta = new Vector2(210f, 64f);
                }

                Text closeText = btnClose.GetComponentInChildren<Text>();
                if (closeText != null)
                {
                    closeText.text = "HOME";
                }

                RectTransform continueRect = m_btnContinue.GetComponent<RectTransform>();
                if (continueRect != null)
                {
                    continueRect.anchoredPosition = new Vector2(125f, -157.2f);
                    continueRect.sizeDelta = new Vector2(210f, 64f);
                }

                Text continueText = m_btnContinue.GetComponentInChildren<Text>();
                if (continueText != null)
                {
                    if (curX >= 32 && curY >= 32)
                    {
                        continueText.text = "REPLAY (32x32)";
                    }
                    else
                    {
                        continueText.text = string.Format("CONTINUE\n({0}x{1})", nextX, nextY);
                    }
                }

                m_btnContinue.gameObject.SetActive(true);
            }
        }
        else
        {
            if (m_btnContinue != null)
            {
                m_btnContinue.gameObject.SetActive(false);
            }

            if (btnClose != null)
            {
                RectTransform closeRect = btnClose.GetComponent<RectTransform>();
                if (closeRect != null)
                {
                    closeRect.anchoredPosition = new Vector2(0f, -157.2f);
                    closeRect.sizeDelta = new Vector2(236.4f, 64f);
                }

                Text closeText = btnClose.GetComponentInChildren<Text>();
                if (closeText != null)
                {
                    closeText.text = "OK";
                }
            }
        }
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

        if (m_btnContinue != null && m_btnContinue.gameObject.activeSelf)
        {
            m_btnContinue.transform.DOKill();
            m_btnContinue.transform.localScale = Vector3.zero;
            m_btnContinue.transform.DOScale(Vector3.one, 0.28f)
                .SetEase(Ease.OutBack)
                .SetDelay(0.15f)
                .SetUpdate(true);
        }
    }
}

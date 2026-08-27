using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPanelMain : MonoBehaviour, IMenu
{
    [SerializeField] private Button btnMoves;

    [SerializeField] private Button btnTimer;

    private readonly List<Button> m_ownedButtons = new List<Button>();

    private UIMainManager m_mngr;
    private bool m_buttonsBuilt;

    private void Awake()
    {
        BuildButtons();
    }

    private void OnDestroy()
    {
        for (int i = 0; i < m_ownedButtons.Count; i++)
        {
            if (m_ownedButtons[i] != null)
            {
                m_ownedButtons[i].onClick.RemoveAllListeners();
            }
        }
    }

    public void Setup(UIMainManager mngr)
    {
        m_mngr = mngr;
    }

    private void BuildButtons()
    {
        if (m_buttonsBuilt)
        {
            return;
        }

        if (btnMoves == null)
        {
            Debug.LogWarning("UIPanelMain: btnMoves is not assigned.", this);
            return;
        }

        ConfigureTemplateButton(btnMoves, "Play", new Vector2(0f, 120f), OnClickPlay);
        m_ownedButtons.Add(btnMoves);

        Button autoplay = CreateButtonClone(btnMoves, "Autoplay", new Vector2(0f, 30f), OnClickAutoplay);
        if (autoplay != null)
        {
            m_ownedButtons.Add(autoplay);
        }

        Button autoLose = CreateButtonClone(btnMoves, "Auto Lose", new Vector2(0f, -60f), OnClickAutoLose);
        if (autoLose != null)
        {
            m_ownedButtons.Add(autoLose);
        }

        Button timeAttack = CreateButtonClone(btnMoves, "Time Attack", new Vector2(0f, -150f), OnClickTimeAttack);
        if (timeAttack != null)
        {
            m_ownedButtons.Add(timeAttack);
        }

        if (btnTimer != null && btnTimer != btnMoves)
        {
            btnTimer.gameObject.SetActive(false);
        }

        m_buttonsBuilt = true;
    }

    private Button CreateButtonClone(Button template, string label, Vector2 anchoredPosition, UnityEngine.Events.UnityAction onClick)
    {
        if (template == null)
        {
            return null;
        }

        Transform parent = template.transform.parent != null ? template.transform.parent : transform;
        Button clone = Instantiate(template, parent);
        if (clone == null)
        {
            return null;
        }

        clone.name = "btn" + label.Replace(" ", string.Empty);

        RectTransform rect = clone.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(360f, 80f);
        }

        ConfigureButton(clone, label, anchoredPosition, onClick);
        return clone;
    }

    private void ConfigureTemplateButton(Button button, string label, Vector2 anchoredPosition, UnityEngine.Events.UnityAction onClick)
    {
        if (button == null)
        {
            return;
        }

        RectTransform rect = button.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(360f, 80f);
        }

        button.name = "btnPlay";

        ConfigureButton(button, label, anchoredPosition, onClick);
    }

    private void ConfigureButton(Button button, string label, Vector2 anchoredPosition, UnityEngine.Events.UnityAction onClick)
    {
        if (button == null)
        {
            return;
        }

        Text text = button.GetComponentInChildren<Text>(true);
        if (text != null)
        {
            text.text = label;
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(onClick);

        RectTransform rect = button.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchoredPosition = anchoredPosition;
        }
    }

    private void OnClickPlay()
    {
        if (m_mngr != null)
        {
            m_mngr.LoadNormalLevel();
        }
    }

    private void OnClickAutoplay()
    {
        if (m_mngr != null)
        {
            m_mngr.LoadAutoplayLevel();
        }
    }

    private void OnClickAutoLose()
    {
        if (m_mngr != null)
        {
            m_mngr.LoadAutoLoseLevel();
        }
    }

    private void OnClickTimeAttack()
    {
        if (m_mngr != null)
        {
            m_mngr.LoadLevelTimer();
        }
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}

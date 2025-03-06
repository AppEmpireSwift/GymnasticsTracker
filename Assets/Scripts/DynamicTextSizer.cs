using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DynamicTextSizer : MonoBehaviour
{
    [Tooltip("Ссылка на TextMeshProUGUI компонент")]
    public TextMeshProUGUI _textComponent;

    [Tooltip("Ссылка на ScrollRect")]
    public ScrollRect _scrollRect;

    [Tooltip("Ссылка на Content RectTransform")]
    public RectTransform _contentRectTransform;

    [Tooltip("Минимальная высота контента")]
    public float _minHeight = 50f;

    [Tooltip("Максимальная высота контента (0 для неограниченной высоты)")]
    public float _maxHeight = 0f;

    [Tooltip("Добавочный отступ по вертикали")]
    public float _verticalPadding = 10f;

    private string _lastText = "";

    private void Awake()
    {
        if (_textComponent == null)
            _textComponent = GetComponent<TextMeshProUGUI>();

        if (_scrollRect == null)
            _scrollRect = GetComponentInParent<ScrollRect>();

        if (_contentRectTransform == null && _scrollRect != null)
            _contentRectTransform = _scrollRect.content;
    }

    private void Update()
    {
        if (_textComponent.text != _lastText)
        {
            _lastText = _textComponent.text;
            ResizeTextContainer();
        }
    }

    private void ResizeTextContainer()
    {
        StartCoroutine(ResizeNextFrame());
    }

    private System.Collections.IEnumerator ResizeNextFrame()
    {
        yield return new WaitForEndOfFrame();

        float preferredHeight = _textComponent.preferredHeight;

        preferredHeight += _verticalPadding * 2;

        if (_maxHeight > 0)
        {
            preferredHeight = Mathf.Min(preferredHeight, _maxHeight);
        }

        preferredHeight = Mathf.Max(preferredHeight, _minHeight);

        if (_contentRectTransform != null)
        {
            Vector2 sizeDelta = _contentRectTransform.sizeDelta;
            sizeDelta.y = preferredHeight;
            _contentRectTransform.sizeDelta = sizeDelta;
        }

        RectTransform rectTransform = _textComponent.rectTransform;
        Vector2 textSizeDelta = rectTransform.sizeDelta;
        textSizeDelta.y = preferredHeight - (_verticalPadding * 2);
        rectTransform.sizeDelta = textSizeDelta;
    }

    public void ForceResize()
    {
        StartCoroutine(ResizeNextFrame());
    }

    public void SetText(string newText)
    {
        _textComponent.text = newText;
        _lastText = newText;
        ResizeTextContainer();
    }
}
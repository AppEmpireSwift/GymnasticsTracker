using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class AutoExpandingInputField : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private ScrollRect scrollRect;
    
    [Header("Height Configuration")]
    [SerializeField] private float initialHeight = 50f;
    [SerializeField] private float minHeight = 50f;
    [SerializeField] private float maxHeight = 200f;
    [SerializeField] private float padding = 10f;
    
    private RectTransform inputRectTransform;
    private RectTransform textAreaRectTransform;
    private RectTransform placeholderRectTransform;
    private RectTransform textRectTransform;
    
    private void Start()
    {
        if (inputField == null)
        {
            inputField = GetComponent<TMP_InputField>();
            if (inputField == null)
            {
                Debug.LogError("No TMP_InputField found");
                return;
            }
        }
        
        inputRectTransform = inputField.GetComponent<RectTransform>();
        textAreaRectTransform = inputField.textViewport;
        textRectTransform = inputField.textComponent.GetComponent<RectTransform>();
        
        if (inputField.placeholder != null)
        {
            placeholderRectTransform = inputField.placeholder.GetComponent<RectTransform>();
        }
        
        SetupInputField();
        
        inputField.onValueChanged.AddListener(OnTextChanged);
    }
    
    private void SetupInputField()
    {
        inputField.textComponent.enableWordWrapping = true;
        inputField.textComponent.overflowMode = TextOverflowModes.Overflow;
        
        inputRectTransform.anchorMin = new Vector2(inputRectTransform.anchorMin.x, 1);
        inputRectTransform.anchorMax = new Vector2(inputRectTransform.anchorMax.x, 1);
        inputRectTransform.pivot = new Vector2(inputRectTransform.pivot.x, 1);
        
        initialHeight = Mathf.Max(initialHeight, minHeight);
        inputRectTransform.sizeDelta = new Vector2(inputRectTransform.sizeDelta.x, initialHeight);
        
        UpdateHeight(inputField.text);
    }
    
    private void OnTextChanged(string newText)
    {
        Canvas.ForceUpdateCanvases();
        float preferredTextHeight = inputField.textComponent.preferredHeight;
        
        if (preferredTextHeight + padding >= maxHeight)
        {
            if (newText.Length < inputField.text.Length)
            {
                inputField.interactable = true;
            }
            else if (newText.Length > inputField.text.Length)
            {
                inputField.text = inputField.text;
                return;
            }
        }
        
        UpdateHeight(newText);
    }
    
    private void UpdateHeight(string text)
    {
        if (inputField == null || textRectTransform == null) return;
        
        Canvas.ForceUpdateCanvases();
        
        float preferredTextHeight = inputField.textComponent.preferredHeight;
        float newHeight = Mathf.Clamp(preferredTextHeight + padding, minHeight, maxHeight);
        
        Vector2 currentPosition = inputRectTransform.anchoredPosition;
        
        inputRectTransform.sizeDelta = new Vector2(inputRectTransform.sizeDelta.x, newHeight);
        
        if (textAreaRectTransform != null)
        {
            textAreaRectTransform.sizeDelta = new Vector2(
                textAreaRectTransform.sizeDelta.x, 
                newHeight - (padding / 2)
            );
        }
        
        inputRectTransform.anchoredPosition = currentPosition;
        
        LayoutRebuilder.ForceRebuildLayoutImmediate(inputRectTransform);
        
        if (scrollRect != null)
        {
            StartCoroutine(ScrollDown());
        }
        
        if (preferredTextHeight + padding >= maxHeight)
        {
            inputField.interactable = false;
            StartCoroutine(ReenableInputAfterDelay());
        }
    }
    
    private IEnumerator ReenableInputAfterDelay()
    {
        yield return new WaitForSeconds(0.1f);
        // Проверяем, уменьшился ли текст
        Canvas.ForceUpdateCanvases();
        float currentTextHeight = inputField.textComponent.preferredHeight;
        
        if (currentTextHeight + padding < maxHeight)
        {
            inputField.interactable = true;
        }
    }
    
    private IEnumerator ScrollDown()
    {
        yield return new WaitForEndOfFrame();
        
        scrollRect.verticalNormalizedPosition = 0f;
        Canvas.ForceUpdateCanvases();
    }
    
    private void OnDestroy()
    {
        if (inputField != null)
        {
            inputField.onValueChanged.RemoveListener(OnTextChanged);
        }
    }
}
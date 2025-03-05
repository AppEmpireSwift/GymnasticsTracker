using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MainScreen
{
    public class DateElement : MonoBehaviour
    {
        [SerializeField] private Image _bgImage;
        [SerializeField] private TMP_Text _textNumber;
        [SerializeField] private TMP_Text _textDay;

        [SerializeField] private Color _defaultTextColor = Color.black;
        [SerializeField] private Color _highlightTextColor = Color.white;
        [SerializeField] private Color _highlightBgColor = Color.blue;

        public void SetDateText(DateTime date, bool isCurrentDay)
        {
            _textNumber.text = date.Day.ToString();

            _textDay.text = date.ToString("ddd");

            if (isCurrentDay)
            {
                _bgImage.enabled = true;
                _bgImage.color = _highlightBgColor;

                _textNumber.color = _highlightTextColor;
                _textDay.color = _highlightTextColor;
            }
            else
            {
                _bgImage.enabled = false;
                _textNumber.color = _defaultTextColor;
                _textDay.color = _defaultTextColor;
            }
        }
    }
}
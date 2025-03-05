using System;
using System.Collections.Generic;
using UnityEngine;

namespace MainScreen
{
    public class DateElementHolder : MonoBehaviour
    {
        [SerializeField] private List<DateElement> _dateElements;
        
        private void Start()
        {
            PopulateDateElements();
        }

        private void PopulateDateElements()
        {
            DateTime currentDate = DateTime.Today;

            DateTime startOfWeek = currentDate.AddDays(-(int)currentDate.DayOfWeek + 1);

            for (int i = 0; i < _dateElements.Count; i++)
            {
                DateTime dateToShow = startOfWeek.AddDays(i);
                _dateElements[i].SetDateText(dateToShow, dateToShow.Date == currentDate.Date);
            }
        }
    }
}
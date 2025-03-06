using System;
using System.Collections.Generic;
using Bitsplash.DatePicker;
using UnityEngine;

namespace EntryLogic
{
    [Serializable]
    public class EntryData
    {
        public string Name;
        public string Goal;
        public string Details;
        public int Progress;
        public DateTime Date;
        public List<ProgressData> ProgressDatas;

        public EntryData(string name, int progress, string goal, string details, DateTime date)
        {
            Name = name;
            Goal = goal;
            Details = details;
            ProgressDatas = new List<ProgressData>();
            Progress = progress;
            Date = date;
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using EntryLogic;
using Newtonsoft.Json;
using UnityEngine;

namespace SaveSystem
{
    public class DataSaver
    {
        private readonly string _savePath = Path.Combine(Application.persistentDataPath, "Entries");

        public void SaveData(List<EntryData> datas)
        {
            try
            {
                DataWrapper wrapper = new DataWrapper(datas);
                string jsonData = JsonConvert.SerializeObject(wrapper, Formatting.Indented);

                File.WriteAllText(_savePath, jsonData);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error saving item data: {e.Message}");
                throw;
            }
        }

        public List<EntryData> LoadData()
        {
            try
            {
                if (!File.Exists(_savePath))
                {
                    return null;
                }

                string jsonData = File.ReadAllText(_savePath);

                DataWrapper wrapper = JsonConvert.DeserializeObject<DataWrapper>(jsonData);

                return wrapper?.EntryDatas ?? new List<EntryData>();
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading item data: {e.Message}");
                return new List<EntryData>();
            }
        }
    }

    [Serializable]
    public class DataWrapper
    {
        public List<EntryData> EntryDatas;

        public DataWrapper(List<EntryData> entryDatas)
        {
            EntryDatas = entryDatas;
        }
    }
}
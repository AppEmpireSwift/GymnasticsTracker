using System.IO;
using UnityEngine;

namespace SaveSystem
{
    public class DataSaver
    {
        private readonly string _savePath = Path.Combine(Application.persistentDataPath, "Items");

        /*public void SaveData(List<ItemData> datas)
        {
            try
            {
                ItemDataWrapper wrapper = new ItemDataWrapper(datas);
                string jsonData = JsonConvert.SerializeObject(wrapper, Formatting.Indented);

                File.WriteAllText(_savePath, jsonData);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error saving item data: {e.Message}");
                throw;
            }
        }

        public List<ItemData> LoadData()
        {
            try
            {
                if (!File.Exists(_savePath))
                {
                    return null;
                }

                string jsonData = File.ReadAllText(_savePath);

                ItemDataWrapper wrapper = JsonConvert.DeserializeObject<ItemDataWrapper>(jsonData);

                return wrapper?.ItemDatas ?? new List<ItemData>();
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading item data: {e.Message}");
                return new List<ItemData>();
            }
        }
    }

    [Serializable]
    public class ItemDataWrapper
    {
        public List<ItemData> ItemDatas;

        public ItemDataWrapper(List<ItemData> itemDatas)
        {
            ItemDatas = itemDatas;
        }
    }*/
    }
}
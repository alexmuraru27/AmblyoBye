using System;
using System.Xml.Serialization;
using Palmmedia.ReportGenerator.Core.Common;
using UnityEngine;
using UnityEngine.Rendering;
using System.IO;
public class SettingsHandler
{
    public static void StoreSettings<T>(string filename, T objectData) where T : struct
    {
        XmlSerializer serializer = new XmlSerializer(typeof(T));
        using (StringWriter writer = new StringWriter())
        {
            serializer.Serialize(writer, objectData);
            StorageHandler.WriteFile(TypeSafeDir.Settings, filename, writer.ToString());
        }
    }

    public static bool RestoreSettings<T>(string filename, ref T objectData) where T : struct
    {
        bool isSuccess = false;
        Tuple<bool, string> fileData = StorageHandler.ReadFile(TypeSafeDir.Settings, filename);
        if (fileData.Item1)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                using (StringReader reader = new StringReader(fileData.Item2))
                {
                    objectData = (T)serializer.Deserialize(reader);
                    isSuccess = true;
                }
            }
            catch (InvalidOperationException ex)
            {
                Debug.Log($"Deserialization error: {ex.Message}");
            }
        }
        return isSuccess;
    }
}

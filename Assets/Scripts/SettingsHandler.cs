using System;
using UnityEngine;
using UnityEngine.Rendering;

public class SettingsHandler
{
    public static bool StoreSettings<T>(string filename, T objectData) where T : struct
    {
        StorageHandler.EnsureDirectory(TypeSafeDir.Settings);
        bool isSuccess = true;
        // TODO
        return isSuccess;
    }

    public static bool RestoreSettings<T>(string filename, ref T objectData) where T : struct
    {
        StorageHandler.EnsureDirectory(TypeSafeDir.Settings);
        bool isSuccess = true;
        // TODO
        return isSuccess;
    }
    // Pass the filename
    // Use StorageHandler to create files
    // Read/Write file content using StorageHandler
    // Read/Write settings structure based (structure type is always passed as template argument) -> structure is serialized
    // Shall ensure that the data in the file is compatible -> if not shall override the old data with new/return something invalid
}

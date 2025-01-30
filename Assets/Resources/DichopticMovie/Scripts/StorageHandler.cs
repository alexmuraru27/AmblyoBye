using System;
using UnityEngine;
using System.IO;
public class StorageHandler
{
    public static void InitFS(string dirName)
    {
        Debug.Log(Application.persistentDataPath);

        string dirPath = AndroidPersistancePathToMovieDir(dirName);
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }

    }

    public static string AndroidPersistancePathToMovieDir(string dirName)
    {
        return Application.persistentDataPath + "/../" + dirName;
    }


    // TODO create directories if not existent
    // TODO search for all movies available
}

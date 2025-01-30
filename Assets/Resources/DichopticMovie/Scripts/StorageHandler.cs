using System;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
public class StorageHandler
{

    public static void InitFS(TypeSafeDir dirName)
    {
        Debug.Log(Application.persistentDataPath);

        string dirPath = AndroidPersistancePathToMovieDir(dirName);
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }

    }

    public static List<string> GetFilePathsFromDir(TypeSafeDir dirName)
    {
        List<string> filePathList = new List<string>();
        string dirPath = AndroidPersistancePathToMovieDir(dirName);
        if (Directory.Exists(dirPath))
        {
            string[] allfiles = Directory.GetFiles("path/to/dir", "*.*", SearchOption.AllDirectories);
            filePathList.AddRange(allfiles);
        }
        return filePathList;
    }
    public static string AndroidPersistancePathToMovieDir(TypeSafeDir dirName)
    {
        return Application.persistentDataPath + "/../" + dirName;
    }


    // TODO create directories if not existent
    // TODO search for all movies available
}

// Typesafe directories
public class TypeSafeDir
{
    private TypeSafeDir(string value) { Value = value; }

    public string Value { get; private set; }

    public static TypeSafeDir Movies { get { return new TypeSafeDir("Movies"); } }

    public override string ToString()
    {
        return Value;
    }
}
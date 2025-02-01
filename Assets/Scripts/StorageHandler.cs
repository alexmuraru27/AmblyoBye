using System;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
public class StorageHandler
{
    public static void InitAllFS()
    {
        foreach (TypeSafeDir typeSafeDir in TypeSafeDir.getAllDirs())
        {
            string dirPath = AndroidPersistancePathToDir(typeSafeDir);
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
        }
    }
    public static List<string> GetFilePathsFromDir(TypeSafeDir dirName)
    {
        List<string> filePathList = new List<string>();
        string dirPath = AndroidPersistancePathToDir(dirName);
        if (Directory.Exists(dirPath))
        {
            string[] allfiles = Directory.GetFiles(dirPath, "*.*", SearchOption.AllDirectories);
            filePathList.AddRange(allfiles);
        }
        return filePathList;
    }

    public static List<string> GetFileNamesFromDir(TypeSafeDir dirName)
    {
        List<string> filePathsList = GetFilePathsFromDir(dirName);
        List<string> fileNamesList = new List<string>();
        foreach (string filePath in filePathsList)
        {
            fileNamesList.Add(Path.GetFileName(filePath));
        }

        return fileNamesList;
    }

    public static string AndroidPersistancePathToDir(TypeSafeDir dirName)
    {
        return Application.persistentDataPath + "/../" + dirName;
    }


    // returns tuple <bool, string> -> isFile, content
    public static Tuple<bool, string> ReadFile(TypeSafeDir dirName, string filename)
    {
        InitAllFS();
        bool isFile = false;
        string fileContent = "";
        string filePath = Path.Join(AndroidPersistancePathToDir(dirName), filename);

        if (File.Exists(filePath))
        {
            try
            {
                fileContent = File.ReadAllText(filePath);
                isFile = true;
            }
            catch (Exception ex)
            {
                Debug.Log($"An error occurred while reading the file: {ex.Message}");
            }
        }
        return new Tuple<bool, string>(isFile, fileContent);
    }

    public static void WriteFile(TypeSafeDir dirName, string filename, string content)
    {
        InitAllFS();
        string filePath = Path.Join(AndroidPersistancePathToDir(dirName), filename);
        try
        {
            File.WriteAllText(filePath, content);
        }
        catch (Exception ex)
        {
            Debug.Log($"An error occurred while writing the file: {ex.Message}");
        }
    }
}

// Typesafe directories
public class TypeSafeDir
{
    private TypeSafeDir(string value) { Value = value; }

    public string Value { get; private set; }

    public static TypeSafeDir Movies { get { return new TypeSafeDir("Movies"); } }

    public static TypeSafeDir Settings { get { return new TypeSafeDir("Settings"); } }

    public static List<TypeSafeDir> getAllDirs()
    {
        return new List<TypeSafeDir>() { Movies, Settings };
    }
    public override string ToString()
    {
        return Value;
    }
}
using System;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;
public class StorageHandler
{

    public static void EnsureDirectory(TypeSafeDir dirName)
    {
        string dirPath = AndroidPersistancePathToDir(dirName);
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
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
        EnsureDirectory(dirName);
        bool isFile = false;
        string fileContent = "";
        string filePath = Path.Join(dirName.Value, filename);

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
        else
        {
            Debug.Log($"The file {filePath} does not exist.");
        }
        return new Tuple<bool, string>(isFile, fileContent);
    }

    public static void WriteFile(TypeSafeDir dirName, string filename, string content)
    {
        EnsureDirectory(dirName);
        string filePath = Path.Join(dirName.Value, filename);
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

    public override string ToString()
    {
        return Value;
    }
}
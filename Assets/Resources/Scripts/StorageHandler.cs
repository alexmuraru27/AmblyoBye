using System;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
public class StorageHandler
{
    public static void InitDirectoryTree()
    {
        foreach (TypeSafeDir typeSafeDir in TypeSafeDir.getAllDirs())
        {
            string dirAbsPath = DirNameToAndroidPersistancePath(typeSafeDir);
            if (!Directory.Exists(dirAbsPath))
            {
                Directory.CreateDirectory(dirAbsPath);
            }
        }
    }
    public static List<string> GetFilePathsFromDir(TypeSafeDir dirName)
    {
        List<string> filePathList = new List<string>();
        string dirAbsPath = DirNameToAndroidPersistancePath(dirName);
        if (Directory.Exists(dirAbsPath))
        {
            string[] allfiles = Directory.GetFiles(dirAbsPath, "*.*", SearchOption.AllDirectories);
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

    public static string DirNameToAndroidPersistancePath(TypeSafeDir dirName)
    {
        return Application.persistentDataPath + "/../" + dirName;
    }


    // returns tuple <bool, string> -> isFile, content
    public static Tuple<bool, string> ReadFile(TypeSafeDir dirName, string filename)
    {
        InitDirectoryTree();
        bool isFile = false;
        string fileContent = "";
        string filePath = Path.Join(DirNameToAndroidPersistancePath(dirName), filename);

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
        InitDirectoryTree();
        string filePath = Path.Join(DirNameToAndroidPersistancePath(dirName), filename);
        try
        {
            File.WriteAllText(filePath, content);
        }
        catch (Exception ex)
        {
            Debug.Log($"An error occurred while writing the file: {ex.Message}");
        }
    }

    public static bool DeleteFileFromDir(TypeSafeDir dirName, string filename)
    {
        List<string> filePathsList = GetFilePathsFromDir(dirName);
        foreach (string filepath in filePathsList)
        {
            if (File.Exists(filepath) && filepath.Contains(filename))
            {
                try
                {
                    File.Delete(filepath);
                }
                catch (Exception ex)
                {
                    Debug.Log($"An error occurred while deleting the file: {ex.Message}");
                }
                return true;
            }
        }
        return false;
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
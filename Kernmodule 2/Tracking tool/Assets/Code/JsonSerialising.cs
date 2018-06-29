using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.IO;

public class JsonSerialising {

    [Serializable]
    private class Wrapper<T> ////Een wrapper class om Unity's JsonUtility met arrays/lists te laten werken
    {
        public T[] Items;
    }

    public static void SerialiseList<Type>(string fileLocation, string fileName, List<Type> list) ////Gebruikt een wrapper om vervolgens met JsonUtility te serialisen naar een bestand.
    {
        Wrapper<Type> wrapper = new Wrapper<Type>();
        wrapper.Items = list.ToArray();
        
        string json = JsonUtility.ToJson(wrapper);
        
        WriteTextFile(fileLocation, fileName, json);
    }

    public static List<Vector2Int> DeserialiseVector2IntList(string fileLocation, string filename) ///Deserialised van een .txt file met een Json string naar een List<Vector2Int> 
    {
        filename += ".txt";
        string json = "";

        json = File.ReadAllText(fileLocation + filename);

        List<Vector2Int> deserialisedList = ArrayFromJson<Vector2Int>(json).ToList();
        return deserialisedList;
    }

    private static T[] ArrayFromJson<T>(string json) ////Helper-functie om de wrapped array te deserialisen
    {
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
        return wrapper.Items;
    }

    private static void WriteTextFile(string fileLocation, string filename, string text) ////Schrijft een json string naar een .txt bestand.
    {
        filename += ".txt";
        FileStream fs = null;
        
        try
        {
            fs = new FileStream(fileLocation + filename, FileMode.Create);
            using (StreamWriter writer = new StreamWriter(fs))
            {
                writer.Write(text);
            }
        }
        finally
        {
            if (fs != null)
                fs.Dispose();
        }

    }

}

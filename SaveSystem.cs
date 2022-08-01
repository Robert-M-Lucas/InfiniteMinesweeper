using System.IO;
using System.Collections.Generic;
using System;
using System.Collections;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;

[Serializable]
public class JsonSave 
{
    public int X = 0;
    public int Y = 0;
    public bool HighscoreEligible = true;
    public float BombRate = 0.125f;
    public float Timer = 0;
    public int Score = 0;
}

public static class SaveSystem{
    public static void Save(GameManagerScript gameManager, Dictionary<Tuple<int, int>, Chunk> _board, JsonSave prefs) 
    {
        BinaryFormatter bf = new BinaryFormatter();
        
        FileStream file = File.Create(gameManager.DataPath + "/save" + gameManager.SaveSlot.ToString() + ".sav");
        Debug.Log($"Save: {gameManager.DataPath + "/save" + gameManager.SaveSlot.ToString() + ".sav"}");
        
        bf.Serialize(file, _board);
        file.Close();

        if (prefs.BombRate != 0.125f) { prefs.HighscoreEligible = false; }
        gameManager.SavePrefs.HighscoreEligible = prefs.HighscoreEligible;

        string jsonString = JsonUtility.ToJson(prefs);
        File.WriteAllText(gameManager.DataPath + "/save" + gameManager.SaveSlot.ToString() + ".json", jsonString);

        gameManager.hide_save = true;
        Debug.Log("Save end");
    }

    public static void TryLoad(GameManagerScript gameManager)
    {
        Debug.Log("Try load");
        if (File.Exists(gameManager.DataPath + "/save" + gameManager.SaveSlot.ToString() + ".sav"))
        {
            FileStream fs = new FileStream(gameManager.DataPath + "/save" + gameManager.SaveSlot.ToString() + ".sav", FileMode.Open);
            BinaryFormatter formatter = new BinaryFormatter();
            gameManager.boardRenderer.Board = (Dictionary<Tuple<int, int>, Chunk>)formatter.Deserialize(fs);
            fs.Close();

            /*if (PlayerPrefs.HasKey("x"))
            {
                cam.transform.position = new Vector3(PlayerPrefs.GetInt("x"), PlayerPrefs.GetInt("y"), cam.transform.position.z);
            }*/
            gameManager.SavePrefs = JsonUtility.FromJson<JsonSave>(System.IO.File.ReadAllText(gameManager.DataPath + "/save" + gameManager.SaveSlot.ToString() + ".json"));

            gameManager.newGame = false;
        }
        else
        {
            Debug.Log($"File {gameManager.DataPath + "/save" + gameManager.SaveSlot.ToString() + ".sav"} doesn't exist");
        }

        if (PlayerPrefs.GetFloat("brate") != -1) { gameManager.SavePrefs.BombRate = PlayerPrefs.GetFloat("brate"); }

        if (gameManager.SavePrefs.BombRate != 0.125f) { gameManager.SavePrefs.HighscoreEligible = false; }

        gameManager.menuController.UpdateInfo();
    }
}
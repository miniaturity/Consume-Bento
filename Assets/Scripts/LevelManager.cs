using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public GameManager gm;
    public LevelRenderer lr;

    private List<string> data = new List<string>();
    private int currentLevel = 0;

    public string resourcesFolder = "Levels";

    private void Awake() {
        gm = GetComponent<GameManager>();
        lr = GetComponent<LevelRenderer>();
        LoadLevels();
    }

    private void LoadLevels() {
        data.Clear();

        TextAsset[] assets = Resources.LoadAll<TextAsset>(resourcesFolder);
        System.Array.Sort(assets, (a, b) => a.name.CompareTo(b));
        
        foreach (TextAsset asset in assets) {
            data.Add(asset.text);
        }
    }

    public void LoadLevel(int index) {
        if (index < 0 || index >= data.Count) {
            Debug.LogError("Invalid LEVEL index");
            return;
        }
        currentLevel = index;
        string json = data[index];

        try {
            GameManager.LevelData lvlData = JsonUtility.FromJson<GameManager.LevelData>(json);

            gm.hasWon = false;
            gm.LoadLevel(lvlData);
            lr.RenderLevel();
            lr.SpawnTargetPieces();
        } catch (System.Exception e) {
            Debug.LogError("Error parsing JSON: " + e);
        }
    }

    public void LoadNextLevel() {
        if (currentLevel + 1 < data.Count) {
            LoadLevel(currentLevel + 1);
        } else {
            Debug.Log("No next level: Levels are complete!");
        }
    }
}

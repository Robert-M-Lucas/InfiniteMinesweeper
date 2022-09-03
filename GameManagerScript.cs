using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine.Advertisements;
using UnityEngine.Analytics;
using Debug = UnityEngine.Debug;
using System.Threading;
using System.Diagnostics;


public class GameManagerScript : MonoBehaviour
{
    #region Inits
    public bool newGame = true;

    private Dictionary<Tuple<int, int>, Chunk> board_copy = new Dictionary<Tuple<int, int>, Chunk>();

    public Vector2Int MinVisible;
    public Vector2Int MaxVisible;

    public Camera cam;

    public Sprite ClosedPrefab;
    public Sprite OpenPrefab;
    public Sprite FlagPrefab;
    public Sprite QPrefab;
    public Sprite Bomb;
    public Sprite[] NumPrefabs;
    public GameObject OnClickPrefab;

    public int cursor = 0;

    public GameMenuController menuController;

    public CameraShake shake;

    public float AdCount = 0;

    public bool Premium;

    public float AutosaveTime = 15;

    public string DataPath;

    public int SaveSlot = -1;

    private float _Timer = 0;

    public bool paused = false;

    public JsonSave SavePrefs = new JsonSave();
    private JsonSave prefs_copy;

    public bool hide_save = false;

    public GameObject SaveIcon;

    public PoolManager poolManager;

    public BoardRenderer boardRenderer;
    #endregion
    // Start is called before the first frame update
    private void Awake()
    {
        DataPath = Application.persistentDataPath;
    }

    void Start()
    {
        boardRenderer = new BoardRenderer(this);
        SaveSlot = PlayerPrefs.GetInt("SaveSlot");
        SaveSystem.TryLoad(this);

        Premium = PlayerPrefs.GetInt("Premium") == 1;
        Advertisement.Initialize("4444347");

        if (PlayerPrefs.HasKey("Autosave"))
        {
            AutosaveTime = PlayerPrefs.GetInt("Autosave");
            if (AutosaveTime == 0) { AutosaveTime = -1; }
            else if (AutosaveTime < 5) { AutosaveTime = 5; }
        }
        else
        {
            AutosaveTime = 15;
        }
       
    } 

    public void OnTap (Vector2 tap_pos) 
    {
        if (paused) { return; }

        Tuple<Vector2Int, Vector2Int> poss = boardRenderer.ScreenToCellPos(tap_pos);

        BoardOpener.Open(this, poss.Item1, poss.Item2);
    }

    public void OnTapDown(Vector2 tap_pos) {
        if (cursor != 0) { return; }

        Tuple<Vector2Int, Vector2Int> poss = boardRenderer.ScreenToCellPos(tap_pos);
        if (BoardOpener.GetCell(this, poss.Item1, poss.Item2) == CellValues.CLOSED || BoardOpener.GetCell(this, poss.Item1, poss.Item2) == CellValues.BOMB_CLOSED)
        {
            OnClickPrefab.transform.position = new Vector3(((poss.Item1.x * boardRenderer.ChunkSize) - (boardRenderer.ChunkSize / 2)) + poss.Item2.x,
            ((poss.Item1.y * boardRenderer.ChunkSize) - (boardRenderer.ChunkSize / 2)) + (boardRenderer.ChunkSize - poss.Item2.y),
            -0.01f);
            OnClickPrefab.SetActive(true);
        }
    }

    public void OnTapUp() {
        OnClickPrefab.SetActive(false);
    }


    // Update is called once per frame
    void Update()
    {
        if (!paused) { SavePrefs.Timer += Time.deltaTime; }
        
        //Vector3Int TopLeft = Vector3Int.FloorToInt((cam.ScreenToWorldPoint(new Vector3(0, Screen.currentResolution.height)) + new Vector3(50, 0)) / chunkSize);
        //Vector3Int BottomRight = Vector3Int.FloorToInt((cam.ScreenToWorldPoint(new Vector3(Screen.currentResolution.width, 0)) + new Vector3(0, 50)) / chunkSize);

        boardRenderer.Render();

        if (AutosaveTime != -1)
        {
            _Timer += Time.deltaTime;
            if (_Timer > AutosaveTime)
            {
                SaveIcon.SetActive(true);
                _Timer = 0;
                board_copy = ObjectCopier.Clone(boardRenderer.Board);
                prefs_copy = ObjectCopier.Clone(SavePrefs);
                Thread save_thread = new Thread(() => SaveSystem.Save(this, board_copy, prefs_copy));
                save_thread.Start();
                Debug.Log("Save start");
            }
        }

        if (hide_save) { hide_save = false; SaveIcon.SetActive(false); }
    }

    /*public void SavePrefs()
    {
        PlayerPrefs.SetInt("x", Vector2Int.FloorToInt(cam.transform.position).x);
        PlayerPrefs.SetInt("y", Vector2Int.FloorToInt(cam.transform.position).y);
        PlayerPrefs.SetInt("score", score);
        PlayerPrefs.SetFloat("timer", timer);
    }*/
}

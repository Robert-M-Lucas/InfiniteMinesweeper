using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Advertisements;
using Unity.VectorGraphics;

public class GameMenuController : MonoBehaviour
{
    #region inits
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject inGameMenu;
    public bool paused = false;

    [SerializeField] private Text score;
    [SerializeField] private Text scoreBG;
    [SerializeField] private Text highscoreText;
    [SerializeField] private Text highscoreTextBG;

    [SerializeField] private GameManagerScript gameManager;

    [SerializeField] private GameObject[] selections;

    [SerializeField] private GameObject FadeImage;
    [SerializeField] private GameObject fade2;

    [SerializeField] private Text timer;
    [SerializeField] private Text timer_bg;

    [SerializeField] private Text info;

    public int Highscore;

    [SerializeField] private GameObject darkenImage;

    [SerializeField] private Image saveButtonImage;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        if (!PlayerPrefs.HasKey("highscore")) { PlayerPrefs.SetInt("highscore", 0); }
        Highscore = PlayerPrefs.GetInt("highscore");
        Advertisement.Initialize("4444347");
        FadeImage.SetActive(true);
        StartCoroutine(HideFade());
    }

    public void UpdateInfo()
    {
        string info = "";
        info += "Time: " + ((int) gameManager.SavePrefs.Timer).ToString();
        info += "\nScore: " + gameManager.SavePrefs.Score.ToString();
        info += "\nHighscore: " + PlayerPrefs.GetInt("highscore").ToString();
        info += "\nBomb Rate: " + gameManager.SavePrefs.BombRate.ToString();
        info += "\nAchievements: " + gameManager.SavePrefs.HighscoreEligible.ToString();
        info += "\nAutosave Interval: ";

        if (!gameManager.Premium) { info += "Off - No Premium"; }
        else if (gameManager.AutosaveTime == -1) { info += "Off"; }
        else { info += gameManager.AutosaveTime.ToString(); }
        this.info.text = info;
    }

    public void Save() {
        gameManager.SavePrefs.X = Vector2Int.FloorToInt(gameManager.cam.transform.position).x;
        gameManager.SavePrefs.Y = Vector2Int.FloorToInt(gameManager.cam.transform.position).y;
        SaveSystem.Save(gameManager, gameManager.boardRenderer.Board, gameManager.SavePrefs);
        saveButtonImage.color = new Color(0.25f, 1f, 0);
    }

    public void UpdateScore(int new_score)
    {
        //5:000000
        string score_string = new_score.ToString();
        while (score_string.Length < 6)
        {
            score_string = "0" + score_string;
        }
        score_string = "5:" + score_string;
        string score_string_bg = "8:888888";
        while (score_string_bg.Length < score_string.Length)
        {
            score_string_bg += "8";
        }
        score.text = score_string;
        scoreBG.text = score_string_bg;
        if (new_score > Highscore && gameManager.Premium && gameManager.SavePrefs.HighscoreEligible)
        {
            Highscore = new_score;
            PlayerPrefs.SetInt("highscore", new_score);
        }
        //X5:000000
        string high_string = Highscore.ToString();
        while (high_string.Length < 6)
        {
            high_string = "0" + high_string;
        }
        high_string = "X5:" + high_string;
        string hscore_string_bg = "88:888888";
        while (hscore_string_bg.Length < high_string.Length)
        {
            hscore_string_bg += "8";
        }
        highscoreText.text = high_string;
        highscoreTextBG.text = hscore_string_bg;
    }

    public void Cursor(int c) { 
        gameManager.cursor = c; 

        switch (c)
        {
            case 0:
                selections[0].SetActive(true);
                selections[1].SetActive(false);
                selections[2].SetActive(false);
                break;
            case 1:
                selections[0].SetActive(false);
                selections[1].SetActive(true);
                selections[2].SetActive(false);
                break;
            case 2:
                selections[0].SetActive(false);
                selections[1].SetActive(false);
                selections[2].SetActive(true);
                break;
        }
    }

    public IEnumerator HideFade()
    {
        yield return new WaitForSeconds(1);
        FadeImage.SetActive(false);
    }

    public void Escape(bool pause)
    {
        paused = pause;
        if (pause) { UpdateInfo(); pauseMenu.SetActive(true); inGameMenu.SetActive(false); gameManager.paused = paused; }
        else { pauseMenu.SetActive(false); inGameMenu.SetActive(true); StartCoroutine(WaitUnpause()); }
        darkenImage.SetActive(pause);
        saveButtonImage.color = new Color(1, 1, 1);

    }

    public IEnumerator WaitUnpause()
    {
        yield return new WaitForSeconds(0.1f);
        gameManager.paused = false;
    }

    public void MainMenu()
    {
        fade2.SetActive(true);
        StartCoroutine(SwitchLevel());
    }

    public IEnumerator SwitchLevel()
    {
        yield return new WaitForSeconds(1);
        SceneManager.LoadScene(0);
    }

    // Update is called once per frame
    void Update()
    {
        string timer_text = ((int)gameManager.SavePrefs.Timer).ToString();
        while (timer_text.Length < 3)
        {
            timer_text = "0" + timer_text;
        }
        string bg_text = "888";
        while (bg_text.Length < timer_text.Length)
        {
            bg_text += "8";
        }

        timer.text = timer_text;
        timer_bg.text = bg_text;
    }
}

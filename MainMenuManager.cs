using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEngine.UI;
using UnityEngine.Purchasing;
using System;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject deleteButton;
    [SerializeField] private GameObject confirm_button;
    [SerializeField] private GameObject play_button;
    [SerializeField] private GameObject parent;

    [SerializeField] private InputField bomb_rate;

    [SerializeField] private InputField autosave_interval;

    [SerializeField] private bool transition = false;

    [SerializeField] private Camera cam;

    [SerializeField] private float accel;
    private float vel;

    private float zvel;
    [SerializeField] private float zoom_speed = 1f;

    private int TransitionPhase = 0;

    [SerializeField] private GameObject premium_shine;
    [SerializeField] private GameObject settings_shine;
    [SerializeField] private GameObject premium_base;
    [SerializeField] private GameObject settings_base;
    [SerializeField] private GameObject help_base;
    [SerializeField] private GameObject BG;
    [SerializeField] private bool premium_text = false;
    [SerializeField] private bool settings = false;
    [SerializeField] private bool help = false;
    [SerializeField] private Text PremiumBuyText;
    [SerializeField] private GameObject FadeImage;
    [SerializeField] private bool Premium = false;
    [SerializeField] private int SaveSlot = 1;
    [SerializeField] private Sprite[] NumSprites;
    [SerializeField] private Image SaveIndicator;
    [SerializeField] private Text ApplyText;
    [SerializeField] private Text VersionText;
    [SerializeField] private Purchaser purchaser;

    // Start is called before the first frame update
    void Start()
    {
        PlayerPrefs.SetInt("Premium", 1);
        // Debug.Log(Application.persistentDataPath);
        VersionText.text = Application.version;
        PlayerPrefs.SetFloat("brate", -1);
        FadeImage.SetActive(true);
        StartCoroutine(HideFade());

        Premium = PlayerPrefs.GetInt("Premium") == 1;

        UpdatePremiumText();
    }

    public void UpdatePremiumText()
    {
        Premium = PlayerPrefs.GetInt("Premium") == 1;
        if (Premium) { PremiumBuyText.text = "Owned"; }
    }

    public void BuyPremium()
    {
        try
        {
            Premium = PlayerPrefs.GetInt("Premium") == 1;
            if (!Premium) { purchaser.BuyNonConsumable(); }
            UpdatePremiumText();
        }
        catch (Exception e)
        {
            VersionText.text = e.ToString();
        }
    }

    public void PremiumClick()
    {
        help_base.SetActive(false);

        if (premium_text)
        {
            premium_base.SetActive(false);
            premium_shine.SetActive(false);
            BG.SetActive(false);
        }
        else
        {
            premium_base.SetActive(true);
            BG.SetActive(true);
            premium_shine.SetActive(true);
            settings_shine.SetActive(false);
            settings = false;
            settings_base.SetActive(false);
        }
        premium_text = !premium_text;
    }

    public void SettingsClick()
    {
        if (!Premium) { PremiumClick(); return; }

        help_base.SetActive(false);

        if (settings)
        {
            settings_base.SetActive(false);
            settings_shine.SetActive(false);
            BG.SetActive(false);
        }
        else
        {
            BG.SetActive(true);
            settings_base.SetActive(true);
            settings_shine.SetActive(true);
            premium_shine.SetActive(false);
            premium_text = false;
            premium_base.SetActive(false);
        }
        settings = !settings;
    }

    public void HelpClick(bool show)
    {
        BG.SetActive(show);
        help_base.SetActive(show);
        help = show;
    }

    public void SaveNumClick()
    {
        if (!Premium) { PremiumClick(); return; }
        print(SaveSlot);
        if (SaveSlot == 8) { SaveSlot = 1; }
        else { SaveSlot += 1; }
        SaveIndicator.sprite = NumSprites[SaveSlot-1];
    }

    public IEnumerator HideFade()
    {
        yield return new WaitForSeconds(1);
        FadeImage.SetActive(false);
    }

    public void Play()
    {
        if (TransitionPhase != 0) { return; }
        transition = true;
        TransitionPhase = 1;
        if (Premium)
        {
            PlayerPrefs.SetInt("SaveSlot", SaveSlot);
        }
        else
        {
            PlayerPrefs.SetInt("SaveSlot", 1);
        }
        StartCoroutine(PlayWait(3));
        
    }
 
    IEnumerator PlayWait(int secs)
    {
        yield return new WaitForSeconds(secs);
        
        SceneManager.LoadScene(1);
    }

    public void confirm()
    {
        if (TransitionPhase != 0) { return; }
        deleteButton.SetActive(false);
        confirm_button.SetActive(true);
    }

    public void DeleteSave()
    {
        if (TransitionPhase != 0) { return; }
        confirm_button.SetActive(false);
        print("Delete");
        try
        {
            File.Delete(Application.persistentDataPath + "/save" + SaveSlot.ToString() + ".sav");
        }
        catch (FileNotFoundException)
        {
            Debug.Log("No Save File");
        }
        try
        {
            File.Delete(Application.persistentDataPath + "/save" + SaveSlot.ToString() + ".json");
        }
        catch (FileNotFoundException)
        {
            Debug.Log("No Properties Save File");
        }

        PlayerPrefs.SetInt("x", 0);
        PlayerPrefs.SetInt("y", 0);
        PlayerPrefs.SetInt("score", 0);
        PlayerPrefs.SetFloat("timer", 0);
    }

    public void ApplySettings()
    {
        PlayerPrefs.SetFloat("brate", float.Parse(bomb_rate.text));
        PlayerPrefs.SetInt("Autosave", (int) float.Parse(autosave_interval.text));

        StartCoroutine(ApplyShow(3));
    }

    IEnumerator ApplyShow(int secs)
    {
        ApplyText.text = "Applied!";
        yield return new WaitForSeconds(secs);
        ApplyText.text = "Apply";
        
    }

    // Update is called once per frame
    void Update()
    {
        if (transition)
        {
            vel += accel * Time.deltaTime;
            parent.transform.position += new Vector3(0, vel, 0) * Time.deltaTime;

            zvel += zoom_speed * Time.deltaTime;
            cam.orthographicSize += zvel * Time.deltaTime;
        }
    }
}

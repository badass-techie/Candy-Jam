using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary> Handles UI clicks </summary>
public class OnButtonClick : MonoBehaviour {
    public GameObject helpPanel, levelPanel;
    public GameObject muteIcon, unmuteIcon;
    public Text muteText;

    // Start is called before the first frame update
    void Start() {
        if (SceneManager.GetActiveScene().name == "Welcome") {
            if (Convert.ToBoolean(PlayerPrefs.GetInt("IsMuted", 0)))
            {
                muteIcon.SetActive(false);
                unmuteIcon.SetActive(true);
                muteText.text = "UNMUTE";
            }
            else
            {
                muteIcon.SetActive(true);
                unmuteIcon.SetActive(false);
                muteText.text = "MUTE";
            }
        }
    }

    public void LoadScene(String sceneName) {
        PlayClick();
        SceneManager.LoadScene(sceneName);
    }
    
    public void ReloadScene() {
        PlayClick();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void ShowPanel(GameObject panel) {
        PlayClick();
        panel.SetActive(true);
    }
    
    public void ClosePanel(GameObject panel) {
        PlayClick();
        panel.SetActive(false);
    }

    // IEnumerator ShowInterstitialAd(float delay) {
    //     yield return new WaitForSecondsRealtime(delay);
    //     if (Time.timeScale == 0) {  //if the game is still paused after the delay
    //         //Toast.Show(FindObjectOfType<InterstitialAdManager>().failReason);
    //         FindObjectOfType<InterstitialAdManager>().ShowAd();
    //     }
    // }

    // public void QuitLevel() {
    //     PlayClick();
    //     quitCanvas.SetActive(true);
    // }

    public void MuteOrUnmute() {
        PlayClick();
        if (Convert.ToBoolean(PlayerPrefs.GetInt("IsMuted", 0)))
        {
            muteIcon.SetActive(true);
            unmuteIcon.SetActive(false);
            muteText.text = "MUTE";
            PlayerPrefs.SetInt("IsMuted", 0);
        }
        else {
            muteIcon.SetActive(false);
            unmuteIcon.SetActive(true);
            muteText.text = "UNMUTE";
            PlayerPrefs.SetInt("IsMuted", 1);
        }
    }
    
    public void QuitGame() {
        PlayClick();
        Application.Quit();
    }

    void PlayClick() {
        if (FindObjectOfType<AudioManager>() != null && Convert.ToBoolean(PlayerPrefs.GetInt("IsMuted", 0)) == false)
            FindObjectOfType<AudioManager>().Play("Click");
    }
}

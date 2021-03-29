using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class LoadGame : MonoBehaviour
{
    private VideoPlayer videoPlayer;
    // Start is called before the first frame update
    void Start()
    {
        videoPlayer = gameObject.GetComponent<VideoPlayer>();
    }

    // Update is called once per frame
    void Update()
    {
        if(videoPlayer.frame > 0 && videoPlayer.isPlaying == false)
        {
            SceneManager.LoadScene("Welcome");
        }
    }
}

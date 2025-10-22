using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject IntroPanel;
    [SerializeField] private GameObject FirstPanel;
    [SerializeField] private GameObject StartButton;
    //[SerializeField] private GameObject IntroPanel;
    //[SerializeField] private GameObject IntroPanel;
    //[SerializeField] private GameObject IntroPanel;
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        IntroPanel.SetActive(false);
        FirstPanel.SetActive(false);
        StartButton.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void ExitGame()
    {
        Application.Quit();
    }
}

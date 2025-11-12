using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject IntroPanel;
    [SerializeField] private GameObject FirstPanel;
    [SerializeField] private GameObject StartButton;
    [SerializeField] private GameObject Helmet;
    [SerializeField] private GameObject RepairsPanel;
    [SerializeField] private GameObject InventoryPanel;
    [SerializeField] private GameObject FunctionsPanel;
    [SerializeField] private GameObject SecondVisor;
    [SerializeField] private GameObject HelmetInPieces;
    [SerializeField] private GameObject InventoryPiecesPanel;
    [SerializeField] private GameObject InfoPanel;
    [SerializeField] private GameObject ThirdVisor;
    [SerializeField] private GameObject FilterPanel;
    [SerializeField] private GameObject BigFilter;
    [SerializeField] private GameObject ComputerPanel;
    [SerializeField] private GameObject BigComputer;
    [SerializeField] private GameObject FilterPiecesPanel;
    [SerializeField] private GameObject ComputerPiecesPanel;
    
    
    
    

    //[SerializeField] private Animator InfoPanelAnimator;
    
    
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        IntroPanel.SetActive(false);
        FirstPanel.SetActive(false);
        StartButton.SetActive(true);
        Helmet.SetActive(false);
        RepairsPanel.SetActive(false);
        InventoryPanel.SetActive(false);
        FunctionsPanel.SetActive(false);
        SecondVisor.SetActive(false);
        HelmetInPieces.SetActive(false);
        InventoryPiecesPanel.SetActive(false);
        InfoPanel.SetActive(false);
        ThirdVisor.SetActive(false);
        FilterPanel.SetActive(false);
        BigFilter.SetActive(false);
        ComputerPanel.SetActive(false);
        BigComputer.SetActive(false);
        
        //InfoPanelAnimator.SetTrigger("ShowInfoPanel");
        
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

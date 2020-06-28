using UnityEngine;
using UnityEngine.UI;

public class CardBuilder : MonoBehaviour
{

    [SerializeField]
    private WishCard writableCard = null;


    [SerializeField]
    private TMPro.TMP_InputField inputField = null;

    [SerializeField]
    private Image colorImage = null;
    [SerializeField]
    private Slider redSlider = null;
    [SerializeField]
    private Slider greenSlider = null;
    [SerializeField]
    private Slider blueSlider = null;




    [SerializeField]
    private GameObject createCardButton = null;

    [SerializeField]
    private GameObject cancelCardButton = null;

    private void Start()
    {
        createCardButton.SetActive(true);
        cancelCardButton.SetActive(false);

        OnColoChanged(1.0f);

        redSlider.onValueChanged.AddListener(OnColoChanged);
        greenSlider.onValueChanged.AddListener(OnColoChanged);
        blueSlider.onValueChanged.AddListener(OnColoChanged);
    }

    public void OnStartCardMaker()
    {
        createCardButton.SetActive(false);
        cancelCardButton.SetActive(true);
    }

    public void OnExitCardMaker()
    {
        createCardButton.SetActive(true);
        cancelCardButton.SetActive(false);
    }


    public void OnColoChanged(float val)
    {
        colorImage.color = new Color(redSlider.value, greenSlider.value, blueSlider.value, 1.0f);
        writableCard.Mat.SetColor("_MainColor", new Color(redSlider.value, greenSlider.value, blueSlider.value, 1.0f));
    }



}

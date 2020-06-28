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

        OnColorChanged(1.0f);

        redSlider.onValueChanged.AddListener(OnColorChanged);
        greenSlider.onValueChanged.AddListener(OnColorChanged);
        blueSlider.onValueChanged.AddListener(OnColorChanged);
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


    public void OnColorChanged(float val)
    {
        colorImage.color = new Color(redSlider.value, greenSlider.value, blueSlider.value, 1.0f);
        writableCard.Mat.SetColor("_MainColor", new Color(redSlider.value, greenSlider.value, blueSlider.value, 1.0f));
    }

    public void OnSendButtonPressed()
    {
        //play animation, get rid of card, clean out data.

        if (inputField.text == "")
        {
            Debug.Log("Can't send empty wish");
            return;
        }

        WishManager.instance.CreateAndSaveWishToFile(inputField.text, new Color(redSlider.value, greenSlider.value, blueSlider.value, 1.0f));
    }


}

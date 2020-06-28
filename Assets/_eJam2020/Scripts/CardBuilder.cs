using UnityEngine;
using UnityEngine.UI;

public class CardBuilder : MonoBehaviour
{
    public static CardBuilder instance = null;


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
    private Button sendButton = null;

    [Space(10)]
    [SerializeField]
    private Animator anim = null;

    [SerializeField]
    private GameObject hand = null;

    [SerializeField]
    private CameraManager camMan = null;

    [Space(10)]
    [SerializeField]
    private GameObject createCardButton = null;

    [SerializeField]
    private GameObject cancelCardButton = null;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogError($"Multiple Instance of Cardbuilder singleton. Destroying");
            Destroy(this);
        }
    }

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

        inputField.interactable = false;
        sendButton.interactable = false;


        if (inputField.text == "")
        {
            Debug.Log("Can't send empty wish");
            return;
        }

        anim.SetTrigger("PickUpCard");
        anim.gameObject.transform.position = Vector3.zero;
        anim.gameObject.transform.rotation = Quaternion.identity;

        WishManager.instance.CreateAndSaveWishToFile(inputField.text, new Color(redSlider.value, greenSlider.value, blueSlider.value, 1.0f));
    }


    //Pick up events
    public void OnPickupEvent()
    {
        writableCard.transform.parent = hand.transform;
    }
    public void OnStepAwayEvent()
    {
        camMan.OnExitCardMaker();
        OnExitCardMaker();
    }

    public void OnDropEvent()
    {
        WishManager.instance.CreatePlayersWishObject(new WishData() { userID = SystemInfo.deviceUniqueIdentifier, userText = inputField.text, colorVal = new WishData.ColorVal(redSlider.value, greenSlider.value, blueSlider.value, 1.0f) });
        Destroy(writableCard.gameObject);
    }



}

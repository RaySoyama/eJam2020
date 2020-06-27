using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class WishManager : MonoBehaviour
{
    public static WishManager instance = null;

    [SerializeField]
    private TMPro.TMP_InputField inputField = null;

    [SerializeField]
    private List<WishData> allWishData = new List<WishData>();

    [SerializeField]
    private List<GameObject> allWishPositions = new List<GameObject>();

    [SerializeField]
    private Dictionary<WishData, WishCard> allWishCards = new Dictionary<WishData, WishCard>();

    [SerializeField]
    private GameObject cardPrefab = null;


    [Header("Debug")]
    [SerializeField]
    private bool useLocalData = false;

    [SerializeField]
    private string fileName = "SaveData.txt";

    [SerializeField]
    private string debugContentText = "";

    [SerializeField]
    private Color debugCardColor = new Color();

    [SerializeField]
    private Mesh cardMeshGizmo = null;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogError($"Multiple Instance of WishManager singleton. Destroying");
            Destroy(this);
        }
    }

    void Start()
    {
        if (inputField == null)
        {
            Debug.LogError($"Missing reference to inputField");
        }

        foreach (GameObject GO in allWishPositions)
        {
            GO.SetActive(false);
        }

        allWishCards = new Dictionary<WishData, WishCard>();

        if (useLocalData == true)
        {
            GetLocalData();
        }
        else
        {

        }

    }

    void Update()
    {

    }


    [ContextMenu("RenderAllWishes")]
    private void RenderAllWishes()
    {
        foreach (WishData wish in allWishData)
        {
            CreateNewWish(wish);
        }
    }


    [ContextMenu("CreateAndSaveWishToFile")]
    private void CreateAndSaveWishToFile()
    {
        allWishData.Add(new WishData() { userText = debugContentText, colorVal = new WishData.ColorVal(debugCardColor) });

        string path = Directory.GetCurrentDirectory() + "\\" + fileName;

        if (File.Exists(path) == false)
        {
            var myFile = File.Create(path);
            myFile.Close();
            Debug.Log($"Save file created at path {path}");
        }

        File.WriteAllText(path, JsonConvert.SerializeObject(allWishData, Formatting.Indented));
    }

    private void CreateNewWish(WishData wish)
    {
        if (cardPrefab == null)
        {
            Debug.LogError("Card Prefab reference missing");
            return;
        }

        List<GameObject> availableCards = new List<GameObject>();

        foreach (GameObject GO in allWishPositions)
        {
            if (GO.activeSelf == false)
            {
                availableCards.Add(GO);
            }
        }

        if (availableCards.Count == 0)
        {
            Debug.LogError("Not enough empty card positions");
        }
        else
        {
            int rand = Random.Range(0, availableCards.Count);

            allWishCards.Add(wish, Instantiate(cardPrefab, availableCards[rand].transform).GetComponent<WishCard>());
            allWishCards[wish].Text.text = wish.userText;
            allWishCards[wish].Mat.SetColor("_MainColor", wish.color);

            availableCards[rand].SetActive(true);
        }
    }

    private void GetLocalData()
    {
        //if no file exist, create
        string path = Directory.GetCurrentDirectory() + "\\" + fileName;
        if (File.Exists(path) == false)
        {
            var myFile = File.Create(path);
            myFile.Close();
            Debug.Log($"Save file created at path {path}");
        }

        allWishData = JsonConvert.DeserializeObject<List<WishData>>(File.ReadAllText(path));

        if (allWishData == null)
        {

            allWishData = new List<WishData>();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        foreach (GameObject wishTransform in allWishPositions)
        {
            if (wishTransform != null)
            {
                //Gizmos.DrawSphere(wishTransform.transform.position, 0.1f);
                if (cardMeshGizmo == null)
                {
                    Debug.LogError("Missing reference to Card Mesh for Gizmo");
                }
                else
                {
                    Gizmos.DrawMesh(cardMeshGizmo, wishTransform.transform.position);
                }
            }
        }
    }

}

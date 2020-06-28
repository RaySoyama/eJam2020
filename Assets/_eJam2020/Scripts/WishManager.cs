using Firebase.Storage;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class WishManager : MonoBehaviour
{
    public static WishManager instance = null;

    [SerializeField]
    private List<WishData> allWishData = new List<WishData>();

    [SerializeField]
    private List<GameObject> allWishPositions = new List<GameObject>();

    [SerializeField]
    private Dictionary<WishData, WishCard> allWishCards = new Dictionary<WishData, WishCard>();

    [SerializeField]
    private GameObject cardPrefab = null;

    [Header("Networking")]
    [SerializeField]
    private string networkURL = "gs://tanabata-8e5be.appspot.com/";


    [Header("Debug")]
    [SerializeField]
    private bool useLocalData = false;

    [SerializeField]
    private string folderName = "Data";

    [SerializeField]
    private string fileName = "Data.JSON";

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
            GetOwnCloudData();
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
            CreateNewWishObject(wish);
        }
    }
    public void CreateAndSaveWishToFile(string cardContent, Color cardColor)
    {
        allWishData.Add(new WishData() { userText = cardContent, colorVal = new WishData.ColorVal(cardColor) });

        string path = Directory.GetCurrentDirectory() + "\\" + folderName + "\\" + fileName;

        ValidateDirectory();
        ValidateFile();

        File.WriteAllText(path, JsonConvert.SerializeObject(allWishData, Formatting.Indented));

        if (useLocalData == false)
        {
            UploadSaveToFirebase();
        }
    }
    private void CreateNewWishObject(WishData wish)
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
            int rand = UnityEngine.Random.Range(0, availableCards.Count);

            allWishCards.Add(wish, Instantiate(cardPrefab, availableCards[rand].transform).GetComponent<WishCard>());
            allWishCards[wish].Text.text = wish.userText;
            allWishCards[wish].Mat.SetColor("_MainColor", wish.color);

            availableCards[rand].SetActive(true);
        }
    }
    private void UploadSaveToFirebase()
    {
        string path = Directory.GetCurrentDirectory() + "\\" + folderName + "\\" + fileName;
        FirebaseStorage storage = FirebaseStorage.DefaultInstance;
        StorageReference storage_ref = storage.GetReferenceFromUrl(networkURL);
        StorageReference fileRef = storage_ref.Child($"TanabataData/{SystemInfo.deviceUniqueIdentifier}/{fileName}");
        fileRef.PutFileAsync(path).ContinueWith((Task<StorageMetadata> task) =>
          {
              if (task.IsFaulted || task.IsCanceled)
              {
                  Debug.Log(task.Exception.ToString());
              }
              else
              {
                  //Task<Uri> dloadTask = fileRef.GetDownloadUrlAsync();
                  Debug.Log("Finished uploading...");
                  //Debug.Log("download url = " + dloadTask.Result.ToString());
              }
          });
    }
    private void GetLocalData()
    {
        //if no file exist, create
        string path = Directory.GetCurrentDirectory() + "\\" + folderName + "\\" + fileName;

        ValidateDirectory();
        ValidateFile();

        allWishData = JsonConvert.DeserializeObject<List<WishData>>(File.ReadAllText(path));

        if (allWishData == null)
        {
            allWishData = new List<WishData>();
        }
    }
    private void GetOwnCloudData()
    {

        FirebaseStorage storage = FirebaseStorage.DefaultInstance;

        StorageReference storage_ref = storage.GetReferenceFromUrl(networkURL);

        string path = Directory.GetCurrentDirectory() + "\\" + folderName + "\\" + fileName;

        ValidateDirectory();
        ValidateFile();

        StorageReference fileRef = storage_ref.Child($"TanabataData/{SystemInfo.deviceUniqueIdentifier}/{fileName}");

        // Download to the local filesystem
        fileRef.GetFileAsync(path).ContinueWith(task =>
        {
            if (!task.IsFaulted && !task.IsCanceled)
            {
                Debug.Log("File downloaded.");
            }
            else
            {
                Debug.Log(task.Exception.ToString());
            }
        });

    }
    private void ValidateDirectory()
    {
        string path = Directory.GetCurrentDirectory() + "\\" + folderName;

        if (Directory.Exists(path) == false)
        {
            Directory.CreateDirectory(path);
            Debug.Log($"No Save Directory found, Creating in {path}");
        }
    }
    private void ValidateFile()
    {
        string path = Directory.GetCurrentDirectory() + "\\" + folderName + "\\" + fileName;

        if (File.Exists(path) == false)
        {
            var myFile = File.Create(path);
            myFile.Close();
            Debug.Log($"Save file created at path {path}");
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
                    Gizmos.DrawMesh(cardMeshGizmo, wishTransform.transform.position, wishTransform.transform.rotation, wishTransform.transform.lossyScale);
                }
            }
        }
    }

}

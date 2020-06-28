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
    private List<WishData> userWishData = new List<WishData>();

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

    [Header("Networked")]

    [SerializeField]
    [ReadOnlyField]
    private int expectedStrangerDataCount = 0;

    [SerializeField]
    [ReadOnlyField]
    private List<string> itinerary = new List<string>();

    [SerializeField]
    [ReadOnlyField]
    private bool userDataDownloaded = false;

    [SerializeField]
    [ReadOnlyField]
    private bool strangerDataDownloaded = false;

    [SerializeField]
    [ReadOnlyField]
    private bool itineraryDataDownloaded = false;


    [SerializeField]
    [ReadOnlyField]
    private bool isTryingToDownloadStrangerData = false;

    [SerializeField]
    [ReadOnlyField]
    private bool isTryingToSetItineraryData = false;



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
            RenderAllWishes();
        }
        else
        {
            GetOwnCloudData();
            StartGetStrangerData();
        }
    }

    void Update()
    {
        if (userDataDownloaded == true)
        {
            GetLocalData();
            RenderAllWishes();
            userDataDownloaded = false;
        }

        if (strangerDataDownloaded == true)
        {
            RenderAllWishes();
            strangerDataDownloaded = false;
        }

        if (itineraryDataDownloaded == true)
        {
            if (isTryingToDownloadStrangerData == true)
            {
                ProcessStrangerData(2);
                isTryingToDownloadStrangerData = false;
            }

            if (isTryingToSetItineraryData == true)
            {
                SetCloudItinerary();
                isTryingToSetItineraryData = false;
            }

            itineraryDataDownloaded = false;
        }


    }

    private void CleanWishes()
    {
        foreach (var card in allWishCards)
        {
            Destroy(card.Value.gameObject);
        }
        allWishCards.Clear();
    }
    private void GetLocalData()
    {
        //if no file exist, create
        string path = Directory.GetCurrentDirectory() + "\\" + folderName + "\\" + fileName;

        ValidateDirectory();
        ValidateFile(fileName);

        userWishData = JsonConvert.DeserializeObject<List<WishData>>(File.ReadAllText(path));

        if (userWishData == null)
        {
            userWishData = new List<WishData>();
        }
    }

    #region Networking
    private void SetSaveToFirebase()
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
                Debug.Log("Finished uploading...");
            }
        });

        StartCloudItinerary();
    }

    private void GetOwnCloudData()
    {
        FirebaseStorage storage = FirebaseStorage.DefaultInstance;

        StorageReference storage_ref = storage.GetReferenceFromUrl(networkURL);

        string path = Directory.GetCurrentDirectory() + "\\" + folderName + "\\" + fileName;

        ValidateDirectory();
        ValidateFile(fileName);

        StorageReference fileRef = storage_ref.Child($"TanabataData/{SystemInfo.deviceUniqueIdentifier}/{fileName}");

        // Download to the local file system
        fileRef.GetFileAsync(path).ContinueWith(task =>
                 {
                     if (!task.IsFaulted && !task.IsCanceled)
                     {
                         Debug.Log("User data File downloaded.");
                     }
                     else
                     {
                         Debug.Log("No user data found in cloud: " + task.Exception.ToString());
                     }

                     userDataDownloaded = true;
                 });
    }

    private void StartGetStrangerData()
    {
        GetCloudItinerary();

        isTryingToDownloadStrangerData = true;
    }
    private void ProcessStrangerData(int count)
    {
        List<string> usedIDs = new List<string>();

        itinerary.Remove(SystemInfo.deviceUniqueIdentifier);

        if (itinerary.Count < count)
        {
            Debug.Log("Not enough user wishes to meet count");

            usedIDs = itinerary;
        }
        else
        {
            while (usedIDs.Count < count)
            {
                int rnd = UnityEngine.Random.Range(0, itinerary.Count);

                if (usedIDs.Contains(itinerary[rnd]) == false)
                {
                    usedIDs.Add(itinerary[rnd]);
                }
            }
        }

        expectedStrangerDataCount = usedIDs.Count;

        foreach (string id in usedIDs)
        {
            GetDataFromCloud(id);
        }
    }


    private void GetDataFromCloud(string uID)
    {
        FirebaseStorage storage = FirebaseStorage.DefaultInstance;

        StorageReference storage_ref = storage.GetReferenceFromUrl(networkURL);

        string path = Directory.GetCurrentDirectory() + "\\" + folderName + "\\" + "TempData.txt";

        ValidateDirectory();
        ValidateFile("TempData.txt");

        StorageReference fileRef = storage_ref.Child($"TanabataData/{uID}/{fileName}");

        // Download to the local file system
        fileRef.GetFileAsync(path).ContinueWith(task =>
        {
            if (!task.IsFaulted && !task.IsCanceled)
            {
                Debug.Log("Stranger File downloaded.");
            }
            else
            {
                Debug.Log(task.Exception.ToString());
            }

            List<WishData> data = JsonConvert.DeserializeObject<List<WishData>>(File.ReadAllText(path));

            if (data == null)
            {

            }
            else if (data.Count > 1)
            {
                //WishData fuck = data[UnityEngine.Random.Range(0, data.Count)];
                WishData fuck = data[0];
                allWishData.Add(fuck);
            }
            else
            {
                allWishData.Add(data[0]);
            }

            expectedStrangerDataCount--;

            if (expectedStrangerDataCount == 0)
            {
                strangerDataDownloaded = true;
            }

        });
    }
    private void GetCloudItinerary()
    {
        FirebaseStorage storage = FirebaseStorage.DefaultInstance;

        StorageReference storage_ref = storage.GetReferenceFromUrl(networkURL);

        string path = Directory.GetCurrentDirectory() + "\\" + folderName + "\\" + "Itinerary.txt";

        ValidateDirectory();
        ValidateFile("Itinerary.txt");

        StorageReference fileRef = storage_ref.Child($"TanabataData/Itinerary.txt");

        // Download to the local file system
        fileRef.GetFileAsync(path).ContinueWith(task =>
        {
            if (!task.IsFaulted && !task.IsCanceled)
            {
                Debug.Log("Itinerary downloaded.");
            }
            else
            {
                Debug.Log("Itinerary not found: " + task.Exception.ToString());
            }

            List<string> data = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(path));

            if (data == null)
            {
                itinerary = new List<string>();
            }
            else
            {
                itinerary = data;
            }

            itineraryDataDownloaded = true;
        });
    }

    private void StartCloudItinerary()
    {
        GetCloudItinerary();
        isTryingToSetItineraryData = true;
    }

    private void SetCloudItinerary()
    {
        if (itinerary.Contains(SystemInfo.deviceUniqueIdentifier) == false)
        {
            //write to file
            string path = Directory.GetCurrentDirectory() + "\\" + folderName + "\\" + "Itinerary.txt";

            ValidateDirectory();
            ValidateFile("Itinerary.txt");

            itinerary.Add(SystemInfo.deviceUniqueIdentifier);

            File.WriteAllText(path, JsonConvert.SerializeObject(itinerary, Formatting.Indented));

            //upload to cloud
            FirebaseStorage storage = FirebaseStorage.DefaultInstance;
            StorageReference storage_ref = storage.GetReferenceFromUrl(networkURL);
            StorageReference fileRef = storage_ref.Child($"TanabataData/Itinerary.txt");

            fileRef.PutFileAsync(path).ContinueWith((Task<StorageMetadata> task) =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.Log(task.Exception.ToString());
                }
                else
                {
                    Debug.Log("Itinerary finished uploading...");
                }
            });
        }
    }
    #endregion

    public void CreateAndSaveWishToFile(string cardContent, Color cardColor)
    {
        userWishData.Add(new WishData() { userText = cardContent, userID = SystemInfo.deviceUniqueIdentifier, colorVal = new WishData.ColorVal(cardColor) });

        string path = Directory.GetCurrentDirectory() + "\\" + folderName + "\\" + fileName;

        ValidateDirectory();
        ValidateFile(fileName);

        File.WriteAllText(path, JsonConvert.SerializeObject(userWishData, Formatting.Indented));

        if (useLocalData == false)
        {
            SetSaveToFirebase();
        }
    }

    private void RenderAllWishes()
    {
        Debug.Log("Rendering");
        CleanWishes();

        if (userWishData.Count > 0)
        {
            CreateNewWishObject(userWishData[UnityEngine.Random.Range(0, userWishData.Count)]);
        }

        foreach (WishData wish in allWishData)
        {
            CreateNewWishObject(wish);
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


    private void ValidateDirectory()
    {
        string path = Directory.GetCurrentDirectory() + "\\" + folderName;

        if (Directory.Exists(path) == false)
        {
            Directory.CreateDirectory(path);
            Debug.Log($"No Save Directory found, Creating in {path}");
        }
    }
    private void ValidateFile(string _fileName)
    {
        string path = Directory.GetCurrentDirectory() + "\\" + folderName + "\\" + _fileName;

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

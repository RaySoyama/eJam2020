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
            GetLocalData();

            GetUserGeneratedWishes(2);
        }
        RenderAllWishes();
    }

    void Update()
    {

    }

    [ContextMenu("RenderAllWishes")]
    private void RenderAllWishes()
    {
        CleanWishes();

        if (userWishData.Count > 0)
        {
            CreateNewWishObject(userWishData[Random.Range(0, userWishData.Count)]);
        }

        foreach (WishData wish in allWishData)
        {
            CreateNewWishObject(wish);
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

    public void CreateAndSaveWishToFile(string cardContent, Color cardColor)
    {
        userWishData.Add(new WishData() { userText = cardContent, colorVal = new WishData.ColorVal(cardColor) });

        string path = Directory.GetCurrentDirectory() + "\\" + folderName + "\\" + fileName;

        ValidateDirectory();
        ValidateFile(fileName);

        File.WriteAllText(path, JsonConvert.SerializeObject(userWishData, Formatting.Indented));

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
            int rand = Random.Range(0, availableCards.Count);

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
                  Debug.Log("Finished uploading...");
              }
          }).ConfigureAwait(true);

        SetCloudItinerary();
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
        }).ConfigureAwait(true);
    }


    private void GetUserGeneratedWishes(int count)
    {
        List<string> userID = GetCloudItinerary();

        List<string> usedIDs = new List<string>();

        userID.Remove(SystemInfo.deviceUniqueIdentifier);

        if (usedIDs.Count < count)
        {
            Debug.Log("Not enough user wishes to meet count");

            foreach (string id in userID)
            {
                allWishData.Add(GetDataFromCloud(id));
            }
        }
        else
        {
            while (usedIDs.Count < count)
            {
                int rnd = Random.Range(0, userID.Count);

                if (usedIDs.Contains(userID[rnd]) == false)
                {
                    usedIDs.Add(userID[rnd]);
                }
            }
        }
    }

    private List<string> GetCloudItinerary()
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
        }).ConfigureAwait(true);

        List<string> data = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(path));

        if (data == null)
        {
            return new List<string>();
        }

        return data;
    }

    private void SetCloudItinerary()
    {
        List<string> itinerary = GetCloudItinerary();

        if (itinerary == null)
        {
            //write to file
            string path = Directory.GetCurrentDirectory() + "\\" + folderName + "\\" + "Itinerary.txt";

            ValidateDirectory();
            ValidateFile("Itinerary.txt");

            itinerary = new List<string>();
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
            }).ConfigureAwait(true);

            return;
        }

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
            }).ConfigureAwait(true);
        }
    }
    private WishData GetDataFromCloud(string uID)
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
                Debug.Log("File downloaded.");
            }
            else
            {
                Debug.Log(task.Exception.ToString());
            }
        }).ConfigureAwait(true);

        List<WishData> data = JsonConvert.DeserializeObject<List<WishData>>(File.ReadAllText(path));

        if (data == null)
        {
            return null;
        }
        else if (data.Count > 1)
        {
            return data[Random.Range(0, data.Count)];
        }

        return data[0];
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

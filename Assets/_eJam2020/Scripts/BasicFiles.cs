using Firebase.Storage;
using System;
using System.IO;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class BasicFiles : MonoBehaviour
{
    // Start is called before the first frame update

    public TMP_InputField fileName;
    public TMP_InputField sampleText;
    void Start()
    {

    }
    public void SaveLocalFile()
    {
        string fName = fileName.text;
        if (fName == "")
            fName = "Test.txt";
        File.WriteAllText(fName, sampleText.text);
        Debug.Log($"File has been written out to {Directory.GetCurrentDirectory()}\\{fName}");
    }

    public void UploadFile()
    {
        // Get a reference to the storage service, using the default Firebase App
        FirebaseStorage storage = FirebaseStorage.DefaultInstance;
        // FirebaseStorage storage = FirebaseStorage.GetInstance("gs://basicfileupload.appspot.com") ;



        // Create a storage reference from our storage service
        StorageReference storage_ref =
          storage.GetReferenceFromUrl("gs://tanabata-8e5be.appspot.com/");

        // Create a child reference
        // images_ref now points to "images"
        StorageReference testFileRef = storage_ref.Child($"Testing/{fileName.text}");

        string download_url;
        // Upload the file to the path "images/rivers.jpg"
        testFileRef.PutFileAsync(fileName.text)

          .ContinueWith((Task<StorageMetadata> task) =>
          {
              if (task.IsFaulted || task.IsCanceled)
              {
                  Debug.Log(task.Exception.ToString());
              }
              else
              {
                  Task<Uri> dloadTask = testFileRef.GetDownloadUrlAsync();
                  download_url = dloadTask.Result.ToString();
                  Debug.Log("Finished uploading...");
                  Debug.Log("download url = " + download_url);
              }
          });
    }




}
/*
 service firebase.storage {
  match /b/{bucket}/o {
    match /{allPaths=**} {
      allow read, write: if request.auth != null;
    }
  }
}

 */

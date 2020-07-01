using System.Collections.Generic;
using UnityEngine;

public class PopulateTrees : MonoBehaviour
{
    public List<GameObject> shit;

    public int numSpawn = 10;

    public GameObject ass;



    void Start()
    {
        for (int i = 0; i < numSpawn; i++)
        {
            List<GameObject> availableCards = new List<GameObject>();

            foreach (GameObject GO in shit)
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

                var poop = Instantiate(ass, availableCards[rand].transform).GetComponent<WishCard>();
                poop.Text.text = "";
                poop.Mat.SetColor("_MainColor", new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f)));

                availableCards[rand].SetActive(true);
            }

        }
    }
}

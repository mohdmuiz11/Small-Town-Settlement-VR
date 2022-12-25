using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class GameUI : MonoBehaviour
{
    [Header("Craft UI")]
    [SerializeField] private Canvas craftUICanvas;
    [SerializeField] private RectTransform craftCanvasRow;
    [SerializeField] private float craftRowStartingYPos;
    [SerializeField] private float craftRowDistance;
    [SerializeField] private Transform locationBuilding;
    private Dictionary<RectTransform, Building> craftRowList = new Dictionary<RectTransform, Building>();

    [Header("Resource UI")]

    // Other vars
    private SlotManager slotManager;
    private GameManager gameManager;

    // Start is called before the first frame update
    void Start()
    {
        // Object referencing
        slotManager = GameObject.Find("GRID System").GetComponent<SlotManager>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        
        InitialSetup();
    }

    private void InitialSetup()
    {
        float posY = craftRowStartingYPos; // initial posY

        for (int i = 0; i < slotManager.buildingList.Length; i++)
        {
            // Instantiate row and position them according to rowStartingPos
            var rowInstance = Instantiate(craftCanvasRow, craftUICanvas.transform);
            rowInstance.localPosition = Vector3.up * posY;

            // Put names and their thumbnail
            rowInstance.GetComponentInChildren<TextMeshProUGUI>().text = slotManager.buildingList[i].BuildingName();
            rowInstance.GetComponentInChildren<Image>().sprite = slotManager.buildingList[i].buildingThumbnail;

            craftRowList.Add(rowInstance, slotManager.buildingList[i]); // add to the row
            posY -= craftRowDistance;
        }

        UpdateOnce();
    }

    public void UpdateOnce()
    {
        // Craft UI
        foreach (KeyValuePair<RectTransform, Building> row in craftRowList)
        {
            RectTransform rowInstance = row.Key;
            Building building = row.Value;

            Debug.Log(building.BuildingName());

            bool eligible = CheckBuild(building);
            Button button = rowInstance.GetComponentInChildren<Button>();

            button.interactable = eligible;
            if (eligible)
                button.onClick.AddListener(() => CraftBuilding(building));
        }
    }



    /// <summary>
    /// Check if the building can be build with enough resources
    /// </summary>
    /// <returns>True or false</returns>
    public bool CheckBuild(Building building)
    {
        // Initial setup
        ResourceType[] resourceRequired = building.resourceRequired;
        int[] resourceAmount = building.resourceAmount;
        bool check = false;

        // If number length are not same, give an error
        if (resourceRequired.Length == resourceAmount.Length)
        {
            for (int i = 0; i < resourceRequired.Length; i++)
            {
                // if current resource are not sufficient, break the loop, check will still false
                Debug.Log(resourceAmount[i]);
                if (resourceAmount[i] > gameManager.CheckResources(resourceRequired[i]))
                    break;
                check = true;
            }
        }
        else
            Debug.LogError("Resource type and amount arrays are not in same length for" + building.buildingType);
        return check;
    }

    // BUG: the building scale is too small, for now manually set the position and tranform parent
    private void CraftBuilding(Building building)
    {
        var spawned = Instantiate(building);
        spawned.transform.position = locationBuilding.position;
        spawned.transform.SetParent(locationBuilding);
    }
}

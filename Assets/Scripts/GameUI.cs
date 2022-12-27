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
    private Dictionary<RectTransform, Building> craftRowList = new();

    [Header("Resource UI")]
    [SerializeField] private Sprite[] resourceThumnails; //has to arrange according to enum, for now, or make it an object bruh
    [SerializeField] private RectTransform resourceUIContent;
    [SerializeField] private RectTransform resourceCanvasRow;
    [SerializeField] private float resourceRowStartingYPos;
    [SerializeField] private float resourceRowDistance;
    private Dictionary<RectTransform, ResourceType> resourceRowList = new();

    // Other vars
    private SlotManager slotManager;
    private GameManager gameManager;

    // Start is called before the first frame update
    void Start()
    {
        // Object referencing
        slotManager = GameObject.Find("GRID System").GetComponent<SlotManager>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        
        InitialCraftUISetup();
        UpdateOnce();
    }

    private void InitialCraftUISetup()
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
    }

    private void InitialResourceUISetup()
    {
        float posY = resourceRowStartingYPos;
        ResourceType[] resourceTypes = (ResourceType[])System.Enum.GetValues(typeof(ResourceType));


        for (int i = 0; i < System.Enum.GetNames(typeof(ResourceType)).Length; i++)
        {
            // Instantiate row inside scrollview content and position them according resource starting pos.
            var rowInstance = Instantiate(resourceCanvasRow, resourceUIContent.transform);
            rowInstance.localPosition = Vector3.up * posY;

            // Put resource amount and thumnail

        }
    }

    public void UpdateOnce()
    {
        // Craft UI
        foreach (KeyValuePair<RectTransform, Building> row in craftRowList)
        {
            RectTransform rowInstance = row.Key;
            Building building = row.Value;

            //Debug.Log(building.BuildingName());

            bool eligible = gameManager.CheckBuild(building);
            Button button = rowInstance.GetComponentInChildren<Button>();

            button.interactable = eligible;
            if (eligible)
                button.onClick.AddListener(() => gameManager.CraftBuilding(building));
        }
    }
}

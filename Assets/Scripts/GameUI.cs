using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

public class GameUI : MonoBehaviour
{
    [Header("Craft UI")]
    [SerializeField] private RectTransform craftUIContent;
    [SerializeField] private RectTransform craftCanvasRow;
    private Dictionary<RectTransform, Building> craftRowList = new();

    [Header("Resource UI")]
    [SerializeField] private RectTransform resourceUIContent;
    [SerializeField] private RectTransform resourceCanvasRow;
    [SerializeField] private float resourceRowStartingYPos;
    [SerializeField] private float resourceRowDistance;
    private List<ResourceUI> resourceRowList = new();

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
        InitialResourceUISetup();
        UpdateOnce();
    }

    // Initialize Craft UI
    private void InitialCraftUISetup()
    {

        for (int i = 0; i < slotManager.buildingList.Length; i++)
        {
            // Instantiate row and position them according to rowStartingPos
            var rowInstance = Instantiate(craftCanvasRow, craftUIContent.transform);

            // Put names and their thumbnail
            rowInstance.GetComponentInChildren<TextMeshProUGUI>().text = EnumToReadableFormat(slotManager.buildingList[i].buildingType);
            rowInstance.GetComponentInChildren<Image>().sprite = slotManager.buildingList[i].buildingThumbnail;

            craftRowList.Add(rowInstance, slotManager.buildingList[i]); // add to the row
        }
    }

    // Initialize Resource UI
    private void InitialResourceUISetup()
    {
        float posY = resourceRowStartingYPos - 11.57f;
        int pos = 0;

        foreach (KeyValuePair<ResourceType, int> resource in gameManager.currentResources)
        {
            ResourceType resourceType = resource.Key;
            int amount = resource.Value;

            // Instantiate row inside scrollview content and position them according resource starting pos.
            var rowInstance = Instantiate(resourceCanvasRow, resourceUIContent.transform);
            rowInstance.localPosition = new Vector3(46.1f, posY);

            // Load sprite according to resourceType name from Assets/Resources
            string spriteName = resourceType.ToString();
            Sprite sprite = Resources.Load<Sprite>($"UI/{spriteName}"); //this gives me a headache for 3 hours straight to figure this out

            // Store everything inside ResourceUI
            ResourceUI resourceRow = new(resourceType, amount, sprite, rowInstance, pos++);
            resourceRowList.Add(resourceRow);

            posY -= resourceRowDistance;
        }
    }

    /// <summary>
    /// Return the name of the any enum types to user readable format
    /// </summary>
    /// <returns>String format</returns>
    private string EnumToReadableFormat(System.Enum @enum)
    {
        // thanks chandragupta
        string str = @enum.ToString(); // convert to string
        str = str.Replace("_", " "); // replace underscore with whitespace

        return str;
    }

    /// <summary>
    /// Update everything related to Game UI
    /// </summary>
    public void UpdateOnce()
    {
        craftUIUpdate();
        resourceUIUpdate();
    }

    // update craft ui
    private void craftUIUpdate()
    {
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

    // update resource UI, the most inefficient code imaginable
    private void resourceUIUpdate()
    {
        // Declare Vector3 position and row position
        Dictionary<Vector3, int> tempPos = new();

        // Store temporary local position from row and pos
        foreach (var originalItem in resourceRowList)
        {
            originalItem.amount = gameManager.CheckResources(originalItem.resourceType);
            tempPos.Add(originalItem.row.localPosition, originalItem.pos);
        }

        // Sort by decending order
        resourceRowList = resourceRowList.OrderByDescending(x => x.amount).ToList();
        int pos = 0;

        // Rearrange and insert stuff
        foreach (var rowInstance in resourceRowList)
        {
            foreach (KeyValuePair<Vector3, int> item in tempPos)
            {
                if (item.Value == pos)
                {
                    rowInstance.row.localPosition = item.Key;
                    rowInstance.pos = pos;
                }
            }

            // Insert resource thumbnail
            rowInstance.row.GetComponentInChildren<Image>().sprite = rowInstance.thumbnail;

            // Load texts into row
            TextMeshProUGUI[] texts = rowInstance.row.GetComponentsInChildren<TextMeshProUGUI>();
            texts[0].text = EnumToReadableFormat(rowInstance.resourceType);
            texts[1].text = rowInstance.amount.ToString();
            pos++;
        }
    }
}

/// <summary>
/// Use for gameUI
/// </summary>
public class ResourceUI
{
    public ResourceType resourceType;
    public int amount;
    public Sprite thumbnail;
    public RectTransform row;
    public int pos;

    public ResourceUI()
    {
        resourceType = ResourceType.Wood;
        amount = 0;
        thumbnail = null;
        row = null;
        pos = 0;
    }

    public ResourceUI(ResourceType resourceType, int amount, Sprite thumbnail, RectTransform row, int pos)
    {
        this.resourceType = resourceType;
        this.amount = amount;
        this.thumbnail = thumbnail;
        this.row = row;
        this.pos = pos;
    }
}
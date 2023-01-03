using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

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

    [Header("Status UI")]
    [SerializeField] private StatusUI statusUIPrefab;
    [SerializeField] private RectTransform statusContent;

    [Header("Information UI")]
    [SerializeField] private GameObject canvasInfoUI;
    [SerializeField] private TextMeshProUGUI titleInfo;
    [SerializeField] private TextMeshProUGUI descriptionInfo;
    [SerializeField] private Image thumnailInfo;
    [SerializeField] private Button buttonInfo;

    [Header("Building stats")]
    [SerializeField] private RectTransform buildingStats;
    [SerializeField] private float statsPosY;

    [Header("Multi tool settings")]
    [SerializeField] private Canvas mtCanvas;
    [SerializeField] private TextMeshProUGUI mtTitle;
    [SerializeField] private Button[] mtButtons;
    [SerializeField] private string[] buttonTexts;
    [SerializeField] private Button[] directionButtons;
    [SerializeField] private GameObject[] mapObj;
    [SerializeField] private float turnDuration;

    // Other vars
    private SlotManager slotManager;
    private GameManager gameManager;
    private float currentRot = 0;

    // Start is called before the first frame update
    void Start()
    {
        // Object referencing
        slotManager = GameObject.Find("GRID System").GetComponent<SlotManager>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        
        InitialCraftUISetup();
        InitialResourceUISetup();
        InitialMtUI();
        ShowInfo(false);
        UpdateNextAction();
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

    private void InitialStatusUISetup()
    {
        for (int i = 0; i < gameManager.listStatus.Length; i++)
        {
            Debug.Log("bruh");
        }
    }

    private void InitialMtUI()
    {
        if (mtButtons.Length == buttonTexts.Length)
        {
            for (int i = 0; i < mtButtons.Length; i++)
            {
                mtButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = buttonTexts[i];
            }

            // Make build & next day button available to use
            mtButtons[0].interactable = true;
            mtButtons[2].interactable = true;

            // initiate right/left buttons 
            directionButtons[0].onClick.AddListener(() => MapTurn(1));
            directionButtons[1].onClick.AddListener(() => MapTurn(-1));
        }
        else
        {
            Debug.LogError("Buttons and text are not in equal lengths!");
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
    public void UpdateNextAction()
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
            {
                buttonInfo.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => ShowInfo(building));
            }
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

    private void MtUIUpdate()
    {
        // Check if the town hall is already built
        if (slotManager.CheckAvailableBuilding(BuildingType.Town_Hall))
            mtButtons[1].interactable = true;
    }

    /// <summary>
    /// Instantiate stats building
    /// </summary>
    /// <param name="building"></param>
    /// <param name="timer"></param>
    public void ShowStats(Building building, int timer)
    {
        var instance = Instantiate(buildingStats, building.transform);
        instance.localPosition = Vector3.up * statsPosY;
        instance.GetComponentInChildren<TextMeshProUGUI>().text = timer.ToString();
    }

    /// <summary>
    /// Update stats what
    /// </summary>
    /// <param name="building"></param>
    /// <param name="timer"></param>
    public void UpdateStats(Building building, int timer)
    {
        building.GetComponentInChildren<RectTransform>().GetComponentInChildren<TextMeshProUGUI>().text = timer.ToString();
    }

    /// <summary>
    /// Show info of something
    /// </summary>
    /// <param name="building">idk</param>
    public void ShowInfo(Building building)
    {
        canvasInfoUI.SetActive(true);
        titleInfo.text = EnumToReadableFormat(building.buildingType);
        descriptionInfo.text = building.buildingDescription;
        thumnailInfo.sprite = building.buildingThumbnail;

        if (building.GetHasPlaced())
        {
            // If the building is placed in a socket
            buttonInfo.GetComponentInChildren<TextMeshProUGUI>().text = "Build?";
            buttonInfo.onClick.RemoveAllListeners();
            buttonInfo.onClick.AddListener(building.TimeToBuild);
            buttonInfo.onClick.AddListener(() => ShowInfo(false));
        }
        else
        {
            // In the process of choosing building to spawn
            buttonInfo.GetComponentInChildren<TextMeshProUGUI>().text = "Confirm?";
            buttonInfo.onClick.RemoveAllListeners();
            buttonInfo.onClick.AddListener(() => gameManager.SpawnBuilding(building));
            buttonInfo.onClick.AddListener(() => ShowInfo(false));
        }
    }

    /// <summary>
    /// Show visible of info UI
    /// </summary>
    /// <param name="isVisible">need bool thank</param>
    public void ShowInfo(bool isVisible)
    {
        canvasInfoUI.SetActive(isVisible);
    }

    // -ve for left, +ve for right
    private void MapTurn(int direction)
    {
        currentRot = (currentRot + (90 * direction)) % 360;

        for (int i = 0; i < mapObj.Length; i++)
        {
            iTween.RotateTo(mapObj[i], iTween.Hash("y", currentRot, "time", turnDuration, "easetype", "easeInOutCubic"));
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
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class GameUI : MonoBehaviour
{
    [Header("Craft UI")]
    [SerializeField] private Canvas craftTaskUICanvas;
    [SerializeField] private TextMeshProUGUI craftTaskTitleText;
    [SerializeField] private RectTransform craftUIContent;
    [SerializeField] private RectTransform craftCanvasRow;
    [SerializeField] private RectTransform craftUIViewPort;
    private Dictionary<RectTransform, Building> craftRowList = new();

    [Header("Task UI")]
    [SerializeField] private RectTransform taskUIContent;
    [SerializeField] private RectTransform taskCanvasRow;
    [SerializeField] private RectTransform taskUIViewPort;
    private List<RectTransform> taskRowList = new();

    [Header("Resource UI")]
    [SerializeField] private RectTransform resourceUIContent;
    [SerializeField] private RectTransform resourceCanvasRow;
    [SerializeField] private float resourceRowStartingYPos;
    [SerializeField] private float resourceRowDistance;
    private List<ResourceUI> resourceRowList = new();

    [Header("Status UI")]
    [SerializeField] private StatusUI statusUIPrefab;
    [SerializeField] private RectTransform statusContent;
    [SerializeField] private TextMeshProUGUI apText;
    [SerializeField] private TextMeshProUGUI dayCounter;
    [SerializeField] private Sprite[] moodImage;
    [SerializeField] private Color[] colorStats;
    private List<StatusUI> listStatusUI = new();

    [Header("Information UI")]
    [SerializeField] private GameObject canvasInfoUI;
    [SerializeField] private TextMeshProUGUI titleInfo;
    [SerializeField] private TextMeshProUGUI descriptionInfo;
    [SerializeField] private Image thumnailInfo;
    [SerializeField] private Button buttonInfo;
    [SerializeField] private GameObject warningBG;
    [SerializeField] private TextMeshProUGUI warning;

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
    private GridSystem gridSystem;
    private GameManager gameManager;
    private float currentRot = 0;
    private SideEvent currentSideEventSelected;
    public bool isNotPlaced;

    // Start is called before the first frame update
    void Start()
    {
        // Object referencing
        slotManager = GameObject.Find("GRID System").GetComponent<SlotManager>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        gridSystem = slotManager.GetComponent<GridSystem>();
        
        InitialCraftUISetup();
        InitialResourceUISetup();
        InitialStatusUISetup();
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
        TaskUIUpdate(false);
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

    // Initialize Status UI
    private void InitialStatusUISetup()
    {
        for (int i = 0; i < gameManager.listStatus.Length; i++)
        {
            // Get the status
            Status status = gameManager.listStatus[i];

            // Get status type
            statusUIPrefab.statusType = status.statusType;
            string statusName = EnumToReadableFormat(status.statusType);
            statusUIPrefab.textStatus.text = statusName;

            // Load sprite according to resourceType name from Assets/Resources
            statusUIPrefab.thumbnail.sprite = Resources.Load<Sprite>($"UI/{statusName}");

            // apply limits
            statusUIPrefab.sliderValue.minValue = status.minLimit;
            statusUIPrefab.sliderValue.maxValue = status.maxLimit;

            // instantiate and keep inside lisStatusUI
            listStatusUI.Add(Instantiate(statusUIPrefab, statusContent));
        }
    }

    private void InitialMtUI()
    {
        if (mtButtons.Length == buttonTexts.Length)
        {
            for (int i = 0; i < mtButtons.Length; i++)
                mtButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = buttonTexts[i];

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
        CraftUIUpdate();
        ResourceUIUpdate();
        StatusUIUpdate();
        MtUIUpdate();
        ActionPointUpdate();
        ShowInfo(false);
    }

    // update action point
    private void ActionPointUpdate()
    {
        apText.text = gameManager.actionPoint.ToString();
    }

    // update craft ui
    private void CraftUIUpdate()
    {
        // Check currently "built" building has not placed in the map yet.
        if (isNotPlaced)
            craftTaskUICanvas.gameObject.SetActive(false);
        else
        {
            if (!craftTaskUICanvas.gameObject.activeSelf)
                craftTaskUICanvas.gameObject.SetActive(true);
            foreach (KeyValuePair<RectTransform, Building> row in craftRowList)
            {
                RectTransform rowInstance = row.Key;
                Building building = row.Value;

                //Debug.Log(building.BuildingName());
                Button button = rowInstance.GetComponentInChildren<Button>();
                buttonInfo.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => ShowInfo(building));
            }
        }
    }

    // update resource UI, the most inefficient code imaginable
    private void ResourceUIUpdate()
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

    // Update Status UI
    private void StatusUIUpdate()
    {
        foreach (StatusUI instance in listStatusUI)
        {
            for (int i = 0; i < gameManager.listStatus.Length; i++)
            {
                // Select a status
                Status status = gameManager.listStatus[i];

                // if same status type, update text the amount and slider
                if (status.statusType == instance.statusType)
                {

                    float amount = status.currentAmount;

                    // Change text inside StatusUI
                    instance.sliderValue.value = amount;
                    instance.textAmount.text = (amount * 100).ToString("F0") + "%";

                    // Change color according to each stats
                    if (status.indicatorStats.Length == colorStats.Length)
                        for (int j = 0; j < status.indicatorStats.Length; j++)
                        {
                            if (amount >= status.indicatorStats[j])
                            {
                                if (status.statusType == StatusType.Mood)
                                {
                                    instance.thumbnail.sprite = moodImage[j];
                                }
                                instance.fill.color = colorStats[j];
                                break;
                            }
                        }
                    else
                        Debug.LogError("The indicatorStats are not the same lenth as colorStatus for " + status.statusType.ToString());
                    break;
                }
            }
        }

        //other stuff to update
        dayCounter.text = "Day: " + gameManager.dayElapsed;
    }

    // Update Multi tool UI
    private void MtUIUpdate()
    {
        // Clear all listeners first before applying onClick functions to the buttons
        mtButtons[0].onClick.RemoveAllListeners();
        mtButtons[1].onClick.RemoveAllListeners();

        // Side event clear event idk
        currentSideEventSelected = null;

        // Check if the building is not placed in the map yet
        if (isNotPlaced)
            for (int i = 0; i < mtButtons.Length; i++)
                mtButtons[i].interactable = false;
        // Building mode
        else if (gridSystem.interactionMode == 0)
        {
            // First button
            if (gameManager.hasBuildTownHall)
            {
                mtButtons[0].GetComponentInChildren<TextMeshProUGUI>().text = "Road";
                mtButtons[0].onClick.AddListener(() => gridSystem.SetInteractionMode(1));
                mtButtons[0].interactable = true;
            }
            else
                mtButtons[0].interactable = false;

            // Second button
            mtButtons[1].GetComponentInChildren<TextMeshProUGUI>().text = "Manage";
            mtButtons[1].onClick.AddListener(() => gridSystem.SetInteractionMode(2));
            mtButtons[1].interactable = true;

            // Third button
            mtButtons[2].interactable = true;

            // Set craft UI canvas to true
            craftTaskUICanvas.gameObject.SetActive(true);
        }
        // Road mode
        else if (gridSystem.interactionMode == 1)
        {
            // First button
            mtButtons[0].GetComponentInChildren<TextMeshProUGUI>().text = "Building";
            mtButtons[0].onClick.AddListener(() => gridSystem.SetInteractionMode(0));

            // Second and third button
            mtButtons[1].interactable = false;
            mtButtons[2].interactable = false;

            // Set craft UI canvas to false
            craftTaskUICanvas.gameObject.SetActive(false);
        }
        else // "Teleport" mode
        {
            // Second button
            mtButtons[1].GetComponentInChildren<TextMeshProUGUI>().text = "Building";
            mtButtons[1].onClick.AddListener(() => gridSystem.SetInteractionMode(0));
            mtButtons[1].onClick.AddListener(() => TaskUIUpdate(false)); // disable task UI if visible
            mtButtons[1].onClick.AddListener(() => ShowInfo(false)); // disable task UI if visible

            // First button
            mtButtons[0].interactable = false;
            mtButtons[2].interactable = true;

            // Set craft UI canvas to false
            craftTaskUICanvas.gameObject.SetActive(false);
        }

    }

    // Check NPC is available to assign task
    private void TaskUIUpdate(SideEvent sideEvent)
    {
        // Switch from craft UI to task UI
        craftTaskUICanvas.gameObject.SetActive(true);
        TaskUIUpdate(true);

        // Clear out remaining task row
        if (taskRowList.Count != 0)
        {
            foreach (RectTransform item in taskRowList)
                Destroy(item.gameObject);
            taskRowList.Clear();
        }


        for (int i = 0; i < sideEvent.resourcesGain.Length; i++)
        {
            // Get resource task from side event
            ResourceTask resourceTask = sideEvent.resourcesGain[i];

            foreach (NPCType npc in gameManager.CheckNPCCapabilities(resourceTask.taskType))
            {
                // Check if the NPC are idle
                if (!gameManager.CheckNPCCurrentTask(npc))
                {
                    var rowInstance = Instantiate(taskCanvasRow, taskUIContent);

                    // Set NPC type
                    TextMeshProUGUI text = rowInstance.GetComponentInChildren<TextMeshProUGUI>();
                    Button button = rowInstance.GetComponentInChildren<Button>();

                    // Set text or something
                    text.text = npc.ToString();
                    button.GetComponentInChildren<TextMeshProUGUI>().text = EnumToReadableFormat(resourceTask.taskType);

                    // Set button listener to edit resources from GameManager
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => gameManager.ChangeValueResource(resourceTask.resourceType, resourceTask.amount));
                    button.onClick.AddListener(() => gameManager.AssignTask(npc,resourceTask.taskType));
                    //button.onClick.AddListener(() => TaskUIUpdate(button, false));

                    // Update action point
                    button.onClick.AddListener(() => gameManager.NextAction());
                    taskRowList.Add(rowInstance);
                }
            }
        }

        // Avoid next day because the player is choosing something
        mtButtons[2].interactable = true;
        buttonInfo.GetComponentInChildren<TextMeshProUGUI>().text = "Cancel Task";
        buttonInfo.onClick.RemoveAllListeners();
        buttonInfo.onClick.AddListener(() => ShowInfo(false));
        buttonInfo.onClick.AddListener(() => craftTaskUICanvas.gameObject.SetActive(false));
    }

    // Change task ui to resources UI
    private void TaskUIUpdate(bool change)
    {
        craftUIViewPort.gameObject.SetActive(!change);
        taskUIViewPort.gameObject.SetActive(change);

        if (change)
            craftTaskTitleText.text = "Assign a Task";
        else
            craftTaskTitleText.text = "Create a building";
    }

    // For use of addlistner event only
    //private void TaskUIUpdate(Button button, bool change)
    //{
    //    //Debug.Log("what");
    //    button.interactable = change;
    //}

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
    /// Destroy building progress rect transform
    /// </summary>
    /// <param name="building">Building</param>
    public void DestroyStats(Building building)
    {
        Destroy(building.GetComponentInChildren<RectTransform>().gameObject);
    }

    /// <summary>
    /// Show info of something
    /// </summary>
    /// <param name="building">idk</param>
    public void ShowInfo(Building building)
    {
        ShowInfo(false);
        canvasInfoUI.SetActive(true);
        titleInfo.text = EnumToReadableFormat(building.buildingType);
        descriptionInfo.text = building.buildingDescription;
        thumnailInfo.sprite = building.buildingThumbnail;

        if (building.GetHasPlaced())
        {


            // In the process of choosing a position to build
            buttonInfo.GetComponentInChildren<TextMeshProUGUI>().text = "Build?";
            buttonInfo.onClick.RemoveAllListeners();
            buttonInfo.onClick.AddListener(building.TimeToBuild);
        }
        else
        {
            // In the processing of creating a building
            buttonInfo.GetComponentInChildren<TextMeshProUGUI>().text = "Confirm?";

            // thanks chandragupta
            bool canBuild = true;

            // Check if the blacksmith is busy
            if (gameManager.CheckNPCCurrentTask(NPCType.Blacksmith))
            {
                BlockUserAction("Blacksmith is busy!");
                canBuild = false;
            }
            else if (building.requiresBuilderHut && !gameManager.hasBuildBuilderHut)
            {
                BlockUserAction("Required builder's hut!");
                canBuild = false;
            }
            else if (!gameManager.CheckBuild(building))
            {
                BlockUserAction("Not enough resources!");
                canBuild = false;
            }
            else if (gameManager.actionPoint <= 0)
            {
                BlockUserAction("Not enough action point!");
                canBuild = false;
            }

            if (canBuild)
            {
                buttonInfo.onClick.RemoveAllListeners();
                buttonInfo.onClick.AddListener(() => gameManager.SpawnBuilding(building));
            }
        }
    }

    /// <summary>
    /// Show info of something
    /// </summary>
    /// <param name="sideEvent">Type of side event</param>
    public void ShowInfo(SideEvent sideEvent)
    {
        // Check if the user is selecting the side event
        if (currentSideEventSelected != sideEvent)
        {
            // Set up everything
            canvasInfoUI.SetActive(true);
            craftTaskUICanvas.gameObject.SetActive(false);

            // Buiding can be side event too i guess
            if (sideEvent.eventType == SideEventType.BuildingInfo)
            {
                Building building = sideEvent.buildingStats;

                titleInfo.text = EnumToReadableFormat(building.buildingType);
                descriptionInfo.text = building.buildingDescription;
                if (building.duration > 0)
                    descriptionInfo.text += " Remaining days to build: " + building.duration;
                thumnailInfo.sprite = building.buildingThumbnail;
            }
            else
            {
                titleInfo.text = EnumToReadableFormat(sideEvent.eventType);
                descriptionInfo.text = sideEvent.description;
                thumnailInfo.sprite = sideEvent.thumbnail;
            }
            currentSideEventSelected = sideEvent;

            // Set up button
            if (sideEvent.resourcesGain.Length == 0)
                buttonInfo.gameObject.SetActive(false);
            else
            {
                buttonInfo.gameObject.SetActive(true);
                buttonInfo.GetComponentInChildren<TextMeshProUGUI>().text = "Assign Task";

                if (gameManager.actionPoint > 0)
                {
                    buttonInfo.onClick.RemoveAllListeners();
                    buttonInfo.onClick.AddListener(() => TaskUIUpdate(sideEvent));
                }
                else
                    BlockUserAction("Not enough action point!");
            }
        }
    }

    private void BlockUserAction(string text)
    {
        buttonInfo.interactable = false;
        warningBG.SetActive(true);
        warning.text = text;
    }

    /// <summary>
    /// Show visible of info UI
    /// </summary>
    /// <param name="isVisible">need bool thank</param>
    public void ShowInfo(bool isVisible)
    {
        if (warningBG != null && warningBG.activeSelf)
            warningBG.SetActive(false);
        if (buttonInfo != null)
            buttonInfo.gameObject.SetActive(true);
        buttonInfo.interactable = true;
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
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// For display information on the building itself
/// </summary>
public class TableUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private RawImage thumbnailImage;

    private Transform playerTravelPos;
    private GridSystem gridSystem;

    void Start()
    {
        gameObject.SetActive(false);
        gridSystem = GameObject.Find("GRID System").GetComponent<GridSystem>();
    }

    public void setText(string title, string desc, Texture2D thumb, Transform pos)
    {
        nameText.text = title;
        descriptionText.text = desc;
        thumbnailImage.texture = thumb;
        playerTravelPos = pos;
    }

    public void TravelToBuilding()
    {
        gridSystem.ResizeWorld(playerTravelPos);
    }

}

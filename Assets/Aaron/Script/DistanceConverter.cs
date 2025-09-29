using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

public class DistanceConverter : MonoBehaviour
{

    [Header("Player Dot stuff")]
    public RectTransform playerDot;

    public RectTransform playerAimingTipReferencePoint;
    public RectTransform miniMapRectTransform;
    private Vector3 lastWorldPos;
    [SerializeField] private float cabinetDefaultDistanceFromPlayer = 0.5f;

    [Header("Mapping 3D to 2D stuff")]
    public float realToMapScale = 10f; // 1 米 = 10 px

    [Header("AR data stuff")]
    private Vector3 currentWorldPos;
    private Vector3 moveDirection;
    private float distanceMoved;

    [Header("Rotation")]
    private Vector3 rotationOnStart;
    private Vector3 currentRotation;
    private float angleDiffInY;

    [Header("Spawning Cabinet")]
    [SerializeField] GameObject cabinetPrefab;
    [SerializeField] List<Color> cabinetColors;
    [SerializeField] GameObject groupingPrefab;
    private int currentColorIndex = 0;

    void Start()
    {
        lastWorldPos = Camera.main.transform.position; // 初始位置
        rotationOnStart = Camera.main.transform.eulerAngles;
        UpdatePlayerDotPosition();
    }

    void Update()
    {
        UpdatingARDataStuff();
        UpdatePlayerDotPosition(); // 每幀更新玩家點位置
    }

    public void UpdatingARDataStuff()
    {
        // check the field header
        currentWorldPos = Camera.main.transform.position;
        currentRotation = Camera.main.transform.eulerAngles;
        angleDiffInY = currentRotation.y - rotationOnStart.y;
        distanceMoved = Vector3.Distance(lastWorldPos, currentWorldPos);
        moveDirection = (currentWorldPos - lastWorldPos).normalized;
    }

    public float MapRealDistanceTo2D(float realDistance)
    {
        return realDistance * realToMapScale;
    }

    public void UpdatePlayerDotPosition()
    {
        float mapDistance = MapRealDistanceTo2D(distanceMoved);
        Vector3 mapMove = moveDirection * mapDistance;

        // 更新玩家點嘅位置（假設地圖原點係 (0,0)）
        if (miniMapRectTransform != null)
        {
            miniMapRectTransform.anchoredPosition -= new Vector2(mapMove.x, mapMove.z); // 只用 x, z 軸
            playerDot.rotation = Quaternion.Euler(0,0,-angleDiffInY);
        }

        lastWorldPos = currentWorldPos; // 更新上次位置
    }

    [Button]
    public void AddingCabinet(int quantity = 0)
    {

 if (!playerDot)
        return;

    RectTransform playerRect = playerDot.GetComponent<RectTransform>();


    Vector2 cabinetOffsetLocal = Vector2.up * MapRealDistanceTo2D(cabinetDefaultDistanceFromPlayer);

    Vector3 cabinetWorldPosition =  playerRect.TransformPoint(cabinetOffsetLocal);


    Vector2 actualCabinetPosition2D = miniMapRectTransform.InverseTransformPoint(cabinetWorldPosition);

    Debug.Log("Cabinet Position 2D: " + actualCabinetPosition2D);


    GameObject grouping = Instantiate(groupingPrefab);

    var groupingRec = grouping.GetComponent<RectTransform>();
    grouping.transform.SetParent(miniMapRectTransform, false); // 'false' keeps local space correct
    grouping.transform.localScale = Vector3.one;


    // var groupingHorizontalLayoutGroup = grouping.AddComponent<HorizontalLayoutGroup>();
    // groupingHorizontalLayoutGroup.childForceExpandHeight = false;
    // groupingHorizontalLayoutGroup.childForceExpandWidth = false;
    // groupingHorizontalLayoutGroup.childControlHeight = false;
    // groupingHorizontalLayoutGroup.childControlWidth = false;
    // groupingHorizontalLayoutGroup.childAlignment = TextAnchor.UpperLeft;
    // groupingHorizontalLayoutGroup.spacing = 0.1f;

    for (int i = 0; i < quantity; i++)
    {
        var currentObj = Instantiate(
            cabinetPrefab,
            grouping.transform.position,
            Quaternion.identity,
            grouping.transform
        );

        var img = currentObj.GetComponent<Image>();
        img.color = cabinetColors[currentColorIndex];
        currentColorIndex = (currentColorIndex + 1) % cabinetColors.Count;
    }
    // tweaking the position at the end because layout group expand to the right and we want to recenter it when it's done to get the correct center
    //groupingRec.pivot = new Vector2(0.5f, 0.5f);

    Debug.Log(actualCabinetPosition2D);
    groupingRec.anchoredPosition = actualCabinetPosition2D;
    groupingRec.rotation = playerRect.rotation;
    }


}

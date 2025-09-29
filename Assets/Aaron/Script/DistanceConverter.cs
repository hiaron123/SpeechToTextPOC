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
       // get current direction facing
         Vector3 forward = Camera.main.transform.forward;
         Vector3 directionWithCabinetDistanceTo2D = forward * MapRealDistanceTo2D(cabinetDefaultDistanceFromPlayer);
         if (!playerDot)
             return;

         // Defined the position 2D first

         // use the following code if we want to use the tip of aim
         // var aimingToWorld =
         //     playerAimingTipReferencePoint.TransformPoint(playerAimingTipReferencePoint.anchoredPosition);
         // var anchoredPosition = miniMapRectTransform.InverseTransformPoint(aimingToWorld);
           var playToWorld =  playerDot.TransformPoint(playerDot.anchoredPosition);
          var playerToMiniMapLocal = miniMapRectTransform.InverseTransformPoint(playToWorld);
         var actualCabinetPosition2D = (Vector2)playerToMiniMapLocal + new Vector2(directionWithCabinetDistanceTo2D.x,
                                           directionWithCabinetDistanceTo2D.z);
         Debug.Log("Cabinet Position 2D: " + actualCabinetPosition2D);

        // Spawn the grouping object
        GameObject grouping = new GameObject("CabinetGroup", typeof(RectTransform));
        grouping.transform.SetParent(miniMapRectTransform);
        grouping.transform.localScale = Vector3.one;

        grouping.GetComponent<RectTransform>().anchoredPosition = actualCabinetPosition2D;
        var groupingHorizontalLayoutGroup = grouping.AddComponent<HorizontalLayoutGroup>();
        groupingHorizontalLayoutGroup.childForceExpandHeight = false;
        groupingHorizontalLayoutGroup.childForceExpandWidth = false;
        groupingHorizontalLayoutGroup.childControlHeight = false;
        groupingHorizontalLayoutGroup.childControlWidth = false;
        groupingHorizontalLayoutGroup.childAlignment = TextAnchor.MiddleCenter;
        groupingHorizontalLayoutGroup.spacing = 0.1f;


        // Spawn the cabinets as children of grouping object in 2D minimap
        for (int i = 0; i < quantity; i++)
        {
            var currentObj = Instantiate(cabinetPrefab,grouping.transform.position, Quaternion.identity,grouping.transform);
            var img = currentObj.GetComponent<Image>();
            img.color = cabinetColors[currentColorIndex];
            currentColorIndex = (currentColorIndex + 1) % cabinetColors.Count;
        }

        // Rotation for the grouping object to face the player dot in minimap
        var cabinGroupFaceDirection = (playerDot.transform.position - grouping.transform.position).normalized;
        float angle = Mathf.Atan2(cabinGroupFaceDirection.y,cabinGroupFaceDirection.x) * Mathf.Rad2Deg -90;
        grouping.GetComponent<RectTransform>().localEulerAngles = new Vector3(0, 0, angle);

    }
}

using NaughtyAttributes;
using UnityEngine;

public class DistanceConverter : MonoBehaviour
{

    public RectTransform playerDot;
    private Vector3 lastWorldPos;
    [SerializeField] private float cabinetDefaultDistanceFromPlayer = 0.5f;

    [Header("Mapping 3D to 2D stuff")]
    public float realToMapScale = 10f; // 1 米 = 10 px

    [Header("AR data stuff")]
    private Vector3 currentWorldPos;
    private Vector3 moveDirection;
    private float distanceMoved;

    void Start()
    {
        lastWorldPos = Camera.main.transform.position; // 初始位置
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
        if (playerDot != null)
        {
            playerDot.anchoredPosition += new Vector2(mapMove.x, mapMove.z); // 只用 x, z 軸
        }

        lastWorldPos = currentWorldPos; // 更新上次位置
    }

    [Button]
    public void AddingCabinet(int quantity = 0)
    {
       // get current direction facing
         Vector3 forward = Camera.main.transform.forward;
         Vector3 directionWithCabinetDistanceTo2D = forward * MapRealDistanceTo2D(cabinetDefaultDistanceFromPlayer);
         if (playerDot != null)
         {
             // Defined the position 2D first
             Vector2 actualCabinetPosition2D = playerDot.anchoredPosition +
                                               new Vector2(directionWithCabinetDistanceTo2D.x,
                                                   directionWithCabinetDistanceTo2D.z);
                Debug.Log("Cabinet Position 2D: " + actualCabinetPosition2D);
         }

    }
}

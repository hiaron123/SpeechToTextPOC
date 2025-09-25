using UnityEngine;

public class DistanceConverter : MonoBehaviour
{
    public float realToMapScale = 10f; // 1 米 = 10 px
    public RectTransform playerDot;
    private Vector3 lastWorldPos;
    void Start()
    {
        lastWorldPos = Camera.main.transform.position; // 初始位置
        UpdatePlayerDotPosition();
    }

    void Update()
    {
        UpdatePlayerDotPosition(); // 每幀更新玩家點位置
    }

    public void UpdatePlayerDotPosition()
    {
        Vector3 currentWorldPos = Camera.main.transform.position;
        float distanceMoved = Vector3.Distance(lastWorldPos, currentWorldPos);
        Vector3 moveDirection = (currentWorldPos - lastWorldPos).normalized;
        float mapDistance = distanceMoved * realToMapScale;
        Vector3 mapMove = moveDirection * mapDistance;

        // 更新玩家點嘅位置（假設地圖原點係 (0,0)）
        if (playerDot != null)
        {
            playerDot.anchoredPosition += new Vector2(mapMove.x, mapMove.z); // 只用 x, z 軸
        }

        lastWorldPos = currentWorldPos; // 更新上次位置
    }
}

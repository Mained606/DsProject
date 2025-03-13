using UnityEngine;

public class MapUI : MonoBehaviour
{
    public RectTransform mapImage;
    public RectTransform playerIcon;
    public Terrain terrain;
    private Transform player;
    private Vector2 worldSize;

    private void Start()
    {
        if (terrain != null)
        {
            //worldSize = new Vector2(terrain.terrainData.size.x, terrain.terrainData.size.z);
            worldSize = new Vector2(1000,1000);
        }
        else
        {
            Debug.LogError("Terrain이 설정되지 않았습니다.");
        }
    }

    private void Update()
    {
        if (player == null) player = GameManager.playerTransform;
        Vector2 mapPos = WorldToMap(player.position);
        playerIcon.anchoredPosition = mapPos;
    }

    private Vector2 WorldToMap(Vector3 worldPos)
    {
        float minX = 0, maxX = worldSize.x;
        float minZ = 0, maxZ = worldSize.y;

        float normalizedX = (worldPos.x - minX) / (maxX - minX);
        float normalizedY = (worldPos.z - minZ) / (maxZ - minZ);

        float mapWidth = mapImage.rect.width;
        float mapHeight = mapImage.rect.height;

        float mapX = normalizedX * mapWidth - (mapWidth / 2);
        float mapY = normalizedY * mapHeight - (mapHeight / 2);

        return new Vector2(mapX, mapY);
    }
}

using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;

public class DungeonMapGenerator : MonoBehaviour
{
    public static DungeonMapGenerator Instance;

    public Tile WallTile;
    public Tile CompensationTile;

    const int MAP_WIDTH = 200;
    const int MAP_HEIGHT = 200;
    const int CELLULAR_ITERATIONS = 20;  // ���귯 ���丶Ÿ ������ �ݺ� Ƚ��
    const int MIN_ROOM_SIZE = 10;        // ���� ��ȿ�ϴٰ� �ǴܵǴ� �ּ� ũ��(CompensationTile ������ �� ���)

    public int TotalCompensation = 100; // �� ��ü�� ������ ������ ��

    Tilemap tilemap;
    Dictionary<Vector3Int, Tile> mapTiles;           // ��� Ÿ���� ���� ���¸� ����
    HashSet<Vector3Int> safeZoneBoundary;           // ���� ������ ��ǥ
    readonly System.Random random = new System.Random();

    // ���������� ũ�⸦ ����� ���� ����
    readonly int safeZoneRadius = 20;                // �߾� ���� ������ �ݰ�
    readonly int safeZoneRadiusSquared;             // �� ���� �Ÿ� Ȯ���� ���� ������ �ݰ�
    readonly int outerSafeZoneRadius;               // �ܺ� ���� ���� ����� �ݰ�
    readonly int outerSafeZoneRadiusSquared;        // �� ���� Ȯ���� ���� ������ �ܺ� �ݰ�
    readonly int mapCenterX = MAP_WIDTH / 2;        // �� �߽��� X ��ǥ
    readonly int mapCenterY = MAP_HEIGHT / 2;       // �� �߽��� Y ��ǥ

    // ����
    static readonly Vector3Int[] adjacentDirections =
    {
        new(1, 0, 0),   // ������
        new(-1, 0, 0),  // ����
        new(0, 1, 0),   // ��
        new(0, -1, 0)   // �Ʒ�
    };

    /// <summary>
    /// �̸� ���� ���� �ڷᱸ���� �� �����⸦ �ʱ�ȭ
    /// </summary>
    public DungeonMapGenerator()
    {
        // ���� �Ÿ��� �̸� ���
        safeZoneRadiusSquared = safeZoneRadius * safeZoneRadius;
        outerSafeZoneRadius = safeZoneRadius + 5;
        outerSafeZoneRadiusSquared = outerSafeZoneRadius * outerSafeZoneRadius;

        mapTiles = new Dictionary<Vector3Int, Tile>(MAP_WIDTH * MAP_HEIGHT);
        safeZoneBoundary = new HashSet<Vector3Int>();
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    void Start()
    {
        tilemap = GetComponent<Tilemap>();
        InitializeRandomMap();           // �ʱ� ���� ������ ����
        CreateSafeZone();               // �߾� ���� ���� ����
        ApplyCellularAutomata();        // ���귯 ���丶Ÿ�� ����Ͽ� �� ������
        DistributeCollectibles();       // ��ȿ�� ��ġ�� ���� ��ġ
        PrintMap();                    // ���� �� ǥ��
    }

    /// <summary>
    /// ��ֹ� �е��� 50%�� �ʱ� ������ ������ ���� ����
    /// </summary>
    void InitializeRandomMap()
    {
        Vector3Int tilePosition = new();
        for (int x = 0; x < MAP_WIDTH; x++)
        {
            for (int y = 0; y < MAP_HEIGHT; y++)
            {
                tilePosition.x = x;
                tilePosition.y = y;

                mapTiles[tilePosition] = random.NextDouble() < 0.5 ? WallTile : null;
            }
        }
    }

    /// <summary>
    /// ���귯 ���丶Ÿ ��Ģ�� �����Ͽ� ���� �ε巴�� �ϰ� �ڿ������� ������ ����
    /// </summary>
    void ApplyCellularAutomata()
    {
        var nextIterationTiles = new Dictionary<Vector3Int, Tile>(MAP_WIDTH * MAP_HEIGHT);
        Vector3Int tilePosition = new();

        for (int iteration = 0; iteration < CELLULAR_ITERATIONS; iteration++)
        {
            for (int x = 0; x < MAP_WIDTH; x++)
            {
                for (int y = 0; y < MAP_HEIGHT; y++)
                {
                    tilePosition.x = x;
                    tilePosition.y = y;

                    // �������� ��� Ÿ�� �ǳʶٱ�
                    if (!safeZoneBoundary.Contains(tilePosition))
                    {
                        // �̿� ���� ���� ���귯 ���丶Ÿ ��Ģ�� ����
                        int adjacentObstacles = CountAdjacentObstacles(x, y);
                        nextIterationTiles[tilePosition] = adjacentObstacles > 5 ? WallTile :
                            adjacentObstacles < 3 ? null :
                            mapTiles[tilePosition];
                    }
                    else
                    {
                        // ���� ���� ��� Ÿ���� ����
                        nextIterationTiles[tilePosition] = mapTiles[tilePosition];
                    }
                }
            }

            // ���� �ݺ��� ���� Ÿ�� �÷����� ��ȯ
            Dictionary<Vector3Int, Tile> tempTiles = mapTiles;
            mapTiles = nextIterationTiles;
            nextIterationTiles = tempTiles;
        }
    }

    /// <summary>
    /// �־��� ��ġ�� ������ ��ֹ� Ÿ���� ���� ���
    /// </summary>
    /// <param name="centerX">�߽� Ÿ���� X ��ǥ</param>
    /// <param name="centerY">�߽� Ÿ���� Y ��ǥ</param>
    /// <returns>������ ��ֹ� Ÿ���� ��</returns>
    int CountAdjacentObstacles(int centerX, int centerY)
    {
        int obstacleCount = 0;
        Vector3Int checkPosition = new();

        // �ֺ� Ÿ�� 8���� ��� Ȯ��
        for (int x = centerX - 1; x <= centerX + 1; x++)
        {
            for (int y = centerY - 1; y <= centerY + 1; y++)
            {
                if (x < 0 || x >= MAP_WIDTH || y < 0 || y >= MAP_HEIGHT)
                {
                    // ��踦 ��� ��ġ�� ��ֹ��� ���
                    obstacleCount++;
                }
                else if (x != centerX || y != centerY)
                {
                    // ��ȿ�� ��ġ�� �ִ� ���� ��ֹ� ���� ���
                    checkPosition.x = x;
                    checkPosition.y = y;
                    obstacleCount += mapTiles[checkPosition] == WallTile ? 1 : 0;
                }
            }
        }
        return obstacleCount;
    }

    /// <summary>
    /// �� �߾ӿ� �������� ���������� ��ȯ�Ǵ� ���� ������ ����
    /// </summary>
    void CreateSafeZone()
    {
        Vector3Int tilePosition = new();

        for (int y = -outerSafeZoneRadius; y <= outerSafeZoneRadius; y++)
        {
            for (int x = -outerSafeZoneRadius; x <= outerSafeZoneRadius; x++)
            {
                int mapX = mapCenterX + x;
                int mapY = mapCenterY + y;

                // ��ġ�� ���� ��� ���� �ִ��� Ȯ��
                if (mapX >= 0 && mapX < MAP_WIDTH && mapY >= 0 && mapY < MAP_HEIGHT)
                {
                    tilePosition.x = mapX;
                    tilePosition.y = mapY;

                    int distanceSquared = x * x + y * y;

                    // ���� ���� ������ ������ ����
                    if (distanceSquared < safeZoneRadiusSquared)
                    {
                        mapTiles[tilePosition] = null;
                    }
                    // ������ Ŭ������� ��ȯ ���� ����
                    else if (distanceSquared < outerSafeZoneRadiusSquared && random.NextDouble() < 0.4)
                    {
                        mapTiles[tilePosition] = null;
                    }

                    // �������� ��躮 ����
                    if (distanceSquared >= (safeZoneRadius - 1) * (safeZoneRadius - 1)
                        && distanceSquared < safeZoneRadiusSquared)
                    {
                        mapTiles[tilePosition] = WallTile;
                        safeZoneBoundary.Add(tilePosition);
                    }
                }
            }
        }
    }

    /// <summary>
    /// ������ ��ȿ�� ��鿡 ������ �й�
    /// </summary>
    void DistributeCollectibles()
    {
        // ��ȿ�� ���� ��� ã�� (�������� �� ���� �� ����)
        var availableRooms = FindAvailableRooms()
            .Where(room => room.Count >= MIN_ROOM_SIZE && !IsSafeZoneOverlap(room))
            .ToList();

        if (availableRooms.Count == 0)
        {
            return;
        }

        // �յ��� �й踦 ���� ��� ������ ���
        int remainingCollectibles = TotalCompensation;
        int collectiblesPerRoom = Mathf.Max(1, remainingCollectibles / availableRooms.Count);

        // �� ��ü�� ���� ����
        foreach (var room in availableRooms)
        {
            var roomTiles = room.ToArray(); // �� ���� ���� �׼����� ���� �迭�� ��ȯ
            for (int i = 0; i < collectiblesPerRoom && remainingCollectibles > 0; i++)
            {
                Vector3Int randomPosition = roomTiles[random.Next(roomTiles.Length)];
                if (HasRequiredSpacing(randomPosition))
                {
                    mapTiles[randomPosition] = CompensationTile;
                    remainingCollectibles--;
                }
            }
        }
    }

    /// <summary>
    /// ���� ������ ���� ����� �� ������ ��ġ �ֺ��� �ִ��� Ȯ��
    /// </summary>
    bool HasRequiredSpacing(Vector3Int position)
    {
        Vector3Int checkPosition = new();
        // ��ġ �ֺ��� 3x3 ������ Ȯ��
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                checkPosition.x = position.x + x;
                checkPosition.y = position.y + y;
                if (!mapTiles.ContainsKey(checkPosition) || mapTiles[checkPosition] != null)
                {
                    return false;
                }
            }
        }
        return true;
    }

    /// <summary>
    /// ���� ���� ������ ��ġ���� Ȯ��
    /// </summary>
    bool IsSafeZoneOverlap(HashSet<Vector3Int> room)
    {
        foreach (var position in room)
        {
            int dx = position.x - mapCenterX;
            int dy = position.y - mapCenterY;
            if (dx * dx + dy * dy <= safeZoneRadiusSquared)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// �ʿ��� ��� ���ӵ� �� ����(���)�� ã��
    /// </summary>
    /// <returns>�� Ÿ�� ��Ʈ�� ����Ʈ</returns>
    List<HashSet<Vector3Int>> FindAvailableRooms()
    {
        var rooms = new List<HashSet<Vector3Int>>();
        var visitedTiles = new HashSet<Vector3Int>();
        Vector3Int tilePosition = new();

        // �湮���� ���� �� ������ �ִ��� ��ü ������ ��ĵ
        for (int x = 0; x < MAP_WIDTH; x++)
        {
            for (int y = 0; y < MAP_HEIGHT; y++)
            {
                tilePosition.x = x;
                tilePosition.y = y;
                if (!visitedTiles.Contains(tilePosition) && mapTiles[tilePosition] == null)
                {
                    rooms.Add(MapConnectedRoom(tilePosition, visitedTiles));
                }
            }
        }

        return rooms;
    }

    /// <summary>
    /// �־��� ��ġ���� �����Ͽ� ����� ��� �� Ÿ���� ����
    /// </summary>
    /// <param name="startPosition">���� Ÿ�� ��ġ</param>
    /// <param name="visitedTiles">�̹� �湮�� Ÿ�ϵ��� ��Ʈ</param>
    /// <returns>����� �� Ÿ�ϵ��� ��Ʈ</returns>
    HashSet<Vector3Int> MapConnectedRoom(Vector3Int startPosition, HashSet<Vector3Int> visitedTiles)
    {
        var roomTiles = new HashSet<Vector3Int>();
        var tilesToCheck = new Queue<Vector3Int>();
        tilesToCheck.Enqueue(startPosition);
        Vector3Int nextPosition = new();

        // ����� Ÿ���� ã�� ���� "�÷��� ��" �˰���
        while (tilesToCheck.Count > 0)
        {
            var currentPosition = tilesToCheck.Dequeue();
            if (!visitedTiles.Contains(currentPosition) &&
                IsValidPosition(currentPosition) &&
                mapTiles[currentPosition] == null)
            {
                roomTiles.Add(currentPosition);
                visitedTiles.Add(currentPosition);

                // ������ Ÿ�� Ȯ��
                foreach (var direction in adjacentDirections)
                {
                    nextPosition.x = currentPosition.x + direction.x;
                    nextPosition.y = currentPosition.y + direction.y;
                    if (!visitedTiles.Contains(nextPosition) && IsValidPosition(nextPosition))
                    {
                        tilesToCheck.Enqueue(nextPosition);
                    }
                }
            }
        }

        return roomTiles;
    }

    /// <summary>
    /// ��ġ�� �� ��� ���� �ְ� Ÿ�� ��ųʸ��� �����ϴ��� Ȯ��
    /// </summary>
    bool IsValidPosition(Vector3Int position)
    {
        return position.x >= 0 && position.x < MAP_WIDTH &&
               position.y >= 0 && position.y < MAP_HEIGHT &&
               mapTiles.ContainsKey(position);
    }

    void PrintMap()
    {
        tilemap.ClearAllTiles();
        foreach (var tileData in mapTiles)
        {
            if (tileData.Value != null)
            {
                tilemap.SetTile(tileData.Key, tileData.Value);
            }
        }
    }
}
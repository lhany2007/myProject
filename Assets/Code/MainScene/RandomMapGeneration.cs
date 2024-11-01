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
    const int CELLULAR_ITERATIONS = 20;  // 셀룰러 오토마타 스무딩 반복 횟수
    const int MIN_ROOM_SIZE = 10;        // 방이 유효하다고 판단되는 최소 크기(CompensationTile 생성할 때 사용)

    public int TotalCompensation = 100; // 맵 전체에 분포될 보상의 수

    Tilemap tilemap;
    Dictionary<Vector3Int, Tile> mapTiles;           // 모든 타일의 현재 상태를 저장
    HashSet<Vector3Int> safeZoneBoundary;           // 안전 지대의 좌표
    readonly System.Random random = new System.Random();

    // 안전지대의 크기를 계산을 위한 값들
    readonly int safeZoneRadius = 20;                // 중앙 안전 지대의 반경
    readonly int safeZoneRadiusSquared;             // 더 빠른 거리 확인을 위한 제곱된 반경
    readonly int outerSafeZoneRadius;               // 외부 안전 구역 경계의 반경
    readonly int outerSafeZoneRadiusSquared;        // 더 빠른 확인을 위한 제곱된 외부 반경
    readonly int mapCenterX = MAP_WIDTH / 2;        // 맵 중심의 X 좌표
    readonly int mapCenterY = MAP_HEIGHT / 2;       // 맵 중심의 Y 좌표

    // 방향
    static readonly Vector3Int[] adjacentDirections =
    {
        new(1, 0, 0),   // 오른쪽
        new(-1, 0, 0),  // 왼쪽
        new(0, 1, 0),   // 위
        new(0, -1, 0)   // 아래
    };

    /// <summary>
    /// 미리 계산된 값과 자료구조로 맵 생성기를 초기화
    /// </summary>
    public DungeonMapGenerator()
    {
        // 제곱 거리를 미리 계산
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
        InitializeRandomMap();           // 초기 랜덤 노이즈 생성
        CreateSafeZone();               // 중앙 안전 구역 생성
        ApplyCellularAutomata();        // 셀룰러 오토마타를 사용하여 맵 스무딩
        DistributeCollectibles();       // 유효한 위치에 보상 배치
        PrintMap();                    // 최종 맵 표시
    }

    /// <summary>
    /// 장애물 밀도가 50%인 초기 무작위 노이즈 맵을 생성
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
    /// 셀룰러 오토마타 규칙을 적용하여 맵을 부드럽게 하고 자연스러운 동굴을 생성
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

                    // 안전지대 경계 타일 건너뛰기
                    if (!safeZoneBoundary.Contains(tilePosition))
                    {
                        // 이웃 수에 따라 셀룰러 오토마타 규칙을 적용
                        int adjacentObstacles = CountAdjacentObstacles(x, y);
                        nextIterationTiles[tilePosition] = adjacentObstacles > 5 ? WallTile :
                            adjacentObstacles < 3 ? null :
                            mapTiles[tilePosition];
                    }
                    else
                    {
                        // 안전 구역 경계 타일을 보존
                        nextIterationTiles[tilePosition] = mapTiles[tilePosition];
                    }
                }
            }

            // 다음 반복을 위해 타일 컬렉션을 교환
            Dictionary<Vector3Int, Tile> tempTiles = mapTiles;
            mapTiles = nextIterationTiles;
            nextIterationTiles = tempTiles;
        }
    }

    /// <summary>
    /// 주어진 위치에 인접한 장애물 타일의 수를 계산
    /// </summary>
    /// <param name="centerX">중심 타일의 X 좌표</param>
    /// <param name="centerY">중심 타일의 Y 좌표</param>
    /// <returns>인접한 장애물 타일의 수</returns>
    int CountAdjacentObstacles(int centerX, int centerY)
    {
        int obstacleCount = 0;
        Vector3Int checkPosition = new();

        // 주변 타일 8개를 모두 확인
        for (int x = centerX - 1; x <= centerX + 1; x++)
        {
            for (int y = centerY - 1; y <= centerY + 1; y++)
            {
                if (x < 0 || x >= MAP_WIDTH || y < 0 || y >= MAP_HEIGHT)
                {
                    // 경계를 벗어난 위치를 장애물로 계산
                    obstacleCount++;
                }
                else if (x != centerX || y != centerY)
                {
                    // 유효한 위치에 있는 실제 장애물 수를 계산
                    checkPosition.x = x;
                    checkPosition.y = y;
                    obstacleCount += mapTiles[checkPosition] == WallTile ? 1 : 0;
                }
            }
        }
        return obstacleCount;
    }

    /// <summary>
    /// 맵 중앙에 던전으로 점진적으로 전환되는 안전 구역을 생성
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

                // 위치가 지도 경계 내에 있는지 확인
                if (mapX >= 0 && mapX < MAP_WIDTH && mapY >= 0 && mapY < MAP_HEIGHT)
                {
                    tilePosition.x = mapX;
                    tilePosition.y = mapY;

                    int distanceSquared = x * x + y * y;

                    // 내부 안전 구역을 완전히 비우기
                    if (distanceSquared < safeZoneRadiusSquared)
                    {
                        mapTiles[tilePosition] = null;
                    }
                    // 무작위 클리어링으로 전환 영역 생성
                    else if (distanceSquared < outerSafeZoneRadiusSquared && random.NextDouble() < 0.4)
                    {
                        mapTiles[tilePosition] = null;
                    }

                    // 안전지대 경계벽 생성
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
    /// 동굴의 유효한 방들에 보상을 분배
    /// </summary>
    void DistributeCollectibles()
    {
        // 유효한 방을 모두 찾음 (안전지대 및 작은 방 제외)
        var availableRooms = FindAvailableRooms()
            .Where(room => room.Count >= MIN_ROOM_SIZE && !IsSafeZoneOverlap(room))
            .ToList();

        if (availableRooms.Count == 0)
        {
            return;
        }

        // 균등한 분배를 위해 방당 보상을 계산
        int remainingCollectibles = TotalCompensation;
        int collectiblesPerRoom = Mathf.Max(1, remainingCollectibles / availableRooms.Count);

        // 방 전체에 보상 배포
        foreach (var room in availableRooms)
        {
            var roomTiles = room.ToArray(); // 더 빠른 랜덤 액세스를 위해 배열로 변환
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
    /// 보상 생성을 위한 충분한 빈 공간이 위치 주변에 있는지 확인
    /// </summary>
    bool HasRequiredSpacing(Vector3Int position)
    {
        Vector3Int checkPosition = new();
        // 위치 주변의 3x3 영역을 확인
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
    /// 방이 안전 구역과 겹치는지 확인
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
    /// 맵에서 모든 연속된 빈 공간(방들)을 찾음
    /// </summary>
    /// <returns>방 타일 세트의 리스트</returns>
    List<HashSet<Vector3Int>> FindAvailableRooms()
    {
        var rooms = new List<HashSet<Vector3Int>>();
        var visitedTiles = new HashSet<Vector3Int>();
        Vector3Int tilePosition = new();

        // 방문하지 않은 빈 공간이 있는지 전체 지도를 스캔
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
    /// 주어진 위치에서 시작하여 연결된 모든 빈 타일을 매핑
    /// </summary>
    /// <param name="startPosition">시작 타일 위치</param>
    /// <param name="visitedTiles">이미 방문한 타일들의 세트</param>
    /// <returns>연결된 방 타일들의 세트</returns>
    HashSet<Vector3Int> MapConnectedRoom(Vector3Int startPosition, HashSet<Vector3Int> visitedTiles)
    {
        var roomTiles = new HashSet<Vector3Int>();
        var tilesToCheck = new Queue<Vector3Int>();
        tilesToCheck.Enqueue(startPosition);
        Vector3Int nextPosition = new();

        // 연결된 타일을 찾기 위한 "플러드 필" 알고리즘
        while (tilesToCheck.Count > 0)
        {
            var currentPosition = tilesToCheck.Dequeue();
            if (!visitedTiles.Contains(currentPosition) &&
                IsValidPosition(currentPosition) &&
                mapTiles[currentPosition] == null)
            {
                roomTiles.Add(currentPosition);
                visitedTiles.Add(currentPosition);

                // 인접한 타일 확인
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
    /// 위치가 맵 경계 내에 있고 타일 딕셔너리에 존재하는지 확인
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
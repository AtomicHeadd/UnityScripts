using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 現状2階対応なし、Y=Yoffset(Default=1)から2UnitのRayを照射
/// 
/// 部屋プレハブ
/// ・接続口のGameObjectの名前にはroomJointName(Default="Joint")という文字列が必要
/// ・床はFloorLayerである必要あり
/// ・部屋は原点を正確(なるべくオブジェクトの中心になるよう)設計する必要あり
/// 
/// </summary>


public class RandomGenerator : MonoBehaviour
{
    [Header("Required")]
    [SerializeField] GameObject firstRoom;
    [SerializeField] int floorLayer;
    [SerializeField] int willBeFloorLayer;
    [SerializeField] bool debugShowRay = true;
    [SerializeField] string roomJointName = "Joint";
    [SerializeField] float yOffset = 1;

    [Header("Custom")]
    [SerializeField] GenerateFinishMethod howToFinish = GenerateFinishMethod.Depth;
    [SerializeField] int finishCount = 50;
    [SerializeField] float floorTestUnit = .1f;
    [SerializeField] float floorTestRadius = 10;

    public List<GameObject> generatedRooms;
    List<GameObject> roomObjects;
    List<GameObject> registeredDoors;

    bool isFloorDoubled = false;

    void Start()
    {
        roomObjects = new List<GameObject>();
        registeredDoors= new List<GameObject>();
        foreach (Transform child in transform) roomObjects.Add(child.gameObject);

        if (firstRoom == null) firstRoom = Instantiate(roomObjects.GetRandom());
        registeredDoors = registeredDoors.Concat(GetDoors(firstRoom)).ToList();
        StartCoroutine(GenerateRooms());
    }

    IEnumerator GenerateRooms()
    {
        print("Generate Start");
        int depth = 0;
        bool isEnd = false;

        while (!isEnd)
        {
            yield return Generate();
            depth++;
            if (howToFinish == GenerateFinishMethod.Depth) isEnd = depth >= finishCount;
            else isEnd = generatedRooms.Count >= finishCount;
            if(registeredDoors.Count == 0)
            {
                print("No more joints!!");
                yield break;
            }
        }

        print("Generate End");
    }

    IEnumerator Generate()
    {
        GameObject targetDoor = registeredDoors.GetRandom();
        
        GameObject newRoom = Instantiate(roomObjects.GetRandom());
        newRoom.SetActive(true);
        newRoom.transform.eulerAngles = (Vector3.up * 90) * Random.Range(0, 4);
        List<GameObject> newDoors = GetDoors(newRoom);
        GameObject newDoor = newDoors.GetRandom();

        Vector3 offset = targetDoor.transform.position - newDoor.transform.position;
        newRoom.transform.position += offset;

        List<GameObject> newFloors = new List<GameObject>();
        foreach (Transform child in newRoom.transform) if (child.gameObject.layer == floorLayer) newFloors.Add(child.gameObject);
        newFloors.ForEach(g => g.layer = willBeFloorLayer);

        isFloorDoubled = false;
        yield return CheckForDoubledFloor();

        IEnumerator CheckForDoubledFloor()
        {
            //transform.positionから100x100の範囲の大きさに対応
            int size = (int)(floorTestRadius * 2 / floorTestUnit);
            yield return new WaitForFixedUpdate();
            //新しい床のチェック
            Vector3 roomOrigin = new Vector3(newRoom.transform.position.x, yOffset, newRoom.transform.position.z);
            for (int x = 0; x < size; x++)
            {
                for (int z = 0; z < size; z++)
                {
                    Vector3 offset = new Vector3(x - size/2, 0, z - size/2) * floorTestUnit;
                    Ray ray = new Ray(roomOrigin + offset, -Vector3.up);
                    if (debugShowRay) Debug.DrawRay(ray.origin, ray.direction * 2, Color.cyan);
                    if (!Physics.Raycast(ray, 2, 1 << willBeFloorLayer)) continue;
                    if (Physics.Raycast(ray, 2, 1 << floorLayer)) //既にある床のチェック
                    {
                        if (debugShowRay) Debug.DrawRay(ray.origin, ray.direction * 2, Color.red, 1);
                        isFloorDoubled = true;
                        yield break;
                    }
                }
                if(debugShowRay) yield return null;
            }
        }

        if (isFloorDoubled)
        {
            Destroy(newRoom);
            yield break;
        }
        newFloors.ForEach(g => g.layer = floorLayer);

        newDoors.Remove(newDoor);
        registeredDoors = registeredDoors.Concat(newDoors).ToList();
        registeredDoors.Remove(targetDoor);
        newDoor.SetActive(false);
        targetDoor.SetActive(false);

        generatedRooms.Add(newRoom);
    }

    List<GameObject> GetDoors(GameObject roomObject)
    {
        List<GameObject> doorObjects = new List<GameObject>();
        foreach(Transform child in roomObject.transform)
        {
            if(child.name.Contains(roomJointName)) doorObjects.Add(child.gameObject);
        }
        return doorObjects;
    }
}

public enum GenerateFinishMethod
{
    Depth,
    RoomCount,
}

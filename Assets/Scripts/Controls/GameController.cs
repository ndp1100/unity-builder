﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GameController : MonoBehaviour {
    private Game currentGame;
    private MapGenerator generator;
    public int centerNumbers = 100;
    private GameObject[,] objectMap;
    public GameObject grass;
    public GameObject tree;
    public GameObject dirt;
    public GameObject treeGrass;
    public bool stopAstar;
    private float groundHeight = 1;
    private GameObject mapObject;

    void Start()
    {
        currentGame = Game.getInstance();
        generator = new MapGenerator();
    }

    void OnGUI()
    {
        if (GUILayout.Button("Save"))
        {
            SaveLoad.Save();
        }
        if (GUILayout.Button("Load"))
        {
            DeleteMapFromScreen(currentGame.map);
            SaveLoad.Load();
            currentGame = Game.getInstance();
            To3D(currentGame.map);
            InstantiateHumans();
        }
        if (GUILayout.Button("Create Map"))
        {
            DeleteMapFromScreen(currentGame.map);
            MapGenerator.TileObject[,] map = generator.CreateMap(centerNumbers);
            To3D(map);
        }
        if (Game.getInstance().gold != null)
            GUILayout.Label(Game.getInstance().gold.Name + " " + Game.getInstance().gold.CurrentAmount);
        GUILayout.Label("humans " + HumanEntity.HumanCount);
        GUILayout.Label("roaming humans " + HumanEntity.RoamingHumanCount);
        GUILayout.Label("housed humans " + HumanEntity.HousedHumanCount);
    }

    private void DeleteMapFromScreen(MapGenerator.TileObject[,] map) {
        if (map != null && objectMap != null)
        {
            for (int i = 0; i < map.GetLength(0); ++i)
            {
                for (int j = 0; j < map.GetLength(1); ++j)
                {
                    if (map[i, j].Building != null)
                    {
                        Destroy(map[i, j].Building.GameObject);
                    }
                    Destroy(objectMap[i, j]);
                }
            }
            Destroy(mapObject);
        }
    }

    private void To3D(MapGenerator.TileObject[,] map) {

        objectMap = new GameObject[map.GetLength(0), map.GetLength(1)];
        GameObject road = Resources.Load("Prefabs/road", typeof(GameObject)) as GameObject;
        GameObject house = Resources.Load("Prefabs/house", typeof(GameObject)) as GameObject;

        float roadY = 1;
        float houseOffset = roadY / 2;
        float treeOffset = 0;

        groundHeight = grass.GetComponent<Renderer>().bounds.size.y;

 

        Collider houseCollider = null;
        Collider roadCol = null;
        Collider treeCollider = null;

        mapObject = new GameObject("map");

        GameObject floor = new GameObject("floor");
        floor.transform.parent = mapObject.transform;
        for (int i = 0; i < map.GetLength(0); ++i)
        {

            for (int j = 0; j < map.GetLength(1); ++j)
            {

                Vector3 pos = new Vector3(i, 0, j);
                switch (map[i, j].Type)
                {
                    case MapGenerator.Tile.DIRT:
                        objectMap[i, j] = Instantiate(dirt, pos, Quaternion.identity);
                        objectMap[i, j].transform.parent = floor.transform;
                        break;
                    case MapGenerator.Tile.GRASS:
                        objectMap[i, j] = Instantiate(grass, pos, Quaternion.identity);
                        objectMap[i, j].transform.parent = floor.transform;
                        break;
                    case MapGenerator.Tile.TREEGRASS:
                        objectMap[i, j] = Instantiate(treeGrass, pos, Quaternion.identity);
                        objectMap[i, j].transform.parent = floor.transform;
                        map[i, j].Building = new Tree();
                        break;
                    default: break;
                }
                Building b = map[i, j].Building;
                
                if (b != null)
                {
                    switch (b.BuildingType)
                    {
                        case BuildingType.ROAD:
                            GameObject go = Instantiate(road, new Vector3(i, roadY, j), Quaternion.identity);
                            if (roadCol == null)
                            {
                                roadCol = go.GetComponent<Collider>();
                                roadY = roadCol.bounds.size.y / 2 + groundHeight / 2;
                                Debug.Log(roadCol.bounds.size);
                            }
                            go.transform.position = new Vector3(i, roadY, j);

                            if (b.Direction >= Direction.UP)
                                go.transform.Rotate(0, 90, 0);
                            b.GameObject = go;
                            b.Prefab = road;
                            go.transform.parent = mapObject.transform;
                        break;
                        case BuildingType.HOUSE:
                            if (houseCollider == null)
                            {
                                houseCollider = house.GetComponent<Collider>();
                                Renderer r = house.GetComponent<Renderer>();
                                houseOffset = r.bounds.size.y / 2 + groundHeight / 2;
                            }
                            GameObject go2 = Instantiate(house, new Vector3(i, houseOffset, j), Quaternion.identity);
                            go2.tag = b.Tag;
                            b.GameObject = go2;
                            go2.GetComponent<HouseEntity>().House = House.FromBuilding(b);

                            go2.transform.parent = mapObject.transform;

                            b.Prefab = house;
                            b.RotateToDirection();
                        break;
                        case BuildingType.TREE:
                            if (treeCollider == null)
                            {
                                treeCollider = tree.GetComponent<Collider>();
                                CapsuleCollider col = tree.GetComponent<CapsuleCollider>();
                                treeOffset = (col.height) / 2 + groundHeight / 2 - col.center.y;
                            }
                            GameObject treeObject = Instantiate(tree, new Vector3(i, treeOffset, j), Quaternion.identity);
                            b.GameObject = treeObject;
                            treeObject.transform.parent = mapObject.transform;

                            b.Prefab = tree;
                            break;
                        default: break;
                    }
                }
            }
        }
        Game.getInstance().map = map;   
        Game.getInstance().enableMouse = true;
    }

    private void InstantiateHumans() {
        GameObject[] houses = GameObject.FindGameObjectsWithTag("house");
        

        GameObject humanPrefab = Resources.Load("Prefabs/Human", typeof(GameObject)) as GameObject;
        if (Game.getInstance().Humans != null) {
            foreach (Human h in Game.getInstance().Humans) {
                GameObject go = Instantiate(humanPrefab, h.Position, Quaternion.identity);
                go.GetComponent<HumanEntity>().Human = h;
                if (houses != null) {
                    foreach (GameObject houseObj in houses) {
                        HouseEntity house = houseObj.GetComponent<HouseEntity>();
                        if (house != null && house.House.Guid.Equals(h.HouseGuid))
                        {
                            h.Target = house.House;
                            break;
                        }                    
                    }
                }
            }
        }


    }

    private void Update()
    {
        Game.getInstance().stopAstar = stopAstar;
    }
}

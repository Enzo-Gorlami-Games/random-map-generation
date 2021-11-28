using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;


/**
 * This class demonstrates the CaveGenerator on a Tilemap.
 * 
 * Original by: Erel Segal-Halevi
 * Adaptation for 3 tile types: Tal Zichlinsky
 * Since: 2021-11
 */

public class TileMapGenerator: MonoBehaviour {
    [SerializeField] Tilemap tilemap = null;

    [Tooltip("The tile that represents a wall (an impassable block)")]
    [SerializeField] TileBase waterTile = null;

    [Tooltip("The tile that represents a floor (a passable block)")]
    [SerializeField] TileBase swampTile = null;

    [Tooltip("The tile that represents a floor (a passable block)")]
    [SerializeField] TileBase rockTile = null;

    [Tooltip("The percent of water in the initial random map")]
    [Range(0, 1)]
    [SerializeField] float randomWaterPercent = 1 / 3;

    [Tooltip("The percent of swamp in the initial random =map")]
    [Range(0, 1)]
    [SerializeField] float randomSwampPercent = 1 / 3;

    [Tooltip("The percent of rock in the initial random map")]
    [Range(0, 1)]
    [SerializeField] float randomRockPercent = 1 / 3;

    [Tooltip("Length and height of the grid")]
    [SerializeField] int gridSize = 100;

    [Tooltip("How many steps do we want to simulate?")]
    [SerializeField] int simulationSteps = 20;

    [Tooltip("For how long will we pause between each simulation step so we can look at the result?")]
    [SerializeField] float pauseTime = 1f;

    private MapGenerator mapGenerator;

    void Start()  {
        // get the same random numbers each time we run the script
        Random.InitState(100);

        mapGenerator = new MapGenerator(randomWaterPercent, randomSwampPercent, randomRockPercent, gridSize);
        mapGenerator.randomizeMap();
                
        // test that init is working
        GenerateAndDisplayTexture(mapGenerator.getMap());
            
        // start the simulation
        StartCoroutine(SimulateMapPattern());
    }


    // simulate in a coroutine so we can pause and see what's going on
    private IEnumerator SimulateMapPattern()  {

        for (int i = 0; i < simulationSteps; i++)
        {
            yield return new WaitForSeconds(pauseTime);

            //Calculate the new values
            mapGenerator.smoothMap();

            //Generate texture and display it on the plane
            GenerateAndDisplayTexture(mapGenerator.getMap());
        }
    }



    // generate a 3-based tile map, depending on the
    private void GenerateAndDisplayTexture(int[,] data) {
        for (int y = 0; y < gridSize; y++) {
            for (int x = 0; x < gridSize; x++) {
                var position = new Vector3Int(x, y, 0);
                TileBase tile;
                if (data[x, y] == 0) tile = waterTile;
                else if (data[x, y] == 1) tile = swampTile;
                else tile = rockTile;
                tilemap.SetTile(position, tile);
            }
        }
    }
}

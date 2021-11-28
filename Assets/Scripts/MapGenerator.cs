using System;


public class MapGenerator
{
    private const float EPSILON = 0.001f; // for comparing percentage sum to 1

    private const int WATER = 0;
    private const int SWAMP = 1;
    private const int ROCK = 2;

    private const int ADJ_SIZE = 8;

    private float randomWaterPercent;
    private float randomRockPercent;
    private float randomSwampPercent;

    private int gridSize;


    private int[,] bufferOld;
    private int[,] bufferNew;

    Random random;

    public MapGenerator(float waterPercent = 1 / 3, float swampPercent = 1 / 3, float rockPercent = 1 / 3, int size = 20)
    {
        if (Math.Abs(waterPercent + rockPercent + swampPercent - 1) > EPSILON)
        {
            throw new ArgumentException("Tile percentage does not add up to 1");
        }

        this.randomWaterPercent = waterPercent;
        this.randomRockPercent = rockPercent;
        this.randomSwampPercent = swampPercent;
        this.gridSize = size;

        random = new Random();

        bufferOld = new int[gridSize, gridSize];
        bufferNew = new int[gridSize, gridSize];
    }

    public int[,] getMap()
    {
        return bufferOld;
    }

    public void printMap()
    {
        for (int i = 0; i < gridSize; i++)
        {
            Console.Write('[');
            for (int j = 0; j < gridSize; j++)
            {
                Console.Write(bufferOld[i, j].ToString() + ',');
            }
            Console.WriteLine(']');
        }
    }

    public void printInitMap()
    {
        for (int i = 0; i < gridSize; i++)
        {
            Console.Write('[');
            for (int j = 0; j < gridSize; j++)
            {
                Console.Write(bufferOld[i, j].ToString() + ',');
            }
            Console.WriteLine(']');
        }
    }

    public void randomizeMap()
    {
        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                // The borders should always be walls 
                if (i == 0 || i == gridSize - 1 || j == 0 || j == gridSize - 1)
                {
                    bufferOld[i, j] = ROCK;
                    continue;
                }

                double r = random.NextDouble();
                if (0 <= r && r <= randomSwampPercent)
                {
                    bufferOld[i, j] = SWAMP;
                }
                else if (randomSwampPercent < r && r <= randomSwampPercent + randomRockPercent)
                {
                    bufferOld[i, j] = ROCK;
                }
                else bufferOld[i, j] = WATER;
            }
        }
    }

    public void smoothMap()
    {
        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                //Border is always wall
                if (i == 0 || i == gridSize - 1 || j == 0 || j == gridSize - 1)
                {
                    bufferNew[i, j] = ROCK;
                    continue;
                }

                // Uses bufferOld to get the surrounding tiles count
                int surroundingRocks = getSurroundingRockCount(i, j);
                int surroundingSwamps = getSurroundingSwampCount(i, j);
                int surroundingWater = ADJ_SIZE - surroundingRocks - surroundingSwamps;

                // If there is some (3, 3, 2) division
                bool noDominant =
                    (surroundingRocks == 3 && surroundingSwamps == 3)
                    || (surroundingRocks == 3 && surroundingWater == 3)
                    || (surroundingWater == 3 && surroundingSwamps == 3);

                // If there is a (4, 4, 0) division
                bool coDominant =
                    (surroundingRocks == 4 && surroundingSwamps == 4)
                    || (surroundingRocks == 4 && surroundingWater == 4)
                    || (surroundingWater == 4 && surroundingSwamps == 4);

                // if no dominant tile, keep old state
                if (noDominant)
                {
                    bufferNew[i, j] = bufferOld[i, j];
                }

                // if there are co dominant tiles, flip a coin to decide new state
                else if (coDominant)
                {
                    double coin = random.NextDouble();
                    if (surroundingWater == 0)
                        bufferNew[i, j] = coin > 0.5 ? ROCK : SWAMP;
                    else if (surroundingRocks == 0)
                        bufferNew[i, j] = coin > 0.5 ? WATER : SWAMP;
                    else
                        bufferNew[i, j] = coin > 0.5 ? WATER : ROCK;
                }
                else
                {
                    bufferNew[i, j] = getDominantNeighbor(surroundingRocks, surroundingSwamps, surroundingWater);
                }
            }
        }

        //Swap the pointers to the buffers
        (bufferOld, bufferNew) = (bufferNew, bufferOld);
    }

    private int getSurroundingRockCount(int i, int j)
    {
        int rockCounter = 0;
        for (int neighborI = i - 1; neighborI <= i + 1; neighborI++)
        {
            for (int neighborJ = j - 1; neighborJ <= j + 1; neighborJ++)
            {
                //We dont need to care about being outside of the grid because we are never looking at the border
                if (neighborI == i && neighborJ == j)
                { //This is the cell itself and no neighbor!
                    continue;
                }

                //This neighbor is a wall
                if (bufferOld[neighborI, neighborJ] == ROCK)
                {
                    rockCounter += 1;
                }
            }
        }
        return rockCounter;
    }

    private int getSurroundingSwampCount(int i, int j)
    {
        int swampCounter = 0;
        for (int neighborI = i - 1; neighborI <= i + 1; neighborI++)
        {
            for (int neighborJ = j - 1; neighborJ <= j + 1; neighborJ++)
            {
                //We dont need to care about being outside of the grid because we are never looking at the border
                if (neighborI == i && neighborJ == j)
                { //This is the cell itself and no neighbor!
                    continue;
                }

                //This neighbor is a wall
                if (bufferOld[neighborI, neighborJ] == SWAMP)
                {
                    swampCounter += 1;
                }
            }
        }
        return swampCounter;
    }

    private int getDominantNeighbor(int rockCount, int swampCount, int waterCount)
    {
        int[] contenders = { waterCount, swampCount, rockCount };
        int max = -1;
        int max_index = -1;
        for (int i = 0; i < contenders.Length; i++)
        {
            if (contenders[i] > max)
            {
                max = contenders[i];
                max_index = i;
            }
        }
        return max_index;
    }


}
class Program
{
    static void Main(string[] args)
    {
        MapGenerator mg = new MapGenerator(0.5f, 0.3f, 0.2f, 20);
        mg.randomizeMap();
        mg.printMap();
        Console.WriteLine('\n');

        for (int i = 0; i < 4; i++)
        {
            mg.smoothMap();
            mg.printMap();
            Console.WriteLine('\n');
        }
    }
}

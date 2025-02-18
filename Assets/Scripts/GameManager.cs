using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public int Size;
    public BoxCollider2D Panel;
    public GameObject defaultToken;
    public GameObject startToken;
    public GameObject endToken;
    public GameObject optimalPathToken;
    public GameObject obstacleToken;
    //private int[,] GameMatrix; //0 not chosen, 1 player, 2 enemy de momento no hago nada con esto
    private Node[,] NodeMatrix;
    private int startPosX, startPosY;
    private int endPosX, endPosY;
    public List<int[]> optimalPathMatrix;

    void Awake()
    {
        Instance = this;
        //GameMatrix = new int[Size, Size];
        Calculs.CalculateDistances(Panel, Size);
    }

    private void Start()
    {
        /*for(int i = 0; i<Size; i++)
        {
            for (int j = 0; j< Size; j++)
            {
                GameMatrix[i, j] = 0;
            }
        }*/

        startPosX = Random.Range(0, Size);
        startPosY = Random.Range(0, Size);
        do
        {
            endPosX = Random.Range(0, Size);
            endPosY = Random.Range(0, Size);
        } while (endPosX == startPosX || endPosY == startPosY);

        //GameMatrix[startPosx, startPosy] = 2;
        //GameMatrix[startPosx, startPosy] = 1;
        NodeMatrix = new Node[Size, Size];
        CreateNodes();
        DebugMatrix();
        CreateObstacles(10, 20);
        optimalPathMatrix = SuperDuperAlgorithm();
        StartCoroutine(RevealOptimalPath());
    }

    public void CreateNodes()
    {
        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                NodeMatrix[i, j] = new Node(i, j, Calculs.CalculatePoint(i, j));
                NodeMatrix[i, j].Heuristic = Calculs.CalculateHeuristic(NodeMatrix[i, j], endPosX, endPosY);
            }
        }
        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                SetWays(NodeMatrix[i, j], i, j);
            }
        }
    }

    public void DebugMatrix()
    {
        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                if (i == startPosX && j == startPosY)
                {
                    Instantiate(startToken, NodeMatrix[i, j].RealPosition, Quaternion.identity);
                }
                else if (i == endPosX && j == endPosY)
                {
                    Instantiate(endToken, NodeMatrix[i, j].RealPosition, Quaternion.identity);
                }
                else
                {
                    Instantiate(defaultToken, NodeMatrix[i, j].RealPosition, Quaternion.identity);
                }
            }
        }
    }

    public void SetWays(Node node, int x, int y)
    {
        node.WayList = new List<Way>();
        if (x > 0)
        {
            node.WayList.Add(new Way(NodeMatrix[x - 1, y], Calculs.LinearDistance));
            if (y > 0)
            {
                node.WayList.Add(new Way(NodeMatrix[x - 1, y - 1], Calculs.DiagonalDistance));
            }
        }
        if (x < Size - 1)
        {
            node.WayList.Add(new Way(NodeMatrix[x + 1, y], Calculs.LinearDistance));
            if (y > 0)
            {
                node.WayList.Add(new Way(NodeMatrix[x + 1, y - 1], Calculs.DiagonalDistance));
            }
        }
        if (y > 0)
        {
            node.WayList.Add(new Way(NodeMatrix[x, y - 1], Calculs.LinearDistance));
        }
        if (y < Size - 1)
        {
            node.WayList.Add(new Way(NodeMatrix[x, y + 1], Calculs.LinearDistance));
            if (x > 0)
            {
                node.WayList.Add(new Way(NodeMatrix[x - 1, y + 1], Calculs.DiagonalDistance));
            }
            if (x < Size - 1)
            {
                node.WayList.Add(new Way(NodeMatrix[x + 1, y + 1], Calculs.DiagonalDistance));
            }
        }
    }

    public List<int[]> SuperDuperAlgorithm()
    {
        List<Node> openList = new();
        HashSet<Node> closedList = new();
        Dictionary<Node, Node> cameFrom = new();
        Dictionary<Node, float> pathCost = new();

        Node startNode = NodeMatrix[startPosX, startPosY];
        Node endNode = NodeMatrix[endPosX, endPosY];

        // Initialize the path cost of all nodes to infinity except the start node
        foreach (Node node in NodeMatrix)
        {
            pathCost[node] = float.MaxValue;
        }
        pathCost[startNode] = 0;

        openList.Add(startNode);
        pathCost[startNode] = 0;

        while (openList.Count > 0)
        {
            // Get the node with the lowest cost
            Node currentNode = openList[0];
            foreach (Node node in openList)
            {
                float totalCost = pathCost[node] + node.Heuristic;
                if (totalCost < pathCost[currentNode] + currentNode.Heuristic)
                {
                    currentNode = node;
                }
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            // Check if we have reached the end node
            if (currentNode == endNode)
            {
                List<int[]> optimalPath = new();
                while (currentNode != startNode)
                {
                    optimalPath.Add(new int[] { currentNode.PositionX, currentNode.PositionY });
                    currentNode = cameFrom[currentNode];
                }
                optimalPath.Add(new int[] { startNode.PositionX, startNode.PositionY });
                optimalPath.Reverse();
                return optimalPath;
            }

            // Check the neighbors of the current node
            foreach (Way way in currentNode.WayList)
            {
                Node neighbor = way.NodeDestiny;
                if (!closedList.Contains(neighbor))
                {
                    float tentativePathCost = pathCost[currentNode] + way.Cost;

                    // If the neighbor is not in the open list or the new path is better
                    if (tentativePathCost < pathCost[neighbor])
                    {
                        cameFrom[neighbor] = currentNode;
                        pathCost[neighbor] = tentativePathCost;

                        if (!openList.Contains(neighbor))
                        {
                            openList.Add(neighbor);
                        }
                    }
                }
            }
        }
        return new List<int[]>();
    }

    private void CreateObstacles(int minObstaclesAmount, int maxObstaclesAmount)
    {
        int x, y;
        int obstacles = Random.Range(minObstaclesAmount, maxObstaclesAmount);
        for (int i = 0; i < obstacles; i++)
        {
            x = Random.Range(0, Size);
            y = Random.Range(0, Size);

            if (x != startPosX && y != startPosY && x != endPosX && y != endPosY)
            {
                Instantiate(obstacleToken, NodeMatrix[x, y].RealPosition, Quaternion.identity);
                foreach (Way way in NodeMatrix[x, y].WayList)
                {
                    way.Cost = float.MaxValue;
                }
            }
            else
            {
                i--;
            }
        }
    }

    IEnumerator RevealOptimalPath()
    {
        foreach (int[] pos in optimalPathMatrix)
        {
            Instantiate(optimalPathToken, NodeMatrix[pos[0], pos[1]].RealPosition, Quaternion.identity);
            yield return new WaitForSeconds(0.65f);
        }

        Debug.Log("Finished reavealing optimal path!");
    }
}

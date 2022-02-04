using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;
using Pandorai.Tilemaps;

namespace Pandorai.AStarSearchAlgorithm
{
	public static class AStarCreatures
	{
		public static List<Point> GetShortestPath(Tile[,] tileMap, Point startingPoint, Point endPoint, bool ignoreCreatures, bool ignoreCollisionOnTarget, bool eightDirections)
		{
			// open list
			Dictionary<Point, bool> nodesToBeChecked = new Dictionary<Point, bool>();
			// closed list
			Dictionary<Point, bool> checkedNodes = new Dictionary<Point, bool>();

			// contains info about parent of a node
			Dictionary<Point, Point> nodeLinks = new Dictionary<Point, Point>();

			// g and f costs of each node
			Dictionary<Point, int> gCost = new Dictionary<Point, int>();
			Dictionary<Point, int> fCost = new Dictionary<Point, int>();

			// set the starting node as the first node that needs to be checked and set its values
			nodesToBeChecked[startingPoint] = true;
			gCost[startingPoint] = 0;
			fCost[startingPoint] = ManhattanDistance(startingPoint, endPoint);

			// algorithm loop
			while (nodesToBeChecked.Count > 0)
			{
				// find node with lowest FCost
				int bestF = int.MaxValue;
				Point bestNode = new Point();
				foreach (Point nodeInOpen in nodesToBeChecked.Keys)
				{
					int currentFCost = fCost[nodeInOpen];
					if (currentFCost < bestF)
					{
						bestNode = nodeInOpen;
						bestF = currentFCost;
					}
				}

				Point currentNode = bestNode;

				//PrintVector(currentNode, "CURRENT");

				if (ArePointsEqual(currentNode, endPoint))
				{
					// we found the path
					return ConstructPath(currentNode, nodeLinks);
				}

				// we remove the current node from the open list and add it to the closed
				nodesToBeChecked.Remove(currentNode);
				checkedNodes[currentNode] = true;

				// generate north, south, east and west neighbours of current node
				List<Point> neighbouringNodes;
				if (eightDirections)
				{
					neighbouringNodes = new List<Point>
					{
						new Point(currentNode.X - 1, currentNode.Y),
						new Point(currentNode.X + 1, currentNode.Y),
						new Point(currentNode.X, currentNode.Y - 1),
						new Point(currentNode.X, currentNode.Y + 1),
						new Point(currentNode.X - 1, currentNode.Y - 1),
						new Point(currentNode.X + 1, currentNode.Y + 1),
						new Point(currentNode.X - 1, currentNode.Y + 1),
						new Point(currentNode.X + 1, currentNode.Y - 1),
					};
				}
				else
				{
					neighbouringNodes = new List<Point>
					{
						new Point(currentNode.X - 1, currentNode.Y),
						new Point(currentNode.X + 1, currentNode.Y),
						new Point(currentNode.X, currentNode.Y - 1),
						new Point(currentNode.X, currentNode.Y + 1),
					};
				}

				int nIndeX = 0;

				foreach (Point neighbour in neighbouringNodes)
				{
					nIndeX++;

					if(ArePointsEqual(neighbour, endPoint) && ignoreCollisionOnTarget)
					{

					}
					else
					{
						if (neighbour.X < 0 || neighbour.X >= tileMap.GetLength(0) || neighbour.Y < 0 || neighbour.Y >= tileMap.GetLength(1))
						{
							// neighbour out of bounds of the tilemap
							continue;
						}

						if (tileMap[neighbour.X, neighbour.Y].CollisionFlag)
						{
							if ((ignoreCreatures && !tileMap[neighbour.X, neighbour.Y].IgnoreCollisionFlagOnSearch) || !ignoreCreatures)
							{
								continue;
							}
							// if this neighbour isn't passable, skip it
						}
					}

					if (checkedNodes.ContainsKey(neighbour))
					{
						// if this neighbour was previously checked, skip it
						continue;
					}

					int neighbourGCost = gCost[currentNode] + 1;

					if (!nodesToBeChecked.ContainsKey(neighbour))
					{
						// if this node isn't on the open list yet, add it
						nodesToBeChecked[neighbour] = true;
					}
					else if (neighbourGCost >= gCost[neighbour])
					{
						// if it is already in the open list but its GCost is smaller, then skip this neighbour
						// as a shorter path to this node has been already found
						continue;
					}

					nodeLinks[neighbour] = currentNode;
					gCost[neighbour] = neighbourGCost;
					fCost[neighbour] = neighbourGCost + ManhattanDistance(neighbour, endPoint);
				}
			}

			return new List<Point>();
		}

		public static List<Point> ConstructPath(Point current, Dictionary<Point, Point> nodeLinks)
		{
			List<Point> path = new List<Point>();
			while (nodeLinks.ContainsKey(current))
			{
				path.Add(current);
				current = nodeLinks[current];
			}

			path.Reverse();
			return path;
		}

		public static int ManhattanDistance(Point pointA, Point pointB)
		{
			int XDist = Math.Abs(pointB.X - pointA.X);
			int yDist = Math.Abs(pointB.Y - pointA.Y);
			return XDist + yDist;
		}

		public static bool ArePointsEqual(Point vectorA, Point vectorB)
		{
			return vectorA.X == vectorB.X && vectorA.Y == vectorB.Y;
		}
	}
}

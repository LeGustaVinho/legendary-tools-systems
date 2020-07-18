using System;
using System.Collections.Generic;
using LegendaryTools.AI.AStar;
using UnityEngine;

namespace LegendaryTools.Systems.HexGrid
{
    public class HexMapView : MonoBehaviour, IAStar<Hex>
    {
        public WorldHexLine AgentStartEnd;

        public HexMap HexMap;
        public WorldHexLine[] LineWalls;
        private List<Hex> neighborsBuffer = new List<Hex>();
        private HashSet<Hex> Path = new HashSet<Hex>();

        public AStar<Hex> Pathfinding;

        private HashSet<Hex> Walls = new HashSet<Hex>();

        public int MapLocationsAmount => HexMap.Count;

        public Hex[] Neighbors(Hex node)
        {
            neighborsBuffer.Clear();

            foreach (Hex current in HexMap.Neighbors(node))
            {
                if (!Walls.Contains(current))
                {
                    neighborsBuffer.Add(current);
                }
            }

            return neighborsBuffer.ToArray();
        }

        public float Heuristic(Hex nodeA, Hex nodeB)
        {
            return 0;
        }

        // Update is called once per frame
        private void OnDrawGizmos()
        {
            if (HexMap == null)
            {
                HexMap = new HexMap(Layout.WorldPlane.XY, Layout.Flat, new Vector3(10, 10, 10), Vector3.zero);
                HexMap.HexagonalShape(10);
                Pathfinding = new AStar<Hex>(this);
            }

            if (LineWalls != null)
            {
                Walls.Clear();
                for (int i = 0; i < LineWalls.Length; i++)
                {
                    if (LineWalls[i].PointA != null && LineWalls[i].PointB != null)
                    {
                        foreach (Hex cell in HexMap.Line(HexMap.PixelToHex(LineWalls[i].PointA.position),
                            HexMap.PixelToHex(LineWalls[i].PointB.position)))
                        {
                            Walls.Add(cell);
                        }
                    }
                }
            }

            Path.Clear();
            if (AgentStartEnd.PointA != null && AgentStartEnd.PointB != null)
            {
                foreach (Hex cell in Pathfinding.FindPath(HexMap.PixelToHex(AgentStartEnd.PointA.position),
                    HexMap.PixelToHex(AgentStartEnd.PointB.position)))
                {
                    Path.Add(cell);
                }
            }

            foreach (Hex cell in HexMap)
            {
                if (Path.Contains(cell))
                {
                    Gizmos.color = Color.red;
                }
                else if (Walls.Contains(cell))
                {
                    Gizmos.color = Color.black;
                }
                else
                {
                    Gizmos.color = Color.white;
                }

                HexMap.DrawCell(cell);
            }
        }

        [Serializable]
        public struct WorldHexLine
        {
            public Transform PointA;
            public Transform PointB;
        }
    }
}
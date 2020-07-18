using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LegendaryTools.Systems.HexGrid
{
    public class HexMap : ICollection<Hex>
    {
        private readonly Layout layout;
        private readonly HashSet<Hex> map = new HashSet<Hex>();

        public HexMap(Layout.WorldPlane plane, Orientation orientation, Vector3 size, Vector3 origin)
        {
            layout = new Layout(plane, orientation, size, origin);
        }

        public void Add(Hex item)
        {
            map.Add(item);
        }

        public void Clear()
        {
            map.Clear();
        }

        public bool Contains(Hex cell)
        {
            return map.Contains(cell);
        }

        public void CopyTo(Hex[] array, int arrayIndex)
        {
            map.CopyTo(array, arrayIndex);
        }

        public bool Remove(Hex item)
        {
            return map.Remove(item);
        }

        public int Count => map.Count;
        public bool IsReadOnly => false;

        public IEnumerator<Hex> GetEnumerator()
        {
            return map.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Draw()
        {
            foreach (Hex cell in map)
            {
                DrawCell(cell);
            }
        }

        public void DrawCell(Hex cell)
        {
            Vector3[] corners = layout.PolygonCorners(cell);

            for (int i = 0; i < corners.Length; i++)
            {
                Gizmos.DrawLine(corners[i], i != corners.Length - 1 ? corners[i + 1] : corners[0]);
            }
        }

        public Hex[] Neighbors(Hex node)
        {
            List<Hex> neighbors = new List<Hex>(6);

            for (int i = 0; i < 6; i++)
            {
                Hex neighbor = node.Neighbor(i);
                if (map.Contains(neighbor))
                {
                    neighbors.Add(neighbor);
                }
            }

            return neighbors.ToArray();
        }

        public Hex[] Line(Hex a, Hex b)
        {
            return FractionalHex.Line(a, b);
        }

        public Hex[] InRange(Hex center, int range)
        {
            List<Hex> cellsInRange = new List<Hex>();

            Hex current;
            for (int q = -range; q <= range; q++)
            {
                for (int r = Mathf.Max(-range, -q - range); r <= Mathf.Min(range, -q + range); r++)
                {
                    current = new Hex(q, r);
                    if (map.Contains(current))
                    {
                        cellsInRange.Add(current);
                    }
                }
            }

            return cellsInRange.ToArray();
        }

        public Vector3 HexToPixel(Hex h)
        {
            return layout.HexToPixel(h);
        }

        public Hex PixelToHex(Vector3 p)
        {
            return layout.PixelToHex(p);
        }

        public void HexagonalShape(int size)
        {
            map.Clear();
            for (int q = -size; q <= size; q++)
            {
                for (int r = Mathf.Max(-size, -q - size); r <= Mathf.Min(size, -q + size); r++)
                {
                    map.Add(new Hex(q, r, -q - r));
                }
            }
        }

        public void RetangularlShape(int width, int height)
        {
            map.Clear();
            for (int q = 0; q < width; q++)
            {
                int qOff = q >> 1;
                for (int r = -qOff; r < height - qOff; r++)
                {
                    map.Add(new Hex(q, r, -q - r));
                }
            }
        }

        public void ParallelogramShape(int width, int height)
        {
            map.Clear();
            for (int q = 0; q <= width; q++)
            {
                for (int r = 0; r <= height; r++)
                {
                    map.Add(new Hex(q, r, -q - r));
                }
            }
        }

        public void TriangleShape(int size)
        {
            map.Clear();
            for (int q = 0; q <= size; q++)
            {
                for (int r = 0; r <= size - q; r++)
                {
                    map.Add(new Hex(q, r, -q - r));
                }
            }
        }
    }
}
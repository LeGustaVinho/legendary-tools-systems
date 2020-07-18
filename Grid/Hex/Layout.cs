using UnityEngine;

namespace LegendaryTools.Systems.HexGrid
{
    public struct Layout
    {
        public enum WorldPlane
        {
            XY,
            XZ
        }

        public readonly WorldPlane Plane;
        public readonly Orientation Orientation;
        public readonly Vector3 Size;
        public readonly Vector3 Origin;

        public static readonly Orientation Pointy = new Orientation(Mathf.Sqrt(3.0f), Mathf.Sqrt(3.0f) / 2.0f, 0.0f,
            3.0f / 2.0f, Mathf.Sqrt(3.0f) / 3.0f, -1.0f / 3.0f, 0.0f, 2.0f / 3.0f, 0.5f);

        public static readonly Orientation Flat = new Orientation(3.0f / 2.0f, 0.0f, Mathf.Sqrt(3.0f) / 2.0f,
            Mathf.Sqrt(3.0f), 2.0f / 3.0f, 0.0f, -1.0f / 3.0f, Mathf.Sqrt(3.0f) / 3.0f, 0.0f);

        public Layout(WorldPlane plane, Orientation orientation, Vector3 size, Vector3 origin)
        {
            Plane = plane;
            Orientation = orientation;
            Size = size;
            Origin = origin;
        }

        public Vector3 HexToPixel(Hex h)
        {
            float x = Origin.x + (Orientation.F0 * h.Q + Orientation.F1 * h.R) * Size.x;
            float y = Origin.y + (Orientation.F2 * h.Q + Orientation.F3 * h.R) * Size.y;

            return coordForPlane(Plane, x, y);
        }

        public Hex PixelToHex(Vector3 p)
        {
            float x = (p.x - Origin.x) / Size.x;
            float y = (p.y - Origin.y) / Size.y;

            Vector3 pt = coordForPlane(Plane, x, y);

            float q = Orientation.B0 * pt.x + Orientation.B1 * pt.y;
            float r = Orientation.B2 * pt.x + Orientation.B3 * pt.y;

            return new FractionalHex(q, r, -q - r).Round();
        }

        public Vector3 HexCornerOffset(int corner)
        {
            float angle = 2.0f * Mathf.PI * (Orientation.StartAngle - corner) / 6.0f;

            float x = Size.x * Mathf.Cos(angle);
            float y = Size.y * Mathf.Sin(angle);

            return coordForPlane(Plane, x, y);
        }

        public Vector3[] PolygonCorners(Hex h)
        {
            Vector3[] corners = new Vector3[6];
            Vector3 center = HexToPixel(h);
            Vector3 offset;
            float x, y;

            for (int i = 0; i < 6; i++)
            {
                offset = HexCornerOffset(i);
                x = center.x + offset.x;
                y = center.y + offset.y;

                corners[i] = coordForPlane(Plane, x, y);
            }

            return corners;
        }

        private Vector3 coordForPlane(WorldPlane plane, float x, float y)
        {
            switch (plane)
            {
                case WorldPlane.XY: return new Vector3(x, y, 0);
                case WorldPlane.XZ: return new Vector3(x, 0, y);
            }

            return Vector3.zero;
        }
    }
}
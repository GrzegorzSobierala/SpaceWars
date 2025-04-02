namespace Game.Physics
{
    public enum ColliderType
    {
        Box = 0,
        Circle = 1,
        Capsule = 2,
        Polygon = 3,   // closed polygon (PolygonCollider2D)
        Edge = 4,      // open polyline (EdgeCollider2D)
        Composite = 5, // treated as a closed set of edges
        Unsuported = 6,
    }
}

namespace Walhalla
{
    public struct Vector2
    {
        public float X { get; private set; }
        public float Y { get; private set; }

        public Vector2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public static Vector2 operator +(Vector2 v1, Vector2 v2)
        {
            return new Vector2(v1.X + v2.X, v1.Y + v2.Y);
        }

        public static Vector2 operator -(Vector2 v1, Vector2 v2)
        {
            return new Vector2(v1.X - v2.X, v1.Y - v2.Y);
        }

        public static float operator *(Vector2 v1, Vector2 v2)
        {
            return v1.X * v2.X + v1.Y * v2.Y;
        }

        #region Scalar Product
        public static Vector2 operator *(Vector2 v1, float m)
        {
            return new Vector2(v1.X * m, v1.Y * m);
        }

        public static Vector2 operator /(Vector2 v1, float m)
        {
            return new Vector2(v1.X / m, v1.Y / m);
        }
        #endregion

        public static float Distance(Vector2 v1, Vector2 v2)
        {
            return (float)Math.Sqrt(Math.Pow(v1.X - v2.X, 2) + Math.Pow(v1.Y - v2.Y, 2));
        }

        public float Magnitude() => Length();
        public float Length()
        {
            return (float)Math.Sqrt(X * X + Y * Y);
        }

        public Vector2 Normalized()
        {
            return this / Length();
        }

        public override string ToString() => '{' + $"{X}, {Y}" + '}';

        #region Defaults
        public static Vector2 Zero => new Vector2(0, 0);
        public static Vector2 One => new Vector2(1, 1);

        public static Vector2 Down => new Vector2(0, -1);
        public static Vector2 Up => new Vector2(0, 1);

        public static Vector2 Right => new Vector2(1, 0);
        public static Vector2 Left => new Vector2(-1, 0);
        #endregion
    }
}
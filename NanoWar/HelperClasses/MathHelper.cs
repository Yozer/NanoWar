namespace NanoWar.HelperClasses
{
    using System;

    using SFML.System;

    public static class MathHelper
    {
        public static readonly float Epsilon = 0.0001f;

        public static float RadianToDegree(double angle)
        {
            return (float)(angle * (180.0 / Math.PI));
        }

        public static float DegreeToRadian(double angle)
        {
            return (float)(Math.PI * angle / 180.0);
        }

        public static float DistanceBetweenTwoPints(Vector2f start, Vector2f end)
        {
            return (float)Math.Sqrt((start.X - end.X) * (start.X - end.X) + (start.Y - end.Y) * (start.Y - end.Y));
        }

        public static float VectorLength(Vector2f vector)
        {
            return (float)Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y);
        }

        public static Vector2f NormalizeVector(Vector2f vector)
        {
            var length = VectorLength(vector);
            return new Vector2f(vector.X / length, vector.Y / length);
        }

        public static Vector2f RotatePoint(Vector2f origin, Vector2f point, float angle)
        {
            var s = (float)Math.Sin(angle);
            var c = (float)Math.Cos(angle);

            point.X -= origin.X;
            point.Y -= origin.Y;

            var xnew = point.X * c - point.Y * s;
            var ynew = point.X * s + point.Y * c;

            point.X = xnew + origin.X;
            point.Y = ynew + origin.Y;
            return point;
        }

        public static float PolarAngle(Vector2f vector)
        {
            return (float)Math.Atan2(vector.X, vector.Y);
        }
    }
}
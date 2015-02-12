namespace NanoWar.HelperClasses
{
    using System;

    using SFML.System;

    public struct PolarVector
    {
        public float A;

        public float R;

        public PolarVector(float rValue, float aValue)
        {
            R = rValue;
            A = aValue;
        }

        public PolarVector(Vector2i vector)
            : this(new Vector2f(vector.X, vector.Y))
        {
        }

        public PolarVector(Vector2f vector)
        {
            R = MathHelper.VectorLength(vector);
            if (Math.Abs(vector.X) < MathHelper.Epsilon && Math.Abs(vector.Y) < MathHelper.Epsilon)
            {
                A = 0;
            }
            else
            {
                A = MathHelper.PolarAngle(vector);
            }
        }

        public static implicit operator Vector2f(PolarVector vector)
        {
            return new Vector2f(
                vector.R * (float)Math.Cos(MathHelper.DegreeToRadian(vector.A)), 
                vector.R * (float)Math.Sin(MathHelper.DegreeToRadian(vector.A)));
        }

        public static implicit operator Vector2i(PolarVector vector)
        {
            return new Vector2i(
                (int)Math.Round(vector.R * MathHelper.RadianToDegree(Math.Cos(vector.A)), 0), 
                (int)Math.Round(vector.R * MathHelper.RadianToDegree(Math.Sin(vector.A)), 0));
        }
    }
}
namespace Walhalla
{
    public static class Random
    {
        /// <summary>
        /// Returns a random value x<br/>
        /// 0.0f <= x < 1.0f
        /// </summary>
        public static float Value => System.Random.Shared.NextSingle();

        /// <summary>
        /// Returns a value random value between or on one of two values
        /// </summary>
        public static float Range(float minIncluded, float maxIncluded) => Value * (maxIncluded - minIncluded) + minIncluded;

        /// <summary>
        /// Returns a value random value between or on the first one of two values
        /// </summary>
        public static int RangeInt(int minIncluded, int maxExcluded) => (int)Math.Round(Range(minIncluded, maxExcluded - 1));

        /// <summary>
        /// Returns a value random value between 0 and maxIncluded or on 0 
        /// </summary>
        public static int RangeInt(int maxExcluded) => RangeInt(0, maxExcluded);

        /// <summary>
        /// Returns a value random value between or on one of two values
        /// </summary>
        public static int RangeInt2(int minIncluded, int maxIncluded) => RangeInt(minIncluded, maxIncluded + 1);

        /// <summary>
        /// Returns a value random value between or on 0 and maxIncluded 
        /// </summary>
        public static int RangeInt2(int maxIncluded) => RangeInt(0, maxIncluded + 1);
    }
}
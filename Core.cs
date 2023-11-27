using System.Collections.Generic;
using System.Text;
using System;

namespace Walhalla
{
    public static class Core
    {
        #region Bit Functions
        /// <summary>
        /// Returns bit value at certain position in the given number
        /// </summary>
        public static bool GetBitAt(this int number, int bitIndex)
        {
            if (bitIndex < 0 || bitIndex >= 32) // C# Integers have 32 bits
                throw new ArgumentOutOfRangeException(nameof(bitIndex), "Bit index is out of range.");

            return ((number >> bitIndex) & 1) == 1;
        }

        /// <summary>
        /// Assigns bit value at certain position in the number<br/>
        /// Special is only that by referencing the value it gets automatically assigned too
        /// </summary>
        public static int SetBitAt(ref int number, int bitIndex, bool newValue) => number = SetBitAt(number, bitIndex, newValue);

        /// <summary>
        /// Assigns bit value at certain position in the number
        /// </summary>
        public static int SetBitAt(this int number, int bitIndex, bool newValue)
        {
            if (bitIndex < 0 || bitIndex >= 32) // C# Integer have 32 bits
                throw new ArgumentOutOfRangeException(nameof(bitIndex), "Bit index is out of range.");

            // Set bit to 0
            number &= ~(1 << bitIndex);

            // If newValue is true, set bit to 1
            if (newValue) number |= (1 << bitIndex);

            return number;
        }

        /// <summary>
        /// Returns array of bits the number holds
        /// </summary>
        public static bool[] GetBitsArray(this int number)
        {
            int numBits = 32; // Annahme: Ein int hat 32 Bits in C#
            bool[] bits = new bool[numBits];

            for (int i = 0; i < numBits; i++)
            {
                bits[i] = ((number >> i) & 1) == 1;
            }

            System.Array.Reverse(bits); // Umkehrung, um die richtige Reihenfolge zu erhalten
            return bits;
        }
        #endregion

        #region Array Functions (Explain themselves)
        public static bool NotEmpty<T>(this T[] array) => array != null && array.Length > 0;
        public static bool IsEmpty<T>(this T[] array) => !NotEmpty(array);

        public static T RandomElement<T>(this T[] array, T @default = default(T)) => array.NotEmpty() ? array[Random.RangeInt(0, array.Length)] : @default;
        public static T GetClampedElement<T>(this T[] array, int index) => array.IsEmpty() ? default(T) : array[Math.Clamp(index, 0, array.Length - 1)];
        #endregion

        #region List Functions (Explain themselves)
        public static bool NotEmpty<T>(this List<T> list) => list != null && list.Count > 0;
        public static bool IsEmpty<T>(this List<T> list) => !NotEmpty(list);

        public static T RandomElement<T>(this List<T> list, T @default = default(T)) => list.NotEmpty() ? list[Random.RangeInt(0, list.Count)] : @default;
        #endregion

        #region Dictionary Functions (Explain themselves)
        public static bool NotEmpty<T, U>(this Dictionary<T, U> dic) => dic != null && dic.Count > 0;
        public static bool IsEmpty<T, U>(this Dictionary<T, U> dic) => !NotEmpty(dic);
        #endregion

        #region Float Functions (Explain themselves)
        public static float abs(this float f) => Math.Abs(f);

        public static float max(this float f0, float f1) => Math.Max(f0, f1);
        public static float max(this float f0, float f1, float f2) => max(max(f0, f1), f2);
        public static float max(this float f0, float f1, float f2, float f3) => max(max(f0, f1, f2), f3);


        public static float min(this float f0, float f1) => Math.Min(f0, f1);
        public static float min(this float f0, float f1, float f2) => min(min(f0, f1), f2);
        public static float min(this float f0, float f1, float f2, float f3) => min(min(f0, f1, f2), f3);
        #endregion

        #region Integer Functions (Explain themselves)
        public static int abs(this int i) => Math.Abs(i);

        public static int max(this int f0, int f1) => Math.Max(f0, f1);
        public static int max(this int f0, int f1, int f2) => max(max(f0, f1), f2);
        public static int max(this int f0, int f1, int f2, int f3) => max(max(f0, f1, f2), f3);

        public static int min(this int f0, int f1) => Math.Min(f0, f1);
        public static int min(this int f0, int f1, int f2) => min(min(f0, f1), f2);
        public static int min(this int f0, int f1, int f2, int f3) => min(min(f0, f1, f2), f3);
        #endregion

        #region String Functions
        /// <summary>
        /// Trimms the string to directiory name only<br/>
        /// Keeping the dir char just keeps the '/' at the end
        /// </summary>
        public static string TrimToDirectory(this string path, bool keepDirChar = false)
        {
            if (string.IsNullOrEmpty(path) || !(path.Contains('/') || path.Contains('\\')))
                return path;

            char c;
            for (int i = path.Length; i > 0; i--)
            {
                c = path[i - 1];

                if (c.Equals('\\') || c.Equals('/'))
                {
                    return path.Substring(0, keepDirChar ? i : i - 1);
                }
            }

            return path;
        }

        /// <summary>
        /// Iterates through text and replaces the first char instance with given value
        /// </summary>
        public static string ReplaceFirst(this string str, char c, object value)
        {
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i].Equals(c))
                {
                    str = str.Substring(0, i++) + value + str.Substring(i, str.Length - i);
                    break;
                }
            }

            return str;
        }

        /// <summary>
        /// Fills empty strings with char to make it a specific length
        /// </summary>
        public static string Fill(this string str, int targetLength, char fillChar, bool before = true)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(str.Trim());

            for (int i = 0; i < (targetLength - builder.Length); i++)
                if (before) builder.Insert(0, fillChar);
                else builder.Append(fillChar);

            return builder.ToString();
        }

        /// <summary>
        /// Checks if string is null or empty
        /// </summary>
        public static bool IsEmpty(this string? value) => string.IsNullOrEmpty(value);

        /// <summary>
        /// Checks if string is not null nor empty
        /// </summary>
        public static bool NotEmpty(this string? value) => !IsEmpty(value);

        // Kind of splits up a string to only write down the contents between the signals. Nice for a lot of stuff.
        public static string Extract(this string source, char signal, out string[] extracted, bool removeSignal = true) => Extract(source, new List<char>() { signal }, out extracted, removeSignal);
        public static string Extract(this string source, List<char> signals, out string[] extracted, bool removeSignals = true)
        {
            extracted = null;

            if (source.IsEmpty()) return source;

            StringBuilder result = new StringBuilder();

            List<string> _extracted = new List<string>();
            StringBuilder str = new StringBuilder();
            bool active = false;

            for (int c = 0; c < source.Length; c++)
            {
                if (signals.Contains(source[c]))
                {
                    active = !active;

                    if (!active && !removeSignals) str.Append(source[c]);

                    if (str.ToString().NotEmpty())
                        _extracted.Add(str.ToString());
                    str.Clear();

                    if (active && !removeSignals) str.Append(source[c]);
                }
                else
                {
                    if (active) str.Append(source[c]);
                    else result.Append(source[c]);
                }
            }

            if (str.ToString().NotEmpty()) _extracted.Add(str.ToString());

            extracted = _extracted.ToArray();
            return result.ToString();
        }
        #endregion

        #region Enum Functions
        /// <summary>
        /// Returns enum array of given enum type
        /// </summary>
        public static Array Array(this Enum Enum) => Enum.GetValues(Enum.GetType());

        /// <summary>
        /// Returns enum array length of given enum type
        /// </summary>
        public static int Length(this Enum Enum) => Array(Enum).Length;


        /// <summary>
        /// Returns enum array of given enum type
        /// </summary>
        public static Array EnumArray<T>() where T : Enum => Enum.GetValues(typeof(T));

        /// <summary>
        /// Returns enum array length of given enum type
        /// </summary>
        public static int EnumLength<T>() where T : Enum => EnumArray<T>().Length;
        #endregion
    }
}
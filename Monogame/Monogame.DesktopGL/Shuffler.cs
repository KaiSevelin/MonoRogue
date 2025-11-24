using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueTest
{
    public static class Shuffler
    {
        public static T[] Shuffle<T>(T[] array, Random rng)
        {
            for (int i = array.Length - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (array[i], array[j]) = (array[j], array[i]);
            }
            return array;
        }
    }
}

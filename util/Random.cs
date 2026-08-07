
namespace panpan.Util
{

    public struct WeightedValue<T>
    {
        public T value;
        public int weight;
    }
    public class Random
    {
        private static System.Random random = new System.Random();

        private static int SeedToInt(string seed)
        {
            int i = 0;
            foreach(char c in seed)
            {
                i += c;
            }
            return i;
        }

        public static void SetSeed(string seed)
        {
            Random.random = new System.Random((int)SeedToInt(seed));
        }

        public static System.Random Get()
        {
            return random;
        }

        public static T WeightedRandom<T>(WeightedValue<T>[] values)
        {
            int total = 0;
            foreach (var v in values)
                total += v.weight;

            int r = random.Next(total);

            int cumulative = 0;
            foreach (var v in values)
            {
                cumulative += v.weight;
                if (r < cumulative)
                    return v.value;
            }

            throw new Exception("Invalid weights");
        }

        public static float Float()
        {
            return (float)random.NextDouble();
        }

        public static double Double()
        {
            return random.NextDouble();
        }

        public static int Range(int min, int max)
        {
            return random.Next(min, max);
        }

        public static T Choose<T>(params T[] values)
        {
            return values[random.Next(values.Count())];
        }
    }
}
namespace SBD_3
{
    // this is a class with many different hash algorithms, choose wisely
    public static class Hash
    {
        public static int HashCode(this int key, int n)
        {
            return key.Hashing() & ((1 << n) - 1); // n least significant bits mask
        }

        public static int Hashing(this int key)
        {
            return key.Hash32Simple();
        }

        //simplest pseudohash
        private static int Hash32Simple(this int key)
        {
            return key;
        }

        private static int Hash32Shiftmult(this int key)
        {
            int c2 = 0x27d4eb2d; // a prime or an odd constant
            key = (key ^ 61) ^ (key >> 16);
            key = key + (key << 3);
            key = key ^ (key >> 4);
            key = key*c2;
            key = key ^ (key >> 15);
            return key;
        }

        public static int Hash32Random(this int k)
        {
            k *= 357913941;
            k ^= k << 24;
            k += ~357913941;
            k ^= k >> 31;
            k ^= k << 31;
            return k;
        }

        public static int Hash32Uns(this int k)
        {
            var a = (uint) k;
            a = (a + 0x7ed55d16) + (a << 12);
            a = (a ^ 0xc761c23c) ^ (a >> 19);
            a = (a + 0x165667b1) + (a << 5);
            a = (a + 0xd3a2646c) ^ (a << 9);
            a = (a + 0xfd7046c5) + (a << 3);
            a = (a ^ 0xb55a4f09) ^ (a >> 16);
            a &= 0x7FFFFFFF;
            return (int) (a);
        }

        public static int Hash32L(this int x)
        {
            var a = (uint) x;
            a = a ^ (a >> 4);
            a = (a ^ 0xdeadbeef) + (a << 5);
            a = a ^ (a >> 11);
            if (a > int.MaxValue)
                a = a & (~(1 << 31));
            return (int) a;
        }

        public static int Hash32X(this int x)
        {
            var a = (uint) x;
            a = (a + 0x479ab41d) + (a << 8);
            a = (a ^ 0xe4aa10ce) ^ (a >> 5);
            a = (a + 0x9942f0a6) - (a << 14);
            a = (a ^ 0x5aedd67d) ^ (a >> 3);
            a = (a + 0x17bea992) + (a << 7);
            if (a > int.MaxValue)
                a = a & (~(1 << 31));
            return (int) a;
        }
    }
}
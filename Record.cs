using System;
using System.Collections.Generic;
using System.Linq;

namespace SBD_3
{
    public class Record
    {
        public const int RecordSize = 4*sizeof (int);
        private readonly double _sum;

        private int _a;
        private int _b;
        private int _c;
        private int _key;


        public Record(int key = 0, int a = 0, int b = 0, int c = 0)
        {
            _a = a;
            _b = b;
            _c = c;
            _key = key;
            try
            {
                _sum = RootsSum();
            }
            catch (NegativeDeltaException)
            {
            }
        }

        public Record(IEnumerable<int> ieints)
        {
            int[] a = ieints.ToArray();
            try
            {
                _a = a.ElementAt(1);
                _b = a.ElementAt(2);
                _c = a.ElementAt(3);
                _key = a.ElementAt(0);
                try
                {
                    _sum = RootsSum();
                }
                catch (NegativeDeltaException)
                {
                }
            }
            catch (IndexOutOfRangeException)
            {
            }
        }

        public int A
        {
            get { return _a; }
            set { _a = value; }
        }

        public int B
        {
            get { return _b; }
            set { _b = value; }
        }

        public int C
        {
            get { return _c; }
            set { _c = value; }
        }

        public double RSum
        {
            get { return _sum; }
        }

        public int Key
        {
            get { return _key; }
            set { _key = value; }
        }

        public bool IsEmpty
        {
            get { return _key == int.MaxValue; }
        }

        public bool IsDeleted
        {
            get { return _key == int.MinValue; }
        }

        public static Record EmptyRecord()
        {
            var rec = new Record {_key = int.MaxValue};
            return rec;
        }

        public static IEnumerable<Record> EmptyArray(int count)
        {
            var arr = new Record[count];
            for (int i = 0; i < arr.Length; i++)
                arr[i] = EmptyRecord();
            return arr;
        }


        public int CompareTo(Record other)
        {
            return _key.CompareTo(other._key);
        }

        protected bool Equals(Record other)
        {
            return _sum.Equals(other._sum);
        }

        public override int GetHashCode()
        {
            return _sum.GetHashCode();
        }

        public static bool operator ==(Record left, Record right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Record left, Record right)
        {
            return !Equals(left, right);
        }

        public static bool operator >(Record lhs, Record rhs)
        {
            return lhs._key > rhs._key;
        }

        public static bool operator >=(Record lhs, Record rhs)
        {
            return lhs._key >= rhs._key;
        }

        public static bool operator <(Record lhs, Record rhs)
        {
            return lhs._key < rhs._key;
        }

        public static bool operator <=(Record lhs, Record rhs)
        {
            return lhs._key <= rhs._key;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Record) obj);
        }

        public override string ToString()
        {
            try
            {
                return string.Format("[{5}] {0} => {1} {2} {3} {4}", _key, _a, _b, _c, RootsSum(),
                    Convert.ToString(_key.Hashing(), 2).PadLeft(32, '0'));
            }
            catch (NegativeDeltaException)
            {
                return string.Format("[{4}] {0} => {1} {2} {3} NaN", _key, _a, _b, _c,
                    Convert.ToString(_key.Hashing(), 2).PadLeft(32, '0'));
            }
            catch (NullReferenceException)
            {
                return string.Format("[{4}] {0} => {1} {2} {3} NaN", _key, _a, _b, _c,
                    Convert.ToString(_key.Hashing(), 2).PadLeft(32, '0'));
            }
        }

        /// <summary>
        ///     Method couting roots sum of equation
        /// </summary>
        /// <returns>sum of roots</returns>
        private double RootsSum()
        {
            double Δ = _b*_b - 4*_a*_c;
            if (Δ < 0)
            {
                throw new NegativeDeltaException("delta is negative");
            }
            if (Math.Abs(_a) < 1e-14)
                return -_c/(double) _b;
            return -_b/(double) _a;
        }

        public IEnumerable<int> AsInts()
        {
            return new[] {_key, _a, _b, _c};
        }

        public IEnumerable<byte> AsBytes()
        {
            IEnumerable<byte> b = BitConverter.GetBytes(_key);
            b = b.Concat(BitConverter.GetBytes(_a));
            b = b.Concat(BitConverter.GetBytes(_b));
            b = b.Concat(BitConverter.GetBytes(_c));
            return b;
        }
    }
}
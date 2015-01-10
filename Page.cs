using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SBD_3
{
    public class Page : IEnumerable<Record>
    {
        public static int PageSize = 100;
        private readonly List<Record> Records;

        public int Count;
        public int Depth;
        public long PageNumber;

        public Page()
        {
            Depth = 0;
            PageNumber = -1;
            Records = Enumerable.Repeat(Record.EmptyRecord(), PageSize).ToList();
            Count = 0;
        }

        public Page(long number, int depth = 0)
        {
            Depth = depth;
            PageNumber = number;
            Records = Enumerable.Repeat(Record.EmptyRecord(), PageSize).ToList();
            Count = 0;
        }

        public int FirstKey
        {
            get { return Count > 0 ? this[0].Key : -1; }
        }

        public static int PageSizeInBytes
        {
            get { return Record.RecordSize*PageSize + 4 + 4; }
        }

        public bool IsFull
        {
            get { return Count >= PageSize; }
        }

        public Record this[int index]
        {
            get { return Records[index]; }
            set { Records[index] = value; }
        }


        public IEnumerator<Record> GetEnumerator()
        {
            for (int index = 0; index < PageSize; index++)
            {
                yield return Records[index];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int IndexOf(int key)
        {
            foreach (Record record in Records.Where(record => record.Key == key))
            {
                return Records.IndexOf(record);
            }
            throw new RecordNotFoundException();
        }

        public static long PageNumberFromAddress(long address)
        {
            return address/PageSizeInBytes;
        }

        public static long PageAddressFromNumber(long number)
        {
            return number*PageSizeInBytes;
        }

        /// <summary>
        ///     Method for adding new record to page. Returns true if successful and false if not.
        /// </summary>
        /// <param name="r">record to add</param>
        /// <returns>true if succeded, false otherwise</returns>
        public bool Add(Record r)
        {
            if (IsFull) return false;
            Records[Count++] = r;
            Sort();
            return true;
        }

        public void Sort()
        {
            Records.Sort((x, y) => x.Key.CompareTo(y.Key));
        }
    }

    public abstract class PageOperator : IDisposable
    {
        protected readonly int PageSize;
        protected readonly string Path;
        protected readonly bool PerformCount = true;
        protected BufferedStream _stream;

        protected PageOperator()
        {
            Path = "main";
            PageSize = 100;
            _stream =
                new BufferedStream(
                    new FileStream(Path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite),
                    PageSizeInBytes);
        }

        protected PageOperator(string path, int size, bool count = true)
        {
            Path = path;
            PageSize = size;
            _stream =
                new BufferedStream(
                    new FileStream(Path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite),
                    PageSizeInBytes);
            PerformCount = count;
        }

        protected Page LastPage { get; set; }

        protected int PageSizeInBytes
        {
            get { return Record.RecordSize*PageSize + 4 + 4; }
        }

        public void Dispose()
        {
            _stream.Dispose();
        }
    }

    public class PageReader : PageOperator
    {
        public PageReader(string path, int size, bool count = true)
            : base(path, size, count)
        {
        }

        public Page ReadPage(long number)
        {
            long position = Page.PageAddressFromNumber(number);
            //if (LastPage != null && number == LastPage.PageNumber)
            //    return LastPage;
            _stream.Position = position;

            var buffer = new byte[PageSizeInBytes];
            int bytesRead = _stream.Read(buffer, 0, PageSizeInBytes);
            if (bytesRead < PageSizeInBytes)
                return null;
            //throw new PageReadException(string.Format("Couldn't read page {0} from position {1}",
            //    Page.PageNumberFromAddress(position), position));

            Page newPage = PageFromBytes(buffer);
            newPage.PageNumber = number;
            LastPage = newPage;
            if (PerformCount)
                Program.Reads++;
            return newPage;
        }

        public Page ReadNextPage()
        {
            long position = _stream.Position;
            var buffer = new byte[PageSizeInBytes];
            int bytesRead = _stream.Read(buffer, 0, PageSizeInBytes);
            if (bytesRead < PageSizeInBytes)
                return null;

            Page newPage = PageFromBytes(buffer);
            newPage.PageNumber = Page.PageNumberFromAddress(position);
            LastPage = newPage;
            if (PerformCount)
                Program.Reads++;
            return newPage;
        }

        public Record GetRecord(long number)
        {
            long page = number/PageSize;
            var offset = (int) (number%PageSize);
            if (LastPage != null && page == LastPage.PageNumber)
                return LastPage[offset];
            try
            {
                Page newPage = ReadPage(page);
                return newPage[offset];
            }
            catch (PageReadException ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        private Page PageFromBytes(byte[] arr)
        {
            var page = new Page {Depth = BitConverter.ToInt32(arr, 4)};

            for (int i = 8; i < arr.Length; i += Record.RecordSize)
            {
                int key = BitConverter.ToInt32(arr, i);
                int a = BitConverter.ToInt32(arr, i + sizeof (int));
                int b = BitConverter.ToInt32(arr, i + sizeof (int)*2);
                int c = BitConverter.ToInt32(arr, i + sizeof (int)*3);
                page.Add(new Record(key, a, b, c));
            }
            page.Count = BitConverter.ToInt32(arr, 0);
            return page;
        }
    }

    public class PageWriter : PageOperator
    {
        public PageWriter(string path, int size, bool count = true)
            : base(path, size, count)
        {
        }

        public void WritePage(Page p)
        {
            LastPage = p;
            byte[] buffer = BytesFromPage(p).ToArray();
            _stream.Position = Page.PageAddressFromNumber(p.PageNumber);
            _stream.Write(buffer, 0, PageSizeInBytes);
            _stream.Flush();
            if (PerformCount)
                Program.Writes++;
        }

        private IEnumerable<byte> BytesFromPage(Page p)
        {
            IEnumerable<byte> result = new Byte[0];
            result = result.Concat(BitConverter.GetBytes(p.Count));
            result = result.Concat(BitConverter.GetBytes(p.Depth));
            foreach (Record record in p)
                result = result.Concat(record.AsBytes());
            return result;
        }

        public long AppendEmptyPage() // returns number of newly allocated page
        {
            var newPage = new Page();
            long lastPos = _stream.Position;
            _stream.Position = _stream.Length;
            newPage.PageNumber = Page.PageNumberFromAddress(_stream.Position);
            byte[] buffer = BytesFromPage(newPage).ToArray();
            _stream.Write(buffer, 0, PageSizeInBytes);
            _stream.Flush();
            if (PerformCount)
                Program.Writes++;
            _stream.Position = lastPos;
            return newPage.PageNumber;
        }
    }
}
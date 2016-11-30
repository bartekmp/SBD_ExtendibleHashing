using System;
using System.Linq;
using Dir = System.Collections.Generic.List<System.Tuple<int, long>>; // alias

namespace SBD_3
{
    public class Directory : IDisposable
    {
        private readonly string _mainPath;
        private readonly PageReader _pageReader;
        private readonly int _pageSize; // size of single page in amount of records
        private readonly PageWriter _pageWriter;
        private int _depth;
        private Dir _pageDirectory;

        public Directory(string mainPath, int pageSize)
        {
            _mainPath = mainPath;
            _pageSize = pageSize;
            Page.PageSize = _pageSize;
            _depth = 0;
            _pageReader = new PageReader(_mainPath, _pageSize);
            _pageWriter = new PageWriter(_mainPath, _pageSize);
            _pageDirectory = new Dir(1) { new Tuple<int, long>(-1, 0) };
            Initialize();
        }

        public void Dispose()
        {
            _pageReader.Dispose();
            _pageWriter.Dispose();
            _pageDirectory = null;
        }

        /// <summary>
        ///     Initializes structure with one, initial page
        /// </summary>
        private void Initialize()
        {
            var firstPage = new Page(0);
            _pageWriter.WritePage(firstPage);
        }

        /// <summary>
        ///     Adds a record to the main file
        /// </summary>
        /// <param name="r">record to add</param>
        public void Add(Record r)
        {
            try
            {
                Get(r.Key); // try to get given record
                Console.WriteLine("Record already added!");
            }
            catch (RecordNotFoundException) // when there's no such record, add it
            {
                while (true) // retry adding, when splitting occurred
                {
                    Page sourcePage = _depth == 0 ? _pageReader.ReadPage(0) : GetPage(r.Key);
                    // if dir has depth == 0, read just first page
                    if (sourcePage == null)
                        return;

                    //try to add record on gotten page
                    if (sourcePage.Add(r))
                    {
                        _pageWriter.WritePage(sourcePage);
                    }
                    else
                    {
                        // if page is overflown and its depth is equal to directory depth, extend the directory
                        bool extensionNeeded = _depth <= sourcePage.Depth;
                        Dir newDir = sourcePage.Depth >= _depth
                            ? PrepareDirectory(ExtendDirectory())
                            : _pageDirectory;

                        // allocate two new pages for splitting sourcePage
                        Page first = new Page(sourcePage.PageNumber, sourcePage.Depth + 1),
                            second = new Page(_pageWriter.AppendEmptyPage(), sourcePage.Depth + 1);

                        // redistribute records according to most significant bit of hashed key
                        foreach (Record record in sourcePage)
                        {
                            int hash = record.Key.HashCode(sourcePage.Depth + 1);
                            int where = ((hash >> sourcePage.Depth) & 1);
                            if (where == 0)
                            {
                                first.Add(record);
                            }
                            else
                            {
                                second.Add(record);
                            }
                        }

                        if (extensionNeeded)
                            _depth++;

                        // register pages in directory
                        int firstIndex = first.FirstKey.HashCode(_depth);
                        newDir[firstIndex] = new Tuple<int, long>(firstIndex, first.PageNumber);
                        int secondIndex = second.FirstKey.HashCode(_depth);
                        newDir[secondIndex] = new Tuple<int, long>(secondIndex, second.PageNumber);

                        // write them all!
                        _pageWriter.WritePage(first);
                        _pageWriter.WritePage(second);

                        _pageDirectory = newDir;
                        continue;
                    }
                    break;
                }
            }
        }

        /// <summary>
        ///     Updates selected record
        /// </summary>
        /// <param name="r">record's key</param>
        public void Update(Record r)
        {
            int key = r.Key.HashCode(_depth);
            Page page = GetPage(key);
            int index = page.IndexOf(r.Key);
            page[index] = r;
            _pageWriter.WritePage(page);
        }

        /// <summary>
        ///     Removes selected record
        /// </summary>
        /// <param name="key">record's key</param>
        public void Remove(int key)
        {
            int hash = key.HashCode(_depth);
            Page page = GetPage(hash);
            int index = page.IndexOf(key);
            page[index].Key = int.MaxValue;
            page.Sort();
            page.Count--;
            _pageWriter.WritePage(page);
        }

        /// <summary>
        ///     Gets record by given key
        /// </summary>
        /// <param name="key">record's key</param>
        /// <returns>wanted record</returns>
        public Record Get(int key)
        {
            int hash = key.HashCode(_depth);
            Page page = GetPage(hash);
            return page[page.IndexOf(key)];
        }

        /// <summary>
        ///     Prints whole directory
        /// </summary>
        public void PrintDirectory()
        {
            Console.WriteLine("--------------");
            Console.WriteLine("#D{0}#", _depth);
            foreach (var tuple in _pageDirectory)
            {
                Console.WriteLine("{0} | {1}",
                    Convert.ToString(tuple.Item1 == -1 ? 0 : tuple.Item1, 2).PadLeft(_depth, '0'),
                    tuple.Item2 != -1 ? tuple.Item2.ToString() : "[-]");
            }
        }

        /// <summary>
        ///     Prints whole main file
        /// </summary>
        public void PrintFile()
        {
            Console.WriteLine("--------------");
            var reader = new PageReader(_mainPath, _pageSize, false);
            Page page;
            while ((page = reader.ReadNextPage()) != null)
            {
                Console.WriteLine("#P:{0}, d:{1}, k:{2}#", page.PageNumber, page.Depth,
                    Convert.ToString(page.FirstKey.HashCode(page.Depth), 2).PadLeft(page.Depth, '0'));
                foreach (Record rec in page.Where(rec => !rec.IsEmpty))
                {
                    Console.WriteLine(rec);
                }
            }
        }

        /// <summary>
        ///     Gets selected page from main file
        /// </summary>
        /// <param name="key">pseudokey</param>
        /// <returns>Gotten page</returns>
        private Page GetPage(int key)
        {
            try
            {
                long page = FindPageByKey(key);
                return _pageReader.ReadPage(page);
            }
            catch (NoPageFoundException)
            {
                return null;
            }
        }

        /// <summary>
        ///     Tries to locate page in file
        /// </summary>
        /// <param name="key">record's key</param>
        /// <returns>page number</returns>
        private long FindPageByKey(int key)
        {
            int hashedKey = key.HashCode(_depth);
            long? page;
            page = _depth == 0 ? 0 : _pageDirectory.Where(k => k.Item1 == hashedKey).Select(k => k.Item2).First();
            if (page == null || page == -1)
                throw new NoPageFoundException();
            return page.Value;
        }

        /// <summary>
        ///     Extends directory by 1 bit (doubles it)
        /// </summary>
        /// <returns>new, extended directory</returns>
        private Dir ExtendDirectory()
        {
            int newDepth = _depth + 1;
            var count = (int)Math.Pow(2, newDepth);
            Dir newDirectory = Enumerable.Repeat(new Tuple<int, long>(-1, -1), count).ToList();
            for (int i = 0; i < count; i++)
            {
                newDirectory[i] = new Tuple<int, long>(i, -1);
            }
            return newDirectory;
        }

        /// <summary>
        /// Prepares new directory with reassigning pointers
        /// </summary>
        /// <param name="newDir">new, extended directory</param>
        /// <returns>prepared directory</returns>
        private Dir PrepareDirectory(Dir newDir)
        {
            long pageNumber = 0L;
            Page page;
            while ((page = _pageReader.ReadPage(pageNumber)) != null) // read file page-by-page
            {
                int localKey = page.FirstKey != -1 ? page.FirstKey.HashCode(page.Depth) : -1;
                if (localKey == -1)
                {
                    pageNumber++;
                    continue;
                }
                for (int i = 0; i < newDir.Count; i++)
                {
                    // extract page.Depth most significant bits from iterator i
                    int mask = _depth <= 0 ? 0 : ((1 << page.Depth) - 1) & i;
                    if (mask == localKey) // save it in directory, if page's local key matches masked directory index
                    {
                        newDir[i] = new Tuple<int, long>(i, page.PageNumber);
                    }
                }
                pageNumber++;
            }
            return newDir;
        }
    }
}
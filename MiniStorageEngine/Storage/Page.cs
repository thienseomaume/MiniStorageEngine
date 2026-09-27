using System;
using System.Collections.Generic;
using System.Text;

namespace MiniStorageEngine.Storage
{
    public class Page
    {
        ushort _pageId;
        ushort _pageSize;
        ushort _startFreeSpaceOffset;
        ushort _endFreeSpaceOffset;
        List<(ushort offset, ushort size)> _slots;
        Byte[] _records;

    }
}

using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Text;

namespace MiniStorageEngine.Storage
{

    internal class Slot
    {
        public ushort Offset {  get; set; }
        public ushort Size { get; set; }
        public byte Status { get; set; }
        public Slot() { }
        public Slot(ushort offset, ushort size, byte Status)
        {
            this.Offset = offset;
            this.Size = size;
            this.Status = Status;
        }
        public byte[] GetBytes()
        {
            Byte[] bytes = new Byte[5];
            Encoding.UTF8.GetBytes(Offset)
        }
    }
    public class Page
    {
        public ushort _pageId { get; }
        private ushort _startFreeSpaceOffset;
        private ushort _endFreeSpaceOffset;
        private List<Slot> _slots;//5 bytes
        private Byte[] _pageData;
        public Page(Byte[] pageData)
        {
            _pageData = pageData;
            _pageId = BinaryPrimitives.ReadUInt16LittleEndian(pageData.AsSpan(0, 2));
            _startFreeSpaceOffset = BinaryPrimitives.ReadUInt16LittleEndian(pageData.AsSpan(2, 2));
            _endFreeSpaceOffset = BinaryPrimitives.ReadUInt16LittleEndian(pageData.AsSpan(4, 2));
            int slotsPointer = 6;
            _slots = new List<Slot>();
            while(slotsPointer < _startFreeSpaceOffset)
            {
                ushort offset = BinaryPrimitives.ReadUInt16LittleEndian(pageData.AsSpan(slotsPointer, 2));
                ushort size = BinaryPrimitives.ReadUInt16LittleEndian(pageData.AsSpan(slotsPointer + 2, 2));
                byte status = pageData[slotsPointer + 4];
                _slots.Add(new Slot(offset,size,status));
            }
        }
        public string GetRecord(ushort slotIndex)
        {
            if(slotIndex >= _slots.Count())
            {
                throw new IndexOutOfRangeException($"Slot index {slotIndex} is out of range. Total slots: {_slots.Count()}");
            }
            if (_slots[slotIndex].Status == 0)
            {
                throw new InvalidOperationException($"Slot index {slotIndex} is marked as deleted.");
            }
            return Encoding.UTF8.GetString(_pageData.AsSpan(_slots[slotIndex].Offset, _slots[slotIndex].Size));
        }
        public ushort InsertRecord(string record)
        {
            byte[] recordByte = Encoding.UTF8.GetBytes(record);
            ushort recordSize = (ushort)recordByte.Length;
            ushort freeSpace = (ushort)(_endFreeSpaceOffset - _startFreeSpaceOffset);
            if(freeSpace < (5 + recordSize))
            {
                throw new OutOfMemoryException($"Page {_pageId} is not enough space to insert");
            }
            _endFreeSpaceOffset -= recordSize;

            
            ushort recordOffset = (ushort)(_endFreeSpaceOffset + 1);
            byte recordStatus = 1;
            _slots.Add(new Slot(recordOffset, recordSize, recordStatus));
            
            _startFreeSpaceOffset += 5;
            recordByte.CopyTo(_pageData, recordOffset);
            return (ushort)(_slots.Count -1);
        }
        public void RemoveRecord(short slotIndex)
        {
            if(slotIndex >= _slots.Count)
            {
                throw new IndexOutOfRangeException($"Slot index {slotIndex} is out of range. Total slots: {_slots.Count()}");
            }
            _slots[slotIndex].Status = 0;
        }
        public byte[] PageToData()
        {

        }
    }
}

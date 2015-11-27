//
// Code copied from More.dll
//

using System;
using System.Text;

public static class Extensions
{
    //
    // Number Parsing
    //
    // Returns the new offset of all the digits parsed.  Returns 0 to indicate overflow or invalid number.
    public static UInt32 TryParseUInt32(Byte[] array, UInt32 offset, UInt32 limit, out UInt32 value)
    {
        if (offset >= limit) // Invalid number
        {
            value = 0;
            return 0;
        }
        UInt32 result;
        {
            Byte c = array[offset];
            if (c > '9' || c < '0')
            {
                value = 0; // Invalid number
                return 0;
            }
            result = (uint)(c - '0');
        }
        while (true)
        {
            offset++;
            if (offset >= limit)
                break;
            var c = array[offset];
            if (c > '9' || c < '0')
                break;

            UInt32 newResult = result * 10 + c - '0';
            if (newResult < result)
            {
                value = 0;
                return 0; // Overflow
            }
            result = newResult;
        }

        value = result;
        return offset;
    }
    // Returns the new offset of all the digits parsed.  Returns 0 to indicate overflow.
    public static UInt32 TryParseInt32(Byte[] array, UInt32 offset, UInt32 limit, out Int32 value)
    {
        if (offset >= limit)
        {
            value = 0;
            return 0;
        }
        Boolean negative;
        {
            var c = array[offset];
            if (c == '-')
            {
                negative = true;
                offset++;
            }
            else
            {
                negative = false;
            }
        }
        if (offset >= limit) // Invalid number
        {
            value = 0;
            return 0;
        }
        UInt32 result;
        {
            Byte c = array[offset];
            if (c > '9' || c < '0')
            {
                value = 0; // Invalid number
                return 0;
            }
            result = (uint)(c - '0');
        }
        while (true)
        {
            offset++;
            if (offset >= limit)
                break;
            var c = array[offset];
            if (c > '9' || c < '0')
                break;

            UInt32 newResult = result * 10 + c - '0';
            if (newResult < result)
            {
                value = 0;
                return 0; // Overflow
            }
            result = newResult;
        }

        if (negative)
        {
            if (result > ((UInt32)Int32.MaxValue) + 1)
            {
                value = 0;
                return 0; // Overflow
            }
            value = -(int)result;
        }
        else
        {
            if (result > (UInt32)Int32.MaxValue)
            {
                value = 0;
                return 0; // Overflow
            }
            value = (int)result;
        }
        value = negative ? -(Int32)result : (Int32)result;
        return offset;
    }
    public static UInt32 ParseByte(Byte[] array, UInt32 offset, UInt32 limit, out Byte value)
    {
        UInt32 uint32;
        var newOffset = TryParseUInt32(array, offset, limit, out uint32);
        if (newOffset == 0 || uint32 > (UInt32)Byte.MaxValue)
            throw new OverflowException(String.Format("Overflow while parsing '{0}' as a Byte",
                Encoding.ASCII.GetString(array, (int)offset, (newOffset == 0) ? (int)ConsumeNum(array, offset, limit) : (int)newOffset)));
        value = (Byte)uint32;
        return newOffset;
    }
    public static UInt32 ParseUInt16(Byte[] array, UInt32 offset, UInt32 limit, out UInt16 value)
    {
        UInt32 uint32;
        var newOffset = TryParseUInt32(array, offset, limit, out uint32);
        if (newOffset == 0 || uint32 > (UInt32)UInt16.MaxValue)
            throw new OverflowException(String.Format("Overflow while parsing '{0}' as a UInt16",
                Encoding.ASCII.GetString(array, (int)offset, (newOffset == 0) ? (int)ConsumeNum(array, offset, limit) : (int)newOffset)));
        value = (UInt16)uint32;
        return newOffset;
    }
    public static UInt32 ParseUInt32(Byte[] array, UInt32 offset, UInt32 limit, out UInt32 value)
    {
        var newOffset = TryParseUInt32(array, offset, limit, out value);
        if (newOffset == 0)
            throw new OverflowException(String.Format("Overflow while parsing '{0}' as a UInt32",
                Encoding.ASCII.GetString(array, (int)offset, (newOffset == 0) ? (int)ConsumeNum(array, offset, limit) : (int)newOffset)));
        return newOffset;
    }
    public static UInt32 ParseInt32(Byte[] array, UInt32 offset, UInt32 limit, out Int32 value)
    {
        var newOffset = TryParseInt32(array, offset, limit, out value);
        if (newOffset == 0)
            throw new OverflowException(String.Format("Overflow while parsing '{0}' as a Int32",
                Encoding.ASCII.GetString(array, (int)offset, (newOffset == 0) ? (int)ConsumeNum(array, offset, limit) : (int)newOffset)));
        return newOffset;
    }
    static UInt32 ConsumeNum(Byte[] array, UInt32 offset, UInt32 limit)
    {
        if (offset >= limit)
            return offset;
        if (array[offset] == '-')
            offset++;
        while (true)
        {
            if (offset >= limit)
                return offset;
            var c = array[offset];
            if (c < '0' || c > '9')
                return offset;
            offset++;
        }
    }
}
using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;


public struct OutputBuffer
{
    public readonly Byte[] ptr;
    public UInt32 length;
    public OutputBuffer(Byte[] ptr)
    {
        this.ptr = ptr;
        this.length = 0;
    }
}

//
// Note: not thread safe
//
public abstract class ThreadUnsafeOutput
{
    protected OutputBuffer buffer;
    public ThreadUnsafeOutput()
        : this(1024 * 4)
    {
    }
    public ThreadUnsafeOutput(UInt32 bufferLength)
    {
        this.buffer = new OutputBuffer(new Byte[bufferLength]);
    }

    protected abstract void Write(Byte[] buffer, UInt32 offset, UInt32 length);
    public abstract void Flush();

    public void Println()
    {
        UInt32 bufferAvailable = (uint)buffer.ptr.Length - buffer.length;
        if (bufferAvailable < 2)
        {
            Flush();
        }
        buffer.ptr[buffer.length    ] = (Byte)'\r';
        buffer.ptr[buffer.length + 1] = (Byte)'\n';
        buffer.length += 2;
        Flush();
    }
    public void Print(String msg)
    {
        Print(msg, 0, (uint)msg.Length);
    }
    public void Println(String msg)
    {
        Print(msg, 0, (uint)msg.Length);
        Println();
    }
    public void Println(String msg, UInt32 offset, UInt32 length)
    {
        Print(msg, offset, length);
        Println();
    }
    public void Print(String msg, UInt32 offset, UInt32 length)
    {
        UInt32 bufferAvailable = (uint)buffer.ptr.Length - buffer.length;
        if (length <= bufferAvailable)
        {
            Encoding.ASCII.GetBytes(msg, (int)offset, (int)length, buffer.ptr, (int)buffer.length);
            buffer.length += length;
        }
        else
        {
            Encoding.ASCII.GetBytes(msg, (int)offset, (int)bufferAvailable, buffer.ptr, (int)buffer.length);
            Flush();

            offset += bufferAvailable;
            length -= bufferAvailable;
            while (length >= buffer.ptr.Length)
            {
                Encoding.ASCII.GetBytes(msg, (int)offset, buffer.ptr.Length, buffer.ptr, 0);
                buffer.length = (uint)buffer.ptr.Length;
                Flush();
                offset += (uint)buffer.ptr.Length;
                length -= (uint)buffer.ptr.Length;
            }

            if (length > 0)
            {
                Encoding.ASCII.GetBytes(msg, (int)offset, (int)length, buffer.ptr, 0);
                buffer.length += length;
            }
        }
    }
    public void Print(LimString limString)
    {
        Print(limString.buffer, limString.offset, limString.limit - limString.offset);
    }
    public void Print(Byte[] msg)
    {
        Print(msg, 0, (uint)msg.Length);
    }
    public void Print(Byte[] msg, UInt32 offset, UInt32 length)
    {
        UInt32 bufferAvailable = (uint)buffer.ptr.Length - buffer.length;
        if (length <= bufferAvailable)
        {
            Array.Copy(msg, offset, buffer.ptr, (int)buffer.length, length);
            buffer.length += length;
        }
        else
        {
            Flush();
            Write(msg, offset, length);
        }
    }
    /*
    public static void Print(UInt32 num)
    {
        unsafe
        {
            Byte* str = stackalloc Byte[10]; // Max is 10 digits
            //stdout.
            //stdout.Write((Byte[])str, 0, 10);
        }
    }
     */

}
public class StdOutput : ThreadUnsafeOutput
{
    public static readonly StdOutput Instance = new StdOutput();

    static IntPtr StdoutHandle;
    static void InitializeStaticData()
    {
        // Double checked locking
        if (StdoutHandle == IntPtr.Zero)
        {
            lock (typeof(StdOutput))
            {
                if (StdoutHandle == IntPtr.Zero)
                {
                    StdoutHandle = Win.GetStdHandle(Win.STD_OUTPUT_HANDLE);
                    if (StdoutHandle == Win.INVALID_HANDLE_VALUE)
                    {
                        throw new IOException("GetStdHandle returned INVALID_HANDLE_VALUE");
                    }
                }
            }
        }
    }
    private StdOutput()
        : base()
    {
        InitializeStaticData();
    }
    public StdOutput(UInt32 bufferLength)
        : base(bufferLength)
    {
        InitializeStaticData();
    }
    ~StdOutput()
    {
        // ERROR: StdoutHandle may be disposed
        Flush();
    }
    protected override void Write(Byte[] data, UInt32 offset, UInt32 length)
    {
        if (length > 0)
        {
            unsafe
            {
                fixed (Byte* dataPtr = data)
                {
                    uint charsWritten;
                    if (false == Win.WriteFile(StdoutHandle, data, buffer.length,
                        out charsWritten, null))
                    {
                        throw new IOException("WriteConsole failed");
                    }
                    if (charsWritten != buffer.length)
                    {
                        throw new IOException(String.Format("Expected to write {0} bytes but wrote {1}",
                            buffer.length, charsWritten));
                    }
                }
            }
        }
    }
    public unsafe override void Flush()
    {
        if (buffer.length > 0)
        {
            uint charsWritten;
            if (false == Win.WriteFile(StdoutHandle, buffer.ptr, buffer.length,
                out charsWritten, null))
            {
                throw new IOException("WriteConsole failed");
            }
            if (charsWritten != buffer.length)
            {
                throw new IOException(String.Format("Expected to write {0} bytes but wrote {1}",
                    buffer.length, charsWritten));
            }
            buffer.length = 0;
        }
    }
}

public static class Win
{
    public const UInt32 STD_OUTPUT_HANDLE = unchecked((uint)-11);
    public static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

    [DllImport("kernel32.dll")]
    public static extern uint GetLastError();

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr GetStdHandle(UInt32 handle);

    [DllImport("kernel32")]
    public static extern bool AllocConsole();
    [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
    public static extern bool FreeConsole();

    /*
    [DllImport("kernel32.dll")]
    public static unsafe extern bool WriteFile(IntPtr hFile, String lpBuffer,
       UInt32 nNumberOfBytesToWrite, out UInt32 lpNumberOfBytesWritten,
        System.Threading.NativeOverlapped* lpOverlapped);
    */

    [DllImport("kernel32.dll")]
    public static unsafe extern bool WriteFile(IntPtr hFile, Byte[] lpBuffer,
       UInt32 nNumberOfBytesToWrite, out UInt32 lpNumberOfBytesWritten,
        System.Threading.NativeOverlapped* lpOverlapped);
       //[In] ref System.Threading.NativeOverlapped lpOverlapped);
    [DllImport("kernel32.dll")]
    public static unsafe extern bool WriteFile(IntPtr hFile, Byte* lpBuffer,
       UInt32 nNumberOfBytesToWrite, out UInt32 lpNumberOfBytesWritten,
        System.Threading.NativeOverlapped* lpOverlapped);
       //[In] ref System.Threading.NativeOverlapped lpOverlapped);
    /*
    [DllImport("kernel32.dll")]
    public static extern bool WriteConsole(IntPtr hConsoleOutput, Byte[] lpBuffer,
       UInt32 nNumberOfCharsToWrite, out UInt32 lpNumberOfCharsWritten,
       IntPtr lpReserved);
    [DllImport("kernel32.dll")]
    public static unsafe extern bool WriteConsole(IntPtr hConsoleOutput, Byte* lpBuffer,
       UInt32 nNumberOfCharsToWrite, out UInt32 lpNumberOfCharsWritten,
       IntPtr lpReserved);
    */
}

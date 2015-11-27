using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("FithCompiler")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("FithCompiler")]
[assembly: AssemblyCopyright("Copyright ©  2015")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

[Flags]
enum FithCharFlags
{
    Whitespace = 0x01,
}

public struct LimString
{
    public Byte[] buffer;
    public UInt32 offset;
    public UInt32 limit;
    public LimString(Byte[] buffer, UInt32 offset, UInt32 limit)
    {
        this.buffer = buffer;
        this.offset = offset;
        this.limit = limit;
    }
    public override String ToString()
    {
        return Encoding.ASCII.GetString(buffer,
            (int)offset, (int)(limit - offset));
    }
}
public class LimStringComparer : IEqualityComparer<LimString>
{
    public static readonly LimStringComparer Instance = new LimStringComparer();
    private LimStringComparer() { }
    public bool Equals(LimString a, LimString b)
    {
        /*
        Console.WriteLine("[DEBUG] ({0}-{1}) ? ({2}-{3})",
            a.offset, a.limit,
            b.offset, b.limit);
        Console.WriteLine("a: {0}", Encoding.ASCII.GetString(a.buffer, 0, (int)a.limit));
        Console.WriteLine("b: {0}", Encoding.ASCII.GetString(b.buffer, 0, (int)b.limit));
        */

        UInt32 length = a.limit - a.offset;
        if (length != b.limit - b.offset)
        {
            //Console.WriteLine("[DEBUG] '{0}' != '{1}' (wrong length {2} != {3})",
            //    a, b, length, b.limit - b.offset);
            return false;
        }

        for (UInt32 i = 0; i < length; i++)
        {
            if (a.buffer[a.offset + i] != b.buffer[b.offset + i])
            {
                //Console.WriteLine("[DEBUG] '{0}' != '{1}' (mismatch at index {2})", a, b, i);
                return false;
            }
        }
        //Console.WriteLine("[DEBUG] '{0}' == '{1}'", a, b);
        return true;
    }
    public int GetHashCode(LimString key)
    {
        int hash = 5381;
        UInt32 offset = key.offset;
        while (offset < key.limit)
        {
            hash = ((hash << 5) + hash) + key.buffer[offset];
            if ((hash & 0x80000000) != 0)
            {
                break;
            }
            offset++;
        }
        return hash;
    }
}

public struct FithTokenizer
{
    public readonly Stream codeStream;
    public LimString token;

    UInt32 dataLimit;

    public FithTokenizer(Stream codeStream, Byte[] buffer)
    {
        this.codeStream = codeStream;
        this.token = new LimString(buffer, 0, 0);
        this.dataLimit = 0;
    }
    public void Next()
    {
        Byte c;
        UInt32 offset = token.limit;

        //
        // Find the start of the word
        //
        while (true)
        {
            if (offset >= dataLimit)
            {
                int length = codeStream.Read(token.buffer, 0, token.buffer.Length);
                if (length <= 0)
                {
                    if (length == 0)
                    {
                        token.limit = 0; // End of input
                        return;
                    }

                    throw new IOException("Failed to read from input stream");
                }
                offset = 0;
                dataLimit = (uint)length;
            }
            c = token.buffer[offset];
            if (!IsWhitespace(c))
            {
                token.offset = offset;
                offset++;
                break;
            }
            offset++;
        }

        //
        // Find end of word
        //
        while (true)
        {
            if (offset >= dataLimit)
            {
                // Shift to the beginning
                {
                    UInt32 partialTokenSize = offset - token.offset;
                    if (partialTokenSize >= token.buffer.Length)
                    {
                        throw new FormatException(String.Format("Buffer size of {0} is too small", token.buffer.Length));
                    }
                    if(token.offset > 0)
                    {
                        for (UInt32 i = 0; i < partialTokenSize; i++)
                        {
                            token.buffer[i] = token.buffer[token.offset + i];
                        }
                        //Console.WriteLine("[DEBUG] SHIFTED {0} BYTES of the token", partialTokenSize);
                    }
                    token.offset = 0;
                    offset = partialTokenSize;
                }

                int length = codeStream.Read(token.buffer, (int)offset,
                    token.buffer.Length - (int)offset);
                if (length <= 0)
                {
                    if (length == 0)
                    {
                        token.limit = offset;
                        return;
                    }

                    throw new IOException("Failed to read from input stream");
                }
                dataLimit = offset + (uint)length;
            }
            c = token.buffer[offset];
            if (IsWhitespace(c))
            {
                token.limit = offset;
                return;
            }
            offset++;
        }
    }


    static readonly FithCharFlags[] CharTable = new FithCharFlags[] {
        0, // 0
        0, // 1
        0, // 2
        0, // 3
        0, // 4
        0, // 5
        0, // 6
        0, // 7
        0, // 8
        FithCharFlags.Whitespace, // 9   '\t'
        FithCharFlags.Whitespace, // 10  '\n'
        0, // 11
        0, // 12
        FithCharFlags.Whitespace, // 13  '\r'
        0, // 14
        0, // 15
        0, // 16
        0, // 17
        0, // 18
        0, // 19
        0, // 20
        0, // 21
        0, // 22
        0, // 23
        0, // 24
        0, // 25
        0, // 26
        0, // 27
        0, // 28
        0, // 29
        0, // 30
        0, // 31
        FithCharFlags.Whitespace, // 32  ' '
        0, // 33  '!'
        0, // 34  '"'
        0, // 35  '#'
        0, // 36  '$'
        0, // 37  '%'
        0, // 38  '&'
        0, // 39  '''
        0, // 40  '('
        0, // 41  ')'
        0, // 42  '*'
        0, // 43  '+'
        0, // 44  ','
        0, // 45  '-'
        0, // 46  '.'
        0, // 47  '/'
        0, // 48  '0'
        0, // 49  '1'
        0, // 50  '2'
        0, // 51  '3'
        0, // 52  '4'
        0, // 53  '5'
        0, // 54  '6'
        0, // 55  '7'
        0, // 56  '8'
        0, // 57  '9'
        0, // 58  ':'
        0, // 59  ';'
        0, // 60  '<'
        0, // 61  '='
        0, // 62  '>'
        0, // 63  '?'
        0, // 64  '@'
        0, // 65  'A'
        0, // 66  'B'
        0, // 67  'C'
        0, // 68  'D'
        0, // 69  'E'
        0, // 70  'F'
        0, // 71  'G'
        0, // 72  'H'
        0, // 73  'I'
        0, // 74  'J'
        0, // 75  'K'
        0, // 76  'L'
        0, // 77  'M,'
        0, // 78  'N'
        0, // 79  'O'
        0, // 80  'P'
        0, // 81  'Q'
        0, // 82  'R'
        0, // 83  'S'
        0, // 84  'T'
        0, // 85  'U'
        0, // 86  'V'
        0, // 87  'W'
        0, // 88  'X'
        0, // 89  'Y'
        0, // 90  'Z'
        0, // 91  '['
        0, // 92  '\'
        0, // 93  ']'
        0, // 94  '^'
        0, // 95  '_'
        0, // 96  '`'
        0, // 97  'a'
        0, // 98  'b'
        0, // 99  'c'
        0, // 100 'd'
        0, // 101 'e'
        0, // 102 'f'
        0, // 103 'g'
        0, // 104 'h'
        0, // 105 'i'
        0, // 106 'j'
        0, // 107 'k'
        0, // 108 'l'
        0, // 109 'm'
        0, // 110 'n'
        0, // 111 'o'
        0, // 112 'p'
        0, // 113 'q'
        0, // 114 'r'
        0, // 115 's'
        0, // 116 't'
        0, // 117 'u'
        0, // 118 'v'
        0, // 119 'w'
        0, // 120 'x'
        0, // 121 'y'
        0, // 122 'z'
        0, // 123 '{'
        0, // 124 '|'
        0, // 125 '}'
        0, // 126 '~'
        0, // 127 'DEL'
    };

    public static Boolean IsWhitespace(Byte b)
    {
        return (b >= CharTable.Length) ? false :
            ((CharTable[b] & FithCharFlags.Whitespace) != 0);
    }
    static void Main(string[] args)
    {
        Console.OpenStandardOutput();

        //StdOutput.Instance.Println("Test");


        //FithCompiler.FithLoop();

        //Byte[] wordBuffer = new Byte[5]; // Large enough for biggest token
        Byte[] wordBuffer = new Byte[32]; // Large enough for biggest token

        FithTokenizer tokenizer = new FithTokenizer(Console.OpenStandardInput(), wordBuffer);
        Stack stack = new Stack32(tokenizer, Console.Out, 10);
        FithPrimitiveDictionary dictionary = new FithPrimitiveDictionary(stack);

        while (true)
        {
            tokenizer.Next();

            if (tokenizer.token.limit == 0)
            {
                StdOutput.Instance.Println("End of Input");
		        break;
            }

            //Console.WriteLine("[DEBUG] Got Token '{0}'", tokenizer.token);

            FithWord word;
            if (dictionary.limStringMap.TryGetValue(tokenizer.token, out word))
            {
                word.code();
                StdOutput.Instance.Println(" ok");
            }
            else if(stack.TryParseNumber(tokenizer.token))
            {
                StdOutput.Instance.Println(" ok");
            }
            else
            {
                StdOutput.Instance.Print(" ERROR: word '");
                StdOutput.Instance.Print(tokenizer.token);
                StdOutput.Instance.Println("' does not exist");
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public interface IFithPrimitives
{
    void print();
    void nodrop_print();
    void print_stack();

    void add();
}
public abstract class Stack : IFithPrimitives
{
    public readonly FithTokenizer tokenizer;
    public readonly TextWriter writer;
    public Stack(FithTokenizer tokenizer, TextWriter writer)
    {
        this.tokenizer = tokenizer;
        this.writer = writer;
    }
    public abstract Boolean TryParseNumber(LimString limString);

    public abstract void print();
    public abstract void print_stack();
    public abstract void nodrop_print();
    public abstract void add();

}
public class Stack32 : Stack
{
    public UInt32[] stack;
    Int32 sp;
    public Stack32(FithTokenizer tokenizer, TextWriter writer, UInt32 stackSize)
        : base(tokenizer, writer)
    {
        this.stack = new UInt32[stackSize];
        this.sp = -1;
    }
    public override Boolean TryParseNumber(LimString limString)
    {
        UInt32 result;
        UInt32 parsed = Extensions.TryParseUInt32(limString.buffer,
            limString.offset, limString.limit, out result);
        if (parsed == limString.limit)
        {
            sp++;
            stack[sp] = result;
            //Console.WriteLine("[DEBUG] Got Number '{0}'", result);
            return true;
        }
        else
        {
            //Console.WriteLine("[DEBUG] Not a Number '{0}'", limString);
            return false;
        }
    }
    public override void print()
    {
        //writer.Write("{0}", stack[sp]);
        sp--;
    }
    public override void print_stack()
    {
        if(sp < 0)
        {
            writer.WriteLine("empty stack");
        }
        else
        {
            for(int i = 0; i <= sp; i++)
            {
                writer.WriteLine("[{0}] {1}", i, stack[sp-i]);
            }
        }
    }
    public override void nodrop_print()
    {
        writer.Write("{0}", stack[sp]);
    }
    public override void add()
    {
        stack[sp - 1] += stack[sp];
        sp--;
    }
}

public delegate void WordCode();
public class FithWord
{
    public readonly String name;
    public readonly WordCode code;
    public FithWord(String name, WordCode code)
    {
        this.name = name;
        this.code = code;
    }
}
public class FithPrimitiveDictionary
{
    public readonly Dictionary<String, FithWord> map;
    public readonly Dictionary<LimString, FithWord> limStringMap;
    public FithPrimitiveDictionary(IFithPrimitives primitives)
    {
        //Byte[] wordBuffer = new Byte[128];
        this.map = new Dictionary<string, FithWord> {
            {"print"       , new FithWord("print", primitives.print)},
            {"print-stack" , new FithWord("print-stack", primitives.print_stack)},
            {"nodrop-print", new FithWord("nodrop-print", primitives.nodrop_print)},
            {"add"         , new FithWord("add", primitives.add)},
        };
        limStringMap = new Dictionary<LimString,FithWord>(LimStringComparer.Instance);
        foreach (var pair in map)
        {
            //int length = Encoding.ASCII.GetBytes(pair.Key, 0, pair.Key.Length, wordBuffer, 0);
            //limStringMap.Add(new LimString(wordBuffer, 0, (uint)length), pair.Value);
            Byte[] wordBytes = Encoding.ASCII.GetBytes(pair.Key);
            limStringMap.Add(new LimString(wordBytes, 0, (uint)wordBytes.Length), pair.Value);
        }
    }

}
class FithCompiler
{
    public static void FithLoop()
    {
        while (true) {// FITH: while{ FORTH: begin

            ConsoleKeyInfo keyInfo = Console.ReadKey();
            Console.WriteLine("Key '{0}'", keyInfo.KeyChar);





        } // FITH: } FORTH: again
    }

}
using System;
using System.IO;
namespace CDCtoPterm
{
    static class Program
    {
        private static UInt16 _chkSum;
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Usage:  dotnet CDCtoPterm inputfile outputfile");
                Console.WriteLine("Takes a .bin file as input.  Make a .mtu as aoutput");
                Console.WriteLine("See: http://www.classiccmp.org/dunfield/img/index.htm");
                return;
            }
            Console.Write("Converting {0} to {1}...", args[0], args[1]);
            FileStream inpt, outpt;
            try
            {
                inpt = File.OpenRead(args[0]);
            }
            catch (Exception e)
            {
                Console.WriteLine();
                Console.WriteLine(e);
                return;
            }
            try
            {
                outpt = File.Create(args[1], 130, FileOptions.WriteThrough);
            }
            catch (Exception e)
            {
                Console.WriteLine();
                Console.WriteLine(e);
                throw;
            }
            // prefix sector - all zeros
            for (int i = 1 ; i <= 130 ; i++)
                outpt.WriteByte(0);
            // sector by sector copy
            for (int sector = 1; sector <= 9856; sector++)
            {   // init word
                _chkSum = 0;
                // copy 128 bytes and compute chkword
                for (int pos = 0; pos < 128; pos++)
                {
                    byte b = (byte) (~inpt.ReadByte());
                    CalcCheck(b);
                    outpt.WriteByte(b);
                }
                // put 2 byte chkword at end of each sector
                // so a sector is 128 data bytes + 2 chkbytes
                byte cupper = (byte)((_chkSum >> 8) & 0xff);
                byte clower = (byte)(_chkSum & 0xff);
                outpt.WriteByte(clower);
                outpt.WriteByte(cupper);
                if ((sector % 10) == 0 )
                    Console.Write(".");
            }
            outpt.Flush();
            outpt.Close();
            inpt.Close();
            Console.WriteLine("");
            Console.WriteLine("Finished!");
        }
        private static void CalcCheck(byte b)
        {
            byte cupper = (byte)((_chkSum >> 8) & 0xff);
            byte clower = (byte)(_chkSum & 0xff);
            cupper ^= b;
            int x = cupper << 1;
            if ((x & 0x100) > 0)
                x = (x | 1) & 0xff;
            cupper = (byte)x;
            clower ^= b;
            int y = 0;
            if ((clower & 1) == 1)
                y = 0x80;
            x = clower >> 1;
            x = (x | y) & 0xff;
            clower = (byte)x;
            _chkSum = (UInt16)(((cupper << 8) & 0xff00) | (UInt16)(clower & 0xff));
        }
    }
}

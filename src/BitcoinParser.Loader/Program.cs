using System;
using System.Diagnostics;
using System.IO;
using Temosoft.Bitcoin.Blockchain;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BitcoinParser.Loader
{
    class Program
    {
        static void Main(string[] args)
        {
            var blocksFolder = @".\..\..\..\..\Samples\";
            var filesPath = Directory.GetFiles(blocksFolder, "blk*.dat", SearchOption.TopDirectoryOnly);
            int blockCount = 0;
            var sw = new Stopwatch();
            sw.Start();

            Parallel.ForEach(filesPath, (file) =>
            {
                var fileBytes = File.ReadAllBytes(file);
                int byteCursor = 0;

                while (IsMagic(fileBytes, byteCursor))
                {
                    blockCount++;
                    var blockSize = BitConverter.ToUInt32(fileBytes, byteCursor + 4); // byteCursor + 4 = Block Size Information
                    byteCursor = byteCursor + 8 + (int) blockSize;//byteCursor + 8 = Move by Magic Number + Size Info
                }
            });

            sw.Stop();
            Console.WriteLine(blockCount);
            Console.WriteLine(sw.ElapsedMilliseconds);


            //var parser = new BlockchainProcessor();
            //parser.Parse(filesPath);
        }

        private static bool IsMagic(byte[] bytes, int startSeq)
        {

            if(bytes.Length != startSeq && 
                    bytes[startSeq] == 0xF9 && 
                bytes[startSeq + 1] == 0xbe && 
                bytes[startSeq + 2] == 0xb4 && 
                bytes[startSeq + 3] == 0xd9)
            {
                return true;
            }

            return false;
        }
    }



    internal class BlockchainProcessor : BlockchainParser
    {
        private long _blks;

        protected override void ProcessBlock(Block block)
        {
            //Console.WriteLine(_blks);
            var i = block.PreviousBlockHash;
            //var trn = block.Transactions.LongCount();
        }
    }
}

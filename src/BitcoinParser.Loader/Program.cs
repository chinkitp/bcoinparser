using System;
using System.IO;
using Temosoft.Bitcoin.Blockchain;

namespace BitcoinParser.Loader
{
    class Program
    {
        static void Main(string[] args)
        {
            var blocksFolder = @".\..\..\..\..\Samples\";
            var filesPath = Directory.GetFiles(blocksFolder, "blk*.dat", SearchOption.TopDirectoryOnly);
            var parser = new BlockchainProcessor();
            parser.Parse(filesPath);
        }
    }

    internal class BlockchainProcessor : BlockchainParser
    {
        private long _blks;

        protected override void ProcessBlock(Block block)
        {
            //Console.WriteLine(_blks);
            Console.WriteLine(block.PreviousBlockHash);
        }
    }
}

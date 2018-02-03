using System;
using System.Diagnostics;
using System.IO;
using Temosoft.Bitcoin.Blockchain;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections;
using System.Data.SqlClient;

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
            var blockInfos = new List<Block>();

            Parallel.ForEach(filesPath, file => {

                var fileBytes = File.ReadAllBytes(file);
                int byteCursor = 0;

                while (IsMagic(fileBytes, byteCursor))
                {
                    blockCount++;
                    var blockSize = BitConverter.ToUInt32(fileBytes, byteCursor + 4); // byteCursor + 4 = Block Size Information

                    var block = new Block(blockSize);
                    Buffer.BlockCopy(fileBytes, byteCursor + 8, block.Raw, 0, (int)blockSize);
                    //block.Init();

                    blockInfos.Add(block);

                    byteCursor = byteCursor + 8 + (int)blockSize;//byteCursor + 8 = Move by Magic Number + Size Info

                }
            });

            Parallel.ForEach(blockInfos, block =>
            {
                block.Init();
            });

            InsertAsync(blockInfos).Wait();

            sw.Stop();
            Console.WriteLine(blockCount);
            Console.WriteLine(sw.ElapsedMilliseconds);


            //var parser = new BlockchainProcessor();
            //parser.Parse(filesPath);
        }

        public static async Task InsertAsync(IEnumerable<Block> blocks, CancellationToken ct = default(CancellationToken))
        {
            using (var connection = new SqlConnection())
            {
                connection.ConnectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\chinkitpatel\source\repos\cp\bcoinparser\src\BitcoinParser.Loader\App_Data\Bitcoin.mdf;Integrated Security=True;Connect Timeout=30";
                await connection.OpenAsync(ct);
                using (var bulk = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, null))
                {
                    //var customers = Customer.Generate(1000000);
                    using (var enumerator = blocks.GetEnumerator())
                    using (var customerReader = new ObjectDataReader<Block>(enumerator))
                    {
                        bulk.DestinationTableName = "Blocks";
                        bulk.ColumnMappings.Add(nameof(Block.Id), "Id");
                        bulk.ColumnMappings.Add(nameof(Block.VersionNumber), "VersionNumber");
                        bulk.ColumnMappings.Add(nameof(Block.PreviousBlockHashAsString), "PreviousBlockHashAsString"); 
                        bulk.ColumnMappings.Add(nameof(Block.MerkelRootHashAsString), "MerkelRootHashAsString");
                        bulk.ColumnMappings.Add(nameof(Block.TimeStamp), "TimeStamp");
                        bulk.ColumnMappings.Add(nameof(Block.Bits), "Bits");
                        bulk.ColumnMappings.Add(nameof(Block.Nonce), "Nonce");
                        bulk.ColumnMappings.Add(nameof(Block.TxnCount), "TxnCount");
                        bulk.ColumnMappings.Add(nameof(Block.Size), "Size");


                        bulk.EnableStreaming = true;
                        bulk.BatchSize = 10000;
                        bulk.NotifyAfter = 1000;
                        bulk.SqlRowsCopied += (sender, e) => Console.WriteLine("RowsCopied: " + e.RowsCopied);

                        await bulk.WriteToServerAsync(customerReader, ct);
                    }
                }
            }
        }

        private static bool IsMagic(byte[] bytes, int startSeq)
        {

            if (bytes.Length != startSeq &&
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
}

using System;
using System.Diagnostics;
using System.IO;
using Temosoft.Bitcoin.Blockchain;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections;
using System.Data.SqlClient;
using System.Linq;

namespace BitcoinParser.Loader
{

    class Program
    {
        static void Main(string[] args)
        {
            var blocksFolder = @".\..\..\..\..\Samples\";
            var filesPath = Directory.GetFiles(blocksFolder, "blk*.dat", SearchOption.TopDirectoryOnly);
            Console.WriteLine($"----- No of files found : {filesPath.Count()}");
            int blockCount = 0;
            var sw = new Stopwatch();
            sw.Start();
            var blockInfos = new List<Block>();

            foreach (var file in filesPath)
            {
                var fileBytes = File.ReadAllBytesAsync(file).Result;
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
            }

            Console.WriteLine($"Reading Files Completed {sw.ElapsedMilliseconds} ms");
            
            Parallel.ForEach(blockInfos, block =>
            {
                block.Init();
            });

            Console.WriteLine($"Block Parsing Completed {sw.ElapsedMilliseconds} ms");

            InsertAsync(blockInfos).Wait();

            sw.Stop();
            Console.WriteLine(blockCount);
            Console.WriteLine($"Process completed {sw.ElapsedMilliseconds} ms");
        }

        public static async Task InsertAsync(IEnumerable<Block> blocks, CancellationToken ct = default(CancellationToken))
        {
            using (var connection = new SqlConnection())
            {
                Console.WriteLine("Starting Bulk Insert");
                connection.ConnectionString = @"Server=.;Database=Bitcoin;Trusted_Connection=True;";
                await connection.OpenAsync(ct);
                using (var bulk = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, null))
                {
                    
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
                        bulk.NotifyAfter = 100000;
                        bulk.SqlRowsCopied += (sender, e) => Console.WriteLine("Blocks Inserted : " + e.RowsCopied);

                        await bulk.WriteToServerAsync(customerReader, ct);
                    }
                }

                using (var bulk = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, null))
                {
                    using (var enumerator = blocks.SelectMany(b => b.Transactions).GetEnumerator())
                    using (var transactionReader = new ObjectDataReader<Transaction>(enumerator))
                    {
                        bulk.DestinationTableName = "Transactions";
                        bulk.ColumnMappings.Add(nameof(Transaction.Id), "Id");
                        bulk.ColumnMappings.Add(nameof(Transaction.BlockId), "BlockId");
                        bulk.ColumnMappings.Add(nameof(Transaction.VersionNumber), "VersionNumber");

                        bulk.EnableStreaming = true;
                        bulk.BatchSize = 10000;
                        bulk.NotifyAfter = 100000;
                        bulk.SqlRowsCopied += (sender, e) => Console.WriteLine("Transactions Inserted : " + e.RowsCopied);

                        await bulk.WriteToServerAsync(transactionReader, ct);
                    }
                }

                using (var bulk = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, null))
                {
                    using (var enumerator = blocks.SelectMany(b => b.Transactions).SelectMany(t => t.Inputs).GetEnumerator())
                    using (var inputReader = new ObjectDataReader<Input>(enumerator))
                    {
                        bulk.DestinationTableName = "Inputs";
                        bulk.ColumnMappings.Add(nameof(Input.Id), "Id");
                        bulk.ColumnMappings.Add(nameof(Input.TransactionId), "TransactionId");
                        bulk.ColumnMappings.Add(nameof(Input.TransactionHash), "TransactionHash");
                        bulk.ColumnMappings.Add(nameof(Input.SequenceNumber), "SequenceNumber");
                        bulk.ColumnMappings.Add(nameof(Input.Script), "Script");
                        bulk.ColumnMappings.Add(nameof(Input.TransactionIndex), "TransactionIndex");
                        

                        bulk.EnableStreaming = true;
                        bulk.BatchSize = 10000;
                        bulk.NotifyAfter = 100000;
                        bulk.SqlRowsCopied += (sender, e) => Console.WriteLine("Inputs Inserted : " + e.RowsCopied);

                        await bulk.WriteToServerAsync(inputReader, ct);
                    }
                }

                using (var bulk = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, null))
                {
                    using (var enumerator = blocks.SelectMany(b => b.Transactions).SelectMany(t => t.Outputs).GetEnumerator())
                    using (var inputReader = new ObjectDataReader<Output>(enumerator))
                    {
                        bulk.DestinationTableName = "Outputs";
                        bulk.ColumnMappings.Add(nameof(Output.Id), "Id");
                        bulk.ColumnMappings.Add(nameof(Output.TransactionId), "TransactionId");
                        bulk.ColumnMappings.Add(nameof(Output.Value), "Value");
                        bulk.ColumnMappings.Add(nameof(Output.Script), "Script");

                        bulk.EnableStreaming = true;
                        bulk.BatchSize = 10000;
                        bulk.NotifyAfter = 100000;
                        bulk.SqlRowsCopied += (sender, e) => Console.WriteLine("Outputs Inserted : " + e.RowsCopied);

                        await bulk.WriteToServerAsync(inputReader, ct);
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

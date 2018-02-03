using System;
using Temosoft.Bitcoin.Blockchain;
using System.Linq;
using System.IO;

namespace BitcoinParser.Loader
{
    public class Block
    {
        public byte[] Raw { get; private set; }
        public Guid Id { get; private set; }
        public Block(uint size)
        {
            Raw = new byte[size];
            Size = (int) size;
            Id = Guid.NewGuid();
        }

        public void Init()
        {
            SetVersionNumber();
            SetPreviousBlockHashAsString();
            SetMerkelRootHashAsString();
            SetTimeStamp();
            SetBits();
            SetNonce();
            SetTransactions();
        }
           

        public int VersionNumber { get; private set; }
        private void SetVersionNumber()
        {
            VersionNumber = (int) BitConverter.ToUInt32(Raw, BlockConstants.Offsets.VersionNumber);
        }
   
        public string PreviousBlockHashAsString { get; private set; }
        private void SetPreviousBlockHashAsString()
        {
            var hash = Raw
                       .Skip(BlockConstants.Offsets.PreviousBlockHash)
                       .Take(BlockConstants.Sizes.PreviousBlockHash)
                       .Reverse()
                       .ToArray();
            PreviousBlockHashAsString = BitConverter.ToString(hash).Replace("-", string.Empty);
        }
        
        public string MerkelRootHashAsString { get; private set; }
        private void SetMerkelRootHashAsString()
        {
            var hash = Raw
                        .Skip(BlockConstants.Offsets.MerkelRootHash)
                        .Take(BlockConstants.Sizes.MerkelRootHash)
                        .Reverse()
                        .ToArray();
            MerkelRootHashAsString = BitConverter.ToString(hash).Replace("-", string.Empty);
        }

        public long TimeStamp { get; private set; }
        private void SetTimeStamp()
        {
            TimeStamp = BitConverter.ToUInt32(Raw, BlockConstants.Offsets.TimeStamp);
        }

        public long Bits { get; private set; }
        private void SetBits()
        {
            Bits = BitConverter.ToUInt32(Raw, BlockConstants.Offsets.Difficulty);
        }

        public long Nonce { get; private set; }
        private void SetNonce()
        {
            Nonce =  BitConverter.ToUInt32(Raw, BlockConstants.Offsets.Nounce);
        }

        public long TxnCount { get; private set; }
        //private void SetTxnCount()
        //{
        //    var t = Raw[BlockConstants.Offsets.TxnCount];
        //    if (t < 0xfd)
        //    {
        //        TxnCount = t;
        //    }
        //    else if (t == 0xfd)
        //    {
        //        TxnCount = BitConverter.ToInt16(Raw, BlockConstants.Offsets.TxnCount);
        //    }
        //    else if (t == 0xfe)
        //    {
        //        TxnCount = BitConverter.ToInt32(Raw, BlockConstants.Offsets.TxnCount);
        //    }
        //    else if (t == 0xff)
        //    {
        //        TxnCount = BitConverter.ToInt64(Raw, BlockConstants.Offsets.TxnCount);
        //    }
        //    else
        //    {
        //        throw new InvalidDataException("Reading Transaction Count");
        //    }
            
        //}


        public int Size { get; private set; }       
        public Transaction[] Transactions { get; internal set; }
        public long LockTime { get; private set; }

        private void SetTransactions()
        {
            using (var ms = new MemoryStream(Raw, BlockConstants.Offsets.TxnCount, Raw.Length - BlockConstants.Offsets.TxnCount))
            {
                using (var reader = new BinaryReader(ms))
                {
                    TxnCount = reader.ReadVarInt();
                    Transactions = new Transaction[TxnCount];

                    for (var ti = 0; ti < TxnCount; ti++)
                    {
                        var t = new Transaction(this);
                        t.VersionNumber = reader.ReadUInt32();

                        var inputCount = reader.ReadVarInt();
                        t.Inputs = new Input[inputCount];


                        for (var ii = 0; ii < inputCount; ii++)
                        {
                            var input = new Input(t);
                            input.TransactionHash = BitConverter.ToString(reader.ReadHashAsByteArray()).Replace("-", string.Empty);
                            input.TransactionIndex = reader.ReadUInt32();
                            input.Script = reader.ReadStringAsByteArray().ToHashString();
                            input.SequenceNumber = reader.ReadUInt32();
                            t.Inputs[ii] = input;
                        }

                        var outputCount = reader.ReadVarInt();
                        t.Outputs = new Output[outputCount];

                        for (var oi = 0; oi < outputCount; oi++)
                        {
                            var output = new Output(t);
                            output.Value = (long) reader.ReadUInt64();
                            output.Script = reader.ReadStringAsByteArray().ToHashString();
                            t.Outputs[oi] = output;
                        }
                        LockTime = reader.ReadUInt32();
                        Transactions[ti] = t;
                    }
                }

            }
        }
    }
}

using System;
using Temosoft.Bitcoin.Blockchain;
using System.Linq;

namespace BitcoinParser.Loader
{
    public class BlockInfo
    {
        public byte[] Raw { get; private set; }
        public Guid Id { get; private set; }
        public BlockInfo(uint size)
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

        public int TimeStamp { get; private set; }
        private void SetTimeStamp()
        {
            TimeStamp = BitConverter.ToInt32(Raw, BlockConstants.Offsets.TimeStamp);
        }

        public int Bits { get; private set; }
        private void SetBits()
        {
            Bits = BitConverter.ToInt32(Raw, BlockConstants.Offsets.Difficulty);
        }

        public int Nonce { get; private set; }
        private void SetNonce()
        {
            Nonce = BitConverter.ToInt32(Raw, BlockConstants.Offsets.Nounce);
        }

        public int TxnCount
        {
            get { return 0; }
        }


        public int Size { get; private set; } 
        

        //public byte[] PreviousBlockHash { get; internal set; }
        //public byte[] MerkleRoot { get; internal set; }
        //public uint TimeStamp { get; internal set; }
        //public uint Bits { get; internal set; }
        //public uint Nonce { get; internal set; }
        public Transaction[] Transactions { get; internal set; }
        //public uint LockTime { get; internal set; }
    }
}

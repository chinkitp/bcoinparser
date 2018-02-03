using BitcoinParser.Loader;
using System;
using System.Collections.Generic;

namespace Temosoft.Bitcoin.Blockchain
{
    public class Transaction
    {
        public Transaction(Block block)
        {
            Id = Guid.NewGuid();
            BlockId = block.Id;
        }

        public Guid Id { get; private set; }
        public Guid BlockId { get; private set; }
        public uint VersionNumber;
        public Input[] Inputs;
        public Output[] Outputs;
    }
}
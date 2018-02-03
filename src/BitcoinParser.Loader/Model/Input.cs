using System;

namespace Temosoft.Bitcoin.Blockchain
{
    public class Input
    {
        public Input(Transaction transaction)
        {
            Id = Guid.NewGuid();
            TransactionId = transaction.Id;
        }

        public Guid Id { get; private set; }
        public Guid TransactionId { get; private set; }
        public string TransactionHash;
        public long TransactionIndex;
        public string Script;
        public long SequenceNumber;
    }
}
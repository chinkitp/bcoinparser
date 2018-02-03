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
        public string TransactionHash { get;  set; }
        public long TransactionIndex { get;  set; }
        public string Script { get;  set; }
        public long SequenceNumber { get;  set; }
    }
}
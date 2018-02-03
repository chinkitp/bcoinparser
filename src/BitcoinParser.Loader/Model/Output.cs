using System;

namespace Temosoft.Bitcoin.Blockchain
{
    public class Output
    {
        public Output(Transaction transaction)
        {
            Id = Guid.NewGuid();
            TransactionId = transaction.Id;
        }

        public Guid Id { get; private set; }
        public Guid TransactionId { get; private set; }
        public ulong Value;
        public string Script;
    }
}
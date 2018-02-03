CREATE TABLE dbo.Blocks
(
	Id							UNIQUEIDENTIFIER PRIMARY KEY NOT NULL, 
	VersionNumber				INT NOT NULL, 
	PreviousBlockHashAsString	VARCHAR(256) NOT NULL, 
	MerkelRootHashAsString		VARCHAR(256) NOT NULL, 
	[TimeStamp]					BIGINT NOT NULL,
	Bits						BIGINT NOT NULL,
	Nonce						BIGINT NOT NULL, 
	TxnCount					INT NOT NULL,

	--Other helpful fields
	Height						INT NULL,
	Size						INT NOT NULL, 
	ReceivedTime				DATETIME2 NULL,
	RelayedBy					VARCHAR(28) NULL
)
GO
/*
DROP TABLE dbo.blocks
SELECT * FROM dbo.blocks

Field Size	Description	Data type	Comments
4	version	int32_t	Block version information (note, this is signed)
32	prev_block	char[32]	The hash value of the previous block this particular block references
32	merkle_root	char[32]	The reference to a Merkle tree collection which is a hash of all transactions related to this block
4	timestamp	uint32_t	A Unix timestamp recording when this block was created (Currently limited to dates before the year 2106!)
4	bits	uint32_t	The calculated difficulty target being used for this block
4	nonce	uint32_t	The nonce used to generate this block… to allow variations of the header and compute different hashes
 ?	txn_count	var_int	Number of transaction entries
 ?	txns	tx[]	Block transactions, in format of "tx" command

*/




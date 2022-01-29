using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Automerge
{
	public class Change
	{
		/// <summary>
		/// The ID of the actor that generated the change, as a lowercase hexadecimal string.
		/// </summary>
		public ActorId ActorId { get; }

		public ChangeHash? ChangeHash { get; }

		/// <summary>
		/// The sequence number of the change, starting with 1 for a given actor ID and proceeding as an incrementing sequence.
		/// </summary>
		public long SequenceNumber { get; }
		/// <summary>
		/// An integer, containing the counter value of the ID of the first operation in this change.
		/// Subsequent operations are assigned IDs in an incrementing sequence.
		/// </summary>
		public ulong StartOperationId { get; }

		/// <summary>
		/// The timestamp at which this change was generated, as an integer indicating the number of milliseconds since the 1970 Unix epoch.
		/// Dates before the epoch are represented as a negative number.
		/// </summary>
		public long Timestamp { get; }

		/// <summary>
		/// An optional human-readable "commit message" that describes the change in a meaningful way.
		/// It is not interpreted by Automerge, only stored for introspection, debugging, undo, and similar purposes.
		/// </summary>
		public string? Message { get; }

		/// <summary>
		/// An array of 64-digit lowercase hexadecimal strings containing the SHA-256 hashes of the binary encoding of
		/// the changes that causally precede this change. The array is empty for the first ever change, contains one
		/// hash in the case of a linear editing history, and multiple hashes in the case of a "merge commit".
		/// </summary>
		public IReadOnlyList<ChangeHash> Dependencies { get; }

		/// <summary>
		/// An array of operations
		/// </summary>
		public IReadOnlyList<Operation> Operations { get; }

		// TODO
		public byte[]? ExtraBytes { get; }

		public Change(
			IEnumerable<Operation> operations,
			ActorId actorId,
			ChangeHash? changeHash,
			long seq,
			ulong startOp,
			long logicalTime,
			string? message,
			IEnumerable<ChangeHash> dependencies,
			byte[]? extraBytes = null)
		{
			this.Operations = operations?.ToList() ?? throw new ArgumentNullException(nameof(operations));
			this.ActorId = actorId ?? throw new ArgumentNullException(nameof(actorId));
			this.ChangeHash = changeHash;
			this.SequenceNumber = seq;
			this.StartOperationId = startOp;
			this.Timestamp = logicalTime;
			this.Message = message;
			this.Dependencies = dependencies?.ToList() ?? throw new ArgumentNullException(nameof(dependencies));
			this.ExtraBytes = extraBytes;
		}
	}

	public class ChangeHash
	{
		public byte[] Hash { get; init; }

		public ChangeHash(byte[] hash)
		{
			if (hash.Length != 32)
			{
				throw new ArgumentException("Hash length must be 32 bytes");
			}
			this.Hash = hash;
		}
	}

	public class ActorId
	{
		public byte[] Value { get; init; }

		public ActorId(byte[] value)
		{
			if (value.Length != 16)
			{
				throw new ArgumentException("Actor id length must be 16 bytes");
			}
			this.Value = value;
		}

		public override string ToString()
		{
			return BitConverter.ToString(this.Value);
		}
	}

	public class ObjectId
	{
		// If null, then it is pointing at the root
		public OperationId? OperationId { get; init; }

		public ObjectId(OperationId? operationId)
		{
			this.OperationId = operationId;
		}

		public static ObjectId Root()
		{
			return new ObjectId(null);
		}
	}

	public class OperationId
	{
		public ulong Counter { get; init; }
		public ActorId ActorId { get; init; }

		public OperationId(ulong counter, ActorId actorId)
		{
			this.Counter = counter;
			this.ActorId = actorId ?? throw new ArgumentNullException(nameof(actorId));
		}

		public override string ToString()
		{
			return $"{this.Counter}@{this.ActorId}";
		}
	}
}
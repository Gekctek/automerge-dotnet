using Automerge.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Automerge
{
	public enum OperationType
	{
		Make,
		Delete,
		Increment,
		Set,
		MultiSet
	}
	public enum ObjectType
	{
		Map,
		Table,
		List,
		Text,
	}
	public class Operation
	{
		private readonly object _value;

		/// <summary>
		/// One of 'set', 'del', 'inc', 'makeMap', 'makeList', 'makeText', or 'makeTable'. These have broadly the same meaning as before:
		/// - set assigns a primitive value (string, number, boolean, or null) to a property of an object or a list element
		/// - del deletes a property or list element
		/// - inc increments or decrements a counter stored in a particular property or list element
		/// - make* creates a new object of the specified type, and assigns it to a property of an object or a list element
		/// </summary>
		public OperationType Type { get; }

		/// <summary>
		/// The objectId of the object being modified in this operation. This may be the UUID consisting of
		/// all zeros (indicating the root object) or a string of the form counter@actorId (indicating the object
		/// created by the operation with that ID). Note: in make* operations, the obj property contains the ID
		/// of the parent object, not the ID of the object being created.
		/// </summary>
		public ObjectId ObjectId { get; }

		/// <summary>
		/// A string that identifies the property of the object obj that is being modified. If the object is a
		/// map, this is a string containing the property name. If the object is a table, this is a string containing
		/// the primary key of the row (a UUID). If the object is a list or text, elemId is used instead.
		/// </summary>
		public Key Key { get; }

		/// <summary>
		/// An array of IDs of operations that are overwritten by this operation, in the form counter@actorId.
		/// Any existing operations that are not overwritten must be concurrent, and result in a conflict.
		/// The pred property appears on all types of operation, but it is always empty on an operation with
		/// insert: true, since such an operation does not overwrite anything.
		/// </summary>
		public List<OperationId> Pred { get; }

		/// <summary>
		/// A boolean that may be present on operations that modify list or text objects, and on all operation
		/// types except del and inc. If the insert property is false or absent, the operation updates the property
		/// or list element identified by elemId. If it is true, the operation inserts a new list element or
		/// character after the element identified by elemId, and the ID of this operation becomes the list
		/// element ID of the new element.
		/// </summary>
		public bool Insert { get; }

		private Operation(
			OperationType type,
			object value,
			ObjectId objectId,
			Key key,
			List<OperationId> pred,
			bool insert)
		{
			this.Type = type;
			this._value = value;
			this.ObjectId = objectId;
			this.Key = key;
			this.Pred = pred;
			this.Insert = insert;
		}

		public ObjectType AsMakeObjectType => GetValue<ObjectType>(OperationType.Make);

		public uint AsDeletionCount => GetValue<uint>(OperationType.Delete);

		public long AsIncrementCount => GetValue<long>(OperationType.Increment);

		public ScalarValue AsSetValue => GetValue<ScalarValue>(OperationType.Set);

		public List<ScalarValue> AsMultiSetValue => GetValue<List<ScalarValue>>(OperationType.MultiSet);


		private T GetValue<T>(OperationType type)
		{
			if (this.Type != type)
			{
				throw new InvalidOperationException($"Cannot convert operation type '{this.Type}' to '{type}'");
			}
			return (T)this._value;
		}

		public static Operation Make(ObjectType type, ObjectId objectId, Key key, List<OperationId> pred, bool insert)
		{
			return new Operation(OperationType.Make, type, objectId, key, pred, insert);
		}

		public static Operation Delete(int count, ObjectId objectId, Key key, List<OperationId> pred)
		{
			return new Operation(OperationType.Delete, count, objectId, key, pred, insert: false);
		}

		public static Operation Increment(long value, ObjectId objectId, Key key, List<OperationId> pred)
		{
			return new Operation(OperationType.Increment, value, objectId, key, pred, insert: false);
		}

		public static Operation Set(ScalarValue value, ObjectId objectId, Key key, List<OperationId> pred, bool insert)
		{
			if(value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}
			return new Operation(OperationType.Set, value, objectId, key, pred, insert);
		}

		public static Operation Set(List<ScalarValue> values, ObjectId objectId, Key key, List<OperationId> pred, bool insert)
		{
			if (values?.Any() != true)
			{
				throw new ArgumentException("At least one value must be specified");
			}
			if(values.Count == 1)
			{
				return Set(values[0], objectId, key, pred, insert);
			}
			return new Operation(OperationType.MultiSet, values, objectId, key, pred, insert);
		}
	}

}

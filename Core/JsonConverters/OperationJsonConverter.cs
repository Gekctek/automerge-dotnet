using System.Text.Json;
using System.Text.Json.Serialization;
using ForgetIt.Core;
using System;
using System.Text.Json.Nodes;

namespace Automerge.Core.JsonConverters
{
	public class OperationJsonConverter : JsonConverter<Operation>
	{
		public override Operation? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			JsonNode? obj = JsonNode.Parse(ref reader);
			if (obj == null)
			{
				return null;
			}
			if (!(obj is JsonObject jObj))
			{
				throw new JsonException("Invalid operation json");
			}
			OperationType type = GetRequiredValue<OperationType>("type");

			ObjectId objectId = GetRequiredValue<ObjectId>("objectId");
			List<OperationId> pred = GetOptionalValue<List<OperationId>>("pred") ?? new List<OperationId>();

			Key key = GetRequiredValue<Key>("key");

			bool insert = GetOptionalValue<bool>("insert");

			switch (type)
			{
				case OperationType.Make:
					ObjectType objType = GetRequiredValue<ObjectType>("objType");
					return Operation.Make(objType, objectId, key, pred, insert);
				case OperationType.Delete:
					int deleteCount = GetRequiredValue<int>();
					return Operation.Delete(deleteCount, objectId, key, pred, insert);
				case OperationType.Increment:
					return Operation.Increment(incrementValue, objectId, key, pred, insert);
				case OperationType.Set:
					JsonNode
					return Operation.Set(setValue, objectId, key, pred, insert);
				case OperationType.MultiSet:
					return Operation.Set(multiSetValue, objectId, key, pred, insert);
				default:
					throw new NotImplementedException();
			};

			T GetRequiredValue<T>(string propertyName)
			{
				if (!TryGetNode(propertyName, out JsonNode? node))
				{
					throw new JsonException("Missing value 'type'");
				}
				return node.Deserialize<T>(options) ?? throw new JsonException($"Missing required property '{propertyName}'");
			}

			T? GetOptionalValue<T>(string propertyName)
			{
				if (!TryGetNode(propertyName, out JsonNode? node))
				{
					return default;
				}
				return node.Deserialize<T>(options);
			}

			bool TryGetNode(string propertyName, out JsonNode? node)
			{
				return jObj.TryGetPropertyValue(propertyName, out node) || node == null;
			}
		}

		public override void Write(Utf8JsonWriter writer, Operation value, JsonSerializerOptions options)
		{
			writer.WriteBase64String("hash", value.Hash);
		}
	}
}
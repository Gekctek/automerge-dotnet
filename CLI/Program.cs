using Automerge;
using Automerge.Core;
using Automerge.Core.JsonConverters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;


namespace Automerge.Sample
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			using (AutomergeBackend automerge = AutomergeBackend.Init())
			{
				var id = ObjectId.Root();
				var state = new State(id);
				state.Set("Test", ScalarValue.Boolean(true));
				ActorId actorId = new ActorId(new byte[16]);
				long logicalTime = 0;
				string message = "Hello World!";
				Change change = state.BuildChange(actorId, logicalTime, message);

				var options = new JsonSerializerOptions();
				options.Converters.Add(new ActorIdJsonConverter());
				options.Converters.Add(new ChangeHashJsonConverter());
				options.Converters.Add(new ChangeJsonConverter());

				string json = JsonSerializer.Serialize(change, options);
				Change? c = JsonSerializer.Deserialize<Change>(json, options);

				automerge.ApplyLocalChange(change);

				Console.WriteLine();
				Console.ReadLine();
			}
		}
	}
}
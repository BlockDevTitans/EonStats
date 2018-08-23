using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EonSharp;

namespace EonStats
{
	class NetworkContext
	{
		Task loop;
		public NetworkContext()
		{
			loop = Task.Run(async () =>
			   {
				   do
				   {
					   Error = "";
					   Addresses = await Poll(e => OnHasErrors(e));
					   await Task.Delay(TimeSpan.FromMinutes(3));
				   } while (true);
			   });
		}


		public static async Task<string[]> Poll(Action<string> callbackOnError = null)
		{
			EonSharp.Configuration.IgnoreSslErrors = true;

			EonClient.ClassMapper[typeof(EonSharp.Network.ITransportContext)] = new ActivatorDescriptor[]
			{
				new ActivatorDescriptor(typeof(EonSharp.Network.Transports.HttpTransportClient)),
				new ActivatorDescriptor(typeof(EonSharp.Logging.HttpTransportLogger), new object[]{ "[HTTP TRANSPORT] " })
			};

			var eonClient = new EonClient(Constants.NETWORK_MAIN);

			var logger = eonClient.TransportContext as EonSharp.Logging.ILog;
			logger.LogChanged += (s, e) =>
			{
				switch (e.Type)
				{
					case EonSharp.Logging.LogMessageType.Information:
						break;
					case EonSharp.Logging.LogMessageType.Warning:
					case EonSharp.Logging.LogMessageType.Error:
						callbackOnError?.Invoke(e.ToString());
						break;
				}
			};

			await eonClient.UpdateBlockchainDetails();

			async Task tryAddNode(HashSet<string> hset, EonClient client, string address)
			{
				if (!hset.Contains(address))
				{
					hset.Add(address);
					var context = client.CreateNewContext($"https://{address}");
					var res = await context.Peer.Metadata.GetWellKnownNodesAsync();
					foreach (var addr in res)
					{
						await tryAddNode(hset, context, addr);
					}
				}
			}

			var set = new HashSet<string>();
			foreach (var address in await eonClient.Peer.Metadata.GetWellKnownNodesAsync())
			{
				await tryAddNode(set, eonClient, address);
			}

			return set.ToArray();
		}

		public event EventHandler<string> HasErrors;
		void OnHasErrors(string message)
		{
			Error = message;
			HasErrors?.Invoke(this, message);
		}

		public string[] Addresses { get; private set; }
		public string Error { get; private set; }
	}
}

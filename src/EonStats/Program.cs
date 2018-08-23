using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nancy.Hosting.Self;

namespace EonStats
{
	class Program
	{
		static NancyHost s_host;

		static void Main(string[] args)
		{
			var hosting = Array.IndexOf(args, "--host");
			if (hosting > -1 && args.Length > hosting + 1)
			{
				var root = Array.IndexOf(args, "--root");
				if (root > -1 && args.Length > root + 1)
				{
					UseViews = true;
					s_host = new NancyHost(new Uri(args[hosting + 1]), new Bootstrapper(args[root + 1]), new HostConfiguration { RewriteLocalhost = true, UrlReservations = new UrlReservations { CreateAutomatically = true } });
				}
				else
				{
					s_host = new NancyHost(new Uri(args[hosting + 1]), new Bootstrapper(null), new HostConfiguration { RewriteLocalhost = true, UrlReservations = new UrlReservations { CreateAutomatically = true } });
				}
				s_host.Start();
				Console.WriteLine($"Listening on {args[hosting + 1]}");
			}
			else
			{
				Task.Run(() =>
				{
					Console.Error.WriteLine();

					string[] list = null;
					var mr = new ManualResetEventSlim(false);

					var tsk = Task.Run(() => NetworkContext.Poll(e =>
					{
						Console.Error.WriteLine(e);
						Environment.Exit(1);
					})).ContinueWith(new Action<Task<string[]>>(t =>
					{
						list = t.Result;
						mr.Set();
					}));

					int count = 0;
					do
					{
						count = (count + 1) % 5;
						Console.Error.Write($"\rSearching{new string('.', count)}{new string(' ', 5 - count)}");
					}
					while (!mr.Wait(100));

					Console.Error.Write("\r                                 \r");

					foreach (var address in list)
					{
						Console.WriteLine(address);
					}

					Console.WriteLine();
					Console.WriteLine("---------------------");
					Console.WriteLine($"Total node count: {list.Length}");

					Environment.Exit(0);
				});
			}

			Console.ReadKey();
		}

		public static bool UseViews { get; private set; } = false;


	}






}

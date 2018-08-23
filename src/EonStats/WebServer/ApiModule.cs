using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nancy;
using Nancy.ModelBinding;

namespace EonStats
{
	public class ApiModule : NancyModule
	{
		static NetworkContext s_nctx = new NetworkContext();
		public ApiModule()
		{
			Get["/"] = p =>
			{
				try
				{
					if (Program.UseViews)
					{
						return View["index", null];
					}

					if (!string.IsNullOrWhiteSpace(s_nctx.Error))
					{
						return s_nctx.Error;
					}
					if (s_nctx.Addresses == null || s_nctx.Addresses.Length == 0)
					{
						return "No results yet, try again later";
					}
					return $"<p>Nodes count: {s_nctx.Addresses.Length}</p><ul><li>{string.Join("</li><li>", s_nctx.Addresses)}</li></ul>";

				}
				catch (Exception ex)
				{
					return new Response { StatusCode = HttpStatusCode.BadRequest, ReasonPhrase = ex.Message };
				}
			};
			Get["/api"] = p =>
			{
				return "API version 1.0";
			};
			Get["/api/nodes"] = p => //gets list of nodes
			{
				try
				{
					return Response.AsJson(s_nctx.Addresses ?? new string[0]);
				}
				catch (Exception ex)
				{
					return new Response { StatusCode = HttpStatusCode.BadRequest, ReasonPhrase = ex.Message };
				}
			};
			Get["/api/nodes/{address}", true] = async (p, ctx) =>  //gets metrics of single node
			{
				try
				{
					using (var clt = new System.Net.Http.HttpClient())
					{
						var url = $"https://{((string)p.address).Trim(" \"".ToCharArray())}/metrics";
						var res = await clt.GetStringAsync(url);
						var list = res.Split("\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToDictionary(str => str.Split(' ')[0], str => str.Split(' ')[1]);
						return Response.AsJson(list);
					}
				}
				catch (Exception ex)
				{
					return new Response { StatusCode = HttpStatusCode.BadRequest, ReasonPhrase = ex.Message };
				}
			};
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Conventions;
using Nancy.Responses;
using Nancy.TinyIoc;
using Nancy.ViewEngines;

namespace EonStats
{
	public class Bootstrapper : DefaultNancyBootstrapper
	{
		public Bootstrapper(string rootPath)
		{
			if (rootPath == null)
			{
				rootPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "eonstats");
				while (!System.IO.Directory.Exists(rootPath))
				{
					System.IO.Directory.CreateDirectory(rootPath);
				}
			}
			s_rp = new RootPathProv(rootPath);
		}
		protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
		{
			base.ApplicationStartup(container, pipelines);
			StaticConfiguration.DisableErrorTraces = false;
			Nancy.Json.JsonSettings.RetainCasing = true;
			Nancy.Json.JsonSettings.MaxJsonLength = 100 * 1024 * 1024;
		}

		static IRootPathProvider s_rp;
		protected override IRootPathProvider RootPathProvider => s_rp;

		protected override byte[] FavIcon => null;


	}
}
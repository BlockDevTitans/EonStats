using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Nancy;

namespace EonStats
{
	class RootPathProv : IRootPathProvider
	{
		string rootPath;
		public RootPathProv(string rootPath)
		{
			this.rootPath = rootPath;
		}
		public string GetRootPath()
		{
			return rootPath;
		}
	}
}

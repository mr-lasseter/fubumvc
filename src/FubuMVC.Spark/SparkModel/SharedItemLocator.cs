﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using FubuCore;

namespace FubuMVC.Spark.SparkModel
{
    public interface ISharedItemLocator
    {
        SparkItem LocateItem(string sparkName, SparkItem fromItem, IEnumerable<SparkItem> items);
    }
	
    public class SharedItemLocator : ISharedItemLocator
    {
        private readonly SharedDirectoryProvider _sharedDirectoryProvider;
        public SharedItemLocator(IEnumerable<string> sharedFolderNames)
        {
            _sharedDirectoryProvider = new SharedDirectoryProvider(sharedFolderNames);
        }

        public SparkItem LocateItem(string sparkName, SparkItem fromItem, IEnumerable<SparkItem> items)
        {
            var reachables = _sharedDirectoryProvider.GetDirectories(fromItem, items);
            return items.ByName(sparkName).InDirectories(reachables).FirstOrDefault();
        }
    }

    public class SharedDirectoryProvider
    {
        private readonly IEnumerable<string> _sharedFolderNames;
        private readonly SharedPathBuilder _sharedPathBuilder;

        public SharedDirectoryProvider(IEnumerable<string> sharedFolderNames)
        {
            _sharedFolderNames = sharedFolderNames;
            _sharedPathBuilder = new SharedPathBuilder(sharedFolderNames);
        }

        public IEnumerable<string> GetDirectories(SparkItem item, IEnumerable<SparkItem> items)
        {
            foreach (var directory in _sharedPathBuilder.BuildFrom(item.FilePath, item.RootPath))
            {
                yield return directory;
            }
			
            if (item.Origin == FubuSparkConstants.HostOrigin)
            {
                yield break;
            }
			
            var hostRoot = findHostRoot(items);
            if (hostRoot.IsEmpty())
            {
                yield break;
            }
			
            foreach (var sharedFolder in _sharedFolderNames)
            {
                yield return Path.Combine(hostRoot, sharedFolder);
            }
        }
		
		private string findHostRoot(IEnumerable<SparkItem> items)
		{
			return items.ByOrigin(FubuSparkConstants.HostOrigin).FirstValue(x => x.RootPath);
		}
    }
	
	public class SharedPathBuilder
    {
        private readonly IEnumerable<string> _sharedFolderNames;
        public SharedPathBuilder(IEnumerable<string> sharedFolderNames)
        {
            _sharedFolderNames = sharedFolderNames;
        }

        public IEnumerable<string> BuildFrom(string path, string root)
        {
            if (path == root) yield break;

            do
            {
                path = Path.GetDirectoryName(path);
                if (path == null) break;
                foreach (var sharedFolder in _sharedFolderNames)
                {
                    yield return Path.Combine(path, sharedFolder);
                }

            } while (path.IsNotEmpty() && path.PathRelativeTo(root).IsNotEmpty());
        }
    }
}
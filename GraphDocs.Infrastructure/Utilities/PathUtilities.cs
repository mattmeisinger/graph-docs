using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphDocs.Infrastructure.Utilities
{
    public static class PathUtilities
    {
        /// <summary>
        /// Joins two or more paths or filenames into one long, slash-separated string.
        /// Note: Also removes all leading or trailing slashes ("/")
        /// </summary>
        public static string Join(params string[] args)
        {
            var pieces = args
                .Where(a => !string.IsNullOrWhiteSpace(a))
                .SelectMany(a => a.Split('/'))
                .Where(a => !string.IsNullOrWhiteSpace(a))
                .ToArray();
            if (pieces.Any())
                return "/" + string.Join("/", pieces);
            else
                return "/";
        }

        public static string ReformatPath(string path)
        {
            return Join(new[] { path });
        }
    }
}

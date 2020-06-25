using System;
using System.Collections;
using System.IO;

namespace ProActive.SharePoint.Build
{
    public static class Guard
    {
        public static void ForNullArgument(object o, string paramName)
        {
            if (Equals(o, null))
            {
                throw new ArgumentNullException(paramName);
            }
        }

        public static void ForNullOrWhiteSpaceArgument(string s, string paramName)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                throw new ArgumentException("Cannot be empty or whitespace", paramName);
            }
        }

        public static void ForInvalidFileArgument(string path, string paramName, string message = null)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException(message ?? $"Parameter name: ${paramName}", path);
            }
        }

        public static void ForNull(object o, string message)
        {
            if (Equals(o, null))
            {
                throw new NullReferenceException(message);
            }
        }

        public static void ForNullOrWhiteSpace(string s, string message)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                throw new NullReferenceException(message);
            }
        }

        public static void ForInvalidFile(string path, string message)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException(message, path);
            }
        }

        public static void ForEmptyList(ICollection collection, string message)
        {
            if (collection.Count == 0)
            {
                throw new InvalidOperationException(message);
            }
        }
    }
}

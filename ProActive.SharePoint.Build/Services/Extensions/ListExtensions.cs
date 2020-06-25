namespace ProActive.SharePoint.Build.Services.Extensions
{
    using System;
    using System.Collections.Generic;

    public static class ListExtensions
    {
        public static void ForEach<T>(this IList<T> list, Action<T, int> predicate)
        {
            Guard.ForNullArgument(predicate, nameof(predicate));

            for (var i = 0; i < list.Count; ++i)
            {
                predicate(list[i], i);
            }
        }
    }
}

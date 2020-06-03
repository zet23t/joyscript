using System.Collections.Generic;

namespace JoyScript
{
    public static class ListExtensions
    {
        public static T Pop<T>(this IList<T> list)
        {
            T v = list[list.Count - 1];
            list.RemoveAt(list.Count - 1);
            return v;
        }

        public static T Last<T>(this IList<T> list, int fromBack = 0, T fallback = default)
        {
            int v = list.Count - 1 - fromBack;
            return v >= 0 ? list[v] : fallback;
        }
    }
}
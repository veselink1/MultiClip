using System;

namespace MultiClip.Utilities
{
    public static class Objects
    {
        public static U Let<T, U>(this T obj, Func<T, U> action)
        {
            return action(obj);
        }

        public static T Also<T>(this T obj, Action<T> action)
        {
            action(obj);
            return obj;
        }
    }
}

using System;
using System.Runtime.InteropServices;

namespace MultiClip.Native
{
    public static class Stdlib
    {
        [DllImport("msvcrt.dll", EntryPoint = "memcmp", CallingConvention = CallingConvention.Cdecl)]
        private static unsafe extern int MemCmp(byte* arr1, byte* arr2, UIntPtr count);

        public static unsafe bool MemCmp(byte* arr1, byte* arr2, int count)
        {
            return MemCmp(arr1, arr2, (UIntPtr)count) == 0;
        }

        [DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern byte* MemCpy(byte* dest, byte* src, UIntPtr count);

        /// <summary>
        /// Compares the data in both arrays using the native <code>memcpy</code> function.
        /// </summary>
        public static bool MemCmp(byte[] arr1, byte[] arr2)
        {
            if (arr1.Length != arr2.Length)
                return false;

            unsafe
            {
                fixed (byte* p1 = arr1)
                fixed (byte* p2 = arr2)
                {
                    return MemCmp(p1, p2, arr1.Length);
                }
            }
        }

        /// <summary>
        /// Compares the data in both arrays using the native <code>memcpy</code> function.
        /// </summary>
        public static bool MemCmp(byte[] arr1, byte[] arr2, int count)
        {
            if (count > arr1.Length || count > arr2.Length)
                throw new ArgumentOutOfRangeException(nameof(count));

            if (arr1.Length != arr2.Length)
                return false;

            unsafe
            {
                fixed (byte* p1 = arr1)
                fixed (byte* p2 = arr2)
                {
                    return MemCmp(p1, p2, count);
                }
            }
        }
        
        /// <summary>
        /// Compares the data in both arrays using the native <code>memcpy</code> function.
        /// </summary>
        public static bool MemCmp(byte[] arr1, byte[] arr2, int index, int count)
        {
            if (count > arr1.Length || count > arr2.Length)
                throw new ArgumentOutOfRangeException(nameof(count));

            if (arr1.Length != arr2.Length)
                return false;

            unsafe
            {
                fixed (byte* p1 = arr1)
                fixed (byte* p2 = arr2)
                {
                    return MemCmp(p1 + index, p2 + index, count);
                }
            }
        }
    }
}

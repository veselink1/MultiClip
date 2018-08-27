using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using MultiClip.Native;

namespace MultiClip.Clipboard
{
    /// <summary>
    /// Filters an array of data formats.
    /// </summary>
    /// <param name="source">The source array.</param>
    /// <returns>The resulting array.</returns>
    public delegate DataFormat[] DataFormatFilter(DataFormat[] source);

    /// <summary>
    /// Contains static methods for working with the native 
    /// Windows clipboard.
    /// </summary>
    public static class ClipboardHelper
    {
        /// <summary>
        /// Reads the current system clipboard state.
        /// </summary>
        /// <returns>The current clipboard state</returns>
        public static ClipboardState GetState()
        {
            IntPtr hWnd = new WindowInteropHelper(Application.Current.MainWindow).EnsureHandle();
            var items = GetClipboardItems(hWnd, x => x);
            return new ClipboardState(items);
        }
        
        /// <summary>
        /// Reads the current system clipboard state.
        /// </summary>
        /// <returns>The current clipboard state</returns>
        public static ClipboardState GetState(DataFormatFilter formatFilter)
        {
            IntPtr hWnd = new WindowInteropHelper(Application.Current.MainWindow).EnsureHandle();
            var items = GetClipboardItems(hWnd, formatFilter);
            return new ClipboardState(items);
        }

        /// <summary>
        /// Writes the state to the system clipboard state.
        /// </summary>
        /// <param name="state">The new clipboard state.</param>
        public static void SetState(ClipboardState state)
        {
            IntPtr hWnd = new WindowInteropHelper(Application.Current.MainWindow).EnsureHandle();
            WriteState(hWnd, state);
        }

        /// <summary>
        /// Reads and returns the data in the system clipboard.
        /// Important! <see cref="NativeMethods.OpenClipboard(IntPtr)"/> must be called before calling this function.
        /// </summary>
        /// <returns>A lazy enumerable of the data in the clipboard.</returns>
        private static (uint format, IntPtr handle)[] GetClipboardData(DataFormatFilter formatFilter)
        {
            // The list of the data blocks.
            var dataBlocks = new List<(uint format, IntPtr handle)>();

            int cFormats = NativeMethods.CountClipboardFormats();
            uint[] formats = new uint[cFormats];

            if (!NativeMethods.GetUpdatedClipboardFormats(formats, (uint)cFormats, out uint cFormatsOut))
            {
                throw new Win32Exception();
            }
            if (cFormats != cFormatsOut)
            {
                throw new Win32Exception("The clipboard was modified unexpectedly!");
            }

            // Apply the data format filter.
            DataFormat[] dataFormats = formats.Select(x => (DataFormat)x).ToArray();
            uint[] filteredFormats = formatFilter(dataFormats).Select(x => (uint)x).ToArray();

            foreach (var format in filteredFormats)
            {
                // Get the pointer for the current clipboard data.
                IntPtr handle = NativeMethods.GetClipboardData(format);
                // Goto next if it's unreachable.
                if (handle == IntPtr.Zero)
                {
                    continue;
                }

                dataBlocks.Add((format, handle));
            }

            // Return the list of data blocks.
            return dataBlocks.ToArray();
        } 

        private static ReadOnlyCollection<ClipboardItem> GetClipboardItems(IntPtr hWnd, DataFormatFilter formatFilter)
        {
            var items = new ConcurrentQueue<ClipboardItem>();

            // Open Clipboard to allow us to read from it.
            if (!NativeMethods.OpenClipboard(hWnd))
            {
                throw new Win32Exception();
            }

            try
            {
                (uint format, IntPtr handle)[] dataBlocks = GetClipboardData(formatFilter);
                
                // Paralelly deflate the clipboard data
                // and add the items to [items]
                Parallel.ForEach(dataBlocks, item =>
                {
                    (uint format, IntPtr handle) = item;
                    unsafe
                    {
                        IntPtr pData = NativeMethods.GlobalLock(handle);
                        // Goto next if it's unreachable.
                        if (pData == IntPtr.Zero)
                        {
                            return;
                        }

                        try
                        {
                            UIntPtr size = NativeMethods.GlobalSize(handle);
                            if (size == UIntPtr.Zero)
                                throw new Win32Exception();
                            
                            var fragment = ClipboardItem.FromPointer
                            (
                                formatId: (DataFormat)format,
                                bufferPtr: (byte*)pData,
                                size: (int)size
                            );

                            // Add the fragment to the list.
                            items.Enqueue(fragment);
                        }
                        finally
                        {
                            NativeMethods.GlobalUnlock(handle);
                        }
                    }
                });
            }
            finally
            {
                // Close the clipboard and release unused resources.
                NativeMethods.CloseClipboard();
            }

            // Returns the list of ClipboardItems as a ReadOnlyCollection
            return new ReadOnlyCollection<ClipboardItem>(items.ToArray());
        }

        /// <summary>
        /// Set the current state of the system clipboard.
        /// </summary>
        /// <param name="hWnd">A handle to the calling window.</param>
        /// <param name="state">The new state of the clipboard.</param>
        /// <returns>True on success, false on failure.</returns>
        public static bool WriteState(IntPtr hWnd, ClipboardState state)
        {
            var items = state.Items;

            // Open clipboard to allow its manipultaion.
            if (!NativeMethods.OpenClipboard(hWnd))
            {
                // Throw an exception and clear the error.
                throw new Win32Exception();
            }

            try
            {
                // Clear the clipboard.
                if (!NativeMethods.EmptyClipboard())
                {
                    throw new Win32Exception();
                }

                Parallel.ForEach(items, item =>
                {
                    // Get the pointer for inserting the buffer data into the clipboard.
                    IntPtr alloc = NativeMethods.GlobalAlloc(NativeMethods.GMEM_MOVEABLE | NativeMethods.GMEM_DDESHARE, (UIntPtr)item.Size);
                    try
                    {
                        IntPtr gLock = NativeMethods.GlobalLock(alloc);
                        bool locked = true;
                        try
                        {
                            unsafe
                            {
                                // Load the fragment's data to the preallocated block.
                                item.CopyTo((byte*)gLock);
                            }

                            // Release pointers.
                            NativeMethods.GlobalUnlock(alloc);
                            locked = false;

                            NativeMethods.SetClipboardData((uint)item.Format, alloc);
                        }
                        finally
                        {
                            if (locked)
                            {
                                locked = false;
                                // Release pointers.
                                NativeMethods.GlobalUnlock(alloc);
                            }
                        }
                    }
                    finally
                    {
                        NativeMethods.GlobalFree(alloc);
                    }
                });
            }
            finally
            {
                // Close the clipboard to realese unused resources.
                NativeMethods.CloseClipboard();
            }

            return true;
        }
    }
}

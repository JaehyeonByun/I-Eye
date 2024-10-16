/******************************************************************************
 * File: ScopePtr.cs
 * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using System.Runtime.InteropServices;

namespace Qualcomm.Snapdragon.Spaces
{
    /*
     * This is a helper class that wraps around the IntPtr. It should be used in conjunction with
     * the 'using' keyword as it will call the Dispose method implemented from the 'IDiposable'
     * interface whenever the instance exits the scope in which it was instantiated.
     *
     * Sample usage:
     *
     *  using (ScopePtr<int> pointer = new ScopePtr<int>())
     *  {
     *      ...
     *  } // Dispose() is called here
     *
     *    or
     *
     *  {
     *      ...
     *      using ScopePtr<int> pointer = new ScopePtr<int>()
     *      ...
     *  } // Dispose is called here
     */
    public sealed class ScopePtr<T> : IDisposable where T : struct
    {
        private IntPtr _ptr;
        private bool _disposed = false;

        private const string DEFAULT_DEBUG_NAME = "Default ScopePtr";
        private readonly string _debugName;

        public IntPtr Raw => _ptr;

        public ScopePtr(string debugName = DEFAULT_DEBUG_NAME)
        {
            _ptr = Marshal.AllocHGlobal(Marshal.SizeOf<T>());
            _debugName = debugName;
        }

        // This constructor transfers ownership of the IntPtr to this class
        public ScopePtr(ref IntPtr ptr, string debugName = DEFAULT_DEBUG_NAME)
        {
            _ptr = ptr;
            ptr = IntPtr.Zero;
            _debugName = debugName;
        }

        public ScopePtr(T source, string debugName = DEFAULT_DEBUG_NAME)
        {
            _ptr = Marshal.AllocHGlobal(Marshal.SizeOf<T>());
            Marshal.StructureToPtr(source, _ptr, false);
            _debugName = debugName;
        }

        // This constructor should be used when creating a ScopePtr for a string
        public ScopePtr(int bufferCount, string debugName = DEFAULT_DEBUG_NAME)
        {
            _ptr = Marshal.AllocHGlobal(bufferCount);
            _debugName = debugName;
        }

        ~ScopePtr()
        {
            DisposeResources();
        }

        public void Dispose()
        {
            DisposeResources();
            GC.SuppressFinalize(this);
        }

        private void DisposeResources()
        {
            if (_disposed) return;
            // dispose unmanaged resources
            Marshal.FreeHGlobal(_ptr);
            _ptr = IntPtr.Zero;

            _disposed = true;
        }

        /// <summary>
        /// Copies 'source' to the ScopePtr
        /// </summary>
        /// <param name="source"></param>
        public void Copy(T source)
        {
            Marshal.StructureToPtr(source, _ptr, false);
        }

        public string AsString()
        {
            return Marshal.PtrToStringAnsi(_ptr);
        }

        public T AsStruct()
        {
            return Marshal.PtrToStructure<T>(_ptr);
        }
    }
}


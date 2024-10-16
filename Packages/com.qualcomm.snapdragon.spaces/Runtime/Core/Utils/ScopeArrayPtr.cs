/******************************************************************************
 * File: ScopeArrayPtr.cs
 * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Qualcomm.Snapdragon.Spaces
{
    /*
    * This is a helper class that wraps around the IntPtr. It should be used in conjunction with
    * the 'using' keyword as it will call the Dispose method implemented from the 'IDiposable'
    * interface whenever the instance exits the scope in which it was instantiated.
    *
    * Sample usage:
    *
    *  using (ScopeArrayPtr<int> pointer = new ScopeArrayPtr<int>(42))
    *  {
    *      ...
    *  } // Dispose() is called here
    *
    *    or
    *
    *  {
    *      ...
    *      using ScopeArrayPtr<int> pointer = new ScopeArrayPtr<int>(42)
    *      ...
    *  } // Dispose is called here
    */
    public sealed class ScopeArrayPtr<T> : IDisposable where T : struct
    {
        private IntPtr _ptr;
        private readonly int _elementCount;
        private bool _disposed = false;

        private const string DEFAULT_DEBUG_NAME = "Default ScopeArrayPtr";
        private readonly string _debugName;

        public int ElementCount => _elementCount;
        public IntPtr Raw => _ptr;


        public ScopeArrayPtr(int count, string debugName = DEFAULT_DEBUG_NAME)
        {
            _ptr = Marshal.AllocHGlobal(Marshal.SizeOf<T>() * count);
            _elementCount = count;
            _debugName = debugName;
        }

        /// <summary>
        /// This constructor transfers ownership of the IntPtr to this class
        /// </summary>
        /// <param name="ptr"></param>
        /// <param name="count"></param>
        /// <param name="debugName"></param>
        public ScopeArrayPtr(ref IntPtr ptr, int count, string debugName = DEFAULT_DEBUG_NAME)
        {
            _ptr = ptr;
            ptr = IntPtr.Zero;
            _elementCount = count;
            _debugName = debugName;
        }

        ~ScopeArrayPtr()
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
        /// Copies 'source' into the struct at 'indexToCopyTo'
        /// </summary>
        /// <param name="source"></param>
        /// <param name="indexToCopyTo"></param>
        public void Copy(T source, int indexToCopyTo)
        {
            if (indexToCopyTo >= _elementCount)
            {
	            Debug.LogError("ScopeArrayPtr's (" + _debugName + ") element count (" + _elementCount + ") is smaller than the specified index (" + indexToCopyTo + ")");
                return;
            }

            Marshal.StructureToPtr(source, _ptr + Marshal.SizeOf<T>() * indexToCopyTo, false);
        }

        /// <summary>
        /// Returns struct T at index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public T AtIndex(int index)
        {
            if (index >= _elementCount)
            {
	            Debug.LogError("ScopeArrayPtr's (" + _debugName + ") element count is smaller than the specified index (" + index + "). Returning default struct");
                return default;
            }
            return Marshal.PtrToStructure<T>(_ptr + Marshal.SizeOf<T>() * index);
        }

        public IntPtr AtIndexRaw(int index)
        {
            if (index >= _elementCount)
            {
                Debug.LogError("ScopeArrayPtr's (" + _debugName + ") element count is smaller than the specified index (" + index + "). Returning default struct");
                return IntPtr.Zero;
            }

            return _ptr + (Marshal.SizeOf<T>() * index);
        }
    }
}


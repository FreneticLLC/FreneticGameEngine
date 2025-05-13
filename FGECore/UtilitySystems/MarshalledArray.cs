using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace FGECore.UtilitySystems;

/// <summary>A class that wraps a native array of unmanaged types, allowing for direct native memory management that avoids the GC where needed.</summary>
public unsafe class MarshalledArray<T>(int size) : IDisposable where T : unmanaged
{
    /// <summary>The length (in units of <typeparamref name="T"/>) of the array.</summary>
    public int Length => size;

    /// <summary>The raw memory pointer for the actual data.</summary>
    public T* Data = (T*)Marshal.AllocHGlobal(size * sizeof(T));

    /// <summary>Zero the memory, and return a copy of the object.</summary>
    public MarshalledArray<T> Zero()
    {
        Unsafe.InitBlockUnaligned(Data, 0, (uint)(size * sizeof(T)));
        return this;
    }

    /// <summary>Gets a value from the underlying array, with safety checks.</summary>
    public ref T this[int index]
    {
        get
        {
            if (unchecked((uint)index) >= size)
            {
                throw new ArgumentException($"Index {index} is out of range for array of size {size}.");
            }
            return ref Data[index];
        }
    }

    /// <summary>If true, the memory is disposed, and the <see cref="Data"/> pointer is null.</summary>
    public bool IsDisposed;

    /// <summary>Disposes of the MarshalledArray, freeing the allocated memory.</summary>
    protected virtual void Dispose(bool disposing)
    {
        if (IsDisposed)
        {
            return;
        }
        IsDisposed = true;
        Marshal.FreeHGlobal((nint)Data);
        Data = null;
    }

    /// <summary>Finalizer for the MarshalledArray class.</summary>
    ~MarshalledArray()
    {
        Dispose(disposing: false);
    }

    /// <summary>Disposes of the MarshalledArray, freeing the allocated memory.</summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace Capitalov
{
    public class MemoryRaider
    {
        #region Imports

        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("Kernel32.dll")]
        private static extern bool ReadProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            [Out] byte[] lpBuffer,
            int nSize,
            IntPtr lpNumberOfBytesRead
        );

        [DllImport("Kernel32.dll")]
        private static extern bool WriteProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            byte[] lpBuffer,
            int size,
            IntPtr lpNumberOfBytesWritten
        );

        [DllImport("kernel32.dll")]
        private static extern uint VirtualQueryEx(
            IntPtr hProcess,
            IntPtr lpAddress,
            out MEMORY_BASIC_INFORMATION lpBuffer,
            uint dwLength
        );

        #endregion

        #region Structs

        [StructLayout(LayoutKind.Sequential)]
        public struct MEMORY_BASIC_INFORMATION
        {
            public IntPtr BaseAddress;
            public IntPtr AllocationBase;
            public uint AllocationProtect;
            public IntPtr RegionSize;
            public uint State;
            public uint Protect;
            public uint Type;
        }

        #endregion

        private Process _process;

        /// <summary>
        /// Attaches to a process by its name.
        /// </summary>
        /// <param name="processName">Name of the process to attach to.</param>
        public void Attach(string processName)
        {
            Process[] processes = Process.GetProcessesByName(processName);
            if (processes.Length == 0)
            {
                throw new ArgumentException($"Process '{processName}' not found.");
            }

            _process = processes[0];
        }

        /// <summary>
        /// Gets the base address of a module by its name.
        /// </summary>
        /// <param name="moduleName">Name of the module.</param>
        /// <returns>Base address of the module.</returns>
        public IntPtr GetModuleBase(string moduleName)
        {
            if (_process == null)
            {
                throw new InvalidOperationException("No process attached.");
            }

            foreach (ProcessModule module in _process.Modules)
            {
                if (module.ModuleName == moduleName)
                {
                    return module.BaseAddress;
                }
            }

            return IntPtr.Zero;
        }

        /// <summary>
        /// Gets a list of all modules in the attached process.
        /// </summary>
        /// <returns>List of module names.</returns>
        public List<string> GetModules()
        {
            if (_process == null)
            {
                throw new InvalidOperationException("No process attached.");
            }

            return _process.Modules.Cast<ProcessModule>().Select(m => m.ModuleName).ToList();
        }

        /// <summary>
        /// Reads a pointer from the specified address.
        /// </summary>
        /// <param name="address">Address to read from.</param>
        /// <returns>Pointer value.</returns>
        public IntPtr ReadPointer(IntPtr address)
        {
            return Read<IntPtr>(address);
        }

        /// <summary>
        /// Reads a value of type T from the specified address.
        /// </summary>
        /// <typeparam name="T">Type of the value to read.</typeparam>
        /// <param name="address">Address to read from.</param>
        /// <returns>Value of type T.</returns>
        public T Read<T>(IntPtr address) where T : struct
        {
            int size = Marshal.SizeOf<T>();
            byte[] buffer = new byte[size];

            if (!ReadProcessMemory(_process.Handle, address, buffer, size, IntPtr.Zero))
            {
                throw new Exception("Failed to read memory.");
            }

            return ByteArrayToStructure<T>(buffer);
        }

        /// <summary>
        /// Reads a Vector3 from the specified address.
        /// </summary>
        /// <param name="address">Address to read from.</param>
        /// <returns>Vector3 value.</returns>
        public Vector3 ReadVector(IntPtr address)
        {
            return new Vector3
            (
                Read<float>(address),
                Read<float>(address + 4),
                Read<float>(address + 8)
            );
        }

        /// <summary>
        /// Reads a string from the specified address.
        /// </summary>
        /// <param name="address">Address to read from.</param>
        /// <param name="length">Length of the string.</param>
        /// <returns>String value.</returns>
        public string ReadString(IntPtr address, int length)
        {
            byte[] buffer = new byte[length];
            if (!ReadProcessMemory(_process.Handle, address, buffer, length, IntPtr.Zero))
            {
                throw new Exception("Failed to read memory.");
            }

            return Encoding.UTF8.GetString(buffer);
        }

        /// <summary>
        /// Reads a matrix (4x4 float array) from the specified address.
        /// </summary>
        /// <param name="address">Address to read from.</param>
        /// <returns>Matrix as a float array.</returns>
        public float[] ReadMatrix(IntPtr address)
        {
            byte[] buffer = new byte[4 * 16];
            if (!ReadProcessMemory(_process.Handle, address, buffer, buffer.Length, IntPtr.Zero))
            {
                throw new Exception("Failed to read memory.");
            }

            float[] matrix = new float[16];
            for (int i = 0; i < 16; i++)
            {
                matrix[i] = BitConverter.ToSingle(buffer, i * 4);
            }

            return matrix;
        }

        /// <summary>
        /// Writes a value of type T to the specified address.
        /// </summary>
        /// <typeparam name="T">Type of the value to write.</typeparam>
        /// <param name="address">Address to write to.</param>
        /// <param name="value">Value to write.</param>
        /// <returns>True if successful, otherwise false.</returns>
        public bool Write<T>(IntPtr address, T value) where T : struct
        {
            byte[] buffer = StructureToByteArray(value);
            return WriteProcessMemory(_process.Handle, address, buffer, buffer.Length, IntPtr.Zero);
        }

        /// <summary>
        /// Writes a Vector3 to the specified address.
        /// </summary>
        /// <param name="address">Address to write to.</param>
        /// <param name="value">Vector3 value to write.</param>
        /// <returns>True if successful, otherwise false.</returns>
        public bool WriteVector(IntPtr address, Vector3 value)
        {
            return Write(address, value.X) && Write(address + 4, value.Y) && Write(address + 8, value.Z);
        }

        /// <summary>
        /// Writes a string to the specified address.
        /// </summary>
        /// <param name="address">Address to write to.</param>
        /// <param name="value">String value to write.</param>
        /// <returns>True if successful, otherwise false.</returns>
        public bool WriteString(IntPtr address, string value)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(value);
            return WriteProcessMemory(_process.Handle, address, buffer, buffer.Length, IntPtr.Zero);
        }

        /// <summary>
        /// Writes an array of bytes to the specified memory address in the attached process.
        /// </summary>
        /// <param name="address">The memory address to write to.</param>
        /// <param name="bytes">The byte array to write.</param>
        /// <returns>True if the write operation succeeds; otherwise, false.</returns>
        public bool WriteBytes(IntPtr address, byte[] bytes)
        {
            return WriteProcessMemory(_process.Handle, address, bytes, bytes.Length, IntPtr.Zero);
        }

        /// <summary>
        /// Fills the specified memory region with NOP instructions (0x90).
        /// </summary>
        /// <param name="address">Address to start writing NOPs.</param>
        /// <param name="length">Number of NOPs to write.</param>
        /// <returns>True if successful, otherwise false.</returns>
        public bool Nop(IntPtr address, int length)
        {
            byte[] buffer = Enumerable.Repeat((byte)0x90, length).ToArray();
            return WriteProcessMemory(_process.Handle, address, buffer, buffer.Length, IntPtr.Zero);
        }

        /// <summary>
        /// Scans the process memory for a specific value of type T.
        /// </summary>
        /// <typeparam name="T">Type of the value to scan for.</typeparam>
        /// <param name="value">Value to search for.</param>
        /// <returns>A dictionary containing addresses and values where the value was found.</returns>
        public Dictionary<IntPtr, T> ScanValue<T>(T value) where T : struct
        {
            var result = new Dictionary<IntPtr, T>();
            IntPtr address = IntPtr.Zero;
            MEMORY_BASIC_INFORMATION mbi;

            while (VirtualQueryEx(_process.Handle, address, out mbi, (uint)Marshal.SizeOf<MEMORY_BASIC_INFORMATION>()) != 0)
            {
                if (mbi.State == 0x1000) // MEM_COMMIT
                {
                    byte[] buffer = new byte[mbi.RegionSize.ToInt32()];
                    if (ReadProcessMemory(_process.Handle, mbi.BaseAddress, buffer, buffer.Length, IntPtr.Zero))
                    {
                        int size = Marshal.SizeOf<T>();
                        for (int i = 0; i <= buffer.Length - size; i++)
                        {
                            T currentValue = ByteArrayToStructure<T>(buffer.Skip(i).Take(size).ToArray());
                            if (EqualityComparer<T>.Default.Equals(currentValue, value))
                            {
                                result[mbi.BaseAddress + i] = currentValue;
                            }
                        }
                    }
                }

                address = (IntPtr)((long)mbi.BaseAddress + mbi.RegionSize.ToInt64());
            }

            return result;
        }

        private T ByteArrayToStructure<T>(byte[] bytes) where T : struct
        {
            T result;
            var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);

            try
            {
                result = Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());
            }
            finally
            {
                handle.Free();
            }
            return result;
        }

        private byte[] StructureToByteArray<T>(T obj) where T : struct
        {
            int size = Marshal.SizeOf<T>();
            byte[] bytes = new byte[size];
            var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);

            try
            {
                Marshal.StructureToPtr(obj, handle.AddrOfPinnedObject(), false);
            }
            finally
            {
                handle.Free();
            }
            return bytes;
        }
    }
}
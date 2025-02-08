using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace Capitalov
{
    public class MemoryRaider
    {
        #region import
        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("Kernel32.dll")]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int nSize, IntPtr lpNumberOfBytesRead);

        [DllImport("Kernel32.dll")]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int size, IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        private static extern uint VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, uint dwLength);
        #endregion

        private Process _process;

        public void Inject(string processName)
        {
            _process = Process.GetProcessesByName(processName)[0];
        }

        public IntPtr GetModuleBase(string moduleName)
        {
            foreach (ProcessModule module in _process.Modules)
            {
                if (module.ModuleName == moduleName)
                {
                    return module.BaseAddress;
                }
            }

            return IntPtr.Zero;
        }

        public List<string> GetModules()
        {
            List<string> modules = new List<string>();

            foreach (ProcessModule module in _process.Modules)
            {
                modules.Add(module.ModuleName);
            }

            return modules;
        }

        public IntPtr ReadPointer(IntPtr ptr)
        {
            byte[] buffer = new byte[8];

            if (ReadProcessMemory(_process.Handle, ptr, buffer, buffer.Length, IntPtr.Zero))
            {
                return (IntPtr)BitConverter.ToInt64(buffer);
            }
            else
            {
                throw new Exception("Failed to read memory");
            }
        }

        public byte[] ReadBytes(IntPtr ptr, int bytes)
        {
            byte[] buffer = new byte[bytes];
            ReadProcessMemory(_process.Handle, ptr, buffer, buffer.Length, IntPtr.Zero);

            return buffer;
        }

        public T Read<T>(IntPtr address) where T : struct
        {
            var size = Marshal.SizeOf(typeof(T));
            var bytes = ReadBytes(address, size);

            return ByteArrayToStructure<T>(bytes);
        }

        public Vector3 ReadVector(IntPtr address)
        {
            return new Vector3(Read<float>(address), Read<float>(address + 4), Read<float>(address + 8));
        }

        public string ReadString(IntPtr address, int length)
        {
            return Encoding.UTF8.GetString(ReadBytes(address, length));
        }

        public string ReadString(IntPtr address, int offset, int length)
        {
            return Encoding.UTF8.GetString(ReadBytes(address + offset, length));
        }

        public float[] ReadMatrix(IntPtr address)
        {
            var bytes = ReadBytes(address, 4 * 16);
            var matrix = new float[16];

            for (int i = 0; i < 16; i++)
            {
                matrix[i] = BitConverter.ToSingle(bytes, i * 4);
            }

            return matrix;
        }

        private T ByteArrayToStructure<T>(byte[] bytes) where T : struct
        {
            T result;
            var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);

            try
            {
                result = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            }
            finally
            {
                handle.Free();
            }

            return result;
        }

        private byte[] StructureToByteArray<T>(T obj) where T : struct
        {
            var size = Marshal.SizeOf(typeof(T));
            var bytes = new byte[size];
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

        public bool WriteBytes(IntPtr address, byte[] newbytes)
        {
            return WriteProcessMemory(_process.Handle, address, newbytes, newbytes.Length, IntPtr.Zero);
        }

        public bool Write<T>(IntPtr address, T value) where T : struct
        {
            var bytes = StructureToByteArray(value);
            return WriteBytes(address, bytes);
        }

        public bool WriteVector(IntPtr address, Vector3 value)
        {
            return Write(address, value.X) && Write(address + 4, value.Y) && Write(address + 8, value.Z);
        }

        public bool WriteString(IntPtr address, string value)
        {
            return WriteBytes(address, Encoding.UTF8.GetBytes(value));
        }

        public bool Nop(IntPtr address, int length)
        {
            byte[] buffer = new byte[length];

            for (int i = 0; i < length; i++)
            {
                buffer[i] = 0x90;
            }

            return WriteBytes(address, buffer);
        }

        public void ChangeValue<T>(T oldValue, T newValue) where T : struct
        {
            IntPtr address = IntPtr.Zero;
            MEMORY_BASIC_INFORMATION mbi;

            while (VirtualQueryEx(_process.Handle, address, out mbi, (uint)Marshal.SizeOf(typeof(MEMORY_BASIC_INFORMATION))) != 0)
            {
                if (mbi.State == 0x1000)
                {
                    byte[] buffer = new byte[mbi.RegionSize.ToInt32()];
                    int bytesRead = 0;

                    if (ReadProcessMemory(_process.Handle, mbi.BaseAddress, buffer, buffer.Length, IntPtr.Zero))
                    {
                        bytesRead = buffer.Length;

                        int size = Marshal.SizeOf(typeof(T));
                        for (int i = 0; i <= bytesRead - size; i++)
                        {
                            T value = ByteArrayToStructure<T>(buffer.Skip(i).Take(size).ToArray());

                            if (EqualityComparer<T>.Default.Equals(value, oldValue))
                            {
                                byte[] newValueBytes = StructureToByteArray(newValue);
                                WriteProcessMemory(_process.Handle, mbi.BaseAddress + i, newValueBytes, newValueBytes.Length, IntPtr.Zero);
                            }
                        }
                    }
                }

                address = (IntPtr)((long)mbi.BaseAddress + mbi.RegionSize.ToInt64());
            }
        }

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
    }
}


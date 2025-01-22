using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace Capitalov
{
    public class MemoryRaider
    {
        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("Kernel32.dll")]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int nSize, IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int size, IntPtr lpNumberOfBytesWritten);

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

        public IntPtr ReadPointer(IntPtr ptr, params int[] offsets)
        {
            foreach (var offset in offsets)
            {
                byte[] array = new byte[4];
                ReadProcessMemory(_process.Handle, ptr + offset, array, array.Length, IntPtr.Zero);
                ptr = (IntPtr)BitConverter.ToInt32(array, 0);
            }
            return ptr;
        }

        public byte[] ReadBytes(IntPtr ptr, int bytes)
        {
            byte[] array = new byte[bytes];
            ReadProcessMemory(_process.Handle, ptr, array, array.Length, IntPtr.Zero);
            return array;
        }

        public T Read<T>(IntPtr address) where T : struct
        {
            var size = Marshal.SizeOf(typeof(T));
            var bytes = ReadBytes(address, size);
            return ByteArrayToStructure<T>(bytes);
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

        public bool WriteBytes(IntPtr address, byte[] newbytes)
        {
            return WriteProcessMemory(_process.Handle, address, newbytes, newbytes.Length, IntPtr.Zero);
        }

        public bool Write<T>(IntPtr address, T value) where T : struct
        {
            var bytes = StructureToByteArray(value);
            return WriteBytes(address, bytes);
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

        public Vector3 ReadVector(IntPtr address)
        {
            return new Vector3(Read<float>(address), Read<float>(address + 4), Read<float>(address + 8));
        }

        public bool WriteVector(IntPtr address, Vector3 value)
        {
            return Write(address, value.X) && Write(address + 4, value.Y) && Write(address + 8, value.Z);
        }

        public string ReadString(IntPtr address, int length)
        {
            return Encoding.UTF8.GetString(ReadBytes(address, length));
        }

        public string ReadString(IntPtr address, int offset, int length)
        {
            return Encoding.UTF8.GetString(ReadBytes(address + offset, length));
        }

        public bool WriteString(IntPtr address, string value)
        {
            return WriteBytes(address, Encoding.UTF8.GetBytes(value));
        }
    }
}

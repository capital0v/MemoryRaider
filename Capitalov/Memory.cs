using System;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace Capitalov
{
    public class Memory
    {
        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("Kernel32.dll")]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int nSize, IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        private static extern IntPtr CreateFile(string lpFileName, uint dwDesiredAccess, uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);

        [DllImport("kernel32.dll")]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int size, IntPtr lpNumberOfBytesWritten);

        private Process _process = new Process();

        public void Inject(string processName)
        {
            _process = Process.GetProcessesByName(processName)[0];
        }

        public IntPtr GetModuleBase(string module)
        {
            try
            {
                foreach (ProcessModule mdl in _process.Modules)
                {
                    if (mdl.ModuleName == module)
                    {
                        return mdl.BaseAddress;
                    }
                }
            }
            catch { }

            return IntPtr.Zero;
        }

        public IntPtr ReadPointer(IntPtr ptr)
        {
            byte[] array = new byte[8];
            ReadProcessMemory(_process.Handle, ptr, array, array.Length, IntPtr.Zero);
            return (IntPtr)BitConverter.ToInt64(array);
        }

        public IntPtr ReadPointer(IntPtr ptr, int offset)
        {
            byte[] array = new byte[8];
            ReadProcessMemory(_process.Handle, ptr + offset, array, array.Length, IntPtr.Zero);
            return (IntPtr)BitConverter.ToInt64(array);
        }

        public IntPtr ReadPointer(long ptr, long offset)
        {
            byte[] array = new byte[8];
            ReadProcessMemory(_process.Handle, (IntPtr)(ptr + offset), array, array.Length, IntPtr.Zero);
            return (IntPtr)BitConverter.ToInt64(array);
        }

        public IntPtr ReadPointer(IntPtr ptr, long offset)
        {
            return ReadPointer(ptr, offset);
        }

        public IntPtr ReadPointer(IntPtr ptr, int[] offsets)
        {
            IntPtr intPtr = ptr;

            foreach (int offset in offsets)
            {
                intPtr = ReadPointer(intPtr, offset);
            }

            return intPtr;
        }

        public IntPtr ReadPointer(IntPtr ptr, long[] offsets)
        {
            IntPtr intPtr = ptr;

            foreach (long offset in offsets)
            {
                intPtr = ReadPointer(intPtr, offset);
            }

            return intPtr;
        }

        public IntPtr ReadPointer(IntPtr ptr, int offset1, int offset2)
        {
            return ReadPointer(ptr, new int[2] { offset1, offset2 });
        }

        public IntPtr ReadPointer(IntPtr ptr, int offset1, int offset2, int offset3)
        {
            return ReadPointer(ptr, new int[3] { offset1, offset2, offset3 });
        }

        public IntPtr ReadPointer(IntPtr ptr, int offset1, int offset2, int offset3, int offset4)
        {
            return ReadPointer(ptr, new int[4] { offset1, offset2, offset3, offset4 });
        }

        public IntPtr ReadPointer(IntPtr ptr, int offset1, int offset2, int offset3, int offset4, int offset5)
        {
            return ReadPointer(ptr, new int[5] { offset1, offset2, offset3, offset4, offset5 });
        }

        public IntPtr ReadPointer(IntPtr ptr, int offset1, int offset2, int offset3, int offset4, int offset5, int offset6)
        {
            return ReadPointer(ptr, new int[6] { offset1, offset2, offset3, offset4, offset5, offset6 });
        }

        public IntPtr ReadPointer(IntPtr ptr, int offset1, int offset2, int offset3, int offset4, int offset5, int offset6, int offset7)
        {
            return ReadPointer(ptr, new int[7] { offset1, offset2, offset3, offset4, offset5, offset6, offset7 });
        }

        public IntPtr ReadPointer(IntPtr ptr, long offset1, long offset2)
        {
            return ReadPointer(ptr, new long[2] { offset1, offset2 });
        }

        public IntPtr ReadPointer(IntPtr ptr, long offset1, long offset2, long offset3)
        {
            return ReadPointer(ptr, new long[3] { offset1, offset2, offset3 });
        }

        public IntPtr ReadPointer(IntPtr ptr, long offset1, long offset2, long offset3, long offset4)
        {
            return ReadPointer(ptr, new long[4] { offset1, offset2, offset3, offset4 });
        }

        public IntPtr ReadPointer(IntPtr ptr, long offset1, long offset2, long offset3, long offset4, long offset5)
        {
            return ReadPointer(ptr, new long[5] { offset1, offset2, offset3, offset4, offset5 });
        }

        public IntPtr ReadPointer(IntPtr ptr, long offset1, long offset2, long offset3, long offset4, long offset5, long offset6)
        {
            return ReadPointer(ptr, new long[6] { offset1, offset2, offset3, offset4, offset5, offset6 });
        }

        public IntPtr ReadPointer(IntPtr ptr, long offset1, long offset2, long offset3, long offset4, long offset5, long offset6, long offset7)
        {
            return ReadPointer(ptr, new long[7] { offset1, offset2, offset3, offset4, offset5, offset6, offset7 });
        }

        public IntPtr ReadPointer(IntPtr ptr, IntPtr offset1, int offset2)
        {
            IntPtr ptr2 = ReadPointer((IntPtr)((long)ptr + (long)offset1));
            return ReadPointer(ptr2, offset2);
        }

        public byte[] ReadBytes(IntPtr ptr, int bytes)
        {
            byte[] array = new byte[bytes];
            ReadProcessMemory(_process.Handle, ptr, array, array.Length, IntPtr.Zero);
            return array;
        }
        public byte[] ReadBytes(IntPtr ptr, int offset, int bytes)
        {
            byte[] array = new byte[bytes];
            ReadProcessMemory(_process.Handle, ptr + offset, array, array.Length, IntPtr.Zero);
            return array;
        }
        public int ReadInt(IntPtr address)
        {
            return BitConverter.ToInt32(ReadBytes(address, 4));
        }
        public int ReadInt(IntPtr address, int offset)
        {
            return BitConverter.ToInt32(ReadBytes(address + offset, 4));
        }
        public IntPtr ReadLong(IntPtr address)
        {
            return (IntPtr)BitConverter.ToInt64(ReadBytes(address, 8));
        }
        public IntPtr ReadLong(IntPtr address, int offset)
        {
            return (IntPtr)BitConverter.ToInt64(ReadBytes(address + offset, 8));
        }
        public float ReadFloat(IntPtr address)
        {
            return BitConverter.ToSingle(ReadBytes(address, 4));
        }

        public float ReadFloat(IntPtr address, int offset)
        {
            return BitConverter.ToSingle(ReadBytes(address + offset, 4));
        }

        public double ReadDouble(IntPtr address)
        {
            return BitConverter.ToDouble(ReadBytes(address, 8));
        }

        public double ReadDouble(IntPtr address, int offset)
        {
            return BitConverter.ToDouble(ReadBytes(address + offset, 4));
        }

        public short ReadShort(IntPtr address)
        {
            return BitConverter.ToInt16(ReadBytes(address, 2));
        }

        public short ReadShort(IntPtr address, int offset)
        {
            return BitConverter.ToInt16(ReadBytes(address + offset, 2));
        }

        public ushort ReadUShort(IntPtr address)
        {
            return BitConverter.ToUInt16(ReadBytes(address, 2));
        }

        public ushort ReadUShort(IntPtr address, int offset)
        {
            return BitConverter.ToUInt16(ReadBytes(address + offset, 2));
        }

        public uint ReadUInt(IntPtr address)
        {
            return BitConverter.ToUInt32(ReadBytes(address, 4));
        }

        public uint ReadUInt(IntPtr address, int offset)
        {
            return BitConverter.ToUInt32(ReadBytes(address + offset, 4));
        }

        public ulong ReadULong(IntPtr address)
        {
            return BitConverter.ToUInt64(ReadBytes(address, 8));
        }

        public ulong ReadULong(IntPtr address, int offset)
        {
            return BitConverter.ToUInt64(ReadBytes(address + offset, 8));
        }

        public bool ReadBool(IntPtr address)
        {
            return BitConverter.ToBoolean(ReadBytes(address, 1));
        }

        public bool ReadBool(IntPtr address, int offset)
        {
            return BitConverter.ToBoolean(ReadBytes(address + offset, 1));
        }

        public string ReadString(IntPtr address, int length)
        {
            return Encoding.UTF8.GetString(ReadBytes(address, length));
        }

        public string ReadString(IntPtr address, int offset, int length)
        {
            return Encoding.UTF8.GetString(ReadBytes(address + offset, length));
        }

        public char ReadChar(IntPtr address)
        {
            return BitConverter.ToChar(ReadBytes(address, 2));
        }

        public char ReadChar(IntPtr address, int offset)
        {
            return BitConverter.ToChar(ReadBytes(address + offset, 2));
        }

        public Vector3 ReadVec(IntPtr address)
        {
            int length = 12;

            var bytes = ReadBytes(address, length);

            return new Vector3
            {
                X = BitConverter.ToSingle(bytes, 0),
                Y = BitConverter.ToSingle(bytes, 4),
                Z = BitConverter.ToSingle(bytes, 8)
            };
        }

        public Vector3 ReadVec(IntPtr address, int offset)
        {
            int length = 12;

            var bytes = ReadBytes(address + offset, length);

            return new Vector3
            {
                X = BitConverter.ToSingle(bytes, 0),
                Y = BitConverter.ToSingle(bytes, 4),
                Z = BitConverter.ToSingle(bytes, 8)
            };
        }

        public bool WriteBytes(IntPtr address, byte[] newbytes)
        {
            return WriteProcessMemory(_process.Handle, address, newbytes, newbytes.Length, IntPtr.Zero);
        }

        public bool WriteBytes(IntPtr address, int offset, byte[] newbytes)
        {
            return WriteProcessMemory(_process.Handle, address + offset, newbytes, newbytes.Length, IntPtr.Zero);
        }

        public bool WriteBytes(IntPtr address, string newbytes)
        {
            byte[] array = (from _byte in newbytes.Split(' ') select Convert.ToByte(_byte, 16)).ToArray();
            return WriteProcessMemory(_process.Handle, address, array, array.Length, IntPtr.Zero);
        }

        public bool WriteBytes(IntPtr address, int offset, string newbytes)
        {
            byte[] array = (from _byte in newbytes.Split(' ') select Convert.ToByte(_byte, 16)).ToArray();
            return WriteProcessMemory(_process.Handle, address + offset, array, array.Length, IntPtr.Zero);
        }

        public bool WriteInt(IntPtr address, int value)
        {
            return WriteBytes(address, BitConverter.GetBytes(value));
        }

        public bool WriteInt(IntPtr address, int offset, int value)
        {
            return WriteBytes(address + offset, BitConverter.GetBytes(value));
        }

        public bool WriteShort(IntPtr address, short value)
        {
            return WriteBytes(address, BitConverter.GetBytes(value));
        }

        public bool WriteShort(IntPtr address, int offset, short value)
        {
            return WriteBytes(address + offset, BitConverter.GetBytes(value));
        }

        public bool WriteUShort(IntPtr address, ushort value)
        {
            return WriteBytes(address, BitConverter.GetBytes(value));
        }

        public bool WriteUShort(IntPtr address, int offset, ushort value)
        {
            return WriteBytes(address + offset, BitConverter.GetBytes(value));
        }

        public bool WriteUInt(IntPtr address, uint value)
        {
            return WriteBytes(address, BitConverter.GetBytes(value));
        }

        public bool WriteUInt(IntPtr address, int offset, uint value)
        {
            return WriteBytes(address + offset, BitConverter.GetBytes(value));
        }

        public bool WriteLong(IntPtr address, long value)
        {
            return WriteBytes(address, BitConverter.GetBytes(value));
        }

        public bool WriteLong(IntPtr address, int offset, long value)
        {
            return WriteBytes(address + offset, BitConverter.GetBytes(value));
        }

        public bool WriteULong(IntPtr address, ulong value)
        {
            return WriteBytes(address, BitConverter.GetBytes(value));
        }

        public bool WriteULong(IntPtr address, int offset, ulong value)
        {
            return WriteBytes(address + offset, BitConverter.GetBytes(value));
        }

        public bool WriteFloat(IntPtr address, float value)
        {
            return WriteBytes(address, BitConverter.GetBytes(value));
        }

        public bool WriteFloat(IntPtr address, int offset, float value)
        {
            return WriteBytes(address + offset, BitConverter.GetBytes(value));
        }

        public bool WriteDouble(IntPtr address, double value)
        {
            return WriteBytes(address, BitConverter.GetBytes(value));
        }

        public bool WriteDouble(IntPtr address, int offset, double value)
        {
            return WriteBytes(address + offset, BitConverter.GetBytes(value));
        }

        public bool WriteBool(IntPtr address, bool value)
        {
            return WriteBytes(address, BitConverter.GetBytes(value));
        }

        public bool WriteBool(IntPtr address, int offset, bool value)
        {
            return WriteBytes(address + offset, BitConverter.GetBytes(value));
        }

        public bool WriteString(IntPtr address, string value)
        {
            return WriteBytes(address, Encoding.UTF8.GetBytes(value));
        }

        public bool WriteVec(IntPtr address, Vector3 value)
        {
            byte[] array = new byte[12];

            byte[] x = BitConverter.GetBytes(value.X);
            byte[] y = BitConverter.GetBytes(value.Y);
            byte[] z = BitConverter.GetBytes(value.Z);

            x.CopyTo(array, 0);
            y.CopyTo(array, 4);
            z.CopyTo(array, 8);

            return WriteBytes(address, array);
        }

        public bool WriteVec(IntPtr address, int offset, Vector3 value)
        {
            byte[] array = new byte[12];

            byte[] x = BitConverter.GetBytes(value.X);
            byte[] y = BitConverter.GetBytes(value.Y);
            byte[] z = BitConverter.GetBytes(value.Z);

            x.CopyTo(array, 0);
            y.CopyTo(array, 4);
            z.CopyTo(array, 8);

            return WriteBytes(address + offset, array);
        }
    }
}
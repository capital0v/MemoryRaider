# Memory Raider Library

Memory Raider is a library for working with process memory in Windows. It allows you to read and write data in the memory of other processes.

## Installation

To use the library, add a reference to the `Capitalov` project in your C# project.

## Main Methods
- `Inject(string processName)`: "Injects into a process by name."
- `"GetModuleBase(string moduleName)`: Retrieves the base address of a module."
- `"ReadPointer(IntPtr ptr, params int[] offsets)`: Reads a pointer with offsets."
- `"ReadBytes(IntPtr ptr, int bytes)`: Reads an array of bytes from memory."
- `"Read<T>(IntPtr address)`: Reads a structure from memory."
- `"WriteBytes(IntPtr address, byte[] newbytes)`: Writes an array of bytes to memory."
- `"Write<T>(IntPtr address, T value)`: Writes a structure to memory."
- `"ReadVector(IntPtr address)`: Reads a vector from memory."
- `"WriteVector(IntPtr address, Vector3 value)`: Writes a vector to memory."
- `"ReadString(IntPtr address, int length)`: Reads a string from memory."

## Usage Example

Here’s a simple example demonstrating how to use the Memory Raider library:

```csharp
using System;
using System.Threading;
using Capitalov;

class Program
{
    static void Main(string[] args)
    {
        MemoryRaider memory = new MemoryRaider();

        memory.Inject("MyGame");

        IntPtr client = memory.GetModuleBase("test.dll");

        int baseAddress = 0x123456;
        int offset = 0x12;

        while (true)
        {
            IntPtr pointer = memory.ReadPointer(client, baseAddress);
            memory.Write<float>(pointer + offset, 10);
            Thread.Sleep(1);
        }
    }
}

```

![Example of reading a value](https://github.com/capital0v/MemoryRaider/tree/main/img/preview.png)

## License

[MIT](https://github.com/capital0v/MemoryRaider/blob/main/LICENSE)
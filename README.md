# MemoryRaider Documentation

## Overview

`MemoryRaider` is a C# library designed for reading and writing memory of other processes. It provides functionality to inject into a process, read and write various data types, and manipulate memory directly. This can be useful for debugging, game hacking, or any application that requires direct memory access.

## Features

- **Process Injection**: Attach to a running process by its name.
- **Module Management**: Retrieve loaded modules and their base addresses.
- **Memory Reading**: Read bytes, structures, pointers, and strings from the target process's memory.
- **Memory Writing**: Write bytes, structures, and strings to the target process's memory.
- **Value Manipulation**: Change specific values in memory by searching for them.

## Installation

To use the library, add a reference to the `Capitalov` project in your C# project.

## Usage

### Injecting into a Process

To start using `MemoryRaider`, you need to capture the process:

```csharp
using Capitalov;
MemoryRaider raider = new MemoryRaider();
raider.Attach("processName");
```

## Reading Memory
#### Read Pointer
```csharp
IntPtr baseAddress = raider.GetModuleBase("moduleName.dll");
IntPtr pointer = raider.ReadPointer(baseAddress, 0x10, 0x20); // Example offsets
```
### Read Bytes
#### To read a specific number of bytes:
```csharp
byte[] data = raider.ReadBytes(pointer, 64); // Read 64 bytes
```
### Read Vector
#### To read a Vector3 from memory:
```csharp
Vector3 position = raider.ReadVector(pointer);
```
### Read String
#### To read a string from memory:
```csharp
string myString = raider.ReadString(pointer, 20); // Read 20 bytes as a string
```
## Writing Memory
### Write Bytes
#### To write bytes to memory:
```csharp
byte[] newData = new byte[] { 0x01, 0x02, 0x03 };
raider.WriteBytes(pointer, newData);
```
### Write Vector
#### To write a Vector3 to memory:
```csharp
Vector3 newPosition = new Vector3(1.0f, 2.0f, 3.0f);
raider.WriteVector(pointer, newPosition);
```
### Write String
#### To write a string to memory:
```csharp
raider.WriteString(pointer, "Hello, World!");
```
## Changing Values
### To change a specific value in memory:
```csharp
raider.ChangeValue(oldValue, newValue);
```
## License
[MIT](https://github.com/capital0v/MemoryRaider/blob/main/LICENSE)
# Capitalov Memory Library
### Just a little tutorial
```
using Capitalov;

Memory memory = new Memory();

memory.Inject("MyGame");

nint client = memory.GetModuleBase("test.dll");

int baseAddress = 0x123456;
int offset = 0x12;

while (true)
{
    IntPtr pointer = memory.ReadPointer(client, baseAddress);
    memory.WriteFloat(pointer, offset, 10);
    Thread.Sleep(1);
}
```
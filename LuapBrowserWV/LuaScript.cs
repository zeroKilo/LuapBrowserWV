using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuapBrowserWV
{
    public class LuaScript
    {
        public bool isBigEndian;
        public byte sizeInteger;
        public byte sizeSizeT;
        public byte sizeInstructions;
        public byte sizeLuaNumber;
        public bool useIntegralNumbers;
        public bool usebyteCodeSharing;
        public bool unknown1;
        public bool unknown2;
        public LuaFunction mainFunc;

        public LuaScript(byte[] data)
        {
            MemoryStream m = new MemoryStream(data);
            uint magic = Helper.ReadU32(m);
            if (magic != 0x1b4c7561 && magic != 0x61754c1b) //Lua Magic
                return;
            if (m.ReadByte() != 0x51) //lua 5.1 compatible
                return;
            if (m.ReadByte() != 0) //format version
                return;
            isBigEndian = m.ReadByte() != 0;
            sizeInteger = (byte)m.ReadByte();
            sizeSizeT = (byte)m.ReadByte();
            sizeInstructions = (byte)m.ReadByte();
            sizeLuaNumber = (byte)m.ReadByte();
            useIntegralNumbers = m.ReadByte() != 0;
            mainFunc = new LuaFunction(m);
        }

        public byte[] Save()
        {
            MemoryStream m = new MemoryStream();
            Helper.WriteU32(m, 0x1b4c7561);
            m.WriteByte(0x51);
            m.WriteByte(0);
            m.WriteByte((byte)(isBigEndian ? 1 : 0));
            m.WriteByte(sizeInteger);
            m.WriteByte(sizeSizeT);
            m.WriteByte(sizeInstructions);
            m.WriteByte(sizeLuaNumber);
            m.WriteByte((byte)(useIntegralNumbers ? 1 : 0));
            mainFunc.Save(m);
            return m.ToArray();
        }
    }
}

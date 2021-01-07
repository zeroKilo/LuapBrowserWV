using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuapBrowserWV
{
    public class LuaPackage
    {
        public class LuaScriptEntry
        {
            public byte[] hash;
            public int offest;
            public int length1;
            public int length2;
            public bool isModule;
            public byte[] rawData;
            public LuaScript script;
            public LuaScriptEntry(Stream s)
            {
                hash = new byte[8];
                s.Read(hash, 0, 8);
                offest = Helper.ReadS32(s);
                length1 = Helper.ReadS32(s);
                length2 = Helper.ReadS32(s);
                isModule = s.ReadByte() != 0;
                rawData = new byte[length1];
                long pos = s.Position;
                s.Seek(offest, 0);
                s.Read(rawData, 0, length1);
                s.Position = pos;
                script = new LuaScript(rawData);
            }

            public void SaveHeader(Stream s)
            {
                s.Write(hash, 0, 8);
                Helper.WriteS32(s, offest);
                Helper.WriteS32(s, length1);
                Helper.WriteS32(s, length2);
                s.WriteByte((byte)(isModule ? 1 : 0));
            }

            public void SaveEntryData(Stream s)
            {
                byte[] data = script.Save();
                offest = (int)s.Position;
                length1 = length2 = data.Length;
                s.Write(data, 0, length1);
            }
        }

        public List<LuaScriptEntry> entries = new List<LuaScriptEntry>();

        public LuaPackage(string path)
        {
            MemoryStream m = new MemoryStream(File.ReadAllBytes(path));
            uint count = Helper.ReadU32(m);
            for (int i = 0; i < count; i++)
                entries.Add(new LuaScriptEntry(m));
        }

        public void Save(string path)
        {
            MemoryStream m = new MemoryStream();
            Helper.WriteS32(m, entries.Count);
            foreach (LuaScriptEntry entry in entries)
                entry.SaveHeader(m);
            foreach (LuaScriptEntry entry in entries)
                entry.SaveEntryData(m);
            m.Seek(4, 0);
            foreach (LuaScriptEntry entry in entries)
                entry.SaveHeader(m);
            File.WriteAllBytes(path, m.ToArray());
        }
    }
}

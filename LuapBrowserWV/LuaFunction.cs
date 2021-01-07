using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuapBrowserWV
{
    public class LuaFunction
    {
        public class LocalVar
        {
            public string name;
            public uint start;
            public uint end;
            public LocalVar(Stream s)
            {
                name = Helper.ReadString(s);
                start = Helper.ReadU32(s);
                end = Helper.ReadU32(s);
            }

            public override string ToString()
            {
                return name + " (" + start + " - " + end + ")";
            }

            public void Save(Stream s)
            {
                Helper.WriteString(s, name);
                Helper.WriteU32(s, start);
                Helper.WriteU32(s, end);
            }
        }

        public string source;
        public uint lineDefined;
        public uint lastLineDefined;
        public byte numUps;
        public byte numParams;
        public byte is_vararg;
        public byte maxStackSize;
        public List<uint> byteCode = new List<uint>();
        public List<LuaConstant> constants = new List<LuaConstant>();
        public List<LuaFunction> subFunc = new List<LuaFunction>();
        public List<uint> lineInfo = new List<uint>();
        public List<LocalVar> locals = new List<LocalVar>();
        public List<string> upVars = new List<string>();

        public LuaFunction(Stream s)
        {
            source = Helper.ReadString(s);
            lineDefined = Helper.ReadU32(s);
            lastLineDefined = Helper.ReadU32(s);
            numUps = (byte)s.ReadByte();
            numParams = (byte)s.ReadByte();
            is_vararg = (byte)s.ReadByte();
            maxStackSize = (byte)s.ReadByte();
            uint count = Helper.ReadU32(s);
            for (int i = 0; i < count; i++)
                byteCode.Add(Helper.ReadU32(s));
            count = Helper.ReadU32(s);
            for (int i = 0; i < count; i++)
                constants.Add(new LuaConstant(s));
            count = Helper.ReadU32(s);
            for (int i = 0; i < count; i++)
                subFunc.Add(new LuaFunction(s));
            count = Helper.ReadU32(s);
            for (int i = 0; i < count; i++)
                lineInfo.Add(Helper.ReadU32(s));
            count = Helper.ReadU32(s);
            for (int i = 0; i < count; i++)
                locals.Add(new LocalVar(s));
            count = Helper.ReadU32(s);
            for (int i = 0; i < count; i++)
                upVars.Add(Helper.ReadString(s));
        }

        public void Save(Stream s)
        {
            Helper.WriteString(s, source);
            Helper.WriteU32(s, lineDefined);
            Helper.WriteU32(s, lastLineDefined);
            s.WriteByte(numUps);
            s.WriteByte(numParams);
            s.WriteByte(is_vararg);
            s.WriteByte(maxStackSize);
            Helper.WriteS32(s, byteCode.Count);
            foreach (uint v in byteCode)
                Helper.WriteU32(s, v);
            Helper.WriteS32(s, constants.Count);
            foreach (LuaConstant c in constants)
                c.Save(s);
            Helper.WriteS32(s, subFunc.Count);
            foreach (LuaFunction f in subFunc)
                f.Save(s);
            Helper.WriteS32(s, lineInfo.Count);
            foreach (uint u in lineInfo)
                Helper.WriteU32(s, u);
            Helper.WriteS32(s, locals.Count);
            foreach (LocalVar l in locals)
                l.Save(s);
            Helper.WriteS32(s, upVars.Count);
            foreach (string u in upVars)
                Helper.WriteString(s, u);
        }

        public string Dump(int tabs)
        {
            StringBuilder sb = new StringBuilder();
            string t = "";
            for (int i = 0; i < tabs; i++)
                t += "\t";
            sb.AppendLine(t + "###########################################");
            sb.AppendLine(t + "Source = " + source);
            sb.AppendLine(t + "Line Defined = " + lineDefined);
            sb.AppendLine(t + "Last Line Defined = " + lastLineDefined);
            sb.AppendLine(t + "Num Upvars = " + numUps);
            sb.AppendLine(t + "Num Params = " + numParams);
            sb.AppendLine(t + "Is Variadic = " + is_vararg);
            sb.AppendLine(t + "Max Stack Size = " + maxStackSize);
            sb.AppendLine();
            sb.AppendLine(t + "Constants:");
            for (int i = 0; i < constants.Count; i++)
                sb.AppendLine(t + i + " : [" + constants[i].type + "] = " + constants[i]);
            sb.AppendLine();
            sb.AppendLine(t + "Local Vars:");
            for (int i = 0; i < locals.Count; i++)
                sb.AppendLine(t + i + " : " + locals[i]);
            sb.AppendLine();
            sb.AppendLine(t + "Up Vars:");
            for (int i = 0; i < upVars.Count; i++)
                sb.AppendLine(t + i + " : " + upVars[i]);
            sb.AppendLine();
            sb.AppendLine(t + "Bytecode:");
            for (int i = 0; i < byteCode.Count; i++)
                sb.AppendLine(t + "<" + (i + 1).ToString("D4") + "> : " + byteCode[i].ToString("X8") + " " + new LuaOpcode(byteCode[i]).Print(i + 1, this));
            sb.AppendLine();
            sb.AppendLine(t + "Sub Functions:");
            for (int i = 0; i < subFunc.Count; i++)
                sb.Append(subFunc[i].Dump(tabs + 1));
            sb.AppendLine();
            return sb.ToString();
        }
    }
}

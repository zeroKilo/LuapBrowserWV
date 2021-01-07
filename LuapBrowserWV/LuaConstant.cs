using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuapBrowserWV
{
    public class LuaConstant
    {
        public enum TYPE
        {
            NULL = 0,
            BOOLEAN = 1,
            RESERVED = 2,
            NUMBER = 3,
            STRING = 4
        }

        public TYPE type;
        public object value;

        public LuaConstant(TYPE t, object v)
        {
            type = t;
            value = v;
        }
        public LuaConstant(Stream s)
        {
            type = (TYPE)s.ReadByte();
            switch (type)
            {
                case TYPE.NULL:
                    break;
                case TYPE.BOOLEAN:
                    value = s.ReadByte() != 0;
                    break;
                case TYPE.NUMBER:
                    value = Helper.ReadDouble(s);
                    break;
                case TYPE.STRING:
                    value = Helper.ReadString(s);
                    break;
                default:
                    throw new Exception("Unknown constant type");
            }
        }

        public void Save(Stream s)
        {
            s.WriteByte((byte)type);
            switch (type)
            {
                case TYPE.NULL:
                    break;
                case TYPE.BOOLEAN:
                    s.WriteByte((byte)((bool)value ? 1 : 0));
                    break;
                case TYPE.NUMBER:
                    Helper.WriteDouble(s, (double)value);
                    break;
                case TYPE.STRING:
                    Helper.WriteString(s, (string)value);
                    break;
                default:
                    throw new Exception("Unknown constant type");
            }
        }

        public override string ToString()
        {
            switch (type)
            {
                case TYPE.NULL:
                    return "NULL";
                case TYPE.BOOLEAN:
                    return ((bool)value).ToString();
                case TYPE.NUMBER:
                    return ((double)value).ToString();
                case TYPE.STRING:
                    return "\""+ (string)value + "\"";
                default:
                    throw new Exception("Unknown constant type");                    
            }
        }
    }
}

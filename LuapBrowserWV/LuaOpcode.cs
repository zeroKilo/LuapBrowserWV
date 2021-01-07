using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuapBrowserWV
{
    public class LuaOpcode
    {
        public static string[] opcNames = new string[]
        {
            "OP_MOVE",      //00
            "OP_LOADK",     //01
            "OP_LOADBOOL",  //02
            "OP_LOADNIL",   //03
            "OP_GETUPVAL",  //04
            "OP_GETGLOBAL", //05
            "OP_GETTABLE",  //06
            "OP_SETGLOBAL", //07
            "OP_SETUPVAL",  //08
            "OP_SETTABLE",  //09
            "OP_NEWTABLE",  //0A
            "OP_SELF",      //0B
            "OP_ADD",       //0C
            "OP_SUB",       //0D
            "OP_MUL",       //0E
            "OP_DIV",       //0F
            "OP_MOD",       //10
            "OP_POW",       //11
            "OP_UNM",       //12
            "OP_NOT",       //13
            "OP_LEN",       //14
            "OP_CONCAT",    //15
            "OP_JMP",       //16
            "OP_EQ",        //17
            "OP_LT",        //18
            "OP_LE",        //19
            "OP_TEST",      //1A
            "OP_TESTSET",   //1B
            "OP_CALL",      //1C
            "OP_TAILCALL",  //1D
            "OP_RETURN",    //1E
            "OP_FORLOOP",   //1F
            "OP_FORPREP",   //20
            "OP_TFORLOOP",  //21
            "OP_SETLIST",   //22
            "OP_CLOSE",     //23
            "OP_CLOSURE",   //24
            "OP_VARARG"     //25
        };

        public uint ID;
        public uint A, B, C;
        public uint Bx;
        public int sBx;
        public int sB, sC;
        public LuaOpcode(uint u)
        {
            //FEDCBA9876543210FEDCBA9876543210 bitpos
            //BBBBBBBBBCCCCCCCCCAAAAAAAAIIIIII layout ABC
            //XXXXXXXXXXXXXXXXXXAAAAAAAAIIIIII layout ABx /AsBx
            ID = u & 0x3F;
            A = (u >> 6) & 0xFF;
            B = (u >> 23) & 0x1FF;
            C = (u >> 14) & 0x1FF;
            Bx = (B << 9) | C;
            sBx = (int)Bx - 0x1FFFF;
            if(B > 0xFF)
                sB = 0xFF - (int)B;
            else
                sB = (int)B;
            if (C > 0xFF)
                sC = 0xFF - (int)C;
            else
                sC = (int)C;
        }

        public string Print(int idx, LuaFunction func)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[" + ID.ToString("X2") + "] ");
            switch (ID)
            {
                case 0://OP_MOVE
                    sb.Append("R" + A + " = R" + B);
                    break;
                case 1://OP_LOADK
                    sb.Append("R" + A + " = " + func.constants[(int)Bx]);
                    break;
                case 2://OP_LOADBOOL
                    sb.Append("R" + A + " = " + (B == 1 ? "true" : "false"));
                    if (C != 0)
                        sb.Append(" goto <" + (idx + 1) + ">");
                    break;
                case 3://OP_LOADNIL
                    for (uint i = A; i < B; i++)
                        sb.Append("R" + i + ", ");
                    sb.Append("R" + B + " = NULL");
                    break;
                case 4://OP_GETUPVAL
                    sb.Append("R" + A + " = " + func.upVars[(int)B]);
                    break;
                case 5://OP_GETGLOBAL
                    if (func.constants.Count > Bx)
                        sb.Append("R" + A + " = GLOBALS[" + func.constants[(int)Bx] + "]");
                    break;
                case 6://OP_GETTABLE
                    sb.Append("R" + A + " = R" + B + "[");
                    sb.Append(GetConstantOrRegister(func, sC));
                    sb.Append("]");
                    break;
                case 7://OP_SETGLOBAL
                    if (func.global.constants.Count > Bx)
                        sb.Append("GLOBALS[" + func.global.constants[(int)Bx] + "] = R" + A);
                    break;
                case 8://OP_SETUPVAL
                    sb.Append(func.upVars[(int)B] + " = R" + A);
                    break;
                case 9://OP_SETTABLE
                    sb.Append("R" + A + "[");
                    sb.Append(GetConstantOrRegister(func, sB));
                    sb.Append("] = ");
                    sb.Append(GetConstantOrRegister(func, sC));
                    break;
                case 0xA://OP_NEWTABLE
                    sb.Append("R" + A + " = {} (size = " + B + ", " + C + ")");
                    break;
                case 0xB:
                    sb.Append("R" + (A + 1) + " = R" + B + "; R" + A + " = R" + B + "[");
                    sb.Append(GetConstantOrRegister(func, sC));
                    sb.Append("]");
                    break;
                case 0xC: //OP_ADD
                    sb.Append(MakeBasicMathOperation(func, "+"));
                    break;
                case 0xD: //OP_SUB
                    sb.Append(MakeBasicMathOperation(func, "-"));
                    break;
                case 0xE: //OP_MUL
                    sb.Append(MakeBasicMathOperation(func, "*"));
                    break;
                case 0xF: //OP_DIV
                    sb.Append(MakeBasicMathOperation(func, "/"));
                    break;
                case 0x10: //OP_MOD
                    sb.Append(MakeBasicMathOperation(func, "%"));
                    break;
                case 0x11: //OP_POW
                    sb.Append(MakeBasicMathOperation(func, "^"));
                    break;
                case 0x12: //OP_UNM
                    sb.Append(MakeBasicBinaryOperation(func, "-"));
                    break;
                case 0x13: //OP_NOT
                    sb.Append(MakeBasicBinaryOperation(func, "!"));
                    break;
                case 0x14: //OP_LEN
                    sb.Append("R" + A + " = LEN(R" + B + ")");
                    break;
                case 0x15: //OP_CONCAT
                    sb.Append("R" + A + " = ");
                    for (uint i = B; i < C; i++)
                        sb.Append("R" + i + "..");
                    sb.Append("R" + C);
                    break;
                case 0x16://OP_JMP
                    if (sBx != 0)
                        sb.Append("GOTO <" + (sBx + idx + 1) + ">");
                    else
                        sb.Append("NOP");
                    break;
                case 0x17://OP_EQ
                    sb.Append("if ");
                    sb.Append(GetConstantOrRegister(func, sB));
                    if (A == 0)
                        sb.Append(" == ");
                    else
                        sb.Append(" != ");
                    sb.Append(GetConstantOrRegister(func, sC));
                    sb.Append(" then GOTO <" + (idx + 2) + ">");
                    break;
                case 0x18://OP_LT
                    sb.Append("if ");
                    sb.Append(GetConstantOrRegister(func, sB));
                    if (A == 0)
                        sb.Append(" < ");
                    else
                        sb.Append(" > ");
                    sb.Append(GetConstantOrRegister(func, sC));
                    sb.Append(" then GOTO <" + (idx + 2) + ">");
                    break;
                case 0x19://OP_LE
                    sb.Append("if ");
                    sb.Append(GetConstantOrRegister(func, sB));
                    if (A == 0)
                        sb.Append(" <= ");
                    else
                        sb.Append(" >= ");
                    sb.Append(GetConstantOrRegister(func, sC));
                    sb.Append(" then GOTO <" + (idx + 2) + ">");
                    break;
                case 0x1A://OP_TEST
                    sb.Append("if R" + A + " != " + (C == 1 ? "true" : "false") + " then GOTO <" + (idx + 2) + ">");
                    break;
                case 0x1B://OP_TESTSET
                    sb.Append("if R" + B + " != " + (C == 1 ? "true" : "false") + " then GOTO <" + (idx + 2) + "> else R" + A + " = R" + B);
                    break;
                case 0x1C://OP_CALL
                    if(C== 0)
                        sb.Append("R" + A + ",.. = ");
                    else
                    {
                        sb.Append("R" + A);
                        for (int i = 1; i < C - 1; i++)
                            sb.Append(", R" + (A + i));
                        sb.Append(" = ");
                    }
                    sb.Append("R" + A + "(R" + (A + 1));
                    if (B == 0)
                        sb.Append(",..)");
                    else
                    {
                        for (int i = 1; i < B - 1; i++)
                            sb.Append(", R" + (A + i + 1));
                        sb.Append(")");
                    }
                    break;
                case 0x1D://OP_TAILCALL                    
                    sb.Append("RETURN R" + A + "(");
                    if (B == 0)
                        sb.Append("R" + (A + 1) + ",..)");
                    else
                    {
                        if (B > 1)
                        {
                            sb.Append("R" + (A + 1));
                            for (int i = 2; i < B; i++)
                                sb.Append(", R" + (A + i));
                        }
                        sb.Append(")");
                    }
                    break;
                case 0x1E: //RETURN
                    sb.Append("RETURN");
                    if (B > 1)
                    {
                        sb.Append(" R" + A);
                        if (B > 2)
                            for (int i = 1; i < B - 1; i++)
                                sb.Append(", R" + (A + i));
                    }
                    break;
                case 0x1F://OP_FORLOOP
                    sb.Append("R" + A + " += R" + (A + 2) + ";");
                    sb.Append(" if R" + A + " <?= R" + (A + 1) + " then {");
                    sb.Append(" R" + (A + 3) + " = R" + A + "; GOTO<" + (idx + sBx + 1) + "> }");
                    break;
                case 0x20://OP_FORPREP
                    sb.Append("R" + A + " -= R" + (A + 2) + ";  GOTO <" + (idx + 1 + sBx) + ">");
                    break;
                case 0x21://OP_TFORLOOP
                    if(C < 2)
                    {
                        sb.Append("Cant decode " + opcNames[ID]);
                        return sb.ToString();
                    }
                    if(C == 2)
                        sb.Append("R" + (A + 3) + ", R" + (A + 4));
                    else
                        sb.Append("R" + (A + 3) + ", .., R" + (A + 2 + C));
                    sb.Append(" = R" + A + "(R" + (A + 1) + ", R" + (A + 2) + ");");
                    sb.Append(" if R" + (A + 3) + " ~= NULL then R" + (A + 2) + " = R" + (A + 3));
                    sb.Append(" else GOTO<" + (idx + 2) + ">");
                    break;
                case 0x22://OP_SETLIST
                    if (C == 0)
                    {
                        sb.Append("Cant decode " + opcNames[ID]);
                        return sb.ToString();
                    }
                    int start = 1 + 50 * ((int)C - 1);
                    sb.Append("R" + A + "[" + start + "]");
                    if (B == 0)
                        sb.Append(",..");
                    if (B == 2)
                        sb.Append(", R" + A + "[" + (start + B - 1) + "]");
                    if (B > 2)
                        sb.Append(",..,R" + A + "[" + (start + B - 1) + "]");
                    sb.Append(" = R" + (A + 1));
                    if (B == 0)
                        sb.Append(",..");
                    if (B == 2)
                        sb.Append(", R" + (A + B + start - 1));
                    if (B > 2)
                        sb.Append(",..,R" + (A + B + start - 1));
                    break;
                case 0x24://OP_CLOSURE
                    sb.Append("R" + A + " = Function#" + (C + 1));
                    break;
                default:
                    sb.Append("Cant decode " + opcNames[ID]);
                    break;
            }
            return sb.ToString();
        }

        private string GetConstantOrRegister(LuaFunction func, int v)
        {
            if (v >= 0)
                return "R" + v;
            else
                return func.constants[-v - 1].ToString();
        }

        private string MakeBasicMathOperation(LuaFunction func, string op)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("R" + A + " = ");
            sb.Append(GetConstantOrRegister(func, sB));
            sb.Append(" " + op + " ");
            sb.Append(GetConstantOrRegister(func, sC));
            return sb.ToString();
        }
        private string MakeBasicBinaryOperation(LuaFunction func, string op)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("R" + A + " = " + op);
            sb.Append(GetConstantOrRegister(func, sB));
            return sb.ToString();
        }
    }
}

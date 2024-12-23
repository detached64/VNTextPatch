﻿using System.Text;
using VNTextPatch.Util;

namespace VNTextPatch.Scripts.Artemis
{
    internal class LuaString : ILuaNode
    {
        public LuaString(string value)
        {
            Value = value;
        }

        public string Value
        {
            get;
            set;
        }

        public override string ToString()
        {
            return StringUtil.QuoteC(Value);
        }

        public void ToString(StringBuilder result, int indentLevel)
        {
            result.Append(ToString());
        }
    }
}

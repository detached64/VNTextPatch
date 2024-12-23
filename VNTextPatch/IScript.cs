﻿using System.Collections.Generic;

namespace VNTextPatch
{
    public interface IScript
    {
        string Extension { get; }
        void Load(ScriptLocation location);
        IEnumerable<ScriptString> GetStrings();
        void WritePatched(IEnumerable<ScriptString> strings, ScriptLocation location);
    }
}

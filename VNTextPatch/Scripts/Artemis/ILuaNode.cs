using System.Text;

namespace VNTextPatch.Scripts.Artemis
{
    internal interface ILuaNode
    {
        void ToString(StringBuilder result, int indentLevel);
    }
}

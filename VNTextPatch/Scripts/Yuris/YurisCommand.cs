using System.IO;
using VNTextPatch.Util;

namespace VNTextPatch.Scripts.Yuris
{
    public struct YurisCommand
    {
        public void Read(BinaryReader reader)
        {
            Id = reader.ReadByte();
            NumAttributes = reader.ReadByte();
            reader.Skip(2);
        }

        public byte Id;
        public byte NumAttributes;
    }
}

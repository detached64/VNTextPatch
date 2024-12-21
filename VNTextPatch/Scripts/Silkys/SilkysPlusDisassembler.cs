using System.Collections.Generic;
using System.IO;

namespace VNTextPatch.Scripts.Silkys
{
    internal class SilkysPlusDisassembler : SilkysDisassemblerBase
    {
        private static readonly SilkysOpcodes SilkysPlusOpcodes =
            new SilkysOpcodes
            {
                Yield = 0x00,
                Add = 0x34,
                EscapeSequence = 0x1C,
                Message1 = 0x0A,
                Message2 = 0x0B,
                PushInt = 0x32,
                PushString = 0x33,
                Syscall = 0x18,
                LineNumber = 0xFF,
                Nop1 = 0xFC,
                Nop2 = 0xFD,

                IsMessage1Obfuscated = true
            };

        private static readonly Dictionary<byte, string> SilkysPlusOperandTemplates =
            new Dictionary<byte, string>
            {
                { 0x00, string.Empty },       // yield
                { 0x01, string.Empty },       // ret
                { 0x02, string.Empty },       // ldglob1.i8
                { 0x03, string.Empty },       // ldglob2.i16
                { 0x04, string.Empty },       // ldglob3.var
                { 0x05, string.Empty },       // ldglob4.var
                { 0x06, string.Empty },       // ldloc.var
                { 0x07, string.Empty },       // ldglob5.i8
                { 0x08, string.Empty },       // ldglob5.i16
                { 0x09, string.Empty },       // ldglob5.i32
                { 0x0A, "s" },      // message
                { 0x0B, "t" },      // message
                { 0x0C, string.Empty },       // stglob1.i8
                { 0x0D, string.Empty },       // stglob2.i16
                { 0x0E, string.Empty },       // stglob3.var
                { 0x0F, string.Empty },       // stglob4.var
                { 0x10, string.Empty },       // stloc.var
                { 0x11, string.Empty },       // stglob5.i8
                { 0x12, string.Empty },       // stglob5.i16
                { 0x13, string.Empty },       // stglob5.i32
                { 0x14, "a" },      // jz
                { 0x15, "a" },      // jmp
                { 0x16, "a" },      // libreg
                { 0x17, string.Empty },       // libcall
                { 0x18, string.Empty },       // syscall
                { 0x19, "i" },      // msgid
                { 0x1A, "i" },      // msgid2
                { 0x1B, "a" },      // choice
                { 0x1C, "b" },      // escape sequence
                { 0x32, "i" },      // ldc.i4
                { 0x33, "s" },      // ldstr
                { 0x34, string.Empty },       // add
                { 0x35, string.Empty },       // sub
                { 0x36, string.Empty },       // mul
                { 0x37, string.Empty },       // div
                { 0x38, string.Empty },       // mod
                { 0x39, string.Empty },       // rand
                { 0x3A, string.Empty },       // logand
                { 0x3B, string.Empty },       // logor
                { 0x3C, string.Empty },       // binand
                { 0x3D, string.Empty },       // binor
                { 0x3E, string.Empty },       // lt
                { 0x3F, string.Empty },       // gt
                { 0x40, string.Empty },       // le
                { 0x41, string.Empty },       // ge
                { 0x42, string.Empty },       // eq
                { 0x43, string.Empty },       // neq
                { 0xFA, string.Empty },
                { 0xFB, string.Empty },
                { 0xFC, string.Empty },
                { 0xFD, string.Empty },
                { 0xFE, string.Empty },
                { 0xFF, string.Empty }
            };

        private static readonly SilkysSyscalls[] SilkysPlusSyscalls =
        {
            new SilkysSyscalls
            {
                Exec = 29,
                ExecSetCharacterName = 11
            },
            new SilkysSyscalls
            {
                Exec = 29,
                ExecSetCharacterName = 15
            }
        };

        private readonly int _numMessages;
        private readonly int _numSpecialMessages;

        public SilkysPlusDisassembler(Stream stream)
            : base(stream)
        {
            _numMessages = _reader.ReadInt32();
            _numSpecialMessages = _reader.ReadInt32();
            CodeOffset = 8 + 4 * (_numMessages + _numSpecialMessages);
        }

        public override SilkysOpcodes Opcodes => SilkysPlusOpcodes;

        protected override Dictionary<byte, string> OperandTemplates => SilkysPlusOperandTemplates;

        public override SilkysSyscalls[] Syscalls => SilkysPlusSyscalls;

        public override int CodeOffset
        {
            get;
        }

        public override void ReadHeader()
        {
            for (int i = 0; i < _numMessages + _numSpecialMessages; i++)
            {
                RaiseLittleEndianAddressEncountered(8 + 4 * i);
            }
            Stream.Position = CodeOffset;
        }
    }
}

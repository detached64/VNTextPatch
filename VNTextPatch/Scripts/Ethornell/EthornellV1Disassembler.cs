using System;
using System.Collections.Generic;
using System.IO;
using VNTextPatch.Shared.Scripts.Ethornell;
using VNTextPatch.Util;

namespace VNTextPatch.Scripts.Ethornell
{
    public class EthornellV1Disassembler : EthornellDisassembler
    {
        /* Script version 1 has the magic string "BurikoCompiledScriptVer1.00\0" followed by the following fields:
         * i32 headerSize
         * byte header[headerSize - 4]
         *     i32 numReferencedScripts
         *     sz referencedScripts[numReferencedScripts]
         *     i32 numLabels
         *     Label labels[numLabels]
         *         sz labelName
         *         i32 address
         * 
         * The opcodes are completely different from version 0.
         */

        public static readonly byte[] Magic =
            {
                0x42, 0x75, 0x72, 0x69, 0x6B, 0x6F, 0x43, 0x6F, 0x6D, 0x70, 0x69, 0x6C, 0x65, 0x64, 0x53, 0x63,
                0x72, 0x69, 0x70, 0x74, 0x56, 0x65, 0x72, 0x31, 0x2E, 0x30, 0x30, 0x00
            };

        private static readonly Dictionary<int, string> OperandTemplates =
            new Dictionary<int, string>
            {
                // i: int
                // c: code offset
                // m: string offset
                { 0x0000, "i" },        // push constant
                { 0x0001, "c" },        // push code address
                { 0x0002, "i" },        // push variable address (bp - value)
                { 0x0003, "m" },        // push string address
                { 0x0008, "i" },        // load  (operand = 0 -> byte, other -> dword)
                { 0x0009, "i" },        // store (operand = 0 -> byte, other -> dword)
                { 0x000A, "i" },
                { 0x0010, string.Empty },         // getbp
                { 0x0011, string.Empty },         // setbp
                { 0x0015, string.Empty },
                { 0x0016, string.Empty },
                { 0x0017, "i" },
                { 0x0018, string.Empty },
                { 0x0019, "i" },
                { 0x001A, string.Empty },
                { 0x001B, string.Empty },
                { 0x001C, string.Empty },         // call user function
                { 0x001D, string.Empty },
                { 0x001E, string.Empty },
                { 0x001F, string.Empty },
                { 0x0020, string.Empty },         // add
                { 0x0021, string.Empty },
                { 0x0022, string.Empty },
                { 0x0023, string.Empty },
                { 0x0024, string.Empty },
                { 0x0025, string.Empty },
                { 0x0026, string.Empty },
                { 0x0027, string.Empty },
                { 0x0028, string.Empty },
                { 0x0029, string.Empty },
                { 0x002A, string.Empty },
                { 0x002B, string.Empty },
                { 0x0030, string.Empty },
                { 0x0031, string.Empty },
                { 0x0032, string.Empty },
                { 0x0033, string.Empty },
                { 0x0034, string.Empty },
                { 0x0035, string.Empty },
                { 0x0038, string.Empty },
                { 0x0039, string.Empty },
                { 0x003A, string.Empty },
                { 0x003E, string.Empty },
                { 0x003F, "i" },
                { 0x0040, string.Empty },
                { 0x0048, string.Empty },
                { 0x007B, "iii" },
                { 0x007E, "i" },
                { 0x007F, "ii" },
                { 0x0080, string.Empty },
                { 0x0081, string.Empty },
                { 0x0082, string.Empty },
                { 0x0083, string.Empty },
                { 0x0090, string.Empty },
                { 0x0091, string.Empty },
                { 0x0092, string.Empty },
                { 0x0093, string.Empty },
                { 0x0094, string.Empty },
                { 0x0095, string.Empty },
                { 0x0098, string.Empty },
                { 0x0099, string.Empty },
                { 0x00A0, string.Empty },
                { 0x00A8, string.Empty },
                { 0x00AA, string.Empty },
                { 0x00AC, string.Empty },
                { 0x00C0, string.Empty },
                { 0x00C1, string.Empty },
                { 0x00C2, string.Empty },
                { 0x00D0, string.Empty },
                { 0x00E0, string.Empty },
                { 0x00E1, string.Empty },
                { 0x00E2, string.Empty },
                { 0x00E3, string.Empty },
                { 0x00E4, string.Empty },
                { 0x00E5, string.Empty },
                { 0x00E6, string.Empty },
                { 0x00E7, string.Empty },
                { 0x00E8, string.Empty },
                { 0x00E9, string.Empty },
                { 0x00EA, string.Empty },
                { 0x00EB, string.Empty },
                { 0x00EC, string.Empty },
                { 0x00ED, string.Empty },
                { 0x00EE, string.Empty },
                { 0x00EF, string.Empty },
                { 0x00F0, string.Empty },
                { 0x00F1, string.Empty },
                { 0x00F3, string.Empty },
                { 0x00F4, string.Empty },
                { 0x00F5, string.Empty },
                { 0x00F6, string.Empty },
                { 0x00F7, string.Empty },
                { 0x00F8, string.Empty },
                { 0x00F9, string.Empty },
                { 0x00FA, string.Empty },
                { 0x00FB, string.Empty },
                { 0x00FC, string.Empty },
                { 0x00FD, string.Empty },
                { 0x00FE, string.Empty },
                { 0x00FF, string.Empty },
                { 0x0100, string.Empty },
                { 0x0101, string.Empty },
                { 0x0102, string.Empty },
                { 0x0103, string.Empty },
                { 0x0104, string.Empty },
                { 0x0105, string.Empty },
                { 0x0106, string.Empty },
                { 0x0107, string.Empty },
                { 0x0108, string.Empty },
                { 0x0109, string.Empty },
                { 0x010A, string.Empty },
                { 0x010B, string.Empty },
                { 0x010D, string.Empty },
                { 0x010E, string.Empty },
                { 0x010F, string.Empty },
                { 0x0110, string.Empty },
                { 0x0111, string.Empty },
                { 0x0112, string.Empty },
                { 0x0113, string.Empty },
                { 0x0114, string.Empty },
                { 0x0115, string.Empty },
                { 0x0116, string.Empty },
                { 0x0117, string.Empty },
                { 0x0118, string.Empty },
                { 0x0119, string.Empty },
                { 0x011A, string.Empty },
                { 0x011B, string.Empty },
                { 0x011C, string.Empty },
                { 0x011D, string.Empty },
                { 0x011E, string.Empty },
                { 0x011F, string.Empty },
                { 0x0120, string.Empty },
                { 0x0121, string.Empty },
                { 0x0122, string.Empty },
                { 0x0123, string.Empty },
                { 0x0124, string.Empty },
                { 0x0125, string.Empty },
                { 0x0126, string.Empty },
                { 0x0127, string.Empty },
                { 0x0128, string.Empty },
                { 0x0129, string.Empty },
                { 0x012A, string.Empty },
                { 0x012C, string.Empty },
                { 0x012D, string.Empty },
                { 0x012E, string.Empty },
                { 0x012F, string.Empty },
                { 0x0130, string.Empty },
                { 0x0131, string.Empty },
                { 0x0132, string.Empty },
                { 0x0133, string.Empty },
                { 0x0134, string.Empty },
                { 0x0135, string.Empty },
                { 0x0136, string.Empty },
                { 0x0137, string.Empty },
                { 0x0138, string.Empty },
                { 0x0139, string.Empty },
                { 0x013A, string.Empty },
                { 0x013B, string.Empty },
                { 0x013C, string.Empty },
                { 0x013D, string.Empty },
                { 0x013E, string.Empty },
                { 0x013F, string.Empty },
                { 0x0140, string.Empty },     // show message
                { 0x0141, string.Empty },
                { 0x0142, string.Empty },
                { 0x0143, string.Empty },
                { 0x0144, string.Empty },
                { 0x0145, string.Empty },
                { 0x0146, string.Empty },
                { 0x0147, string.Empty },
                { 0x0148, string.Empty },
                { 0x0149, string.Empty },
                { 0x014A, string.Empty },
                { 0x014B, string.Empty },
                { 0x014C, string.Empty },
                { 0x014D, string.Empty },
                { 0x014E, string.Empty },
                { 0x014F, string.Empty },
                { 0x0150, string.Empty },
                { 0x0151, string.Empty },
                { 0x0152, string.Empty },
                { 0x0153, string.Empty },
                { 0x0156, string.Empty },
                { 0x0157, string.Empty },
                { 0x0158, string.Empty },
                { 0x0159, string.Empty },
                { 0x015A, string.Empty },
                { 0x015C, string.Empty },
                { 0x015D, string.Empty },
                { 0x015E, string.Empty },
                { 0x015F, string.Empty },
                { 0x0160, string.Empty },     // show choice screen
                { 0x0161, string.Empty },
                { 0x0163, string.Empty },
                { 0x0164, string.Empty },
                { 0x0165, string.Empty },
                { 0x0166, string.Empty },
                { 0x0167, string.Empty },
                { 0x0168, string.Empty },
                { 0x0169, string.Empty },
                { 0x016A, string.Empty },
                { 0x016B, string.Empty },
                { 0x016C, string.Empty },
                { 0x016D, string.Empty },
                { 0x016E, string.Empty },
                { 0x016F, string.Empty },
                { 0x0170, string.Empty },
                { 0x0171, string.Empty },
                { 0x0172, string.Empty },
                { 0x0173, string.Empty },
                { 0x0174, string.Empty },
                { 0x0175, string.Empty },
                { 0x0176, string.Empty },
                { 0x0178, string.Empty },
                { 0x0179, string.Empty },
                { 0x017A, string.Empty },
                { 0x017D, string.Empty },
                { 0x017E, string.Empty },
                { 0x017F, string.Empty },
                { 0x0180, string.Empty },
                { 0x0181, string.Empty },
                { 0x0182, string.Empty },
                { 0x0184, string.Empty },
                { 0x0185, string.Empty },
                { 0x0186, string.Empty },
                { 0x0187, string.Empty },
                { 0x0188, string.Empty },
                { 0x0189, string.Empty },
                { 0x018A, string.Empty },
                { 0x018B, string.Empty },
                { 0x018D, string.Empty },
                { 0x018E, string.Empty },
                { 0x018F, string.Empty },
                { 0x0190, string.Empty },
                { 0x0191, string.Empty },
                { 0x0194, string.Empty },
                { 0x0195, string.Empty },
                { 0x0196, string.Empty },
                { 0x0197, string.Empty },
                { 0x0198, string.Empty },
                { 0x0199, string.Empty },
                { 0x019C, string.Empty },
                { 0x019D, string.Empty },
                { 0x019E, string.Empty },
                { 0x019F, string.Empty },
                { 0x01A0, string.Empty },
                { 0x01A1, string.Empty },
                { 0x01A2, string.Empty },
                { 0x01A3, string.Empty },
                { 0x01A4, string.Empty },
                { 0x01A5, string.Empty },
                { 0x01A6, string.Empty },
                { 0x01A7, string.Empty },
                { 0x01A8, string.Empty },
                { 0x01A9, string.Empty },
                { 0x01AA, string.Empty },
                { 0x01AB, string.Empty },
                { 0x01AC, string.Empty },
                { 0x01AD, string.Empty },
                { 0x01AE, string.Empty },
                { 0x01AF, string.Empty },
                { 0x01B0, string.Empty },
                { 0x01B1, string.Empty },
                { 0x01B2, string.Empty },
                { 0x01B4, string.Empty },
                { 0x01B5, string.Empty },
                { 0x01B6, string.Empty },
                { 0x01B7, string.Empty },
                { 0x01BF, string.Empty },
                { 0x01D0, string.Empty },
                { 0x01D4, string.Empty },
                { 0x01D8, string.Empty },
                { 0x01D9, string.Empty },
                { 0x01E0, string.Empty },
                { 0x01F0, string.Empty },
                { 0x0200, string.Empty },
                { 0x0204, string.Empty },
                { 0x0205, string.Empty },
                { 0x0208, string.Empty },
                { 0x0209, string.Empty },
                { 0x020A, string.Empty },
                { 0x020C, string.Empty },
                { 0x020E, string.Empty },
                { 0x020F, string.Empty },
                { 0x0210, string.Empty },
                { 0x0220, string.Empty },
                { 0x0222, string.Empty },
                { 0x0225, string.Empty },
                { 0x0226, string.Empty },
                { 0x0228, string.Empty },
                { 0x0229, string.Empty },
                { 0x022A, string.Empty },
                { 0x0230, string.Empty },
                { 0x0231, string.Empty },
                { 0x0232, string.Empty },
                { 0x0233, string.Empty },
                { 0x0234, string.Empty },
                { 0x0235, string.Empty },
                { 0x0236, string.Empty },
                { 0x0237, string.Empty },
                { 0x0238, string.Empty },
                { 0x0239, string.Empty },
                { 0x023A, string.Empty },
                { 0x023B, string.Empty },
                { 0x023C, string.Empty },
                { 0x023D, string.Empty },
                { 0x0240, string.Empty },
                { 0x0241, string.Empty },
                { 0x0242, string.Empty },
                { 0x0244, string.Empty },
                { 0x0245, string.Empty },
                { 0x0248, string.Empty },
                { 0x024C, string.Empty },
                { 0x024D, string.Empty },
                { 0x024E, string.Empty },
                { 0x0250, string.Empty },
                { 0x0251, string.Empty },
                { 0x0252, string.Empty },
                { 0x0254, string.Empty },
                { 0x0255, string.Empty },
                { 0x0256, string.Empty },
                { 0x0257, string.Empty },
                { 0x0258, string.Empty },
                { 0x025E, string.Empty },
                { 0x025F, string.Empty },
                { 0x0260, string.Empty },
                { 0x0261, string.Empty },
                { 0x0262, string.Empty },
                { 0x0266, string.Empty },
                { 0x0268, string.Empty },
                { 0x027F, string.Empty },
                { 0x0280, string.Empty },
                { 0x0281, string.Empty },
                { 0x0284, string.Empty },
                { 0x0288, string.Empty },
                { 0x0289, string.Empty },
                { 0x028A, string.Empty },
                { 0x0290, string.Empty },
                { 0x0294, string.Empty },
                { 0x0295, string.Empty },
                { 0x0296, string.Empty },
                { 0x0297, string.Empty },
                { 0x0298, string.Empty },
                { 0x0299, string.Empty },
                { 0x029C, string.Empty },
                { 0x02A0, string.Empty },
                { 0x02A1, string.Empty },
                { 0x02A2, string.Empty },
                { 0x02A3, string.Empty },
                { 0x02A4, string.Empty },
                { 0x02A8, string.Empty },
                { 0x02C0, string.Empty },
                { 0x02C1, string.Empty },
                { 0x02C2, string.Empty },
                { 0x02C3, string.Empty },
                { 0x02C4, string.Empty },
                { 0x02C5, string.Empty },
                { 0x02C6, string.Empty },
                { 0x02C7, string.Empty },
                { 0x02C8, string.Empty },
                { 0x02CA, string.Empty },
                { 0x02CB, string.Empty },
                { 0x02CC, string.Empty },
                { 0x02CD, string.Empty },
                { 0x02CE, string.Empty },
                { 0x02CF, string.Empty },
                { 0x02D0, string.Empty },
                { 0x02D2, string.Empty },
                { 0x02D4, string.Empty },
                { 0x02D5, string.Empty },
                { 0x02D6, string.Empty },
                { 0x02D7, string.Empty },
                { 0x02D8, string.Empty },
                { 0x02D9, string.Empty },
                { 0x02DA, string.Empty },
                { 0x02DB, string.Empty },
                { 0x02DC, string.Empty },
                { 0x02DD, string.Empty },
                { 0x02DE, string.Empty },
                { 0x02DF, string.Empty },
                { 0x02E0, string.Empty },
                { 0x02E1, string.Empty },
                { 0x02E2, string.Empty },
                { 0x02E3, string.Empty },
                { 0x02E4, string.Empty },
                { 0x02E5, string.Empty },
                { 0x02E6, string.Empty },
                { 0x02E7, string.Empty },
                { 0x02E8, string.Empty },
                { 0x02E9, string.Empty },
                { 0x02EA, string.Empty },
                { 0x02EB, string.Empty },
                { 0x02EC, string.Empty },
                { 0x02EE, string.Empty },
                { 0x02F0, string.Empty },
                { 0x02F1, string.Empty },
                { 0x02F3, string.Empty },
                { 0x02F4, string.Empty },
                { 0x02F8, string.Empty },
                { 0x02FA, string.Empty },
                { 0x02FC, string.Empty },
                { 0x02FD, string.Empty },
                { 0x0300, string.Empty },
                { 0x0301, string.Empty },
                { 0x0302, string.Empty },
                { 0x0303, string.Empty },
                { 0x0304, string.Empty },
                { 0x0306, string.Empty },
                { 0x0307, string.Empty },
                { 0x0308, string.Empty },
                { 0x0309, string.Empty },
                { 0x030A, string.Empty },
                { 0x030C, string.Empty },
                { 0x030D, string.Empty },
                { 0x030E, string.Empty },
                { 0x0310, string.Empty },
                { 0x0311, string.Empty },
                { 0x0314, string.Empty },
                { 0x031E, string.Empty },
                { 0x031F, string.Empty },
                { 0x0320, string.Empty },
                { 0x0328, string.Empty },
                { 0x032C, string.Empty },
                { 0x0330, string.Empty },
                { 0x0331, string.Empty },
                { 0x0334, string.Empty },
                { 0x0335, string.Empty },
                { 0x0336, string.Empty },
                { 0x0337, string.Empty },
                { 0x0338, string.Empty },
                { 0x0339, string.Empty },
                { 0x033F, string.Empty },
                { 0x0340, string.Empty },
                { 0x0341, string.Empty },
                { 0x0348, string.Empty },
                { 0x0350, string.Empty },
                { 0x0351, string.Empty },
                { 0x0352, string.Empty },
                { 0x0353, string.Empty },
                { 0x0354, string.Empty },
                { 0x0355, string.Empty },
                { 0x0358, string.Empty },
                { 0x0360, string.Empty },
                { 0x0368, string.Empty },
                { 0x0380, string.Empty },
                { 0x0388, string.Empty },
                { 0x038D, string.Empty },
                { 0x038E, string.Empty },
                { 0x038F, string.Empty },
                { 0x0390, string.Empty },
                { 0x0391, string.Empty },
                { 0x0392, string.Empty },
                { 0x0393, string.Empty },
                { 0x0394, string.Empty },
                { 0x03AF, string.Empty },
                { 0x03C0, string.Empty },
                { 0x03C1, string.Empty },
                { 0x03C2, string.Empty },
                { 0x03C4, string.Empty },
                { 0x03C5, string.Empty },
                { 0x03C6, string.Empty },
                { 0x03C7, string.Empty },
                { 0x03C8, string.Empty },
                { 0x03C9, string.Empty },
                { 0x03CA, string.Empty },
                { 0x03D0, string.Empty },
                { 0x03D2, string.Empty },
                { 0x03D4, string.Empty },
                { 0x03D5, string.Empty },
                { 0x03D6, string.Empty },
                { 0x03D8, string.Empty },
                { 0x03DC, string.Empty },
                { 0x03F0, string.Empty },
                { 0x03F1, string.Empty },
                { 0x03F4, string.Empty },
                { 0x03F5, string.Empty },
                { 0x03F6, string.Empty },
                { 0x03F7, string.Empty },
                { 0x03F8, string.Empty },
                { 0x03FA, string.Empty },
                { 0x03FB, string.Empty },
                { 0x03FC, string.Empty },
                { 0x03FD, string.Empty },
                { 0x03FE, string.Empty },
                { 0x03FF, string.Empty },
                { 0x0400, string.Empty },
                { 0x0401, string.Empty },
                { 0x0402, string.Empty },
                { 0x0403, string.Empty },
                { 0x0404, string.Empty },
                { 0x0405, string.Empty },
                { 0x0408, string.Empty },
                { 0x0409, string.Empty },
                { 0x040A, string.Empty },
                { 0x040B, string.Empty },
                { 0x040C, string.Empty },
                { 0x040D, string.Empty },
                { 0x040F, string.Empty },
                { 0x0410, string.Empty },
                { 0x0411, string.Empty },
                { 0x0412, string.Empty },
                { 0x0413, string.Empty },
                { 0x0418, string.Empty },
                { 0x0427, string.Empty },
                { 0x0428, string.Empty },
                { 0x0429, string.Empty },
                { 0x042A, string.Empty },
                { 0x042B, string.Empty },
                { 0x042C, string.Empty },
                { 0x042D, string.Empty },
                { 0x042F, string.Empty },
                { 0x0430, string.Empty },
                { 0x0431, string.Empty },
                { 0x0432, string.Empty },
                { 0x0440, string.Empty },
                { 0x0441, string.Empty },
                { 0x0442, string.Empty },
                { 0x0444, string.Empty },
                { 0x0448, string.Empty },
                { 0x0449, string.Empty },
                { 0x0450, string.Empty },
                { 0x0451, string.Empty },
                { 0x0452, string.Empty },
                { 0x0453, string.Empty },
                { 0x0454, string.Empty },
                { 0x0455, string.Empty },
                { 0x0458, string.Empty },
                { 0x0459, string.Empty },
                { 0x045C, string.Empty },
                { 0x045D, string.Empty },
                { 0x045E, string.Empty },
                { 0x0480, string.Empty },
                { 0x0481, string.Empty },
                { 0x0482, string.Empty },
                { 0x0483, string.Empty },
                { 0x0484, string.Empty },
                { 0x0485, string.Empty },
                { 0x04C0, string.Empty },
                { 0x04C1, string.Empty },
                { 0x04C2, string.Empty },
                { 0x04C3, string.Empty },
                { 0x04C4, string.Empty },
                { 0x04C5, string.Empty },
                { 0x04C6, string.Empty },
                { 0x04C7, string.Empty },
                { 0x04C8, string.Empty },
                { 0x04C9, string.Empty },
                { 0x04CA, string.Empty },
                { 0x04CB, string.Empty },
                { 0x04D0, string.Empty },
                { 0x04D1, string.Empty },
                { 0x04D5, string.Empty },
                { 0x04D8, string.Empty },
                { 0x04D9, string.Empty },
                { 0x04DA, string.Empty },
                { 0x04E0, string.Empty },
                { 0x04E4, string.Empty },
                { 0x04E5, string.Empty },
                { 0x04E8, string.Empty },
                { 0x04E9, string.Empty },
                { 0x04EA, string.Empty },
                { 0x04EB, string.Empty }
            };

        private readonly int _codeOffset;
        private readonly Dictionary<int, Action> _operandReaders;

        private readonly Stack<StackItem> _stringStack = new Stack<StackItem>();

        public EthornellV1Disassembler(Stream stream)
            : base(stream)
        {
            _reader.BaseStream.Position = Magic.Length;
            int headerSize = _reader.ReadInt32();
            _codeOffset = Magic.Length + headerSize;

            _operandReaders =
                new Dictionary<int, Action>
                {
                    { 0x0003, ReadPushStringAddressOperand },
                    { 0x001C, HandleUserFunctionCall },
                    { 0x0140, HandleMessage },
                    { 0x0143, HandleMessage },
                    { 0x0160, HandleChoiceScreen }
                };
        }

        public override int CodeOffset
        {
            get { return _codeOffset; }
        }

        public override void Disassemble()
        {
            _reader.BaseStream.Position = CodeOffset;
            while (true)
            {
                int opcode = _reader.ReadInt32();
                Action specializedReader = _operandReaders.GetOrDefault(opcode);
                if (specializedReader != null)
                {
                    specializedReader();
                }
                else
                {
                    ReadOperands(OperandTemplates[opcode]);
                }

                if ((opcode == 0x001B || opcode == 0x00F4) && _largestCodeAddressOperandEncountered < (int)_reader.BaseStream.Position - CodeOffset)
                {
                    break;
                }

                if (opcode == 0x007E || opcode == 0x007F || opcode == 0x00FE)
                {
                    OutputInternalStrings();
                }
            }
            OutputInternalStrings();
        }

        private void ReadPushStringAddressOperand()
        {
            int offset = (int)_reader.BaseStream.Position;
            int address = _reader.ReadInt32();
            _stringStack.Push(new StackItem(offset, address));
        }

        private void HandleUserFunctionCall()
        {
            if (_stringStack.Count == 0)
            {
                return;
            }

            StackItem item = _stringStack.Pop();
            OnStringAddressEncountered(item.Offset, item.Value, ScriptStringType.Internal);
            string funcName = ReadStringAtAddress(item.Value);
            if (funcName == "_SelectEx")
            {
                HandleChoiceScreen();
            }
        }

        private void HandleMessage()
        {
            StackItem message = _stringStack.Pop();

            if (_stringStack.Count > 0)
            {
                StackItem name = _stringStack.Pop();
                OnStringAddressEncountered(name.Offset, name.Value, !IsEmptyString(name.Value) ? ScriptStringType.CharacterName : ScriptStringType.Internal);
            }

            OnStringAddressEncountered(message.Offset, message.Value, !IsEmptyString(message.Value) ? ScriptStringType.Message : ScriptStringType.Internal);
        }

        private void HandleChoiceScreen()
        {
            List<StackItem> choiceOperands = new List<StackItem>();
            while (_stringStack.Count > 0)
            {
                StackItem item = _stringStack.Pop();
                choiceOperands.Insert(0, item);
            }

            foreach (StackItem item in choiceOperands)
            {
                OnStringAddressEncountered(item.Offset, item.Value, ScriptStringType.Message);
            }
        }

        private void OutputInternalStrings()
        {
            while (_stringStack.Count > 0)
            {
                StackItem item = _stringStack.Pop();
                OnStringAddressEncountered(item.Offset, item.Value, ScriptStringType.Internal);
            }
        }

        private struct StackItem
        {
            public StackItem(int offset, int value)
            {
                Offset = offset;
                Value = value;
            }

            public int Offset;
            public int Value;
        }
    }
}

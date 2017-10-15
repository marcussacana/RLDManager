using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RLDManager
{
    public class RLD
    {
        private bool Status = false;
        private byte[] Script;
        public Encoding SJIS = Encoding.GetEncoding(932);
        uint[] Offsets;
        uint[] Lenghts;
        public RLD(byte[] Script) {
            this.Script = Script;
        }

        public string[] Import() {
            if (!Status) {
                XOR(ref Script);
                //System.IO.File.WriteAllBytes("dump.rld", Script);//debug
                Status = true;
            }
            Offsets = new uint[0];
            for (uint i = 0; i < Script.Length; i++) {
                Result Info = Scan(i);
                if (!Info.Valid)
                    continue;

                foreach (uint ptr in Info.StrIndxs) {
                    if (Offsets.Contains(ptr) || GetStrLen(ptr) < 3 || IsCommand(ptr))
                        continue;
                    ;
                    //Save offset
                    Array.Resize(ref Offsets, Offsets.Length + 1);
                    Offsets[Offsets.Length - 1] = ptr;
                }
            }
            //Initialize Variables
            string[] Strs = new string[Offsets.Length];
            Lenghts = new uint[Offsets.Length];

            for (int i = 0; i < Strs.Length; i++) {
                //Initialize String Variables
                byte[] Buff = new byte[0];
                uint Pos = Offsets[i];

                //Copy String
                while (Script[Pos] != 0x00) {
                    Array.Resize(ref Buff, Buff.Length + 1);
                    Buff[Buff.Length - 1] = Script[Pos++];
                }

                //Save String and Lenght
                Strs[i] = SJIS.GetString(Buff);
                Lenghts[i] = (uint)Buff.LongLength;
            }
            return Strs;
        }

        private bool IsCommand(uint ptr) {
            bool IsCommand = true;
            while (Script[ptr] != 0x00) {
                byte Char = Script[ptr++];
                IsCommand &= (Char >= '0' && Char <= '9') || Char == '-' || Char == ',';
            }
            return IsCommand;
        }

        public byte[] Export(string[] Strings) {
            if (Strings.Length != Offsets.Length)
                throw new Exception("You Can't add new strings.");
            byte[] OutScript = new byte[Script.Length];
            Script.CopyTo(OutScript, 0);
            //Reverse Replace to don't need update offsets after a change.
            for (int i = Offsets.Length - 1; i >= 0; i--) {
                uint POS = Offsets[i];
                uint LEN = Lenghts[i];

                //Get Content Before the string
                byte[] Bef = GetRegion(OutScript, 0, POS);

                //Compile String
                byte[] Str = SJIS.GetBytes(Strings[i]);

                //Get Content After the string
                byte[] Aft = GetRegion(OutScript, POS + LEN, (uint)OutScript.LongLength - (POS + LEN));

                //Resize OutScript
                OutScript = new byte[Bef.LongLength + Aft.LongLength + Str.LongLength];

                //Merge Data
                Bef.CopyTo(OutScript, 0);
                Str.CopyTo(OutScript, Bef.LongLength);
                Aft.CopyTo(OutScript, Bef.LongLength + Str.LongLength);
            }
            //Encrypt and Return
            XOR(ref OutScript);
            return OutScript;
        }

        private byte[] GetRegion(byte[] From, uint Start, uint Lenght) {
            byte[] To = new byte[Lenght];
            for (uint i = Start; i - Start < Lenght; i++)
                To[i - Start] = From[i];
            return To;
        }

        private Result Scan(uint Pos) {
            Result rst = new Result() {
                Valid = false,
                StrIndxs = new List<uint>()
            };
            if (Pos + 6 < Script.Length && Script[Pos] == 0xFF && Script[Pos+1] == 0xFF) {
                if (Script[Pos + 2] == 0x2A && Script[Pos + 3] == 0x00) {
                    uint StrIndx = Pos + 4;
                    rst.Valid = IsChar(Script[StrIndx]) && IsChar(Script[StrIndx+1]);
                    if (rst.Valid)
                        rst.StrIndxs.Add(StrIndx);
                }
            }
            if (!rst.Valid)
                if (Pos + 6 < Script.Length && Script[Pos] == 0xFF && Script[Pos + 1] == 0xFF && Script[Pos + 2] == 0xFF && Script[Pos + 3] == 0xFF) {
                    if (IsChar(Script[Pos + 4])) {
                        if (IsValidStr(Pos + 4)) {
                            rst.Valid = true;
                            if (IsValidDoubleStr(Pos + 4)) {
                                uint Ptr = Pos + 4;
                                rst.StrIndxs.Add(Ptr);
                                while (Script[Ptr++] != 0x00)
                                    continue;
                                rst.StrIndxs.Add(Ptr);
                            } else {
                                rst.StrIndxs.Add(Pos + 4);
                            }
                        }
                    }
                }
            if (!rst.Valid) {
                if (Script[Pos] == 0x2A && Script[Pos+1] == 0x00) {
                    uint Ptr = Pos - 1;
                    while (Ptr > 0x10 && Script[Ptr] == 0x00)
                        Ptr--;
                    uint i = Pos + 2;
                    if (Script[Ptr] == 0xFF && Script[Ptr-1] == 0xFF) {
                        if (IsChar(Script[i]) && IsChar(Script[i + 2])) {
                            rst.Valid = true;
                            rst.StrIndxs.Add(i);
                            while (i < Script.Length && Script[i] != 0x00)
                                i++;
                            if (IsChar(Script[++i])) {
                                if (IsChar(Script[i+2])) {
                                    rst.StrIndxs.Add(i);
                                }
                            }
                        }
                    }
                }
            }
            return rst;
        }

        private uint GetStrLen(uint At) {
            uint Len = 0;
            while (Script[At + Len] != 0x00)
                Len++;
            return Len;
        }
        private bool IsValidDoubleStr(uint At) {
            if (IsValidStr(At)) {
                while (Script[At++] != 0x00)
                    continue;
                if (IsValidStr(At))
                    return true;
            }
            return false;
        }
        private bool IsValidStr(uint At) {
            byte b = 0x00;
            while ((b = Script[At++]) != 0x00)
                if (!IsChar(b))
                    return false;
            return true;
        }
        private bool IsChar(byte b) {
            return (b >= 0x20 && b <= 0x7F) || b == 0x82 || b == 0x81;
        }
        public void XOR(ref byte[] Content) {
            //uint Key = 0x39AA8BA0; //Princess Evangile
            uint Key = 0xE69A420B; //Princess Evangile W
            //uint Key = 0xAD2B78EA; //Sakura no Mori Dreamers

            uint BlockCount = (uint)Content.Length;

            if (BlockCount > 0xFFCF)
                BlockCount = 0xFFCF;

            BlockCount -= (BlockCount % 4);
            uint[] Keys = GenKeyTable(Key);
            for (uint i = 0x10, ri = 0; i < BlockCount; i += 4, ri++) {
                uint enc = GetDW(Content, i);
                uint tmp = Keys[(ri & 0xFF)];
                SetDWAt(ref Content, i, tmp ^ enc);
            }
        }
        
        public static bool FindKey(byte[] Script, out uint Key, bool Reverse) {
            uint Rst = GetDW(Script, 0x10u);
            if (Reverse) {
                for (uint TKey = uint.MaxValue; TKey >= 0; TKey--) {
                    if (((FindKey(TKey) ^ Rst) & 0xFFFFFFFE) == 0) {
                        Key = TKey;
                        return true;
                    }
                }
            } else {
                for (uint TKey = 0; TKey < uint.MaxValue; TKey++) {
                    if (((FindKey(TKey) ^ Rst) & 0xFFFFFFFE) == 0) {
                        Key = TKey;
                        return true;
                    }
                }
            }
            Key = 0;
            return false;
        }

        private static uint[] GenKeyTable(uint Seed) {
            uint[] Keys = GenSeeds(Seed);
            uint[] Seeds = new uint[Keys.Length];
            Keys.CopyTo(Seeds, 0);
            for (uint i = 0; i < Keys.Length; i++) {
                uint Key = KeyWork(i, ref Keys, ref Seeds);
                Keys[i] = Key ^ Seed;
            }
            return Keys;
        }
        private static uint FindKey(uint Seed) {
            uint[] Keys = GenSeedsFast(Seed);
            uint[] Seeds = new uint[0x270];
            Keys.CopyTo(Seeds, 0);
            uint Key = KeyWork(0, ref Keys, ref Seeds);
            return Key ^ Seed;
        }
        private static uint[] GenSeeds(uint Key) {
            uint EDX = Key;
            uint EAX = 0x00510010;
            uint ECX = 0x0000225C;

            uint EBX = 0x270;
            uint[] Buffer = new uint[EBX];
            uint EDI = 0;

            do {
                //Gen Seed
                ECX = (EDX * 0x10DCD) & 0xFFFFFFFF;
                EAX = (EDX & 0xFFFF0000) | (((ECX + 1) >> 0x10) & 0x0000FFFF);
                Buffer[EDI++] = EAX;

                //Begin Next
                EDX = (EDX * 0x1C587629) & 0xFFFFFFFF;
                EDX += 0x10DCE;

                EBX--;
            } while (EBX != 0);
            return Buffer;
        }

        private static uint[] GenSeedsFast(uint Key) {
            uint EDX = Key;
            uint ECX = 0x0000225C;

            uint End = 0x18E;
            uint[] Buffer = new uint[End];
            uint Index = 0;

            do {
                //Gen Seed
                ECX = (EDX * 0x10DCD) & 0xFFFFFFFF;
                Buffer[Index++] = (EDX & 0xFFFF0000) | (((ECX + 1) >> 0x10) & 0x0000FFFF);

                //Next
                EDX = ((EDX * 0x1C587629) & 0xFFFFFFFF) + 0x10DCE;                
            } while (Index < End);
            return Buffer;
        }

        private static uint KeyWork(uint i, ref uint[] Keys, ref uint[] OriKeys) {
            uint ECX = Keys[i];
            uint EDX = i + 1;
            if (EDX >= Keys.Length)
                EDX = 0;

            uint EDI = (((Keys[EDX] ^ ECX) & 0x7FFFFFFF) ^ ECX);

            bool CF = (EDI & 0x1) == 0x1;//SHR CF Check
            EDI >>= 1;
            EDI ^= (CF ? uint.MaxValue : uint.MinValue) & 0x9908B0DF;
            ECX = i + 0x18D;
            if (ECX >= Keys.Length)
                ECX = (uint)(ECX - Keys.LongLength);

            EDI ^= OriKeys[ECX];
            OriKeys[i] = EDI;

            uint EAX = EDI ^ (EDI >> 0x0B);
            EAX = ((EAX & 0xFF3A58AD) << 0x7) ^ EAX;
            EAX = ((EAX & 0xFFFFDF8C) << 0xF) ^ EAX;
            return (EAX >> 0x12) ^ EAX;
        }

        struct Result {
            internal bool Valid;
            internal List<uint> StrIndxs;
        }       

        //Set DWORD At
        private static void SetDWAt(ref byte[] content, uint pos, uint val) {
            byte[] DW = BitConverter.GetBytes(val);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(DW, 0, 4);
            DW.CopyTo(content, pos);
        }

        //Get DWord At
        public static uint GetDW(byte[] Arr, uint Pos) {
            byte[] DW = new byte[] { Arr[Pos], Arr[Pos + 1], Arr[Pos + 2], Arr[Pos + 3]};
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(DW, 0, 4);
            return BitConverter.ToUInt32(DW, 0);
        }
        
    }
}

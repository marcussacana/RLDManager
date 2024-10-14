﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RLDManager
{
    class RLDExt
    {
        internal static uint[] GetLocalKeys()
        {
            List<uint> KnowedKeys = new List<uint>();
            if (System.IO.File.Exists("RLDKeys.txt"))
            {
                string[] CustomKeys = System.IO.File.ReadAllLines("RLDKeys.txt");
                foreach (string Content in CustomKeys)
                {
                    uint Key;
                    if (Content.StartsWith("0x"))
                    {
                        string Hex = Content.Substring(2, Content.Length - 2);
                        Key = Convert.ToUInt32(Hex, 16);
                    }
                    else
                    {
                        if (!uint.TryParse(Content, out Key))
                            continue;
                    }

                    KnowedKeys.Add(Key);
                }
            }
            return KnowedKeys.ToArray();
        }

        internal static void DbgDump(byte[] Script)
        {
            if (System.Diagnostics.Debugger.IsAttached)
                System.IO.File.WriteAllBytes("dump.rld", Script);
        }
    }
    public class RLD
    {
        public static Dictionary<string, uint> KnowedKeys = new Dictionary<string, uint>() {
            { "Sakura no Mori Dreamers [JP]",       0xAD2B78EA },
            { "Sakura no Mori Dreamers [JP] [Alt]", 0x0C328491 },
            { "Princess Evangile",                  0x39AA8BA0 },
            { "Princess Evangile W",                0xE69A420B },
            { "Magical Marriage Lunatics [JP]",     0xC9E849B1 },
            { "Magical Marriage Lunatics [EN]",     0xFA267D75 },
            { "Imouto Paradise 3 [JP]",             0x2A17122A },
            { "Imouto Paradise 3 [EN]",             0x2A18A1A4 }
        };

        /// <summary>
        /// Get The Encryption Key to the given script
        /// </summary>
        /// <param name="Script">The Script to Get the Key</param>
        /// <returns>The key, if fails returns uint.MaxValue</returns>
        public static uint SelectKey(byte[] Script)
        {
            uint[] KnowedKeys = RLD.KnowedKeys.Values.ToArray();

            try
            {
                KnowedKeys = KnowedKeys.Append(RLDExt.GetLocalKeys());
                KnowedKeys = KnowedKeys.Reverse().ToArray();
            }
            catch { }

            byte[] Tmp = new byte[Script.Length];
            if (LastKnowedKey != 0)
            {
                Script.CopyTo(Tmp, 0);
                if (new RLD(Tmp, LastKnowedKey).Import().Length > 2)
                    return LastKnowedKey;
            }

            foreach (uint Key in KnowedKeys)
            {
                Script.CopyTo(Tmp, 0);
                if (new RLD(Tmp, Key).Import().Length > 2)
                {
                    LastKnowedKey = Key;
                    return Key;
                }
            }

            return uint.MaxValue;
        }

        static uint LastKnowedKey = 0;
        public uint EncryptionKey = 0;
        public Encoding Encoding = Encoding.GetEncoding(932);

        bool Decrypted = false;
        byte[] Script;
        uint[] Offsets;
        uint[] Lenghts;
        public RLD(byte[] Script)
        {
            this.Script = Script;

            uint Key = SelectKey(Script);
            if (Key == uint.MaxValue)
                throw new Exception("Unknown Encryption Key");

            EncryptionKey = Key;
        }

        public RLD(byte[] Script, uint Key)
        {
            this.Script = Script;
            EncryptionKey = Key;
        }


        public string[] Import()
        {
            if (!Decrypted)
            {
                Decrypted = true;
                XOR(ref Script);

                try
                {
                    RLDExt.DbgDump(Script);
                }
                catch { }
            }
            Offsets = new uint[0];
            for (uint i = 0; i < Script.Length; i++)
            {
                Result Info = Scan(i);
                if (!Info.Valid)
                    continue;

                foreach (uint ptr in Info.StrIndxs)
                {
                    if (Offsets.Contains(ptr) || GetStrLen(ptr) < 3 || IsCommand(ptr))
                        continue;

                    Offsets = Offsets.Append(ptr);
                }
            }
            //Initialize Variables
            string[] Strs = new string[Offsets.Length];
            Lenghts = new uint[Offsets.Length];

            for (int i = 0; i < Strs.Length; i++)
            {
                Strs[i] = Script.GetStringAt(Offsets[i], Encoding);
                Lenghts[i] = GetStrLen(Offsets[i]);
            }
            return Strs;
        }

        private bool IsCommand(uint ptr)
        {
            string String = Script.GetStringAt(ptr, Encoding);
            string Minified = string.Empty;
            foreach (char c in String)
            {
                if (c == '「' || c == '」')
                    return false;

                if (c >= '0' && c <= '9')
                    continue;
                if (c == ',' || c == '.')
                    continue;
                if (c == '-' || c == '*')
                    continue;
                if (c == ':' || c == '#')
                    continue;
                if (c == '@')
                    continue;

                if (IsWTF((byte)(c & 0xFF)))
                    continue;

                Minified += c;
            }

            return Minified.Length < String.Length / 4;
        }

        public byte[] Export(string[] Strings)
        {
            if (Strings.Length != Offsets.Length)
                throw new Exception("You Can't add new strings.");
            byte[] Output = new byte[Script.Length];
            Script.CopyTo(Output, 0);
            //Reverse Replace to don't need update offsets after a change.
            for (int i = Offsets.Length - 1; i >= 0; i--)
            {
                uint POS = Offsets[i];
                uint LEN = Lenghts[i];

                //Get Content Before the string
                byte[] Bef = Output.GetRange(0, POS);

                //Compile String
                byte[] Str = Encoding.GetBytes(Strings[i]);

                //Get Content After the string
                byte[] Aft = Output.GetRange(POS + LEN, (uint)Output.LongLength - (POS + LEN));

                //Merge Parts
                Output = new byte[0];
                Output = Output.Append(Bef);
                Output = Output.Append(Str);
                Output = Output.Append(Aft);
            }
            //Encrypt and Return
            XOR(ref Output);
            return Output;
        }



        private Result Scan(uint Pos)
        {
            Result rst = new Result()
            {
                Valid = false,
                StrIndxs = new List<uint>()
            };

            if (Pos + 7 < Script.Length)
            {
                if (Script[Pos] == 0xFF && Script[Pos + 1] == 0xFF)
                {
                    if (Script[Pos + 2] == 0x2A && Script[Pos + 3] == 0x00)
                    {
                        uint StrIndx = Pos + 4;
                        rst.Valid = IsChar(Script[StrIndx], out _) && IsChar(Script[StrIndx + 1], out _);
                        if (rst.Valid)
                            rst.StrIndxs.Add(StrIndx);
                    }
                }
                if (!rst.Valid)
                    if (Script[Pos] == 0xFF && Script[Pos + 1] == 0xFF && Script[Pos + 2] == 0xFF && Script[Pos + 3] == 0xFF)
                    {
                        if (IsChar(Script[Pos + 4], out _))
                        {
                            if (IsValidStr(Pos + 4))
                            {
                                rst.Valid = true;
                                if (IsValidDoubleStr(Pos + 4))
                                {
                                    uint Ptr = Pos + 4;
                                    rst.StrIndxs.Add(Ptr);
                                    while (Script[Ptr++] != 0x00)
                                        continue;
                                    rst.StrIndxs.Add(Ptr);
                                }
                                else
                                {
                                    rst.StrIndxs.Add(Pos + 4);
                                }
                            }
                        }
                    }
                if (!rst.Valid)
                {
                    uint Val = Script.GetDW(Pos);
                    if (Val == 0x64 && IsChar(Script[Pos + 4], out _))
                    {
                        if (Script[Pos + 4] == 0x2A)
                        {
                            rst.Valid = true;
                            rst.StrIndxs.Add(Pos + 0x6);
                        }
                        else
                        {
                            rst.StrIndxs.Add(Pos + 0x4);
                            if (IsValidDoubleStr(Pos + 4))
                            {
                                uint Ptr = Pos + 4;
                                Ptr += GetStrLen(Ptr);
                                rst.StrIndxs.Add(Ptr);
                            }
                        }
                    }
                }
            }
            if (Pos + 3 < Script.Length)
            {
                if (!rst.Valid)
                {
                    if (Script[Pos] == 0x2A && Script[Pos + 1] == 0x00)
                    {
                        uint Ptr = Pos - 1;
                        while (Ptr > 0x10 && Script[Ptr] == 0x00)
                            Ptr--;
                        uint i = Pos + 2;
                        if (Script[Ptr] == 0xFF && Script[Ptr - 1] == 0xFF)
                        {
                            if (IsChar(Script[i], out _) && IsChar(Script[i + 2], out _))
                            {
                                rst.Valid = true;
                                rst.StrIndxs.Add(i);
                                while (i < Script.Length && Script[i] != 0x00)
                                    i++;
                                if (IsChar(Script[++i], out _))
                                {
                                    if (IsChar(Script[i + 2], out _))
                                    {
                                        rst.StrIndxs.Add(i);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return rst;
        }

        private uint GetStrLen(uint At)
        {
            uint Len = 0;
            while (Script[At + Len] != 0x00)
                Len++;
            return Len;
        }
        private bool IsValidDoubleStr(uint At)
        {
            if (IsValidStr(At))
            {
                At += GetStrLen(At) + 1;
                if (IsValidStr(At))
                    return true;
            }
            return false;
        }
        private bool IsValidStr(uint At)
        {
            byte b = 0x00;
            while ((b = Script[At++]) != 0x00)
            {
                if (!IsChar(b, out bool MB) && !IsWTF(b))
                    return false;

                if (MB)
                    At++;
            }
            return true;
        }
        private bool IsChar(byte b, out bool MultiByte)
        {
            MultiByte = true;
            if (b == 0x82 || b == 0x81 || b == 0x83 || (b >= 0x88 && b <= 0x9F))
                return true;

            MultiByte = false;

            return (b >= 0x20 && b <= 0x7F) || b == 0x0A || b == 0x0D;
        }

        //Magical Marriage Lunatics!! [EN]
        private bool IsWTF(byte b)
        {
            return (b == 0x0C || (b >= 0xAA && b <= 0xAF));
        }

        public void XOR(ref byte[] Content)
        {
            uint Key = EncryptionKey;

            uint BlockCount = (uint)Content.Length;

            if (BlockCount > 0xFFCF)
                BlockCount = 0xFFCF;

            BlockCount -= (BlockCount % 4);
            uint[] Keys = GenKeyTable(Key);
            for (uint i = 0x10, ri = 0; i < BlockCount; i += 4, ri++)
            {
                uint enc = Content.GetDW(i);
                uint tmp = Keys[(ri & 0xFF)];
                SetDWAt(ref Content, i, tmp ^ enc);
            }
        }

        static long Progress = 0;
        public static string FindProgress
        {
            get
            {
                double Prog = ((double)Progress / uint.MaxValue) * 100;
                return string.Format("Remaining: {0}|Progress: {1}%", uint.MaxValue - Progress, (int)Prog);
            }
        }

        public static bool FindKey(byte[] Script, out uint[] FoundKey)
        {
            Progress = 0;
            ConcurrentQueue<uint> Results = new ConcurrentQueue<uint>();
            uint Rst = Script.GetDW(0x10u);

            var Thread = new System.Threading.Thread(() =>
            {
                Parallel.For(0, uint.MaxValue, (a, loop) =>
                {
                    System.Threading.Interlocked.Increment(ref Progress);

                    unchecked
                    {
                        uint Seed = (uint)a;

                        uint[] Keys;
                        uint EDX = Seed;
                        uint ECX = 0;

                        uint End = 0x18E;
                        uint[] Buffer = new uint[End];
                        uint Index = 0;

                        do
                        {
                            //Gen Seed
                            ECX = (EDX * 0x10DCD) + 1;
                            Buffer[Index++] = (EDX & 0xFFFF0000) | ((ECX >> 0x10) & 0x0000FFFF);

                            //Next
                            EDX = (EDX * 0x1C587629) + 0x10DCE;
                        } while (Index < End);
                        Keys = Buffer;

                        ECX = Keys[0];
                        EDX = 1;
                        if (EDX >= Keys.Length)
                            EDX = 0;

                        uint EDI = (((Keys[EDX] ^ ECX) & 0x7FFFFFFF) ^ ECX);

                        bool CF = (EDI & 0x1) == 1;//SHR CF Check
                        EDI >>= 1;

                        if (CF)
                            EDI ^= 0x9908B0DF;

                        ECX = 0x18D;
                        if (ECX >= Keys.Length)
                            ECX -= (uint)Keys.Length;

                        EDI ^= Keys[ECX];
                        Keys[0] = EDI;

                        EDI ^= (EDI >> 0x0B);
                        EDI ^= ((EDI & 0xFF3A58AD) << 0x7);
                        EDI ^= ((EDI & 0xFFFFDF8C) << 0xF);
                        uint Key = (EDI >> 0x12) ^ EDI;


                        Key ^= Seed;

                        if ((Key ^ Rst) < 0xF)
                        {
                            // defensive copy, since RLD import process writes to the array
                            byte[] LocalScript = Script.ToArray();

                            if (new RLD(LocalScript, Seed).Import().Length >= 2)
                            {
                                Results.Enqueue(Seed);
                                loop.Break();
                            }
                        }
                    }
                });
            });

            Thread.Start();
            Thread.Join();

            Progress = uint.MaxValue;
            FoundKey = Results.ToArray();
            return FoundKey.Length > 0;
        }


        private static uint[] GenKeyTable(uint Seed)
        {
            uint[] Keys = GenSeeds(Seed);
            uint[] Seeds = new uint[Keys.Length];
            Keys.CopyTo(Seeds, 0);
            for (uint i = 0; i < Keys.Length; i++)
            {
                uint Key = KeyWork(i, ref Keys, ref Seeds);
                Keys[i] = Key ^ Seed;
            }
            return Keys;
        }

        private static uint[] GenSeeds(uint Key)
        {
            uint EDX = Key;
            uint EAX = 0x00510010;
            uint ECX = 0x0000225C;

            uint EBX = 0x270;
            uint[] Buffer = new uint[EBX];
            uint EDI = 0;

            do
            {
                //Gen Seed
                ECX = (EDX * 0x10DCD);
                EAX = (EDX & 0xFFFF0000) | (((ECX + 1) >> 0x10) & 0x0000FFFF);
                Buffer[EDI++] = EAX;

                //Begin Next
                EDX = (EDX * 0x1C587629);
                EDX += 0x10DCE;

                EBX--;
            } while (EBX != 0);

            return Buffer;
        }

        private static uint KeyWork(uint i, ref uint[] Keys, ref uint[] OriKeys)
        {
            unchecked
            {
                uint ECX = Keys[i];
                uint EDX = i + 1;
                if (EDX >= Keys.Length)
                    EDX = 0;

                uint EDI = (((Keys[EDX] ^ ECX) & 0x7FFFFFFF) ^ ECX);

                bool CF = (EDI & 0x1) == 1;//SHR CF Check
                EDI >>= 1;

                if (CF)
                    EDI ^= 0x9908B0DF;

                ECX = i + 0x18D;
                if (ECX >= Keys.Length)
                    ECX -= (uint)Keys.Length;

                EDI ^= OriKeys[ECX];
                OriKeys[i] = EDI;

                EDI ^= (EDI >> 0x0B);
                EDI ^= ((EDI & 0xFF3A58AD) << 0x7);
                EDI ^= ((EDI & 0xFFFFDF8C) << 0xF);
                return (EDI >> 0x12) ^ EDI;
            }
        }

        struct Result
        {
            internal bool Valid;
            internal List<uint> StrIndxs;
        }

        private static void SetDWAt(ref byte[] content, uint pos, uint val)
        {
            byte[] DW = BitConverter.GetBytes(val);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(DW, 0, 4);
            DW.CopyTo(content, pos);
        }

    }
}
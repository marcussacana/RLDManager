using System.Collections.Generic;
using System.Linq;

namespace RLDManager {
    public class RLDFilter {

        string[] Blacklist = new string[] {
            "com", "ＭＳ Ｐゴシッ"
        };
        RLD Editor;
        string[] Strings;
        public RLDFilter(byte[] Script) {
            Editor = new RLD(Script);
        }
        public RLDFilter(byte[] Script, uint Key) {
            Editor = new RLD(Script, Key);
        }

        public string[] Import() {
            Strings = Editor.Import();
            List<string> Result = new List<string>();
            foreach (string Str in Strings) {
                if (Blacklist.Contains(Str) || RLD.IsCommand(Str))
                    continue;

                if (Str.Contains('\t')) {
                    foreach (string Choice in Str.Split('\t')) {
                        if (!RLD.IsCommand(Choice))
                            Result.Add(Choice);
                    }
                } else Result.Add(Str);
            }

            return Result.ToArray();
        }

        public byte[] Export(string[] Strings) {
            List<string> Result = new List<string>();
            int Index = 0;
            foreach (string Str in this.Strings) {
                if (Blacklist.Contains(Str)) {
                    Result.Add(Str);
                    continue;
                }

                if (Str.Contains('\t')) {
                    string[] Parts = Str.Split('\t');
                    int Count = Parts.Length;
                    string rst = string.Empty;
                    for (int i = 0; i < Count; i++) {
                        rst += RLD.IsCommand(Parts[i]) ? Parts[i] : Strings[Index++];
                        if (i + 1 < Count)
                            rst += '\t';
                    }
                    Result.Add(rst);
                } else
                    Result.Add(Strings[Index++]);
            }

            return Editor.Export(Result.ToArray());
        }
    }
}

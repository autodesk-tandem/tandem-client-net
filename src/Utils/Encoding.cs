using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TandemSDK.Utils
{
    internal class Encoding
    {
        private const int kModelIdSize = 16;
        private const int kElementIdSize = 20;
        private const int kElementFlagsSize = 4;
        private const int kElementIdWithFlagsSize = kElementIdSize + kElementFlagsSize;

        private static readonly int[] B64ToUInt6;

        static Encoding()
        {
            B64ToUInt6 = new int[128];

            for (int i = 0; i < 128; i++)
            {
                B64ToUInt6[i] = B64ToUInt6Gen(i);
            }
        }
        
        public static string FromShortKey(string shortKey, long? flags)
        {
            var buff = Decode(shortKey, kElementIdWithFlagsSize, 4);
            
            if (flags.HasValue)
            {
                buff[0] = (byte)((flags >> 24) & 0xFF);
                buff[1] = (byte)((flags >> 16) & 0xFF);
                buff[2] = (byte)((flags >> 8) & 0xFF);
                buff[3] = (byte)(flags & 0xFF);
            }
            string result = Convert.ToBase64String(buff);

            return MakeWebSafe(result);
        }

        public static (string, string) FromXrefKey(string key)
        {
            var buff = Decode(key, kModelIdSize + kElementIdWithFlagsSize);
            var modelBuff = new byte[kModelIdSize];

            Buffer.BlockCopy(buff, 0, modelBuff, 0, 16);
            string modelKey = MakeWebSafe(Convert.ToBase64String(modelBuff));
            var elementBuff = new byte[kElementIdWithFlagsSize];

            Buffer.BlockCopy(buff, 16, elementBuff, 0, kElementIdWithFlagsSize);
            string elementKey = MakeWebSafe(Convert.ToBase64String(elementBuff));

            return (modelKey, elementKey);
        }

        private static byte[] Decode(string text, int size = kElementIdWithFlagsSize, int start = 0)
        {
            var len = text.Length;
            var outputLen = Base64DecodedLength(len);
            var buff = new byte[size];

            for (int nMod3, nMod4, nUInt24 = 0, nOutIdx = 0, nInIdx = 0; nInIdx < len; nInIdx++)
            {
                nMod4 = nInIdx & 3;
                nUInt24 |= B64ToUInt6[text[nInIdx]] << 18 - 6 * nMod4;
                if (nMod4 == 3 || len - nInIdx == 1)
                {
                    for (nMod3 = 0; nMod3 < 3 && nOutIdx < outputLen; nMod3++, nOutIdx++)
                    {
                        buff[start + nOutIdx] = (byte)(int)((uint)nUInt24 >> ((int)((uint)16 >> nMod3 & 24)) & 255);
                    }
                    nUInt24 = 0;
                }
            }
            return buff;
        }

        private static int B64ToUInt6Gen(int nChr)
        {
            return nChr > 64 && nChr < 91 ?
                nChr - 65
                : nChr > 96 && nChr < 123 ?
                    nChr - 71
                    : nChr > 47 && nChr < 58 ?
                        nChr + 4
                        : nChr == 43 || nChr == 45 ?
                            62
                            : nChr == 47 || nChr == 95 ?
                                63
                                :
                                0;
        }

        private static int Base64DecodedLength(int length)
        {
            return (int)((uint)(length * 3 + 1) >> 2);
        }

        private static string MakeWebSafe(string text)
        {
            var sb = new StringBuilder();

            foreach (char c in text)
            {
                if (c == '+')
                {
                    sb.Append('-');
                }
                else if (c == '/')
                {
                    sb.Append('_');
                }
                else if (c != '=')
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
    }
}

using System.Text;

namespace TandemSDK.Utils
{
    public class Encoding
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
            var buff = Decode(shortKey, 4);
            
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

        public static string[] FromShortKeyArray(string key)
        {
            var buff = Decode(key);
            int offset = 0;
            var elementKeys = new List<string>();

            while (offset < buff.Length)
            {
                var size = buff.Length - offset;

                if (size < kElementIdSize)
                {
                    break;
                }
                var elementBuff = new byte[kElementIdWithFlagsSize];

                Buffer.BlockCopy(buff, offset, elementBuff, kElementFlagsSize, kElementIdSize);
                string elementKey = MakeWebSafe(Convert.ToBase64String(elementBuff));

                elementKeys.Add(elementKey);
                offset += kElementIdSize;
            }
            return (elementKeys.ToArray());
        }

        public static (string[], string[]) FromXrefKey(string key)
        {
            var buff = Decode(key);
            int offset = 0;
            var modelKeys = new List<string>();
            var elementKeys = new List<string>();

            while (offset < buff.Length)
            {
                var size = buff.Length - offset;

                if (size < (kModelIdSize + kElementIdWithFlagsSize))
                {
                    break;
                }
                var modelBuff = new byte[kModelIdSize];

                Buffer.BlockCopy(buff, offset, modelBuff, 0, kModelIdSize);
                string modelKey = MakeWebSafe(Convert.ToBase64String(modelBuff));

                modelKeys.Add(modelKey);
                var elementBuff = new byte[kElementIdWithFlagsSize];

                Buffer.BlockCopy(buff, offset + kModelIdSize, elementBuff, 0, kElementIdWithFlagsSize);
                string elementKey = MakeWebSafe(Convert.ToBase64String(elementBuff));

                elementKeys.Add(elementKey);
                offset += (kModelIdSize + kElementIdWithFlagsSize);
            }
            return (modelKeys.ToArray(), elementKeys.ToArray());
        }

        public static string ToShortKey(string key)
        {
            key = key.Replace('-', '+');
            key = key.Replace('_', '/');
            key = Pad(key);
            var buff = Convert.FromBase64String(key);
            var result = new byte[kElementIdSize];

            Buffer.BlockCopy(buff, kElementFlagsSize, result, 0, kElementIdSize);
            return MakeWebSafe(Convert.ToBase64String(result));
        }

        public static string ToSystemId(string key)
        {
            var buff = Decode(key);

            int id = buff[^4] << 24;
            id |= buff[^3] << 16;
            id |= buff[^2] << 8;
            id |= buff[^1];

            var buff2 = new byte[9];
            var offset = new int[] { 0 };

            var len = WriteVarint(buff2, offset, id);
            var text = Convert.ToBase64String(buff2, 0, len);

            // remove padding
            return text.Replace("=", string.Empty);
        }

        public static string ToXrefKey(string modelId, string elementKey)
        {
            modelId = modelId.Replace(Prefixes.Model, string.Empty);
            modelId = modelId.Replace('-', '+');
            modelId = modelId.Replace('_', '/');
            modelId = Pad(modelId);
            var modelBuff = Convert.FromBase64String(modelId);

            elementKey = elementKey.Replace('-', '+');
            elementKey = elementKey.Replace('_', '/');
            elementKey = Pad(elementKey);
            var elementBuff = Convert.FromBase64String(elementKey);
            var result = new byte[kModelIdSize + kElementIdWithFlagsSize];

            Buffer.BlockCopy(modelBuff, 0, result, 0, kModelIdSize);
            Buffer.BlockCopy(elementBuff, 0, result, kModelIdSize, kElementIdWithFlagsSize);
            return MakeWebSafe(Convert.ToBase64String(result));
        }

        private static string Pad(string text)
        {
            int count = text.Length % 4;
            if (count > 0)
            {
                text += new string('=', 4 - count);
            }
            return text;
        }

        private static int WriteVarint(byte[] buff, int[] offset, int value)
        {
            var startOffset = offset[0];

            value = 0 | value;
            do
            {
                var b = 0 | (value & 0x7f);

                value = (int)((uint) value >> 7);
                if (value != 0)
                {
                    b |= 0x80;
                }
                buff[offset[0]++] = (byte)b;
            } while (value != 0);

            return offset[0] - startOffset;
        }

        private static byte[] Decode(string text, int start = 0)
        {
            var len = text.Length;
            var outputLen = Base64DecodedLength(len);
            var buff = new List<byte>();

            if (start > 0)
            {
                while (buff.Count < start)
                {
                    buff.Add(0);
                }
            }
            for (int nMod3, nMod4, nUInt24 = 0, nOutIdx = 0, nInIdx = 0; nInIdx < len; nInIdx++)
            {
                nMod4 = nInIdx & 3;
                nUInt24 |= B64ToUInt6[text[nInIdx]] << 18 - 6 * nMod4;
                if (nMod4 == 3 || len - nInIdx == 1)
                {
                    for (nMod3 = 0; nMod3 < 3 && nOutIdx < outputLen; nMod3++, nOutIdx++)
                    {
                        buff.Insert(start + nOutIdx, (byte)(int)((uint)nUInt24 >> ((int)((uint)16 >> nMod3 & 24)) & 255));
                    }
                    nUInt24 = 0;
                }
            }
            return buff.ToArray();
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

        public static string MakeWebSafe(string text)
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

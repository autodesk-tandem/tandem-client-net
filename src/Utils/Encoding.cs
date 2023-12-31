﻿using System.Text;

namespace Autodesk.Tandem.Client.Utils
{
    public class Encoding
    {
        private const int kModelIdSize = 16;
        private const int kElementIdSize = 20;
        private const int kElementFlagsSize = 4;
        private const int kElementIdWithFlagsSize = kElementIdSize + kElementFlagsSize;

        public static string FromShortKey(string shortKey, long? flags = null)
        {
            var buff = Decode(shortKey, kElementFlagsSize);
            
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

        /// <summary>
        /// Converts full element key to Revit GUID
        /// </summary>
        /// <param name="key"></param>
        /// <returns>A string in the form of Revit Unique ID</returns>
        public static string ToElementGUID(string key)
        {
            key = key.Replace('-', '+');
            key = key.Replace('_', '/');
            key = Pad(key);
            var buff = Convert.FromBase64String(key);
            var bin = new byte[kElementIdSize];

            Buffer.BlockCopy(buff, kElementFlagsSize, bin, 0, kElementIdSize);
            var hex = BitConverter.ToString(bin).Replace("-", "").ToLower();
            var hexGroups = new int[] { 8, 4, 4, 4, 12 };
            var start = 0;
            var result = string.Empty;

            for (var i = 0; i < hexGroups.Length; i++)
            {
                var len = hexGroups[i];

                result += hex.Substring(start, len);
                result += "-";
                start += len;
            }
            if (start < hex.Length)
            {
                result += hex[start..];
            }
            return result;
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
            text = text.Replace('-', '+');
            text = text.Replace('_', '/');
            text = Pad(text);
            var buff = Convert.FromBase64String(text);

            if (start == 0)
            {
                return buff;
            }
            var result = new byte[start + buff.Length];

            buff.CopyTo(result, start);
            return result;
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

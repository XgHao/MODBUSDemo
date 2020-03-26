using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Common
{
    public static class ExtendMethod
    {
        /// <summary>
        /// 去掉头部和校验码，只返回正文
        /// string[]=>byte[]
        /// </summary>
        /// <param name="strArr"></param>
        /// <returns></returns>
        public static byte[] ToByteArray(this string[] strArr)
        {
            if (strArr == null)
            {
                return null;
            }
            var res = new byte[strArr.Length - 5];
            for (int i = 3; i < strArr.Length - 2;  i++)
            {
                if (byte.TryParse(strArr[i], out byte _byte)) 
                {
                    res[i - 3] = _byte;
                }
                else
                {
                    throw new Exception("转换失败");
                }
            }
            return res;
        }

        /// <summary>
        /// 刷新listbox项
        /// </summary>
        /// <param name="listBox"></param>
        /// <param name="lists"></param>
        public static void RefreshItemWithInvoke(this ListBox listBox, List<string> lists)
        {
            if (listBox.InvokeRequired)
            {
                listBox.Invoke(new Action<List<string>>(items =>
                {
                    listBox.Items.Clear();
                    listBox.Items.AddRange(items.ToArray());
                }), lists);
                return;
            }

            listBox.Items.Clear();
            listBox.Items.AddRange(lists.ToArray());
        }

        /// <summary>
        /// 获取正文byte数组
        /// </summary>
        /// <param name="strs"></param>
        /// <returns></returns>
        public static byte[] GetContextByArrbyte(this List<string> strs)
        {
            int head = strs.IndexOf("**头部**");
            int end = strs.LastIndexOf("**校验码**");
            string[] strArr = strs.Skip(head).Take(end - head).ToArray();
            byte[] bytes = new byte[strArr.Length];
            for (int i = 0; i < strArr.Length; i++)
            {
                //转换失败
                if (!byte.TryParse(strArr[i], out bytes[i])) 
                {
                    throw new Exception("List<string>转byte[]失败");
                }
            }
            return bytes;
        } 

        /// <summary>
        /// 字节数组是否相等
        /// </summary>
        /// <param name="bytesA"></param>
        /// <param name="bytesB"></param>
        /// <returns></returns>
        public static bool ByteArrIsEqual(this byte[] bytesA,byte[] bytesB)
        {
            if (bytesA != null && bytesB != null && bytesA.Length == bytesB.Length)  
            {
                for (int i = 0; i < bytesA.Length; i++)
                {
                    if (bytesA[i] != bytesB[i])  
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }
    }
}

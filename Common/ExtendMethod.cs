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
            var res = new byte[strArr.Length];
            for (int i = 0; i < strArr.Length; i++)
            {
                if (byte.TryParse(strArr[i], out byte _byte)) 
                {
                    res[i] = _byte;
                }
                else
                {
                    throw new Exception("转换失败");
                }
            }
            return res;
        }


        public static void RefreshItemWithInvoke(this ListBox listBox, object[] objs)
        {

        }
    }
}

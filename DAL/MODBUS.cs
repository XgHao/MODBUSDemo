using Common;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace DAL
{
    public class MODBUS
    {
        private SerialPort com;

        //定义接受字节缓冲区
        readonly byte[] bData = new byte[1024];

        //定义设备地址
        int CurrentAddr;    //从站地址
        int iMWordLen;      //寄存区长度

        //接收到的字符串信息
        string strUpData = string.Empty;

        /// <summary>
        /// 信号量-数据接收是否完成
        /// </summary>
        public CancellationTokenSource tokenSource;

        /// <summary>
        /// 默认【9600 N 8 1】
        /// </summary>
        /// <param name="_baudRate">串行波特率</param>
        /// <param name="_dataBits">数据位长度</param>
        /// <param name="_parity">奇偶校验</param>
        /// <param name="_portName">通信端口</param>
        /// <param name="_stopBits">停止位</param>
        public MODBUS(string _portName, int _baudRate = 9600, int _dataBits = 8, Parity _parity = Parity.None, StopBits _stopBits = StopBits.One)
        {
            com = new SerialPort
            {
                BaudRate = _baudRate,
                DataBits = _dataBits,
                Parity = _parity,
                PortName = _portName,
                StopBits = _stopBits,
                ReceivedBytesThreshold = 1
            };
        }

        /// <summary>
        /// 打开串口
        /// </summary>
        /// <returns></returns>
        public bool Open()
        {
            if (com.IsOpen)
            {
                com.Close();
            }
            try
            {
                com.Open();
                com.DataReceived += Com_DataReceived;
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 端口接受数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Com_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            //数据缓冲区游标及字节容器
            int mReceivedByteCnt = 0;
            byte mReceiveByte;

            //判断接收缓冲区是否还有数据
             while (com.BytesToRead > 0) 
            {
                mReceiveByte = (byte)com.ReadByte();
                bData[mReceivedByteCnt++] = mReceiveByte;
                //判断是否超出缓冲区大小
                if (mReceivedByteCnt >= 1024) 
                {
                    //抛弃后面的数据
                    com.DiscardInBuffer();
                    break;
                }
            }
            //读取保持型寄存器 功能码0x03
            if (bData[0] == (byte)CurrentAddr && bData[1] == 0x03 && mReceivedByteCnt >= iMWordLen * 2 + 5)
            {
                strUpData = string.Empty;
                for (int i = 0; i < iMWordLen * 2 + 5; i++)
                {
                    strUpData += $" {bData[i]:X2}";
                }
                com.DiscardInBuffer();
            }
            //预置单字保持型寄存器 功能码0x06
            else if (bData[0] == (byte)CurrentAddr && bData[1] == 0x06 && mReceivedByteCnt >= 8) 
            {
                strUpData = string.Empty;
                for (int i = 0; i < 8; i++)
                {
                    strUpData += $" {bData[i]:X2}";
                }
                com.DiscardInBuffer();
            }
            //接收完成
            tokenSource?.Cancel();
        }

        /// <summary>
        /// 关闭串口
        /// </summary>
        /// <returns></returns>
        public bool Close()
        {
            if (com.IsOpen)
            {
                try
                {
                    com.Close();
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 读保保持性寄存器
        /// </summary>
        /// <param name="iDevAdd">从站地址</param>
        /// <param name="iAddress">起始地址</param>
        /// <param name="iLength">长度</param>
        /// <returns></returns>
        public byte[] ReadKeepReg(int iDevAdd,int iAddress,int iLength)
        {
            byte[] ResByte = null;
            iMWordLen = iLength;
            CurrentAddr = iDevAdd;
            
            //1.拼接报文
            byte[] sendCommand = new byte[8];
            sendCommand[0] = (byte)iDevAdd;
            sendCommand[1] = 0x03;
            sendCommand[3] = (byte)(iAddress % 256);
            sendCommand[2] = (byte)((iAddress - iAddress % 256) / 256);
            sendCommand[5] = (byte)(iLength % 256);
            sendCommand[4] = (byte)((iLength - iLength % 256) / 256);
            Crc16(sendCommand, 6, out sendCommand[6], out sendCommand[7]);
            //2.发送报文
            try
            {
                com.Write(sendCommand, 0, 8);
            }
            catch (Exception)
            {
                throw;
            }
            return ResByte;
        }

        /// <summary>
        /// 预置单字保持性寄存器 功能码06
        /// </summary>
        /// <param name="iDevAdd"></param>
        /// <param name="iAddress"></param>
        /// <param name="SetValue"></param>
        /// <returns></returns>
        public bool PreSetKeepReg(int iDevAdd,int iAddress,int SetValue)
        {
            byte[] SendCommand = new byte[8];
            CurrentAddr = iDevAdd;
            //1.拼接报文
            SendCommand[0] = (byte)iDevAdd;
            SendCommand[1] = 0x06;
            SendCommand[2] = (byte)((iDevAdd - iAddress % 256) / 256);
            SendCommand[3] = (byte)(iDevAdd % 256);
            SendCommand[4] = (byte)((SetValue - SetValue % 256) / 256);
            SendCommand[5] = (byte)(SetValue % 256);
            Crc16(SendCommand, 6, out SendCommand[6],out SendCommand[7]);
            //2.发送报文
            try
            {
                com.Write(SendCommand, 0, 8);
            }
            catch (Exception)
            {
                return false;
            }
            //3.解析报文
            var ResByte = StringListFromHexStr(0, 0).GetContextByArrbyte();
            return ResByte.ByteArrIsEqual(SendCommand);
        }

        /// <summary>
        /// 预置双字保持性寄存器 功能码10
        /// </summary>
        /// <param name="iDevAdd"></param>
        /// <param name="iAddress"></param>
        /// <param name="SetValue"></param>
        /// <returns></returns>
        public bool PreSetFloatKeepReg(int iDevAdd, int iAddress, float SetValue)
        {
            byte[] SendCommand = new byte[13];
            CurrentAddr = iDevAdd;
            //1.拼接报文
            SendCommand[0] = (byte)iDevAdd;
            SendCommand[1] = 0x10;
            SendCommand[2] = (byte)((iAddress - iAddress % 256) / 256);
            SendCommand[3] = (byte)(iAddress % 256);
            SendCommand[4] = 0x00;
            SendCommand[5] = 0x02;
            SendCommand[6] = 0x04;
            byte[] bSetValue = BitConverter.GetBytes(SetValue);
            SendCommand[7] = bSetValue[3];
            SendCommand[8] = bSetValue[2];
            SendCommand[9] = bSetValue[1];
            SendCommand[10] = bSetValue[0];
            Crc16(SendCommand, 11, out SendCommand[11], out SendCommand[12]);
            //2.发送报文
            try
            {
                com.Write(SendCommand, 0, 13);
            }
            catch (Exception)
            {
                return false;
            }
            //3.解析报文
            byte[] ResByte = StringListFromHexStr(0, 0).GetContextByArrbyte();
            //解析报文的校验码
            Crc16(ResByte, 6, out byte Low, out byte Hi);
            //比较前六个字节及校验码是否正确
            return ResByte.Take(6).ToArray().ByteArrIsEqual(SendCommand.Take(6).ToArray()) && Low == ResByte[6] && Hi == ResByte[7];
        }

        /// <summary>
        /// 读取输出线圈  功能码01
        /// </summary>
        /// <param name="iDevAdd"></param>
        /// <param name="iAddress"></param>
        /// <param name="iLength"></param>
        /// <returns></returns>
        public byte[] ReadOutputStatus(int iDevAdd,int iAddress,int iLength)
        {
            byte[] SendCommand = new byte[8];
            CurrentAddr = iAddress;
            int iMBitLen = (int)Math.Ceiling(((decimal)iLength) / 8);

            //1.拼接报文
            SendCommand[0] = (byte)iDevAdd;
            SendCommand[1] = 0x01;
            SendCommand[2] = (byte)((iAddress - iAddress % 256) / 256);
            SendCommand[3] = (byte)(iAddress % 256);
            SendCommand[4] = (byte)((iLength - iLength % 256) / 256);
            SendCommand[5] = (byte)(iLength % 256);
            Crc16(SendCommand, 6, out SendCommand[6], out SendCommand[7]);

            return null;
        }

        /// <summary>
        /// 转字节数组
        /// </summary>
        /// <param name="strHex"></param>
        /// <returns></returns>
        public List<string> StringListFromHexStr(int start, int end)
        {
            //分隔成数组
            string[] strArray = strUpData.Trim().Split(' ');

            string[] res = new string[strArray.Length + 3];
            res[0] = "**头部**";
            Array.Copy(strArray, 0, res, 1, start);
            res[4] = "**正文**";
            Array.Copy(strArray, start, res, start + 2, strArray.Length - start - end);
            res[25] = "**校验码**";
            Array.Copy(strArray, strArray.Length - end, res, res.Length - end, end);
            return res.ToList(); 
        }

        #region  CRC校验
        private static readonly byte[] aucCRCHi = {
             0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
             0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
             0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
             0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
             0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
             0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
             0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
             0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
             0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
             0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
             0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
             0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
             0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
             0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
             0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
             0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
             0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
             0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
             0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
             0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
             0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
             0x00, 0xC1, 0x81, 0x40
         };
        private static readonly byte[] aucCRCLo = {
             0x00, 0xC0, 0xC1, 0x01, 0xC3, 0x03, 0x02, 0xC2, 0xC6, 0x06, 0x07, 0xC7,
             0x05, 0xC5, 0xC4, 0x04, 0xCC, 0x0C, 0x0D, 0xCD, 0x0F, 0xCF, 0xCE, 0x0E,
             0x0A, 0xCA, 0xCB, 0x0B, 0xC9, 0x09, 0x08, 0xC8, 0xD8, 0x18, 0x19, 0xD9,
             0x1B, 0xDB, 0xDA, 0x1A, 0x1E, 0xDE, 0xDF, 0x1F, 0xDD, 0x1D, 0x1C, 0xDC,
             0x14, 0xD4, 0xD5, 0x15, 0xD7, 0x17, 0x16, 0xD6, 0xD2, 0x12, 0x13, 0xD3,
             0x11, 0xD1, 0xD0, 0x10, 0xF0, 0x30, 0x31, 0xF1, 0x33, 0xF3, 0xF2, 0x32,
             0x36, 0xF6, 0xF7, 0x37, 0xF5, 0x35, 0x34, 0xF4, 0x3C, 0xFC, 0xFD, 0x3D,
             0xFF, 0x3F, 0x3E, 0xFE, 0xFA, 0x3A, 0x3B, 0xFB, 0x39, 0xF9, 0xF8, 0x38,
             0x28, 0xE8, 0xE9, 0x29, 0xEB, 0x2B, 0x2A, 0xEA, 0xEE, 0x2E, 0x2F, 0xEF,
             0x2D, 0xED, 0xEC, 0x2C, 0xE4, 0x24, 0x25, 0xE5, 0x27, 0xE7, 0xE6, 0x26,
             0x22, 0xE2, 0xE3, 0x23, 0xE1, 0x21, 0x20, 0xE0, 0xA0, 0x60, 0x61, 0xA1,
             0x63, 0xA3, 0xA2, 0x62, 0x66, 0xA6, 0xA7, 0x67, 0xA5, 0x65, 0x64, 0xA4,
             0x6C, 0xAC, 0xAD, 0x6D, 0xAF, 0x6F, 0x6E, 0xAE, 0xAA, 0x6A, 0x6B, 0xAB,
             0x69, 0xA9, 0xA8, 0x68, 0x78, 0xB8, 0xB9, 0x79, 0xBB, 0x7B, 0x7A, 0xBA,
             0xBE, 0x7E, 0x7F, 0xBF, 0x7D, 0xBD, 0xBC, 0x7C, 0xB4, 0x74, 0x75, 0xB5,
             0x77, 0xB7, 0xB6, 0x76, 0x72, 0xB2, 0xB3, 0x73, 0xB1, 0x71, 0x70, 0xB0,
             0x50, 0x90, 0x91, 0x51, 0x93, 0x53, 0x52, 0x92, 0x96, 0x56, 0x57, 0x97,
             0x55, 0x95, 0x94, 0x54, 0x9C, 0x5C, 0x5D, 0x9D, 0x5F, 0x9F, 0x9E, 0x5E,
             0x5A, 0x9A, 0x9B, 0x5B, 0x99, 0x59, 0x58, 0x98, 0x88, 0x48, 0x49, 0x89,
             0x4B, 0x8B, 0x8A, 0x4A, 0x4E, 0x8E, 0x8F, 0x4F, 0x8D, 0x4D, 0x4C, 0x8C,
             0x44, 0x84, 0x85, 0x45, 0x87, 0x47, 0x46, 0x86, 0x82, 0x42, 0x43, 0x83,
             0x41, 0x81, 0x80, 0x40
         };
        /// <summary>
        /// CRC检验
        /// </summary>
        /// <param name="pucFrame">字节数组</param>
        /// <param name="usLen">长度</param>
        /// <param name="ucCRCHi">检验码高位</param>
        /// <param name="ucCRCLo">校验码低位</param>
        private void Crc16(byte[] pucFrame, int usLen, out byte ucCRCLo, out byte ucCRCHi)
        {
            int i = 0;
            ucCRCHi = 0xFF;
            ucCRCLo = 0xFF;
            UInt16 iIndex = 0x0000;

            while (usLen-- > 0) ;
            {
                iIndex = (UInt16)(ucCRCLo ^ pucFrame[i++]);
                ucCRCLo = (byte)(ucCRCHi ^ aucCRCHi[iIndex]);
                ucCRCHi = aucCRCLo[iIndex];
            }

        }

        #endregion

    }
}

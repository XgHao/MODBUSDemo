using Common;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Data.Common;

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
        int iMBitLen;

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
            else if (bData[0] == (byte)CurrentAddr && bData[1] == 0x10 && mReceivedByteCnt >= 8)
            {
                strUpData = string.Empty;
                for (int i = 0; i < 8; i++)
                {
                    strUpData += $" {bData[i]:X2}";
                }
                com.DiscardInBuffer();
            }
            else if (bData[0] == (byte)CurrentAddr && bData[1] == 0x01 && mReceivedByteCnt >= iMBitLen + 5) 
            {
                strUpData = string.Empty;
                for (int i = 0; i < iMBitLen + 5; i++)
                {
                    strUpData += $" {bData[i]:X2}";
                }
                com.DiscardInBuffer();
            }
            else if (bData[0] == (byte)CurrentAddr && bData[1] == 0x02 && mReceivedByteCnt >= iMBitLen + 5)
            {
                strUpData = string.Empty;
                for (int i = 0; i < iMBitLen + 5; i++)
                {
                    strUpData += $" {bData[i]:X2}";
                }
                com.DiscardInBuffer();
            }
            else if (bData[0] == (byte)CurrentAddr && bData[1] == 0x05 && mReceivedByteCnt >= 8)
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
            Algorithm.Crc16(sendCommand, 6, out sendCommand[6], out sendCommand[7]);
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
            Algorithm.Crc16(SendCommand, 6, out SendCommand[6],out SendCommand[7]);
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
            Algorithm.Crc16(SendCommand, 11, out SendCommand[11], out SendCommand[12]);
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
            Algorithm.Crc16(ResByte, 6, out byte Low, out byte Hi);
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
            iMBitLen = (int)Math.Ceiling(((decimal)iLength) / 8);

            //1.拼接报文
            SendCommand[0] = (byte)iDevAdd;
            SendCommand[1] = 0x01;
            SendCommand[2] = (byte)((iAddress - iAddress % 256) / 256);
            SendCommand[3] = (byte)(iAddress % 256);
            SendCommand[4] = (byte)((iLength - iLength % 256) / 256);
            SendCommand[5] = (byte)(iLength % 256);
            Algorithm.Crc16(SendCommand, 6, out SendCommand[6], out SendCommand[7]);
            //发送
            try
            {
                com.Write(SendCommand, 0, 8);
            }
            catch (Exception)
            {
                return null;
            }
            return StringListFromHexStr(3, 2).GetContextByArrbyte();
        }

        /// <summary>
        /// 读取输入线圈  功能码02
        /// </summary>
        /// <param name="iDevAdd"></param>
        /// <param name="iAddress"></param>
        /// <param name="iLength"></param>
        /// <returns></returns>
        public byte[] ReadInputStatus(int iDevAdd, int iAddress, int iLength)
        {
            byte[] SendCommand = new byte[8];
            CurrentAddr = iAddress;
            iMBitLen = (int)Math.Ceiling(((decimal)iLength) / 8);

            //1.拼接报文
            SendCommand[0] = (byte)iDevAdd;
            SendCommand[1] = 0x02;
            SendCommand[2] = (byte)((iAddress - iAddress % 256) / 256);
            SendCommand[3] = (byte)(iAddress % 256);
            SendCommand[4] = (byte)((iLength - iLength % 256) / 256);
            SendCommand[5] = (byte)(iLength % 256);
            Algorithm.Crc16(SendCommand, 6, out SendCommand[6], out SendCommand[7]);
            //发送
            try
            {
                com.Write(SendCommand, 0, 8);
            }
            catch (Exception)
            {
                return null;
            }
            return StringListFromHexStr(3, 2).GetContextByArrbyte();
        }

        /// <summary>
        /// 强制单线圈 功能码05
        /// </summary>
        /// <param name="iDevAdd"></param>
        /// <param name="iAddress"></param>
        /// <param name="SetValue"></param>
        /// <returns></returns>
        public bool ForceCoil(int iDevAdd,int iAddress,bool SetValue)
        {
            byte[] SendCommand = new byte[8];
            CurrentAddr = iDevAdd;
            SendCommand[0] = (byte)iDevAdd;
            SendCommand[1] = 0x05;
            SendCommand[2] = (byte)((iAddress - iAddress % 256) / 256);
            SendCommand[3] = (byte)(iDevAdd % 256);
            SendCommand[4] = SetValue ? (byte)0xFF : (byte)0x00;
            SendCommand[5] = 0x00;
            Algorithm.Crc16(SendCommand, 6,out SendCommand[6],out SendCommand[7]);

            try
            {
                com.Write(SendCommand, 0, 8);
            }
            catch (Exception)
            {
                return false;
            }

            return SendCommand.ByteArrIsEqual(StringListFromHexStr(0, 0).GetContextByArrbyte());
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
    }
}

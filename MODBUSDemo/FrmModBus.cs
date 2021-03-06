﻿using Common;
using DAL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace MODBUSDemo
{
    public partial class FrmModBus : Form
    {
        public FrmModBus()
        {
            InitializeComponent();
        }

        MODBUS modbusObj = null;

        /// <summary>
        /// 连接串口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_Connect_Click(object sender, EventArgs e)
        {
            modbusObj = new MODBUS("COM1");
            if (modbusObj.Open())
            {
                MessageBox.Show("chenggong");
                return;
            }
            MessageBox.Show("shibai");
        }

        /// <summary>
        /// 读取寄存器
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_ReadReg_Click(object sender, EventArgs e)
        {
            modbusObj.tokenSource = new CancellationTokenSource();
            lb_Mesage.DataSource = modbusObj?.ReadKeepReg(1, 0, 10);
        }

        /// <summary>
        /// 读取输出线圈
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_ReadOutCoil_Click(object sender, EventArgs e)
        {
            modbusObj.tokenSource = new CancellationTokenSource();
            lb_Mesage.DataSource = modbusObj?.ReadOutputStatus(1, 0, 10);
        }

        /// <summary>
        /// 读取输入线圈
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_ReadInCoil_Click(object sender, EventArgs e)
        {
            modbusObj.tokenSource = new CancellationTokenSource();
            lb_Mesage.DataSource = modbusObj?.ReadInputStatus(1, 0, 10);
        }

        /// <summary>
        /// 强制线圈
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_FroceCoil_Click(object sender, EventArgs e)
        {
            string msg = modbusObj.ForceCoil(1, 0, true) ? "强制成功" : "强制失败";
            MessageBox.Show(msg);
        }

        /// <summary>
        /// 写入单寄存器
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_WriteSingleReg_Click(object sender, EventArgs e)
        {
            string msg = modbusObj.PreSetKeepReg(1, 0, 32) ? "强制成功" : "强制失败";
            MessageBox.Show(msg);
        }

        /// <summary>
        /// 写入双寄存器
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_WriteDoubleReg_Click(object sender, EventArgs e)
        {
            string msg = modbusObj.PreSetFloatKeepReg(1, 0, 123.33f) ? "强支成功" : "强制失败";
            MessageBox.Show(msg);
        }
    }
}

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
            modbusObj?.ReadKeepReg(1, 0, 10);
            Task.Run(() =>
            {
                while (true)
                {
                    if (modbusObj.IsFinish)
                    {
                        lb_Mesage.Items.Clear();

                    }
                }
            });
        }
    }
}

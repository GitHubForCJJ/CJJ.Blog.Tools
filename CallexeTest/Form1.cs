using CJJ.Blog.Tools.CallexeTest;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CallexeTest
{
    public partial class Form1 : Form
    {
        public Form1(string[] args)
        {
            InitializeComponent();
            string apppath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;//获取程序目录    
            bool result = RegeditHelper.RegeditAdd("chenjianjun", apppath, "");//这里就是创建协议

            var par = RegeditHelper.getValue();
            this.richTextBox1.Text = par;
        }

        private void btn1_Click(object sender, EventArgs e)
        {
            //string apppath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;//获取程序目录    
            //bool result = RegeditHelper.RegeditAdd("carsonyang", apppath, "");//这里就是创建协议
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace nurturing
{
    public partial class Form_Pick : Form
    {
        public Form_Pick()
        {
            InitializeComponent();
            this.AutoScaleMode = AutoScaleMode.None;

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            radioButton_Red.Checked = true;
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            radioButton_Wing.Checked = true;
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            radioButton_Rock.Checked = true;
        }
    }
}

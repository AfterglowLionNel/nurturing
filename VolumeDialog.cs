using System;
using System.Drawing;
using System.Windows.Forms;

namespace nurturing
{
    public class VolumeDialog : Form
    {
        private TrackBar trackBar;
        private Label lblValue;
        private Button btnOk;
        private Button btnCancel;

        public event Action<float> VolumeChanged; // 0.0～1.0

        public float SelectedVolume => trackBar.Value / 100f;

        public VolumeDialog(int initialPercent = 30)
        {
            Text = "音量調整";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(320, 140);

            trackBar = new TrackBar
            {
                Minimum = 0,
                Maximum = 100,
                TickFrequency = 10,
                SmallChange = 5,
                LargeChange = 10,
                Value = Math.Max(0, Math.Min(100, initialPercent)),
                Location = new Point(20, 20),
                Size = new Size(280, 45)
            };
            trackBar.Scroll += TrackBar_Scroll;
            trackBar.ValueChanged += TrackBar_Scroll;

            lblValue = new Label
            {
                AutoSize = true,
                Location = new Point(20, 65),
                Text = $"{trackBar.Value}%"
            };

            btnOk = new Button
            {
                Text = "OK",
                DialogResult = DialogResult.OK,
                Location = new Point(140, 100),
                Size = new Size(75, 25)
            };

            btnCancel = new Button
            {
                Text = "キャンセル",
                DialogResult = DialogResult.Cancel,
                Location = new Point(225, 100),
                Size = new Size(75, 25)
            };

            Controls.Add(trackBar);
            Controls.Add(lblValue);
            Controls.Add(btnOk);
            Controls.Add(btnCancel);

            AcceptButton = btnOk;
            CancelButton = btnCancel;
        }

        private void TrackBar_Scroll(object sender, EventArgs e)
        {
            lblValue.Text = $"{trackBar.Value}%";
            VolumeChanged?.Invoke(SelectedVolume);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

// 型名衝突回避
using WinButton = System.Windows.Forms.Button;
using WinProgressBar = System.Windows.Forms.ProgressBar;

namespace nurturing
{
    public partial class Form_Pick : Form
    {
        //==================== キャラクター情報 ====================
        private class CharacterInfo
        {
            public string Name { get; set; }
            public Image Image { get; set; }
            public string Description { get; set; }
            public int Health { get; set; }
            public int Attack { get; set; }
            public int Defense { get; set; }
            public Color ThemeColor { get; set; }
        }

        //==================== フィールド ====================
        private List<CharacterInfo> characters;
        private int currentIndex = 0;
        private Timer animationTimer;
        private int animationStep = 0;
        private bool isAnimating = false;
        private string[] customNames;

        // ステータスUI
        private Panel statsPanel;
        private Label lblDescription;
        private WinProgressBar healthBar;
        private WinProgressBar attackBar;
        private WinProgressBar defenseBar;
        private Label lblHealth;
        private Label lblAttack;
        private Label lblDefense;

        // フォント
        private PrivateFontCollection privateFonts;
        private FontFamily customFamily;
        private Font titleFont;
        private Font labelFont;
        private Font statsFont;

        // ===== サウンド =====
        private WaveOutEvent outputDevice;
        private MemoryStream wavMemStream;
        private WaveFileReader fileReader;
        private VolumeSampleProvider volumeProvider;
        private bool bgmInitialized = false;   // 二重初期化防止

        // メニュー
        private MenuStrip menuStrip;
        private ToolStripMenuItem toolsMenuItem;
        private ToolStripMenuItem volumeMenuItem;

        //==================== コンストラクタ ====================
        public Form_Pick()
        {
            InitializeComponent();
            this.AutoScaleMode = AutoScaleMode.None;

            LoadCustomFonts();
            EnsureFonts();

            SetupMenu();
            SetupFormDesign();
            InitializeCharacters();
            SetupUI();
            UpdateDisplay();

            if (customFamily != null)
            {
                ApplyFontToAllControls(this, customFamily, menuStrip); // ToolStrip除外
                label_Title.Font = new Font(customFamily, 28, FontStyle.Bold);
                label_Chara1.Font = new Font(customFamily, 18, FontStyle.Bold);
                label_Chara2.Font = new Font(customFamily, 18, FontStyle.Bold);
                label_Chara3.Font = new Font(customFamily, 18, FontStyle.Bold);
                lblHealth.Font = statsFont;
                lblAttack.Font = statsFont;
                lblDefense.Font = statsFont;
                lblDescription.Font = statsFont;
            }

            animationTimer = new Timer { Interval = 10 };
            animationTimer.Tick += AnimationTimer_Tick;

            button_submitPick.Click += Button_submitPick_Click;
            button_changeName.Click += Button_changeName_Click;

            this.Load += Form_Pick_Load;
        }

        //==================== メニュー ====================
        private void SetupMenu()
        {
            menuStrip = new MenuStrip();
            toolsMenuItem = new ToolStripMenuItem("ツール(&T)");
            volumeMenuItem = new ToolStripMenuItem("音量調整(&V)");
            volumeMenuItem.Click += VolumeMenuItem_Click;

            toolsMenuItem.DropDownItems.Add(volumeMenuItem);
            menuStrip.Items.Add(toolsMenuItem);
            menuStrip.Font = SystemFonts.MenuFont; // 太字防止

            this.MainMenuStrip = menuStrip;
            this.Controls.Add(menuStrip);
            menuStrip.BringToFront();
        }

        private void VolumeMenuItem_Click(object sender, EventArgs e)
        {
            int currentPercent = volumeProvider != null ? (int)(volumeProvider.Volume * 100f) : 30;
            using (var dlg = new VolumeDialog(currentPercent))
            {
                // リアルタイム反映
                dlg.VolumeChanged += v => SetVolume(v);

                if (dlg.ShowDialog(this) == DialogResult.OK && volumeProvider != null)
                {
                    SetVolume(dlg.SelectedVolume); // 最終値を反映
                }
            }
        }

        private void SetVolume(float v)
        {
            if (volumeProvider == null || outputDevice == null) return;

            v = Math.Max(0f, Math.Min(1f, v));
            volumeProvider.Volume = v;

            if (v <= 0.0001f)
            {
                if (outputDevice.PlaybackState == PlaybackState.Playing)
                    outputDevice.Pause();   // 完全ミュート
            }
            else
            {
                if (outputDevice.PlaybackState != PlaybackState.Playing)
                    outputDevice.Play();
            }
        }

        //==================== フォント読み込み ====================
        [DllImport("gdi32.dll")]
        private static extern IntPtr AddFontMemResourceEx(IntPtr pbFont, uint cbFont, IntPtr pdv, [In] ref uint pcFonts);

        private void LoadCustomFonts()
        {
            privateFonts = new PrivateFontCollection();
            try
            {
                byte[] fontData = Properties.Resources.pikminneue; // リソース名確認
                IntPtr fontPtr = Marshal.AllocCoTaskMem(fontData.Length);
                Marshal.Copy(fontData, 0, fontPtr, fontData.Length);

                uint dummy = 0;
                privateFonts.AddMemoryFont(fontPtr, fontData.Length);
                AddFontMemResourceEx(fontPtr, (uint)fontData.Length, IntPtr.Zero, ref dummy);

                Marshal.FreeCoTaskMem(fontPtr);

                if (privateFonts.Families.Length > 0)
                {
                    customFamily = privateFonts.Families[0];
                    titleFont = new Font(customFamily, 24, FontStyle.Bold);
                    labelFont = new Font(customFamily, 14, FontStyle.Bold);
                    statsFont = new Font(customFamily, 11, FontStyle.Regular);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("フォント読み込み失敗: " + ex.Message);
            }
        }

        private void EnsureFonts()
        {
            if (titleFont == null) titleFont = new Font("MS UI Gothic", 24, FontStyle.Bold);
            if (labelFont == null) labelFont = new Font("MS UI Gothic", 14, FontStyle.Bold);
            if (statsFont == null) statsFont = new Font("MS UI Gothic", 11, FontStyle.Regular);
        }

        private void ApplyFontToAllControls(Control root, FontFamily fam, params Control[] excludeControls)
        {
            foreach (Control c in root.Controls)
            {
                bool excluded = false;
                foreach (var ex in excludeControls)
                {
                    if (ex != null && (c == ex || ex.Contains(c)))
                    {
                        excluded = true;
                        break;
                    }
                }
                if (!excluded)
                {
                    c.Font = new Font(fam, c.Font.Size, c.Font.Style);
                    if (c.HasChildren) ApplyFontToAllControls(c, fam, excludeControls);
                }
            }
        }

        //==================== フォームロード ====================
        private void Form_Pick_Load(object sender, EventArgs e)
        {
            if (bgmInitialized) return;
            bgmInitialized = true;

            StopAndDisposeBgm();

            using (var rs = Properties.Resources.Lifelog__Main_Menu____Pikmin_Bloom_OST)
            {
                wavMemStream = new MemoryStream();
                rs.CopyTo(wavMemStream);
            }
            wavMemStream.Position = 0;

            fileReader = new WaveFileReader(wavMemStream);
            var loop = new LoopStream(fileReader);
            volumeProvider = new VolumeSampleProvider(loop.ToSampleProvider())
            {
                Volume = 0.3f // デフォルト30%
            };

            outputDevice = new WaveOutEvent();
            outputDevice.Init(volumeProvider);
            outputDevice.Play();
        }

        private void StopAndDisposeBgm()
        {
            try { outputDevice?.Stop(); } catch { }

            outputDevice?.Dispose();
            outputDevice = null;

            volumeProvider = null;

            fileReader?.Dispose();
            fileReader = null;

            wavMemStream?.Dispose();
            wavMemStream = null;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            StopAndDisposeBgm();

            titleFont?.Dispose();
            labelFont?.Dispose();
            statsFont?.Dispose();
            privateFonts?.Dispose();

            base.OnFormClosed(e);
        }

        //==================== デザイン・UI ====================
        private void SetupFormDesign()
        {
            this.Paint += (s, e) =>
            {
                using (var brush = new LinearGradientBrush(
                    this.ClientRectangle,
                    Color.FromArgb(240, 248, 255),
                    Color.FromArgb(176, 224, 230),
                    LinearGradientMode.Vertical))
                {
                    e.Graphics.FillRectangle(brush, this.ClientRectangle);
                }
            };

            label_Title.Font = titleFont;
            label_Title.ForeColor = Color.FromArgb(25, 25, 112);
            label_Title.BackColor = Color.Transparent;
        }

        private void SetupUI()
        {
            if (label2 != null) this.Controls.Remove(label2);

            statsPanel = new Panel
            {
                Location = new Point(411, 90),
                Size = new Size(348, 165),
                BackColor = Color.FromArgb(220, 255, 255, 255)
            };
            statsPanel.Paint += StatsPanel_Paint;

            lblDescription = new Label
            {
                Location = new Point(15, 10),
                Size = new Size(318, 45),
                Font = statsFont,
                ForeColor = Color.FromArgb(64, 64, 64),
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.TopLeft
            };

            int barY = 60;
            int barHeight = 20;
            int spacing = 30;

            lblHealth = new Label
            {
                Text = "体力",
                Location = new Point(15, barY),
                Size = new Size(50, 20),
                Font = statsFont,
                BackColor = Color.Transparent
            };

            healthBar = new WinProgressBar
            {
                Location = new Point(70, barY),
                Size = new Size(200, barHeight),
                Style = ProgressBarStyle.Continuous
            };

            lblAttack = new Label
            {
                Text = "攻撃力",
                Location = new Point(15, barY + spacing),
                Size = new Size(50, 20),
                Font = statsFont,
                BackColor = Color.Transparent
            };

            attackBar = new WinProgressBar
            {
                Location = new Point(70, barY + spacing),
                Size = new Size(200, barHeight),
                Style = ProgressBarStyle.Continuous
            };

            lblDefense = new Label
            {
                Text = "防御力",
                Location = new Point(15, barY + spacing * 2),
                Size = new Size(50, 20),
                Font = statsFont,
                BackColor = Color.Transparent
            };

            defenseBar = new WinProgressBar
            {
                Location = new Point(70, barY + spacing * 2),
                Size = new Size(200, barHeight),
                Style = ProgressBarStyle.Continuous
            };

            statsPanel.Controls.AddRange(new Control[]
            {
                lblDescription, lblHealth, healthBar,
                lblAttack, attackBar, lblDefense, defenseBar
            });

            this.Controls.Add(statsPanel);

            StyleButton(button_submitPick, Color.FromArgb(50, 205, 50));
            StyleButton(button_changeName, Color.FromArgb(30, 144, 255));
        }

        private void StyleButton(WinButton button, Color color)
        {
            if (button == null) return;

            var family = (labelFont?.FontFamily) ?? SystemFonts.DefaultFont.FontFamily;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.BackColor = color;
            button.ForeColor = Color.White;
            button.Font = new Font(family, 12, FontStyle.Bold);
            button.Cursor = Cursors.Hand;

            button.MouseEnter += (s, e) => button.BackColor = ControlPaint.Light(color, 0.2f);
            button.MouseLeave += (s, e) => button.BackColor = color;
        }

        private void StatsPanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle rect = new Rectangle(0, 0, statsPanel.Width - 1, statsPanel.Height - 1);
            int radius = 15;

            using (GraphicsPath path = GetRoundedRectangle(rect, radius))
            {
                using (PathGradientBrush shadowBrush = new PathGradientBrush(path))
                {
                    shadowBrush.CenterColor = Color.FromArgb(30, 0, 0, 0);
                    shadowBrush.SurroundColors = new Color[] { Color.Transparent };

                    Rectangle shadowRect = new Rectangle(2, 2, rect.Width, rect.Height);
                    using (GraphicsPath shadowPath = GetRoundedRectangle(shadowRect, radius))
                    {
                        g.FillPath(shadowBrush, shadowPath);
                    }
                }

                using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(250, 255, 255, 255)))
                {
                    g.FillPath(bgBrush, path);
                }

                using (Pen pen = new Pen(Color.FromArgb(200, 200, 200), 1))
                {
                    g.DrawPath(pen, path);
                }
            }
        }

        //==================== 汎用 ====================
        private GraphicsPath GetRoundedRectangle(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
            path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
            path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
            path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
            path.CloseFigure();
            return path;
        }

        private Image MakeTransparentImage(Image source, float opacity)
        {
            Bitmap bmp = new Bitmap(source.Width, source.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                ColorMatrix colorMatrix = new ColorMatrix();
                colorMatrix.Matrix33 = opacity;
                ImageAttributes attributes = new ImageAttributes();
                attributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                g.DrawImage(source, new Rectangle(0, 0, bmp.Width, bmp.Height),
                    0, 0, source.Width, source.Height, GraphicsUnit.Pixel, attributes);
            }
            return bmp;
        }

        //==================== キャラ初期化 ====================
        private void InitializeCharacters()
        {
            characters = new List<CharacterInfo>
            {
                new CharacterInfo
                {
                    Name = "赤ピクミン",
                    Image = Properties.Resources.RedPikumin,
                    Description = "火に強く、攻撃力が高い勇敢な戦士。\n炎の中でも平気で活動できる。",
                    Health = 100,
                    Attack = 80,
                    Defense = 60,
                    ThemeColor = Color.FromArgb(220, 20, 60)
                },
                new CharacterInfo
                {
                    Name = "羽ピクミン",
                    Image = Properties.Resources.WingPikumin,
                    Description = "空を飛べて素早い。\n障害物を越えて移動できる特殊能力を持つ。",
                    Health = 80,
                    Attack = 50,
                    Defense = 40,
                    ThemeColor = Color.FromArgb(255, 20, 147)
                },
                new CharacterInfo
                {
                    Name = "岩ピクミン",
                    Image = Properties.Resources.RockPikumin,
                    Description = "防御力が高く頑丈。\nガラスや氷を破壊できる特殊能力を持つ。",
                    Health = 120,
                    Attack = 70,
                    Defense = 100,
                    ThemeColor = Color.FromArgb(105, 105, 105)
                }
            };

            customNames = new string[characters.Count];

            foreach (var character in characters)
            {
                if (character.Image == null)
                {
                    Bitmap tempImage = new Bitmap(250, 349);
                    using (Graphics g = Graphics.FromImage(tempImage))
                    {
                        g.Clear(Color.LightGray);
                        g.DrawString(character.Name, new Font("MS UI Gothic", 20), Brushes.Black,
                            new RectangleF(0, 150, 250, 50),
                            new StringFormat { Alignment = StringAlignment.Center });
                    }
                    character.Image = tempImage;
                }
            }
        }

        //==================== 表示更新 ====================
        private void UpdateDisplay()
        {
            if (isAnimating) return;

            int centerIndex = currentIndex;
            int leftIndex = (currentIndex - 1 + characters.Count) % characters.Count;
            int rightIndex = (currentIndex + 1) % characters.Count;

            pictureBox1.Image = MakeTransparentImage(characters[leftIndex].Image, 0.3f);
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;

            pictureBox2.Image = characters[centerIndex].Image;
            pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox2.BorderStyle = BorderStyle.FixedSingle;

            pictureBox3.Image = MakeTransparentImage(characters[rightIndex].Image, 0.3f);
            pictureBox3.SizeMode = PictureBoxSizeMode.StretchImage;

            label_Chara1.Text = !string.IsNullOrEmpty(customNames[leftIndex]) ? customNames[leftIndex] : characters[leftIndex].Name;
            label_Chara1.ForeColor = Color.FromArgb(169, 169, 169);

            label_Chara2.Text = !string.IsNullOrEmpty(customNames[centerIndex]) ? customNames[centerIndex] : characters[centerIndex].Name;
            label_Chara2.ForeColor = characters[centerIndex].ThemeColor;
            label_Chara2.Font = new Font((labelFont?.FontFamily) ?? SystemFonts.DefaultFont.FontFamily, 18, FontStyle.Bold);

            label_Chara3.Text = !string.IsNullOrEmpty(customNames[rightIndex]) ? customNames[rightIndex] : characters[rightIndex].Name;
            label_Chara3.ForeColor = Color.FromArgb(169, 169, 169);

            var selected = characters[centerIndex];
            lblDescription.Text = selected.Description;

            healthBar.Maximum = 150;
            healthBar.Value = selected.Health;
            SetProgressBarColor(healthBar, Color.FromArgb(255, 99, 71));

            attackBar.Maximum = 100;
            attackBar.Value = selected.Attack;
            SetProgressBarColor(attackBar, Color.FromArgb(255, 140, 0));

            defenseBar.Maximum = 100;
            defenseBar.Value = selected.Defense;
            SetProgressBarColor(defenseBar, Color.FromArgb(70, 130, 180));

            lblHealth.Text = $"体力 {selected.Health}";
            lblAttack.Text = $"攻撃力 {selected.Attack}";
            lblDefense.Text = $"防御力 {selected.Defense}";
        }

        private void SetProgressBarColor(WinProgressBar bar, Color color)
        {
            bar.Style = ProgressBarStyle.Continuous;
            ModifyProgressBarColor.SetState(bar, 1);
        }

        public static class ModifyProgressBarColor
        {
            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
            static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr w, IntPtr l);
            public static void SetState(WinProgressBar pBar, int state)
            {
                SendMessage(pBar.Handle, 1040, (IntPtr)state, IntPtr.Zero);
            }
        }

        //==================== 操作系 ====================
        private void pictureBox1_Click(object sender, EventArgs e) => MoveCarousel(-1);
        private void pictureBox2_Click(object sender, EventArgs e) { }
        private void pictureBox3_Click(object sender, EventArgs e) => MoveCarousel(1);

        private void MoveCarousel(int direction)
        {
            if (isAnimating) return;
            currentIndex = (currentIndex + direction + characters.Count) % characters.Count;
            StartAnimation();
        }

        private void StartAnimation()
        {
            isAnimating = true;
            animationStep = 0;
            animationTimer.Start();
        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            animationStep++;
            if (animationStep >= 10)
            {
                animationTimer.Stop();
                isAnimating = false;
                UpdateDisplay();
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Left)
            {
                MoveCarousel(-1);
                return true;
            }
            else if (keyData == Keys.Right)
            {
                MoveCarousel(1);
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (e.Delta > 0) MoveCarousel(-1);
            else if (e.Delta < 0) MoveCarousel(1);
        }

        private void Button_submitPick_Click(object sender, EventArgs e)
        {
            string finalName = !string.IsNullOrEmpty(customNames[currentIndex])
                ? customNames[currentIndex]
                : characters[currentIndex].Name;

            var selected = characters[currentIndex];
            string message =
                "以下の内容で確定しますか？\n\n" +
                $"名前：{finalName}\n" +
                $"種類：{selected.Name}\n" +
                $"体力：{selected.Health}\n" +
                $"攻撃力：{selected.Attack}\n" +
                $"防御力：{selected.Defense}";

            DialogResult result = MessageBox.Show(message, "確認",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                FormNurture nurtureForm = new FormNurture(
                    finalName,
                    selected.Name,
                    selected.Health,
                    selected.Attack,
                    selected.Defense,
                    selected.Image
                );

                this.Hide();
                nurtureForm.ShowDialog();
                this.Show();
            }
        }

        private void Button_changeName_Click(object sender, EventArgs e)
        {
            FormNameChange nameChangeForm = new FormNameChange();
            nameChangeForm.CurrentName = !string.IsNullOrEmpty(customNames[currentIndex])
                ? customNames[currentIndex]
                : characters[currentIndex].Name;

            if (nameChangeForm.ShowDialog(this) == DialogResult.OK)
            {
                customNames[currentIndex] = nameChangeForm.NewName;
                UpdateDisplay();
            }
        }

        private void DisposeCustomResources()
        {
            titleFont?.Dispose();
            labelFont?.Dispose();
            statsFont?.Dispose();
            privateFonts?.Dispose();
        }
    }

    /// <summary>
    /// WaveStream を無限ループさせる簡易ラッパー
    /// </summary>
    public class LoopStream : WaveStream
    {
        private readonly WaveStream sourceStream;

        public LoopStream(WaveStream source)
        {
            this.sourceStream = source;
            this.EnableLooping = true;
        }

        public bool EnableLooping { get; set; }

        public override WaveFormat WaveFormat => sourceStream.WaveFormat;

        public override long Length => sourceStream.Length;

        public override long Position
        {
            get => sourceStream.Position;
            set => sourceStream.Position = value;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int totalBytesRead = 0;

            while (totalBytesRead < count)
            {
                int bytesRead = sourceStream.Read(buffer, offset + totalBytesRead, count - totalBytesRead);
                if (bytesRead == 0)
                {
                    if (sourceStream.Position == 0 || !EnableLooping)
                    {
                        break;
                    }
                    sourceStream.Position = 0;
                }
                totalBytesRead += bytesRead;
            }
            return totalBytesRead;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                sourceStream.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

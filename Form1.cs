using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using nurturing;

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
            public string Name { get; set; }   // OriginalName と同じ
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
        private string[] customNames;               // CSV から読み取ったカスタム名

        // ステータス UI
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
        private bool bgmInitialized = false;

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
            InitializeCharacters();      // デフォルト名で構築
            LoadSavedNamesFromCsv();     // CSV があれば customNames[] にロード
            SetupUI();
            UpdateDisplay();

            if (customFamily != null)
            {
                ApplyFontToAllControls(this, customFamily, menuStrip); // ToolStrip は除外
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

        //==================== CSV からカスタム名をロード ====================
        private void LoadSavedNamesFromCsv()
        {
            try
            {
                string saveDir = Path.Combine(Application.StartupPath, "SaveData");
                if (!Directory.Exists(saveDir)) return;

                for (int i = 0; i < characters.Count; i++)
                {
                    string originalName = characters[i].Name;                // ファイル名 = OriginalName.csv
                    string path = Path.Combine(saveDir, originalName + ".csv");
                    if (!File.Exists(path)) continue;

                    using (StreamReader sr = new StreamReader(path, Encoding.UTF8))
                    {
                        sr.ReadLine();                      // ヘッダー
                        string line = sr.ReadLine();        // データ行
                        if (string.IsNullOrEmpty(line)) continue;

                        string[] cols = line.Split(',');
                        if (cols.Length > 0)
                        {
                            string savedName = cols[0].Trim();
                            if (!string.IsNullOrEmpty(savedName) && savedName != originalName)
                            {
                                customNames[i] = savedName;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("CSV 読み込みエラー: " + ex.Message);
            }
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
            menuStrip.Font = SystemFonts.MenuFont; // ToolStrip は太字にしない

            this.MainMenuStrip = menuStrip;
            this.Controls.Add(menuStrip);
            menuStrip.BringToFront();
        }

        private void VolumeMenuItem_Click(object sender, EventArgs e)
        {
            int cur = volumeProvider != null ? (int)(volumeProvider.Volume * 100f) : 30;
            var dlg = new VolumeDialog(cur);
            dlg.VolumeChanged += v => SetVolume(v); // スライダー移動で即反映
            try
            {
                if (dlg.ShowDialog(this) == DialogResult.OK && volumeProvider != null)
                    SetVolume(dlg.SelectedVolume);
            }
            finally { dlg.Dispose(); }
        }

        private void SetVolume(float v)
        {
            if (volumeProvider == null || outputDevice == null) return;

            v = Math.Max(0f, Math.Min(1f, v));
            volumeProvider.Volume = v;

            if (v <= 0.0001f)
            {
                if (outputDevice.PlaybackState == PlaybackState.Playing)
                    outputDevice.Pause();
            }
            else
            {
                if (outputDevice.PlaybackState != PlaybackState.Playing)
                    outputDevice.Play();
            }
        }

        //==================== フォント読み込み ====================
        [DllImport("gdi32.dll")]
        private static extern IntPtr AddFontMemResourceEx(IntPtr pbFont, uint cbFont,
                                                          IntPtr pdv, [In] ref uint pcFonts);

        private void LoadCustomFonts()
        {
            privateFonts = new PrivateFontCollection();
            try
            {
                byte[] fontData = Properties.Resources.pikminneue; // .otf をプロジェクトリソースに追加済み
                IntPtr ptr = Marshal.AllocCoTaskMem(fontData.Length);
                Marshal.Copy(fontData, 0, ptr, fontData.Length);

                uint dummy = 0;
                privateFonts.AddMemoryFont(ptr, fontData.Length);
                AddFontMemResourceEx(ptr, (uint)fontData.Length, IntPtr.Zero, ref dummy);

                Marshal.FreeCoTaskMem(ptr);

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
                Debug.WriteLine("フォント読み込み失敗: " + ex.Message);
            }
        }

        private void EnsureFonts()
        {
            if (titleFont == null) titleFont = new Font("MS UI Gothic", 24, FontStyle.Bold);
            if (labelFont == null) labelFont = new Font("MS UI Gothic", 14, FontStyle.Bold);
            if (statsFont == null) statsFont = new Font("MS UI Gothic", 11, FontStyle.Regular);
        }

        private void ApplyFontToAllControls(Control root, FontFamily fam, params Control[] excludes)
        {
            foreach (Control c in root.Controls)
            {
                bool skip = false;
                foreach (var ex in excludes)
                    if (ex != null && (c == ex || ex.Contains(c))) { skip = true; break; }

                if (!skip)
                {
                    c.Font = new Font(fam, c.Font.Size, c.Font.Style);
                    if (c.HasChildren) ApplyFontToAllControls(c, fam, excludes);
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
            volumeProvider = new VolumeSampleProvider(loop.ToSampleProvider()) { Volume = 0.3f };

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
                using (var br = new LinearGradientBrush(
                    this.ClientRectangle,
                    Color.FromArgb(240, 248, 255),
                    Color.FromArgb(176, 224, 230),
                    LinearGradientMode.Vertical))
                {
                    e.Graphics.FillRectangle(br, this.ClientRectangle);
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

            int y0 = 60, h = 20, sp = 30;

            lblHealth = new Label
            {
                Text = "体力",
                Location = new Point(15, y0),
                Size = new Size(50, 20),
                Font = statsFont,
                BackColor = Color.Transparent
            };
            healthBar = new WinProgressBar
            {
                Location = new Point(70, y0),
                Size = new Size(200, h),
                Style = ProgressBarStyle.Continuous
            };

            lblAttack = new Label
            {
                Text = "攻撃力",
                Location = new Point(15, y0 + sp),
                Size = new Size(50, 20),
                Font = statsFont,
                BackColor = Color.Transparent
            };
            attackBar = new WinProgressBar
            {
                Location = new Point(70, y0 + sp),
                Size = new Size(200, h),
                Style = ProgressBarStyle.Continuous
            };

            lblDefense = new Label
            {
                Text = "防御力",
                Location = new Point(15, y0 + sp * 2),
                Size = new Size(50, 20),
                Font = statsFont,
                BackColor = Color.Transparent
            };
            defenseBar = new WinProgressBar
            {
                Location = new Point(70, y0 + sp * 2),
                Size = new Size(200, h),
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

        private void StyleButton(WinButton btn, Color color)
        {
            if (btn == null) return;

            var fam = (labelFont?.FontFamily) ?? SystemFonts.DefaultFont.FontFamily;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = color;
            btn.ForeColor = Color.White;
            btn.Font = new Font(fam, 12, FontStyle.Bold);
            btn.Cursor = Cursors.Hand;

            btn.MouseEnter += (s, e) => btn.BackColor = ControlPaint.Light(color, 0.2f);
            btn.MouseLeave += (s, e) => btn.BackColor = color;
        }

        private void StatsPanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle r = new Rectangle(0, 0, statsPanel.Width - 1, statsPanel.Height - 1);
            int rad = 15;

            using (GraphicsPath p = GetRoundedRectangle(r, rad))
            {
                using (PathGradientBrush sh = new PathGradientBrush(p))
                {
                    sh.CenterColor = Color.FromArgb(30, 0, 0, 0);
                    sh.SurroundColors = new[] { Color.Transparent };

                    Rectangle sr = new Rectangle(2, 2, r.Width, r.Height);
                    using (GraphicsPath sp = GetRoundedRectangle(sr, rad))
                    {
                        g.FillPath(sh, sp);
                    }
                }

                using (SolidBrush bg = new SolidBrush(Color.FromArgb(250, 255, 255, 255)))
                    g.FillPath(bg, p);

                using (Pen pen = new Pen(Color.FromArgb(200, 200, 200), 1))
                    g.DrawPath(pen, p);
            }
        }

        //==================== 汎用 ====================
        private GraphicsPath GetRoundedRectangle(Rectangle rect, int rad)
        {
            GraphicsPath p = new GraphicsPath();
            p.AddArc(rect.X, rect.Y, rad, rad, 180, 90);
            p.AddArc(rect.Right - rad, rect.Y, rad, rad, 270, 90);
            p.AddArc(rect.Right - rad, rect.Bottom - rad, rad, rad, 0, 90);
            p.AddArc(rect.X, rect.Bottom - rad, rad, rad, 90, 90);
            p.CloseFigure();
            return p;
        }

        private Image MakeTransparentImage(Image src, float opacity)
        {
            Bitmap bmp = new Bitmap(src.Width, src.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                ColorMatrix m = new ColorMatrix { Matrix33 = opacity };
                ImageAttributes ia = new ImageAttributes();
                ia.SetColorMatrix(m, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                g.DrawImage(src, new Rectangle(0, 0, bmp.Width, bmp.Height),
                    0, 0, src.Width, src.Height, GraphicsUnit.Pixel, ia);
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
                    Name="赤ピクミン",
                    Image=Properties.Resources.RedPikumin,
                    Description="火に強く、攻撃力が高い勇敢な戦士",
                    Health=100, Attack=80, Defense=60,
                    ThemeColor=Color.FromArgb(220,20,60)
                },
                new CharacterInfo
                {
                    Name="羽ピクミン",
                    Image=Properties.Resources.WingPikumin,
                    Description="空を飛べて素早い",
                    Health=80, Attack=50, Defense=40,
                    ThemeColor=Color.FromArgb(255,20,147)
                },
                new CharacterInfo
                {
                    Name="岩ピクミン",
                    Image=Properties.Resources.RockPikumin,
                    Description="防御力が高く頑丈",
                    Health=120, Attack=70, Defense=100,
                    ThemeColor=Color.FromArgb(105,105,105)
                }
            };

            customNames = new string[characters.Count];

            // 画像が無い場合のプレースホルダー
            foreach (var c in characters)
            {
                if (c.Image != null) continue;
                Bitmap tmp = new Bitmap(250, 349);
                using (Graphics g = Graphics.FromImage(tmp))
                {
                    g.Clear(Color.LightGray);
                    g.DrawString(c.Name, new Font("MS UI Gothic", 20),
                        Brushes.Black,
                        new RectangleF(0, 150, 250, 50),
                        new StringFormat { Alignment = StringAlignment.Center });
                }
                c.Image = tmp;
            }
        }

        //==================== 表示更新 ====================
        private void UpdateDisplay()
        {
            if (isAnimating) return;

            int c = currentIndex;
            int l = (c - 1 + characters.Count) % characters.Count;
            int r = (c + 1) % characters.Count;

            pictureBox1.Image = MakeTransparentImage(characters[l].Image, 0.3f);
            pictureBox2.Image = characters[c].Image;
            pictureBox3.Image = MakeTransparentImage(characters[r].Image, 0.3f);

            pictureBox1.SizeMode = pictureBox2.SizeMode = pictureBox3.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox2.BorderStyle = BorderStyle.FixedSingle;

            label_Chara1.Text = string.IsNullOrEmpty(customNames[l]) ? characters[l].Name : customNames[l];
            label_Chara2.Text = string.IsNullOrEmpty(customNames[c]) ? characters[c].Name : customNames[c];
            label_Chara3.Text = string.IsNullOrEmpty(customNames[r]) ? characters[r].Name : customNames[r];

            label_Chara1.ForeColor = Color.FromArgb(169, 169, 169);
            label_Chara3.ForeColor = Color.FromArgb(169, 169, 169);
            label_Chara2.ForeColor = characters[c].ThemeColor;

            var sel = characters[c];
            lblDescription.Text = sel.Description;

            healthBar.Maximum = 150; healthBar.Value = sel.Health;
            attackBar.Maximum = 100; attackBar.Value = sel.Attack;
            defenseBar.Maximum = 100; defenseBar.Value = sel.Defense;

            lblHealth.Text = $"体力 {sel.Health}";
            lblAttack.Text = $"攻撃力 {sel.Attack}";
            lblDefense.Text = $"防御力 {sel.Defense}";
        }

        //==================== 操作系 ====================
        private void pictureBox1_Click(object sender, EventArgs e) { MoveCarousel(-1); }
        private void pictureBox2_Click(object sender, EventArgs e) { }
        private void pictureBox3_Click(object sender, EventArgs e) { MoveCarousel(1); }

        private void MoveCarousel(int dir)
        {
            if (isAnimating) return;
            currentIndex = (currentIndex + dir + characters.Count) % characters.Count;
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
            if (keyData == Keys.Left) { MoveCarousel(-1); return true; }
            if (keyData == Keys.Right) { MoveCarousel(1); return true; }
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
            string finalName = string.IsNullOrEmpty(customNames[currentIndex]) ?
                               characters[currentIndex].Name : customNames[currentIndex];

            var sel = characters[currentIndex];
            string msg =
                "以下の内容で確定しますか？\n\n" +
                $"名前：{finalName}\n" +
                $"種類：{sel.Name}\n" +
                $"体力：{sel.Health}\n" +
                $"攻撃力：{sel.Attack}\n" +
                $"防御力：{sel.Defense}";

            if (MessageBox.Show(msg, "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            StopAndDisposeBgm();

            var nurture = new FormNurture(
                finalName, sel.Name, sel.Health, sel.Attack, sel.Defense, sel.Image);
            try
            {
                this.Hide();
                nurture.ShowDialog();
            }
            finally
            {
                nurture.Dispose();
                this.Show();
            }

            bgmInitialized = false;
            Form_Pick_Load(this, EventArgs.Empty); // BGM 再開
        }

        private void Button_changeName_Click(object sender, EventArgs e)
        {
            var dlg = new FormNameChange
            {
                CurrentName = string.IsNullOrEmpty(customNames[currentIndex]) ?
                              characters[currentIndex].Name : customNames[currentIndex]
            };
            try
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    customNames[currentIndex] = dlg.NewName;
                    UpdateDisplay();
                }
            }
            finally { dlg.Dispose(); }
        }
    }
}

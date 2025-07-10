using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace nurturing
{
    public partial class Form_Pick : Form
    {
        // キャラクター情報を管理するクラス
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

        // キャラクターリスト
        private List<CharacterInfo> characters;
        private int currentIndex = 0;
        private Timer animationTimer;
        private int animationStep = 0;
        private bool isAnimating = false;
        private string[] customNames;  // 各キャラクターのカスタム名を保存

        // UI要素
        private Panel statsPanel;
        private System.Windows.Forms.Label lblDescription;
        private ProgressBar healthBar;
        private ProgressBar attackBar;
        private ProgressBar defenseBar;
        private System.Windows.Forms.Label lblHealth;
        private System.Windows.Forms.Label lblAttack;
        private System.Windows.Forms.Label lblDefense;

        // カスタムフォント
        private PrivateFontCollection privateFonts;
        private Font titleFont;
        private Font labelFont;
        private Font statsFont;

        public Form_Pick()
        {
            InitializeComponent();
            this.AutoScaleMode = AutoScaleMode.None;

            // カスタムフォントの読み込み
            LoadCustomFonts();

            // フォームのデザイン設定
            SetupFormDesign();

            InitializeCharacters();
            SetupUI();
            UpdateDisplay();

            // アニメーション用タイマー
            animationTimer = new Timer();
            animationTimer.Interval = 10;
            animationTimer.Tick += AnimationTimer_Tick;

            // イベントハンドラ
            button_submitPick.Click += Button_submitPick_Click;
            button_changeName.Click += Button_changeName_Click;
        }

        private void LoadCustomFonts()
        {
            privateFonts = new PrivateFontCollection();

            // デフォルトフォントの設定（カスタムフォントが読み込めない場合の代替）
            titleFont = new Font("メイリオ", 24, FontStyle.Bold);
            labelFont = new Font("メイリオ", 14, FontStyle.Bold);
            statsFont = new Font("メイリオ", 11, FontStyle.Regular);

            // カスタムフォントの読み込み
            try
            {
                // フォントファイルがプロジェクトのFontsフォルダにある場合
                string[] fontFiles = {
                    "nurturing.Fonts.YourFont.ttf",  // 実際のフォントファイル名に変更してください
                    // 複数のフォントファイルがある場合はここに追加
                };

                foreach (string fontFile in fontFiles)
                {
                    LoadFontFromResource(fontFile);
                }

                // フォントが正常に読み込まれた場合
                if (privateFonts.Families.Length > 0)
                {
                    // 最初に読み込まれたフォントを使用
                    var fontFamily = privateFonts.Families[0];
                    titleFont = new Font(fontFamily, 24, FontStyle.Bold);
                    labelFont = new Font(fontFamily, 14, FontStyle.Bold);
                    statsFont = new Font(fontFamily, 11, FontStyle.Regular);

                    Console.WriteLine($"カスタムフォント '{fontFamily.Name}' を読み込みました。");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"フォント読み込みエラー: {ex.Message}");
            }
        }

        private void LoadFontFromResource(string resourceName)
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        byte[] fontData = new byte[stream.Length];
                        stream.Read(fontData, 0, (int)stream.Length);

                        IntPtr fontPtr = Marshal.AllocCoTaskMem(fontData.Length);
                        Marshal.Copy(fontData, 0, fontPtr, fontData.Length);

                        uint dummy = 0;
                        privateFonts.AddMemoryFont(fontPtr, fontData.Length);
                        AddFontMemResourceEx(fontPtr, (uint)fontData.Length, IntPtr.Zero, ref dummy);

                        Marshal.FreeCoTaskMem(fontPtr);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"リソースフォント読み込みエラー: {ex.Message}");
            }
        }

        [DllImport("gdi32.dll")]
        private static extern IntPtr AddFontMemResourceEx(IntPtr pbFont, uint cbFont, IntPtr pdv, [In] ref uint pcFonts);

        private void SetupFormDesign()
        {
            // フォームの背景をグラデーションに
            this.Paint += (s, e) =>
            {
                using (LinearGradientBrush brush = new LinearGradientBrush(
                    this.ClientRectangle,
                    Color.FromArgb(240, 248, 255),  // Alice Blue
                    Color.FromArgb(176, 224, 230),  // Powder Blue
                    LinearGradientMode.Vertical))
                {
                    e.Graphics.FillRectangle(brush, this.ClientRectangle);
                }
            };

            // タイトルラベルのカスタマイズ
            label_Title.Font = titleFont;
            label_Title.ForeColor = Color.FromArgb(25, 25, 112);  // Midnight Blue
            label_Title.BackColor = Color.Transparent;
        }

        private void SetupUI()
        {
            // 既存のlabel2を削除して新しいUIを作成
            this.Controls.Remove(label2);

            // ステータスパネルの作成（位置を調整）
            statsPanel = new Panel
            {
                Location = new Point(411, 90),  // Y座標を下げて重ならないように
                Size = new Size(348, 165),      // 高さを少し調整
                BackColor = Color.FromArgb(220, 255, 255, 255)
            };
            statsPanel.Paint += StatsPanel_Paint;

            // 説明ラベル
            lblDescription = new System.Windows.Forms.Label
            {
                Location = new Point(15, 10),
                Size = new Size(318, 45),
                Font = statsFont,
                ForeColor = Color.FromArgb(64, 64, 64),
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.TopLeft
            };

            // ステータスバーの作成
            int barY = 60;
            int barHeight = 20;
            int spacing = 30;

            // 体力
            lblHealth = new System.Windows.Forms.Label
            {
                Text = "体力",
                Location = new Point(15, barY),
                Size = new Size(50, 20),
                Font = statsFont,
                BackColor = Color.Transparent
            };

            healthBar = new ProgressBar
            {
                Location = new Point(70, barY),
                Size = new Size(200, barHeight),
                Style = ProgressBarStyle.Continuous
            };

            // 攻撃力
            lblAttack = new System.Windows.Forms.Label
            {
                Text = "攻撃力",
                Location = new Point(15, barY + spacing),
                Size = new Size(50, 20),
                Font = statsFont,
                BackColor = Color.Transparent
            };

            attackBar = new ProgressBar
            {
                Location = new Point(70, barY + spacing),
                Size = new Size(200, barHeight),
                Style = ProgressBarStyle.Continuous
            };

            // 防御力
            lblDefense = new System.Windows.Forms.Label
            {
                Text = "防御力",
                Location = new Point(15, barY + spacing * 2),
                Size = new Size(50, 20),
                Font = statsFont,
                BackColor = Color.Transparent
            };

            defenseBar = new ProgressBar
            {
                Location = new Point(70, barY + spacing * 2),
                Size = new Size(200, barHeight),
                Style = ProgressBarStyle.Continuous
            };

            // ステータスパネルにコントロールを追加
            statsPanel.Controls.AddRange(new Control[] {
                lblDescription, lblHealth, healthBar,
                lblAttack, attackBar, lblDefense, defenseBar
            });

            this.Controls.Add(statsPanel);

            // ボタンのスタイル設定
            StyleButton(button_submitPick, Color.FromArgb(50, 205, 50));  // Lime Green
            StyleButton(button_changeName, Color.FromArgb(30, 144, 255));  // Dodger Blue

            // キャラクター名ラベルのスタイル
            label_Chara1.Font = labelFont;
            label_Chara1.BackColor = Color.Transparent;
            label_Chara2.Font = labelFont;
            label_Chara2.BackColor = Color.Transparent;
            label_Chara3.Font = labelFont;
            label_Chara3.BackColor = Color.Transparent;
        }

        private void StyleButton(Button button, Color color)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.BackColor = color;
            button.ForeColor = Color.White;
            button.Font = new Font(labelFont.FontFamily, 12, FontStyle.Bold);
            button.Cursor = Cursors.Hand;

            // ホバー効果
            button.MouseEnter += (s, e) =>
            {
                button.BackColor = ControlPaint.Light(color, 0.2f);
            };
            button.MouseLeave += (s, e) =>
            {
                button.BackColor = color;
            };
        }

        private void StatsPanel_Paint(object sender, PaintEventArgs e)
        {
            // パネルに角丸と影を追加
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle rect = new Rectangle(0, 0, statsPanel.Width - 1, statsPanel.Height - 1);
            int radius = 15;

            using (GraphicsPath path = GetRoundedRectangle(rect, radius))
            {
                // 影（より薄く）
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

                // 背景
                using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(250, 255, 255, 255)))
                {
                    g.FillPath(bgBrush, path);
                }

                // 枠線
                using (Pen pen = new Pen(Color.FromArgb(200, 200, 200), 1))
                {
                    g.DrawPath(pen, path);
                }
            }
        }

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
                    ThemeColor = Color.FromArgb(220, 20, 60)  // Crimson
                },
                new CharacterInfo
                {
                    Name = "羽ピクミン",
                    Image = Properties.Resources.WingPikumin,
                    Description = "空を飛べて素早い。\n障害物を越えて移動できる特殊能力を持つ。",
                    Health = 80,
                    Attack = 50,
                    Defense = 40,
                    ThemeColor = Color.FromArgb(255, 20, 147)  // Deep Pink（より濃いピンク）
                },
                new CharacterInfo
                {
                    Name = "岩ピクミン",
                    Image = Properties.Resources.RockPikumin,
                    Description = "防御力が高く頑丈。\nガラスや氷を破壊できる特殊能力を持つ。",
                    Health = 120,
                    Attack = 70,
                    Defense = 100,
                    ThemeColor = Color.FromArgb(105, 105, 105)  // Dim Gray（より濃いグレー）
                }
            };

            // カスタム名の配列を初期化
            customNames = new string[characters.Count];

            // 画像がない場合の仮画像を作成
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

        private void UpdateDisplay()
        {
            if (isAnimating) return;

            // 中央のキャラクターを大きく表示
            int centerIndex = currentIndex;
            int leftIndex = (currentIndex - 1 + characters.Count) % characters.Count;
            int rightIndex = (currentIndex + 1) % characters.Count;

            // 左側（薄く表示）
            pictureBox1.Image = MakeTransparentImage(characters[leftIndex].Image, 0.3f);
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;

            // 中央（選択中）
            pictureBox2.Image = characters[centerIndex].Image;
            pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox2.BorderStyle = BorderStyle.FixedSingle;

            // 右側（薄く表示）
            pictureBox3.Image = MakeTransparentImage(characters[rightIndex].Image, 0.3f);
            pictureBox3.SizeMode = PictureBoxSizeMode.StretchImage;

            // ラベルの更新（カスタム名があればそれを表示）
            label_Chara1.Text = !string.IsNullOrEmpty(customNames[leftIndex])
                ? customNames[leftIndex]
                : characters[leftIndex].Name;
            label_Chara1.ForeColor = Color.FromArgb(169, 169, 169);  // Dark Gray（薄すぎない色）

            label_Chara2.Text = !string.IsNullOrEmpty(customNames[centerIndex])
                ? customNames[centerIndex]
                : characters[centerIndex].Name;
            label_Chara2.ForeColor = characters[centerIndex].ThemeColor;
            label_Chara2.Font = new Font(labelFont.FontFamily, 16, FontStyle.Bold);

            label_Chara3.Text = !string.IsNullOrEmpty(customNames[rightIndex])
                ? customNames[rightIndex]
                : characters[rightIndex].Name;
            label_Chara3.ForeColor = Color.FromArgb(169, 169, 169);  // Dark Gray（薄すぎない色）

            // ステータス表示を更新
            var selected = characters[centerIndex];
            lblDescription.Text = selected.Description;

            // プログレスバーの更新
            healthBar.Maximum = 150;
            healthBar.Value = selected.Health;
            SetProgressBarColor(healthBar, Color.FromArgb(255, 99, 71));  // Tomato

            attackBar.Maximum = 100;
            attackBar.Value = selected.Attack;
            SetProgressBarColor(attackBar, Color.FromArgb(255, 140, 0));  // Dark Orange

            defenseBar.Maximum = 100;
            defenseBar.Value = selected.Defense;
            SetProgressBarColor(defenseBar, Color.FromArgb(70, 130, 180));  // Steel Blue

            // 数値表示
            lblHealth.Text = $"体力 {selected.Health}";
            lblAttack.Text = $"攻撃力 {selected.Attack}";
            lblDefense.Text = $"防御力 {selected.Defense}";
        }

        private void SetProgressBarColor(ProgressBar bar, Color color)
        {
            // Windows APIを使用してプログレスバーの色を変更
            bar.Style = ProgressBarStyle.Continuous;
            ModifyProgressBarColor.SetState(bar, 1);  // 通常の状態
        }

        // プログレスバーの色を変更するヘルパークラス
        public static class ModifyProgressBarColor
        {
            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
            static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr w, IntPtr l);
            public static void SetState(ProgressBar pBar, int state)
            {
                SendMessage(pBar.Handle, 1040, (IntPtr)state, IntPtr.Zero);
            }
        }

        // 画像を半透明にする
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

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            MoveCarousel(-1);
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            // 中央クリック時は何もしない
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            MoveCarousel(1);
        }

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
            if (e.Delta > 0)
                MoveCarousel(-1);
            else if (e.Delta < 0)
                MoveCarousel(1);
        }

        private void Button_submitPick_Click(object sender, EventArgs e)
        {
            string finalName = !string.IsNullOrEmpty(customNames[currentIndex])
                ? customNames[currentIndex]
                : characters[currentIndex].Name;

            var selected = characters[currentIndex];
            string message = $"以下の内容で確定しますか？\n\n" +
                           $"名前：{finalName}\n" +
                           $"種類：{selected.Name}\n" +
                           $"体力：{selected.Health}\n" +
                           $"攻撃力：{selected.Attack}\n" +
                           $"防御力：{selected.Defense}";

            DialogResult result = MessageBox.Show(message, "確認",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                // 育成画面へ遷移
                FormNurture nurtureForm = new FormNurture(
                    finalName,
                    selected.Name,
                    selected.Health,
                    selected.Attack,
                    selected.Defense,
                    selected.Image
                );

                // モーダルで表示
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

        // Disposeメソッドの部分実装（Designer.csのDisposeメソッドを補完）
        private void DisposeCustomResources()
        {
            // カスタムフォントの解放
            titleFont?.Dispose();
            labelFont?.Dispose();
            statsFont?.Dispose();
            privateFonts?.Dispose();
        }
    }
}
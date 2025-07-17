using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace nurturing
{
    public partial class FormNurture : Form
    {
        // キャラクターデータクラス
        public class CharacterData
        {
            public string Name { get; set; }
            public string OriginalName { get; set; }  // 元のキャラクター種類名
            public int Level { get; set; }
            public int Experience { get; set; }
            public int MaxExperience { get; set; }
            public int Health { get; set; }
            public int Attack { get; set; }
            public int Defense { get; set; }
            public Image CharacterImage { get; set; }
            public int ExtractCount { get; set; }  // エキスの保有数
            public DateTime LastSaved { get; set; }

            public CharacterData()
            {
                Level = 1;
                Experience = 0;
                MaxExperience = 100;
                ExtractCount = 10;  // 初期エキス数
                LastSaved = DateTime.Now;
            }
        }

        // 草の位置を保持する構造体
        private struct GrassPosition
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int Height { get; set; }
            public int Direction { get; set; } // -1 or 1 for left/right lean
        }

        private CharacterData currentCharacter;
        private bool isDragging = false;
        private Point dragStartPoint;
        private PictureBox draggingExtract;
        private Timer animationTimer;
        private List<FloatingText> floatingTexts = new List<FloatingText>();
        private Random random = new Random();

        // 草の位置を保持するリスト
        private List<GrassPosition> grassPositions = new List<GrassPosition>();
        private Bitmap backgroundBuffer; // ダブルバッファリング用

        // 浮遊テキストクラス（経験値表示用）
        private class FloatingText
        {
            public string Text { get; set; }
            public Point Position { get; set; }
            public int Life { get; set; }
            public Color Color { get; set; }
        }

        public FormNurture(string characterName, string originalName, int health, int attack, int defense, Image characterImage)
        {
            InitializeComponent();

            // ダブルバッファリングを有効化
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.UserPaint |
                    ControlStyles.DoubleBuffer |
                    ControlStyles.ResizeRedraw, true);

            // キャラクターデータの初期化
            currentCharacter = new CharacterData
            {
                Name = characterName,
                OriginalName = originalName,
                Health = health,
                Attack = attack,
                Defense = defense,
                CharacterImage = characterImage
            };

            // 既存のセーブデータを読み込む
            LoadCharacterData();

            // UIの初期化
            SetupUI();
            UpdateUI();

            // 草の位置を初期生成
            GenerateGrassPositions();

            // アニメーションタイマーの設定
            animationTimer = new Timer();
            animationTimer.Interval = 50;
            animationTimer.Tick += AnimationTimer_Tick;
            animationTimer.Start();
        }

        private void GenerateGrassPositions()
        {
            grassPositions.Clear();

            // 草の本数を100本に増やして密度を上げる
            for (int i = 0; i < 100; i++)
            {
                grassPositions.Add(new GrassPosition
                {
                    X = random.Next(10, panel_gameArea.Width - 10),
                    Y = random.Next(panel_gameArea.Height / 2, panel_gameArea.Height - 10),
                    Height = random.Next(15, 30),
                    Direction = random.Next(2) == 0 ? -1 : 1
                });
            }

            // 背景バッファを再生成
            CreateBackgroundBuffer();
        }

        private void CreateBackgroundBuffer()
        {
            if (backgroundBuffer != null)
                backgroundBuffer.Dispose();

            backgroundBuffer = new Bitmap(panel_gameArea.Width, panel_gameArea.Height);
            using (Graphics g = Graphics.FromImage(backgroundBuffer))
            {
                // グラデーション背景
                using (LinearGradientBrush brush = new LinearGradientBrush(
                    new Rectangle(0, 0, panel_gameArea.Width, panel_gameArea.Height),
                    Color.FromArgb(144, 238, 144),
                    Color.FromArgb(34, 139, 34),
                    LinearGradientMode.Vertical))
                {
                    g.FillRectangle(brush, 0, 0, panel_gameArea.Width, panel_gameArea.Height);
                }

                // 草を描画
                DrawGrassTexture(g);
            }
        }

        private void SetupUI()
        {
            // フォームの設定
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            // パネルのダブルバッファリングを有効化
            typeof(Panel).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.SetProperty |
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic,
                null, panel_gameArea, new object[] { true });

            // 背景描画イベント設定
            panel_gameArea.Paint += Panel_gameArea_Paint;

            // キャラクター画像の設定
            pictureBox_character.Image = currentCharacter.CharacterImage;

            // エキス（餌）の描画設定
            DrawExtract();

            // ドラッグ&ドロップのイベント設定
            pictureBox_extract.MouseDown += PictureBox_extract_MouseDown;
            pictureBox_extract.MouseMove += PictureBox_extract_MouseMove;
            pictureBox_extract.MouseUp += PictureBox_extract_MouseUp;

            // キャラクターのドロップ判定を有効化
            pictureBox_character.AllowDrop = true;
            pictureBox_character.DragEnter += PictureBox_character_DragEnter;
            pictureBox_character.DragDrop += PictureBox_character_DragDrop;

            // ボタンのイベント設定
            button_save.Click += Button_save_Click;
            button_back.Click += Button_back_Click;

            // ボタンのスタイル設定
            StyleButton(button_save, Color.FromArgb(50, 205, 50));
            StyleButton(button_back, Color.FromArgb(30, 144, 255));

            // ProgressBarのスタイル設定（存在する場合）
            SetupProgressBars();
        }

        private void SetupProgressBars()
        {
            // ProgressBarの色設定
            if (progressBar_hp != null)
            {
                progressBar_hp.Style = ProgressBarStyle.Continuous;
                progressBar_hp.ForeColor = Color.Red;
            }

            if (progressBar_attack != null)
            {
                progressBar_attack.Style = ProgressBarStyle.Continuous;
                progressBar_attack.ForeColor = Color.Orange;
            }

            if (progressBar_defense != null)
            {
                progressBar_defense.Style = ProgressBarStyle.Continuous;
                progressBar_defense.ForeColor = Color.Blue;
            }
        }

        private void DrawExtract()
        {
            // エキス（餌）の画像を作成
            Bitmap extractImage = new Bitmap(50, 50);
            using (Graphics g = Graphics.FromImage(extractImage))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // 黄色い球体を描画
                using (GraphicsPath path = new GraphicsPath())
                {
                    path.AddEllipse(5, 5, 40, 40);

                    using (PathGradientBrush brush = new PathGradientBrush(path))
                    {
                        brush.CenterColor = Color.FromArgb(255, 255, 200);
                        brush.SurroundColors = new Color[] { Color.FromArgb(255, 215, 0) };
                        g.FillPath(brush, path);
                    }

                    // 輪郭
                    using (Pen pen = new Pen(Color.FromArgb(255, 165, 0), 2))
                    {
                        g.DrawPath(pen, path);
                    }
                }

                // ハイライト
                using (GraphicsPath highlight = new GraphicsPath())
                {
                    highlight.AddEllipse(12, 10, 15, 12);
                    using (LinearGradientBrush highlightBrush = new LinearGradientBrush(
                        new Rectangle(12, 10, 15, 12),
                        Color.FromArgb(180, 255, 255, 255),
                        Color.Transparent,
                        LinearGradientMode.Vertical))
                    {
                        g.FillPath(highlightBrush, highlight);
                    }
                }
            }

            pictureBox_extract.Image = extractImage;
        }

        private void Panel_gameArea_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.HighSpeed;

            // 背景バッファを描画
            if (backgroundBuffer != null)
            {
                g.DrawImage(backgroundBuffer, 0, 0);
            }

            // 浮遊テキストの描画（これは動的なので毎回描画）
            DrawFloatingTexts(g);
        }

        private void DrawGrassTexture(Graphics g)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // 草のグラデーションペンを作成
            foreach (var grass in grassPositions)
            {
                // 草の色を深さによって変える
                int colorIntensity = 80 + (grass.Y * 40 / panel_gameArea.Height);
                Color grassColor = Color.FromArgb(150, 0, colorIntensity, 0);

                using (Pen grassPen = new Pen(grassColor, 2))
                {
                    // 草の根元
                    int baseX = grass.X;
                    int baseY = grass.Y;

                    // 草の先端（風に揺れているような効果）
                    int tipX = baseX + (grass.Direction * grass.Height / 3);
                    int tipY = baseY - grass.Height;

                    // 曲線を描く
                    Point[] points = new Point[]
                    {
                        new Point(baseX, baseY),
                        new Point(baseX + grass.Direction * grass.Height / 6, baseY - grass.Height / 2),
                        new Point(tipX, tipY)
                    };

                    g.DrawCurve(grassPen, points, 0.5f);
                }
            }
        }

        private void DrawFloatingTexts(Graphics g)
        {
            using (Font font = new Font("Arial", 16, FontStyle.Bold))
            {
                foreach (var text in floatingTexts)
                {
                    using (Brush brush = new SolidBrush(Color.FromArgb(text.Life, text.Color)))
                    {
                        // 縁取り効果
                        using (Brush outlineBrush = new SolidBrush(Color.FromArgb(text.Life / 2, Color.Black)))
                        {
                            for (int dx = -1; dx <= 1; dx++)
                            {
                                for (int dy = -1; dy <= 1; dy++)
                                {
                                    if (dx != 0 || dy != 0)
                                    {
                                        g.DrawString(text.Text, font, outlineBrush,
                                            text.Position.X + dx, text.Position.Y + dy);
                                    }
                                }
                            }
                        }
                        g.DrawString(text.Text, font, brush, text.Position);
                    }
                }
            }
        }

        private void StyleButton(Button button, Color color)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.BackColor = color;
            button.ForeColor = Color.White;
            button.Cursor = Cursors.Hand;

            button.MouseEnter += (s, e) => button.BackColor = ControlPaint.Light(color, 0.2f);
            button.MouseLeave += (s, e) => button.BackColor = color;
        }

        private void UpdateUI()
        {
            // デバッグ：ProgressBarの存在確認
            System.Diagnostics.Debug.WriteLine("=== ProgressBar確認 ===");
            System.Diagnostics.Debug.WriteLine($"progressBar_hp: {progressBar_hp != null}");
            System.Diagnostics.Debug.WriteLine($"progressBar_attack: {progressBar_attack != null}");
            System.Diagnostics.Debug.WriteLine($"progressBar_defense: {progressBar_defense != null}");

            // null チェックを追加
            if (label_characterName != null)
                label_characterName.Text = currentCharacter.Name;

            if (label_level != null)
                label_level.Text = $"レベル: {currentCharacter.Level}";

            // label_expが存在する場合のみ更新
            var expLabels = this.Controls.Find("label_exp", true);
            if (expLabels.Length > 0 && expLabels[0] is Label expLabel)
            {
                expLabel.Text = $"経験値: {currentCharacter.Experience} / {currentCharacter.MaxExperience}";
            }

            if (label_hp != null)
                label_hp.Text = $"体力: {currentCharacter.Health}";

            if (label_attack != null)
                label_attack.Text = $"攻撃力: {currentCharacter.Attack}";

            if (label_defense != null)
                label_defense.Text = $"防御力: {currentCharacter.Defense}";

            if (label_extractCount != null)
                label_extractCount.Text = $"エキス: {currentCharacter.ExtractCount}";

            // ProgressBarの更新
            if (progressBar_hp != null)
            {
                progressBar_hp.Maximum = 150;
                progressBar_hp.Value = Math.Min(currentCharacter.Health, 150);
                System.Diagnostics.Debug.WriteLine($"HP Bar - Max: {progressBar_hp.Maximum}, Value: {progressBar_hp.Value}");
            }

            if (progressBar_attack != null)
            {
                progressBar_attack.Maximum = 100;
                progressBar_attack.Value = Math.Min(currentCharacter.Attack, 100);
                System.Diagnostics.Debug.WriteLine($"Attack Bar - Max: {progressBar_attack.Maximum}, Value: {progressBar_attack.Value}");
            }

            if (progressBar_defense != null)
            {
                progressBar_defense.Maximum = 100;
                progressBar_defense.Value = Math.Min(currentCharacter.Defense, 100);
                System.Diagnostics.Debug.WriteLine($"Defense Bar - Max: {progressBar_defense.Maximum}, Value: {progressBar_defense.Value}");
            }

            // 経験値バーの更新（progressBar_exp2を使用）
            if (progressBar_exp2 != null)
            {
                progressBar_exp2.Maximum = currentCharacter.MaxExperience;
                progressBar_exp2.Value = currentCharacter.Experience;
                System.Diagnostics.Debug.WriteLine($"Exp Bar - Max: {progressBar_exp2.Maximum}, Value: {progressBar_exp2.Value}");
            }

            // エキスが0の場合は非表示に
            if (pictureBox_extract != null)
                pictureBox_extract.Visible = currentCharacter.ExtractCount > 0;
        }

        // ドラッグ&ドロップ処理
        private void PictureBox_extract_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && currentCharacter.ExtractCount > 0)
            {
                isDragging = true;
                dragStartPoint = e.Location;

                // ドラッグ用の一時的なPictureBoxを作成
                draggingExtract = new PictureBox();
                draggingExtract.Image = pictureBox_extract.Image;
                draggingExtract.Size = pictureBox_extract.Size;
                draggingExtract.BackColor = Color.Transparent;

                Point screenPoint = pictureBox_extract.PointToScreen(Point.Empty);
                draggingExtract.Location = this.PointToClient(screenPoint);

                this.Controls.Add(draggingExtract);
                draggingExtract.BringToFront();

                // カーソルを変更
                Cursor = Cursors.Hand;
            }
        }

        private void PictureBox_extract_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging && draggingExtract != null)
            {
                Point currentScreenPoint = Control.MousePosition;
                draggingExtract.Location = this.PointToClient(currentScreenPoint);
            }
        }

        private void PictureBox_extract_MouseUp(object sender, MouseEventArgs e)
        {
            if (isDragging && draggingExtract != null)
            {
                // キャラクターとの衝突判定
                Rectangle charRect = new Rectangle(
                    panel_gameArea.Location.X + pictureBox_character.Location.X,
                    panel_gameArea.Location.Y + pictureBox_character.Location.Y,
                    pictureBox_character.Width,
                    pictureBox_character.Height);

                Rectangle extractRect = new Rectangle(
                    draggingExtract.Location,
                    draggingExtract.Size);

                if (charRect.IntersectsWith(extractRect))
                {
                    // 餌やり成功
                    FeedCharacter();
                }

                // ドラッグ中のエキスを削除
                this.Controls.Remove(draggingExtract);
                draggingExtract.Dispose();
                draggingExtract = null;

                isDragging = false;
                Cursor = Cursors.Default;
            }
        }

        private void PictureBox_character_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Bitmap))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void PictureBox_character_DragDrop(object sender, DragEventArgs e)
        {
            FeedCharacter();
        }

        private void FeedCharacter()
        {
            if (currentCharacter.ExtractCount <= 0) return;

            // エキスを消費
            currentCharacter.ExtractCount--;

            // 経験値を増加（10-20のランダム値）
            int expGain = random.Next(10, 21);
            currentCharacter.Experience += expGain;

            // 浮遊テキストを追加
            floatingTexts.Add(new FloatingText
            {
                Text = $"+{expGain} EXP",
                Position = new Point(
                    pictureBox_character.Location.X + pictureBox_character.Width / 2 - 30,
                    pictureBox_character.Location.Y - 20),
                Life = 255,
                Color = Color.Yellow
            });

            // レベルアップ判定
            CheckLevelUp();

            // UI更新
            UpdateUI();

            // 餌やりエフェクト
            PlayFeedEffect();
        }

        private void CheckLevelUp()
        {
            while (currentCharacter.Experience >= currentCharacter.MaxExperience)
            {
                // レベルアップ
                currentCharacter.Experience -= currentCharacter.MaxExperience;
                currentCharacter.Level++;
                currentCharacter.MaxExperience = currentCharacter.Level * 100;

                // ステータスアップ
                int hpUp = random.Next(5, 11);
                int atkUp = random.Next(2, 6);
                int defUp = random.Next(2, 6);

                currentCharacter.Health += hpUp;
                currentCharacter.Attack += atkUp;
                currentCharacter.Defense += defUp;

                // レベルアップテキスト
                floatingTexts.Add(new FloatingText
                {
                    Text = "LEVEL UP!",
                    Position = new Point(
                        pictureBox_character.Location.X + pictureBox_character.Width / 2 - 50,
                        pictureBox_character.Location.Y - 50),
                    Life = 255,
                    Color = Color.Gold
                });

                // エキスボーナス
                currentCharacter.ExtractCount += 5;
            }
        }

        private void PlayFeedEffect()
        {
            // 簡単なアニメーション効果
            Timer effectTimer = new Timer();
            effectTimer.Interval = 50;
            int count = 0;

            effectTimer.Tick += (s, e) =>
            {
                count++;
                if (count < 5)
                {
                    pictureBox_character.Size = new Size(
                        pictureBox_character.Width + 2,
                        pictureBox_character.Height + 2);
                    pictureBox_character.Location = new Point(
                        pictureBox_character.Location.X - 1,
                        pictureBox_character.Location.Y - 1);
                }
                else if (count < 10)
                {
                    pictureBox_character.Size = new Size(
                        pictureBox_character.Width - 2,
                        pictureBox_character.Height - 2);
                    pictureBox_character.Location = new Point(
                        pictureBox_character.Location.X + 1,
                        pictureBox_character.Location.Y + 1);
                }
                else
                {
                    effectTimer.Stop();
                    effectTimer.Dispose();
                }
            };

            effectTimer.Start();
        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            bool needsRedraw = false;

            // 浮遊テキストのアニメーション
            for (int i = floatingTexts.Count - 1; i >= 0; i--)
            {
                var text = floatingTexts[i];
                text.Position = new Point(text.Position.X, text.Position.Y - 2);
                text.Life -= 10;

                if (text.Life <= 0)
                {
                    floatingTexts.RemoveAt(i);
                }
                needsRedraw = true;
            }

            // 浮遊テキストがある場合のみ再描画
            if (needsRedraw)
            {
                panel_gameArea.Invalidate();
            }
        }

        // データ保存処理
        private void Button_save_Click(object sender, EventArgs e)
        {
            SaveCharacterData();
            MessageBox.Show("育成データを保存しました！", "保存完了",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void SaveCharacterData()
        {
            try
            {
                string saveDirectory = Path.Combine(Application.StartupPath, "SaveData");
                if (!Directory.Exists(saveDirectory))
                {
                    Directory.CreateDirectory(saveDirectory);
                }

                string filename = $"{currentCharacter.OriginalName}_{currentCharacter.Name}.csv";
                string filepath = Path.Combine(saveDirectory, filename);

                using (StreamWriter writer = new StreamWriter(filepath, false, Encoding.UTF8))
                {
                    // ヘッダー
                    writer.WriteLine("Name,OriginalName,Level,Experience,MaxExperience,Health,Attack,Defense,ExtractCount,LastSaved");

                    // データ
                    writer.WriteLine($"{currentCharacter.Name},{currentCharacter.OriginalName},{currentCharacter.Level}," +
                        $"{currentCharacter.Experience},{currentCharacter.MaxExperience},{currentCharacter.Health}," +
                        $"{currentCharacter.Attack},{currentCharacter.Defense},{currentCharacter.ExtractCount}," +
                        $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                }

                currentCharacter.LastSaved = DateTime.Now;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存中にエラーが発生しました: {ex.Message}", "エラー",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadCharacterData()
        {
            try
            {
                string saveDirectory = Path.Combine(Application.StartupPath, "SaveData");
                if (!Directory.Exists(saveDirectory)) return;

                string filename = $"{currentCharacter.OriginalName}_{currentCharacter.Name}.csv";
                string filepath = Path.Combine(saveDirectory, filename);

                if (File.Exists(filepath))
                {
                    using (StreamReader reader = new StreamReader(filepath, Encoding.UTF8))
                    {
                        // ヘッダーをスキップ
                        reader.ReadLine();

                        // データを読み込む
                        string line = reader.ReadLine();
                        if (!string.IsNullOrEmpty(line))
                        {
                            string[] data = line.Split(',');
                            if (data.Length >= 10)
                            {
                                currentCharacter.Level = int.Parse(data[2]);
                                currentCharacter.Experience = int.Parse(data[3]);
                                currentCharacter.MaxExperience = int.Parse(data[4]);
                                currentCharacter.Health = int.Parse(data[5]);
                                currentCharacter.Attack = int.Parse(data[6]);
                                currentCharacter.Defense = int.Parse(data[7]);
                                currentCharacter.ExtractCount = int.Parse(data[8]);
                                currentCharacter.LastSaved = DateTime.Parse(data[9]);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"データ読み込み中にエラーが発生しました: {ex.Message}", "エラー",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void Button_back_Click(object sender, EventArgs e)
        {
            // 確認ダイアログ
            DialogResult result = MessageBox.Show(
                "選択画面に戻りますか？\n未保存のデータは失われます。",
                "確認",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                this.Close();
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            // タイマーの停止
            if (animationTimer != null)
            {
                animationTimer.Stop();
                animationTimer.Dispose();
            }

            // 背景バッファの破棄
            if (backgroundBuffer != null)
            {
                backgroundBuffer.Dispose();
            }
        }

        // パネルのサイズ変更時に背景を再生成
        private void Panel_gameArea_Resize(object sender, EventArgs e)
        {
            GenerateGrassPositions();
        }
    }
}
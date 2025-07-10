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

        private CharacterData currentCharacter;
        private bool isDragging = false;
        private Point dragStartPoint;
        private PictureBox draggingExtract;
        private Timer animationTimer;
        private List<FloatingText> floatingTexts = new List<FloatingText>();
        private Random random = new Random();

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

            // アニメーションタイマーの設定
            animationTimer = new Timer();
            animationTimer.Interval = 50;
            animationTimer.Tick += AnimationTimer_Tick;
            animationTimer.Start();
        }

        private void SetupUI()
        {
            // フォームの設定
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            // 背景にグラデーションを設定
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
            // 草原風の背景を描画
            Graphics g = e.Graphics;

            // グラデーション背景
            using (LinearGradientBrush brush = new LinearGradientBrush(
                panel_gameArea.ClientRectangle,
                Color.FromArgb(144, 238, 144),
                Color.FromArgb(34, 139, 34),
                LinearGradientMode.Vertical))
            {
                g.FillRectangle(brush, panel_gameArea.ClientRectangle);
            }

            // 草のテクスチャを描画
            DrawGrassTexture(g);

            // 浮遊テキストの描画
            DrawFloatingTexts(g);
        }

        private void DrawGrassTexture(Graphics g)
        {
            Pen grassPen = new Pen(Color.FromArgb(100, 0, 100, 0), 2);

            for (int i = 0; i < 50; i++)
            {
                int x = random.Next(panel_gameArea.Width);
                int y = random.Next(panel_gameArea.Height);
                int height = random.Next(10, 20);

                g.DrawLine(grassPen, x, y, x - 2, y - height);
                g.DrawLine(grassPen, x, y, x + 2, y - height);
            }

            grassPen.Dispose();
        }

        private void DrawFloatingTexts(Graphics g)
        {
            Font font = new Font("Arial", 16, FontStyle.Bold);

            foreach (var text in floatingTexts)
            {
                using (Brush brush = new SolidBrush(Color.FromArgb(text.Life, text.Color)))
                {
                    g.DrawString(text.Text, font, brush, text.Position);
                }
            }

            font.Dispose();
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
            label_characterName.Text = currentCharacter.Name;
            label_level.Text = $"レベル: {currentCharacter.Level}";
            label_exp.Text = $"経験値: {currentCharacter.Experience} / {currentCharacter.MaxExperience}";
            label_hp.Text = $"体力: {currentCharacter.Health}";
            label_attack.Text = $"攻撃力: {currentCharacter.Attack}";
            label_defense.Text = $"防御力: {currentCharacter.Defense}";
            label_extractCount.Text = $"エキス: {currentCharacter.ExtractCount}";

            // 経験値バーの更新
            progressBar_exp.Maximum = currentCharacter.MaxExperience;
            progressBar_exp.Value = currentCharacter.Experience;

            // エキスが0の場合は非表示に
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
            }

            // パネルを再描画
            panel_gameArea.Invalidate();
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
        }
    }
}
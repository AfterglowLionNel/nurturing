using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace nurturing
{
    public partial class FormNurture : Form
    {
        // 育成メイン画面
        // 機能が多いのでコメントも多め
        //================ キャラクターデータクラス ================
        public class CharacterData
        {
            // 育成キャラの状態を保持するクラス
            public string Name { get; set; }
            public string OriginalName { get; set; }
            public int Level { get; set; }
            public int Experience { get; set; }
            public int MaxExperience { get; set; }
            public int Health { get; set; }
            public int Attack { get; set; }
            public int Defense { get; set; }
            public Image CharacterImage { get; set; }
            public int ExtractCount { get; set; }
            public DateTime LastSaved { get; set; }

            public CharacterData()
            {
                Level = 1;
                Experience = 0;
                MaxExperience = 100;
                ExtractCount = SettingsManager.ExtractCount; // 全体設定から取得
                LastSaved = DateTime.Now;
            }
        }

        //================ 構造体／内部クラス ================
        private struct GrassPosition
        {
            public int X;
            public int Y;
            public int Height;
            public int Direction;
        }

        private class FloatingText
        {
            public string Text;
            public Point Position;
            public int Life;
            public Color Color;
        }

        //================ フィールド ================
        private CharacterData currentCharacter;
        private bool isDragging = false;
        private Point dragStartPoint;
        private PictureBox draggingExtract;
        private Timer animationTimer;
        private readonly List<FloatingText> floatingTexts = new List<FloatingText>();
        private readonly Random random = new Random();
        private readonly List<GrassPosition> grassPositions = new List<GrassPosition>();
        private Bitmap backgroundBuffer;
        private Size originalCharacterSize;
        private Point originalCharacterLocation;



        // ===== サウンド =====
        private WaveOutEvent outputDevice;
        private MemoryStream wavMemStream;
        private WaveFileReader fileReader;
        private VolumeSampleProvider volumeProvider;
        private bool bgmInitialized = false;

        //================ コンストラクタ ================
        public FormNurture(string characterName, string originalName, int health, int attack, int defense, Image characterImage)
        {
            // 渡されたキャラで初期設定する
            InitializeComponent();

            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.DoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            currentCharacter = new CharacterData
            {
                Name = characterName,
                OriginalName = originalName,
                Health = health,
                Attack = attack,
                Defense = defense,
                CharacterImage = characterImage
            };

            LoadCharacterData();   // キャラ個別データ
            currentCharacter.ExtractCount = SettingsManager.ExtractCount; // 共通エキスを同期

            SetupUI();
            UpdateUI();

            GenerateGrassPositions();

            animationTimer = new Timer { Interval = 50 };
            animationTimer.Tick += AnimationTimer_Tick;
            animationTimer.Start();

            this.Load += FormNurture_Load;
        }

        //================ BGM =================
        private void FormNurture_Load(object sender, EventArgs e)
        {
            InitBgm();
        }

        private void InitBgm()
        {
            if (bgmInitialized) return;
            bgmInitialized = true;

            StopAndDisposeBgm();

            using (var rs = Properties.Resources.Pikmin_Park___Pikmin_Bloom_OST)
            {
                wavMemStream = new MemoryStream();
                rs.CopyTo(wavMemStream);
            }
            wavMemStream.Position = 0;

            fileReader = new WaveFileReader(wavMemStream);
            var loop = new LoopStream(fileReader);
            volumeProvider = new VolumeSampleProvider(loop.ToSampleProvider())
            {
                Volume = 0.3f
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

        //================ 終了処理 =================
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // ここで確実にセーブ
            SaveAll();

            animationTimer?.Stop();
            animationTimer?.Dispose();

            backgroundBuffer?.Dispose();

            StopAndDisposeBgm();

            base.OnFormClosing(e);
        }

        //================ 草生成/描画 =================
        private void GenerateGrassPositions()
        {
            // 背景に生やす草の位置をランダムで決める。
            grassPositions.Clear();

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

            CreateBackgroundBuffer();
        }

        private void CreateBackgroundBuffer()
        {
            // 草は毎回描くと重いため一度だけ描き溜める
            backgroundBuffer?.Dispose();

            backgroundBuffer = new Bitmap(panel_gameArea.Width, panel_gameArea.Height);
            using (Graphics g = Graphics.FromImage(backgroundBuffer))
            {
                using (LinearGradientBrush brush = new LinearGradientBrush(
                    new Rectangle(0, 0, panel_gameArea.Width, panel_gameArea.Height),
                    Color.FromArgb(144, 238, 144),
                    Color.FromArgb(34, 139, 34),
                    LinearGradientMode.Vertical))
                {
                    g.FillRectangle(brush, 0, 0, panel_gameArea.Width, panel_gameArea.Height);
                }

                DrawGrassTexture(g);
            }
        }

        private void DrawGrassTexture(Graphics g)
        {
            // 草を一本ずつカーブで描画する
            g.SmoothingMode = SmoothingMode.AntiAlias;

            foreach (var grass in grassPositions)
            {
                int colorIntensity = 80 + (grass.Y * 40 / panel_gameArea.Height);
                Color grassColor = Color.FromArgb(150, 0, colorIntensity, 0);

                using (Pen grassPen = new Pen(grassColor, 2))
                {
                    int baseX = grass.X;
                    int baseY = grass.Y;

                    int tipX = baseX + (grass.Direction * grass.Height / 3);
                    int tipY = baseY - grass.Height;

                    Point[] points =
                    {
                        new Point(baseX, baseY),
                        new Point(baseX + grass.Direction * grass.Height / 6, baseY - grass.Height / 2),
                        new Point(tipX, tipY)
                    };

                    g.DrawCurve(grassPen, points, 0.5f);
                }
            }
        }

        //================ 描画 =================
        private void Panel_gameArea_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.HighSpeed;

            if (backgroundBuffer != null)
                g.DrawImage(backgroundBuffer, 0, 0);

            DrawFloatingTexts(g);
        }

        private void DrawFloatingTexts(Graphics g)
        {
            // エキスを与えたときのポップアップ文字を描画する
            using (Font font = new Font("Arial", 16, FontStyle.Bold))
            {
                foreach (var text in floatingTexts)
                {
                    using (Brush brush = new SolidBrush(Color.FromArgb(text.Life, text.Color)))
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
                        g.DrawString(text.Text, font, brush, text.Position);
                    }
                }
            }
        }

        private void PlaySoundEffect(string fileName)
        {
            // 効果音再生の共通処理。
            try
            {
                string path = Path.Combine(Application.StartupPath, "Sounds", fileName);
                if (!File.Exists(path))
                {
                    Debug.WriteLine($"ファイルが見つかりません: {path}");
                    return;
                }

                var reader = new AudioFileReader(path);
                Debug.WriteLine($"[DEBUG] SoundVolume raw: '{SettingsManager.SoundVolume}'");

                var volumeProvider = new VolumeSampleProvider(reader.ToSampleProvider())
                {
                    Volume = SettingsManager.SoundVolume
                };

                var waveOut = new WaveOutEvent();
                waveOut.Init(volumeProvider);
                waveOut.Play();

                waveOut.PlaybackStopped += (s, e) =>
                {
                    waveOut.Dispose();
                    reader.Dispose();
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine("効果音再生エラー: " + ex.Message);
            }
        }




        //================ UI =================
        private void SetupUI()
        {
            // UIを配置してイベントを登録
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            typeof(Panel).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.SetProperty |
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic,
                null, panel_gameArea, new object[] { true });

            panel_gameArea.Paint += Panel_gameArea_Paint;
            panel_gameArea.Resize += Panel_gameArea_Resize;

            pictureBox_character.Image = currentCharacter.CharacterImage;
            pictureBox_character.BackColor = Color.Transparent;

            DrawExtract();

            pictureBox_extract.MouseDown += PictureBox_extract_MouseDown;
            pictureBox_extract.MouseMove += PictureBox_extract_MouseMove;
            pictureBox_extract.MouseUp += PictureBox_extract_MouseUp;

            pictureBox_character.AllowDrop = true;
            pictureBox_character.DragEnter += PictureBox_character_DragEnter;
            pictureBox_character.DragDrop += PictureBox_character_DragDrop;

            button_back.Click += Button_back_Click;
            StyleButton(button_back, Color.FromArgb(30, 144, 255));

            SetupProgressBars();

            originalCharacterSize = pictureBox_character.Size;
            originalCharacterLocation = pictureBox_character.Location;

        }

        private void SetupProgressBars()
        {
            // ステータス用バーの位置を調整。
            int margin = 6;
            int barWidth = 180;

            if (label_hp != null && progressBar_hp != null)
            {
                progressBar_hp.Visible = true;
                progressBar_hp.Left = label_hp.Left;
                progressBar_hp.Top = label_hp.Bottom + margin;
                progressBar_hp.Width = barWidth;
                progressBar_hp.Style = ProgressBarStyle.Continuous;
            }

            if (label_attack != null && progressBar_attack != null)
            {
                progressBar_attack.Visible = true;
                progressBar_attack.Left = label_attack.Left;
                progressBar_attack.Top = label_attack.Bottom + margin;
                progressBar_attack.Width = barWidth;
                progressBar_attack.Style = ProgressBarStyle.Continuous;
            }

            if (label_defense != null && progressBar_defense != null)
            {
                progressBar_defense.Visible = true;
                progressBar_defense.Left = label_defense.Left;
                progressBar_defense.Top = label_defense.Bottom + margin;
                progressBar_defense.Width = barWidth;
                progressBar_defense.Style = ProgressBarStyle.Continuous;
            }

            if (progressBar_exp2 != null && label_level != null)
            {
                progressBar_exp2.Left = label_level.Left;
                progressBar_exp2.Top = label_level.Bottom + margin;
                progressBar_exp2.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                progressBar_exp2.Style = ProgressBarStyle.Continuous;
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

        private void DrawExtract()
        {
            // エキスアイコンを自前で描く。
            Bitmap extractImage = new Bitmap(50, 50);
            using (Graphics g = Graphics.FromImage(extractImage))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;

                using (GraphicsPath path = new GraphicsPath())
                {
                    path.AddEllipse(5, 5, 40, 40);

                    using (PathGradientBrush brush = new PathGradientBrush(path))
                    {
                        brush.CenterColor = Color.FromArgb(255, 255, 200);
                        brush.SurroundColors = new[] { Color.FromArgb(255, 215, 0) };
                        g.FillPath(brush, path);
                    }

                    using (Pen pen = new Pen(Color.FromArgb(255, 165, 0), 2))
                    {
                        g.DrawPath(pen, path);
                    }
                }

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

        private void UpdateUI()
        {
            if (label_characterName != null)
                label_characterName.Text = currentCharacter.Name;

            if (label_level != null)
                label_level.Text = $"レベル: {currentCharacter.Level}";

            var expLabels = Controls.Find("label_exp", true);
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

            if (progressBar_hp != null)
            {
                progressBar_hp.Maximum = 150;
                progressBar_hp.Value = Math.Min(currentCharacter.Health, 150);
            }

            if (progressBar_attack != null)
            {
                progressBar_attack.Maximum = 100;
                progressBar_attack.Value = Math.Min(currentCharacter.Attack, 100);
            }

            if (progressBar_defense != null)
            {
                progressBar_defense.Maximum = 100;
                progressBar_defense.Value = Math.Min(currentCharacter.Defense, 100);
            }

            if (progressBar_exp2 != null)
            {
                progressBar_exp2.Maximum = currentCharacter.MaxExperience;
                progressBar_exp2.Value = Math.Min(currentCharacter.Experience, currentCharacter.MaxExperience);
            }

            if (pictureBox_extract != null)
                pictureBox_extract.Visible = currentCharacter.ExtractCount > 0;
        }

        //================ ドラッグ＆ドロップ =================
        private void PictureBox_extract_MouseDown(object sender, MouseEventArgs e)
        {
            // エキスのドラッグを開始。
            if (e.Button == MouseButtons.Left && currentCharacter.ExtractCount > 0)
            {
                isDragging = true;
                dragStartPoint = e.Location;

                draggingExtract = new PictureBox
                {
                    Image = pictureBox_extract.Image,
                    Size = pictureBox_extract.Size,
                    BackColor = Color.Transparent
                };

                Point screenPoint = pictureBox_extract.PointToScreen(Point.Empty);
                draggingExtract.Location = PointToClient(screenPoint);

                Controls.Add(draggingExtract);
                draggingExtract.BringToFront();

                Cursor = Cursors.Hand;
            }
        }

        private void PictureBox_extract_MouseMove(object sender, MouseEventArgs e)
        {
            // ドラッグ中はマウスに合わせてアイコンを移動。
            if (isDragging && draggingExtract != null)
            {
                Point currentScreenPoint = Control.MousePosition;
                draggingExtract.Location = PointToClient(currentScreenPoint);
            }
        }

        private void PictureBox_extract_MouseUp(object sender, MouseEventArgs e)
        {
            // ドロップ終了、キャラに重なったら餌やり成功。
            if (isDragging && draggingExtract != null)
            {
                Rectangle charRect = new Rectangle(
                    panel_gameArea.Location.X + pictureBox_character.Location.X,
                    panel_gameArea.Location.Y + pictureBox_character.Location.Y,
                    pictureBox_character.Width,
                    pictureBox_character.Height);

                Rectangle extractRect = new Rectangle(draggingExtract.Location, draggingExtract.Size);

                if (charRect.IntersectsWith(extractRect))
                {
                    FeedCharacter();
                }

                Controls.Remove(draggingExtract);
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
            // エキスを与えて経験値アップ。
            if (currentCharacter.ExtractCount <= 0) return;

            currentCharacter.ExtractCount--;
            SettingsManager.ExtractCount = currentCharacter.ExtractCount; // 共通設定に反映

            int expGain = random.Next(10, 21);
            currentCharacter.Experience += expGain;

            floatingTexts.Add(new FloatingText
            {
                Text = $"+{expGain} EXP",
                Position = new Point(
                    pictureBox_character.Location.X + pictureBox_character.Width / 2 - 30,
                    pictureBox_character.Location.Y - 20),
                Life = 255,
                Color = Color.Yellow
            });

            CheckLevelUp();
            UpdateUI();
            SaveAll();      // オートセーブ
            PlayFeedEffect();
            PlaySoundEffect("Ekisu2.mp3");
        }

        private void CheckLevelUp()
        {
            // 経験値がたまったらレベルアップ処理。
            bool leveled = false;
            while (currentCharacter.Experience >= currentCharacter.MaxExperience)
            {
                currentCharacter.Experience -= currentCharacter.MaxExperience;
                currentCharacter.Level++;
                currentCharacter.MaxExperience = currentCharacter.Level * 100;

                int hpUp = random.Next(5, 11);
                int atkUp = random.Next(2, 6);
                int defUp = random.Next(2, 6);

                currentCharacter.Health += hpUp;
                currentCharacter.Attack += atkUp;
                currentCharacter.Defense += defUp;

                currentCharacter.ExtractCount += 5;
                SettingsManager.ExtractCount = currentCharacter.ExtractCount;

                floatingTexts.Add(new FloatingText
                {
                    Text = "LEVEL UP!",
                    Position = new Point(
                        pictureBox_character.Location.X + pictureBox_character.Width / 2 - 50,
                        pictureBox_character.Location.Y - 50),
                    Life = 255,
                    Color = Color.Gold
                });

                leveled = true;
            }

            if (leveled)
            {
                SaveAll(); // レベルアップでも保存
            }
        }

        private void PlayFeedEffect()
        {
            // 餌を与えたときのぷるぷる演出
            Timer effectTimer = new Timer { Interval = 50 };
            int count = 0;

            effectTimer.Tick += (s, e) =>
            {
                count++;
                if (count < 5)
                {
                    pictureBox_character.Size = new Size(
                        originalCharacterSize.Width + 2,
                        originalCharacterSize.Height + 2);
                    pictureBox_character.Location = new Point(
                        originalCharacterLocation.X - 1,
                        originalCharacterLocation.Y - 1);
                }
                else if (count < 10)
                {
                    pictureBox_character.Size = new Size(
                        originalCharacterSize.Width - 2,
                        originalCharacterSize.Height - 2);
                    pictureBox_character.Location = new Point(
                        originalCharacterLocation.X + 1,
                        originalCharacterLocation.Y + 1);
                }
                else
                {
                    pictureBox_character.Size = originalCharacterSize;
                    pictureBox_character.Location = originalCharacterLocation;
                    effectTimer.Stop();
                    effectTimer.Dispose();
                }
            };

            effectTimer.Start();
        }


        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            // 浮き文字の寿命管理や再描画要求を行う。
            bool needsRedraw = false;

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

            if (needsRedraw)
            {
                panel_gameArea.Invalidate();
            }
        }

        //================ 保存/読み込み =================
        private void SaveAll()
        {
            // すべて保存し設定も更新
            SaveCharacterData();
            SettingsManager.ExtractCount = currentCharacter.ExtractCount;
        }

        private void SaveCharacterData()
        {
            // キャラのステータスをCSVで保存。
            try
            {
                string saveDirectory = Path.Combine(Application.StartupPath, "SaveData");
                if (!Directory.Exists(saveDirectory))
                {
                    Directory.CreateDirectory(saveDirectory);
                }

                // OriginalName固定で保存
                string filename = $"{currentCharacter.OriginalName}.csv";
                string filepath = Path.Combine(saveDirectory, filename);

                using (StreamWriter writer = new StreamWriter(filepath, false, Encoding.UTF8))
                {
                    writer.WriteLine("Name,OriginalName,Level,Experience,MaxExperience,Health,Attack,Defense,LastSaved");
                    writer.WriteLine($"{currentCharacter.Name},{currentCharacter.OriginalName},{currentCharacter.Level}," +
                        $"{currentCharacter.Experience},{currentCharacter.MaxExperience},{currentCharacter.Health}," +
                        $"{currentCharacter.Attack},{currentCharacter.Defense}," +
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
            // 保存済みCSVがあれば読み込む。
            try
            {
                string saveDirectory = Path.Combine(Application.StartupPath, "SaveData");
                if (!Directory.Exists(saveDirectory)) return;

                string filename = $"{currentCharacter.OriginalName}.csv";
                string filepath = Path.Combine(saveDirectory, filename);

                if (File.Exists(filepath))
                {
                    using (StreamReader reader = new StreamReader(filepath, Encoding.UTF8))
                    {
                        reader.ReadLine(); // ヘッダ

                        string line = reader.ReadLine();
                        if (!string.IsNullOrEmpty(line))
                        {
                            string[] data = line.Split(',');
                            if (data.Length >= 9)
                            {
                                currentCharacter.Name = data[0];
                                currentCharacter.OriginalName = data[1];
                                currentCharacter.Level = int.Parse(data[2]);
                                currentCharacter.Experience = int.Parse(data[3]);
                                currentCharacter.MaxExperience = int.Parse(data[4]);
                                currentCharacter.Health = int.Parse(data[5]);
                                currentCharacter.Attack = int.Parse(data[6]);
                                currentCharacter.Defense = int.Parse(data[7]);
                                currentCharacter.LastSaved = DateTime.Parse(data[8]);
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

        //================ 画面遷移 =================
        private void Button_back_Click(object sender, EventArgs e)
        {
            // 選択画面に戻るボタン
            SaveAll();
            Close();
        }

        //================ リサイズ =================
        private void Panel_gameArea_Resize(object sender, EventArgs e)
        {
            // リサイズ時に草の位置を作り直す
            GenerateGrassPositions();
        }

        private void pictureBox_extract_Click(object sender, EventArgs e)
        {
            // クリック時に効果音だけ再生
            PlaySoundEffect("Ekisu2.mp3");
        }

        private void button_back_Click_1(object sender, EventArgs e)
        {
            // 戻るボタン用の効果音を再生
            PlaySoundEffect("Back.mp3");

        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
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
        }

        // キャラクターリスト
        private List<CharacterInfo> characters;
        private int currentIndex = 0;
        private Timer animationTimer;
        private int animationStep = 0;
        private bool isAnimating = false;
        private Label label_CharaName;  // キャラクター名表示用ラベル
        private string customName = "";  // カスタム名（ユーザーが変更した名前）

        public Form_Pick()
        {
            InitializeComponent();
            this.AutoScaleMode = AutoScaleMode.None;

            // 名前表示用ラベルを作成
            label_CharaName = new Label();
            label_CharaName.Font = new Font("MS UI Gothic", 18, FontStyle.Bold);
            label_CharaName.Location = new Point(34, 117);
            label_CharaName.Size = new Size(200, 32);
            label_CharaName.Text = "";
            label_CharaName.TextAlign = ContentAlignment.MiddleLeft;
            this.Controls.Add(label_CharaName);

            // 名前のラベルを追加
            Label label_Name = new Label();
            label_Name.Text = "名前";
            label_Name.Font = new Font("MS UI Gothic", 18);
            label_Name.Location = new Point(38, 90);
            label_Name.Size = new Size(58, 24);
            this.Controls.Add(label_Name);

            InitializeCharacters();
            UpdateDisplay();

            // アニメーション用タイマー
            animationTimer = new Timer();
            animationTimer.Interval = 10; // 10ms間隔
            animationTimer.Tick += AnimationTimer_Tick;

            // 選択ボタンのイベントハンドラを追加
            button_submitPick.Click += Button_submitPick_Click;

            // 名前変更ボタンのイベントハンドラを追加
            button_changeName.Click += Button_changeName_Click;
        }

        private void InitializeCharacters()
        {
            characters = new List<CharacterInfo>
            {
                new CharacterInfo
                {
                    Name = "赤ピクミン",
                    Image = Properties.Resources.RedPikumin,
                    Description = "火に強く、攻撃力が高い",
                    Health = 100,
                    Attack = 80,
                    Defense = 60
                },
                new CharacterInfo
                {
                    Name = "羽ピクミン",
                    Image = Properties.Resources.WingPikumin,
                    Description = "空を飛べて素早い",
                    Health = 80,
                    Attack = 50,
                    Defense = 40
                },
                new CharacterInfo
                {
                    Name = "岩ピクミン",
                    Image = Properties.Resources.RockPikumin,
                    Description = "防御力が高く頑丈",
                    Health = 120,
                    Attack = 70,
                    Defense = 100
                }
            };

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

            // 右側（薄く表示）
            pictureBox3.Image = MakeTransparentImage(characters[rightIndex].Image, 0.3f);
            pictureBox3.SizeMode = PictureBoxSizeMode.StretchImage;

            // ラベルの更新
            label_Chara1.Text = characters[leftIndex].Name;
            label_Chara1.ForeColor = Color.Gray;
            label4.Text = characters[centerIndex].Name;
            label4.ForeColor = Color.Black;
            label4.Font = new Font(label4.Font, FontStyle.Bold);
            label5.Text = characters[rightIndex].Name;
            label5.ForeColor = Color.Gray;

            // ステータス表示を更新
            var selected = characters[centerIndex];
            label2.Text = $"{selected.Description}\n\n" +
                         $"体力：{selected.Health}\n" +
                         $"攻撃力：{selected.Attack}\n" +
                         $"防御力：{selected.Defense}";

            // 名前表示を更新（カスタム名があればそれを、なければキャラクター名を表示）
            if (!string.IsNullOrEmpty(customName))
            {
                label_CharaName.Text = customName;
            }
            else
            {
                label_CharaName.Text = selected.Name;
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
            // 左に移動
            MoveCarousel(-1);
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            // 中央クリック時は何もしない（既に選択されている）
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            // 右に移動
            MoveCarousel(1);
        }

        private void MoveCarousel(int direction)
        {
            if (isAnimating) return;

            currentIndex = (currentIndex + direction + characters.Count) % characters.Count;

            // キャラクターを切り替えたらカスタム名をクリア
            customName = "";

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

            // アニメーション処理（フェードイン・フェードアウト効果）
            double opacity = Math.Sin(animationStep * 0.1);

            if (animationStep >= 10)
            {
                animationTimer.Stop();
                isAnimating = false;
                UpdateDisplay();
            }
        }

        // キーボードでの操作を追加
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

        // マウスホイールでの操作を追加
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (e.Delta > 0)
                MoveCarousel(-1);
            else if (e.Delta < 0)
                MoveCarousel(1);
        }

        // 選択ボタンのクリックイベント
        private void Button_submitPick_Click(object sender, EventArgs e)
        {
            // 名前を取得（カスタム名があればそれを、なければキャラクター名を使用）
            string finalName = !string.IsNullOrEmpty(customName)
                ? customName
                : characters[currentIndex].Name;

            // 選択確定のメッセージを表示
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
                MessageBox.Show("育成対象を確定しました！", "確定",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                // ここで次の画面に遷移する処理を追加できます
                // 例: this.Hide(); new Form_Main().Show();
            }
        }

        // 名前変更ボタンのクリックイベント
        private void Button_changeName_Click(object sender, EventArgs e)
        {
            // FormNameChangeを作成して表示
            FormNameChange nameChangeForm = new FormNameChange();

            // 現在の名前を渡す（カスタム名があればそれを、なければキャラクター名を使用）
            nameChangeForm.CurrentName = !string.IsNullOrEmpty(customName)
                ? customName
                : characters[currentIndex].Name;

            // ダイアログとして表示
            if (nameChangeForm.ShowDialog(this) == DialogResult.OK)
            {
                // OKが押された場合、新しい名前を取得
                customName = nameChangeForm.NewName;
                // 表示を更新
                UpdateDisplay();
            }
        }
    }
}
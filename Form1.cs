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
        // キャラクター情報を管理するクラス
        private class CharacterInfo
        {
            public string Name { get; set; }
            public Image Image { get; set; }
            public string Status { get; set; }
            public RadioButton RadioButton { get; set; }
        }

        // キャラクターリスト
        private List<CharacterInfo> characters;
        private int currentIndex = 0;
        private Timer animationTimer;
        private int animationStep = 0;
        private bool isAnimating = false;

        public Form_Pick()
        {
            InitializeComponent();
            this.AutoScaleMode = AutoScaleMode.None;
            InitializeCharacters();
            UpdateDisplay();

            // アニメーション用タイマー
            animationTimer = new Timer();
            animationTimer.Interval = 10; // 10ms間隔
            animationTimer.Tick += AnimationTimer_Tick;
        }

        private void InitializeCharacters()
        {
            characters = new List<CharacterInfo>
            {
                new CharacterInfo
                {
                    Name = "赤ピクミン",
                    Image = Properties.Resources.RedPikumin,
                    Status = "火に強い\n攻撃力：★★★\n速度：★★☆",
                },
                new CharacterInfo
                {
                    Name = "羽ピクミン",
                    Image = Properties.Resources.WingPikumin,
                    Status = "空を飛べる\n攻撃力：★☆☆\n速度：★★★",
                },
                new CharacterInfo
                {
                    Name = "岩ピクミン",
                    Image = Properties.Resources.RockPikumin,
                    Status = "防御力が高い\n攻撃力：★★★\n速度：★☆☆",
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

            // 左側
            pictureBox1.Image = characters[leftIndex].Image;
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;

            // 中央（選択中）
            pictureBox2.Image = characters[centerIndex].Image;
            pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;

            // 右側
            pictureBox3.Image = characters[rightIndex].Image;
            pictureBox3.SizeMode = PictureBoxSizeMode.StretchImage;

            // 中央のPictureBoxを少し大きくする（視覚的効果）
            pictureBox2.Size = new Size(270, 369);
            pictureBox1.Size = new Size(250, 349);
            pictureBox3.Size = new Size(250, 349);

            // ラジオボタンの選択を更新
            characters[centerIndex].RadioButton.Checked = true;

            // ステータス表示を更新
            label2.Text = characters[centerIndex].Status;
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
    }
}
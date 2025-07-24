namespace nurturing
{
    partial class Form_Pick
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージド リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_Pick));
            this.label_Title = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.button_submitPick = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.pictureBox3 = new System.Windows.Forms.PictureBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.label_Chara1 = new System.Windows.Forms.Label();
            this.label_Chara2 = new System.Windows.Forms.Label();
            this.label_Chara3 = new System.Windows.Forms.Label();
            this.button_changeName = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.SuspendLayout();
            // 
            // label_Title
            // 
            this.label_Title.Font = new System.Drawing.Font("MS UI Gothic", 27.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label_Title.Location = new System.Drawing.Point(15, 11);
            this.label_Title.Name = "label_Title";
            this.label_Title.Size = new System.Drawing.Size(1032, 80);
            this.label_Title.TabIndex = 0;
            this.label_Title.Text = "キャラクターを選択してください";
            this.label_Title.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label2
            // 
            this.label2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label2.Font = new System.Drawing.Font("MS UI Gothic", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label2.Location = new System.Drawing.Point(548, 91);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(464, 195);
            this.label2.TabIndex = 4;
            // 
            // button_submitPick
            // 
            this.button_submitPick.Font = new System.Drawing.Font("MS UI Gothic", 16.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.button_submitPick.Location = new System.Drawing.Point(548, 722);
            this.button_submitPick.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.button_submitPick.Name = "button_submitPick";
            this.button_submitPick.Size = new System.Drawing.Size(147, 52);
            this.button_submitPick.TabIndex = 11;
            this.button_submitPick.Text = "選択";
            this.button_submitPick.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 5;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 26.53061F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10.20408F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 26.53061F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10.20408F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 26.53061F));
            this.tableLayoutPanel1.Controls.Add(this.pictureBox3, 4, 0);
            this.tableLayoutPanel1.Controls.Add(this.pictureBox1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.pictureBox2, 2, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(63, 362);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(967, 355);
            this.tableLayoutPanel1.TabIndex = 9;
            // 
            // pictureBox3
            // 
            this.pictureBox3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox3.Image = global::nurturing.Properties.Resources.RockPikumin;
            this.pictureBox3.Location = new System.Drawing.Point(711, 2);
            this.pictureBox3.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pictureBox3.Name = "pictureBox3";
            this.pictureBox3.Size = new System.Drawing.Size(253, 350);
            this.pictureBox3.TabIndex = 2;
            this.pictureBox3.TabStop = false;
            this.pictureBox3.Click += new System.EventHandler(this.pictureBox3_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox1.Image = global::nurturing.Properties.Resources.RedPikumin;
            this.pictureBox1.Location = new System.Drawing.Point(3, 2);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(250, 350);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 3;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // pictureBox2
            // 
            this.pictureBox2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox2.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox2.Image")));
            this.pictureBox2.Location = new System.Drawing.Point(357, 2);
            this.pictureBox2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(250, 350);
            this.pictureBox2.TabIndex = 1;
            this.pictureBox2.TabStop = false;
            this.pictureBox2.Click += new System.EventHandler(this.pictureBox2_Click);
            // 
            // label_Chara1
            // 
            this.label_Chara1.Font = new System.Drawing.Font("MS UI Gothic", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label_Chara1.Location = new System.Drawing.Point(60, 324);
            this.label_Chara1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_Chara1.Name = "label_Chara1";
            this.label_Chara1.Size = new System.Drawing.Size(256, 36);
            this.label_Chara1.TabIndex = 12;
            this.label_Chara1.Text = "赤ピクミン";
            this.label_Chara1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label_Chara2
            // 
            this.label_Chara2.Font = new System.Drawing.Font("MS UI Gothic", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label_Chara2.Location = new System.Drawing.Point(413, 316);
            this.label_Chara2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_Chara2.Name = "label_Chara2";
            this.label_Chara2.Size = new System.Drawing.Size(256, 44);
            this.label_Chara2.TabIndex = 13;
            this.label_Chara2.Text = "羽ピクミン";
            this.label_Chara2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label_Chara3
            // 
            this.label_Chara3.Font = new System.Drawing.Font("MS UI Gothic", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label_Chara3.Location = new System.Drawing.Point(767, 316);
            this.label_Chara3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_Chara3.Name = "label_Chara3";
            this.label_Chara3.Size = new System.Drawing.Size(259, 44);
            this.label_Chara3.TabIndex = 14;
            this.label_Chara3.Text = "岩ピクミン";
            this.label_Chara3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // button_changeName
            // 
            this.button_changeName.Font = new System.Drawing.Font("MS UI Gothic", 16.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.button_changeName.Location = new System.Drawing.Point(383, 722);
            this.button_changeName.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.button_changeName.Name = "button_changeName";
            this.button_changeName.Size = new System.Drawing.Size(147, 52);
            this.button_changeName.TabIndex = 15;
            this.button_changeName.Text = "名前変更";
            this.button_changeName.UseVisualStyleBackColor = true;
            // 
            // Form_Pick
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1059, 789);
            this.Controls.Add(this.button_changeName);
            this.Controls.Add(this.label_Chara3);
            this.Controls.Add(this.label_Chara2);
            this.Controls.Add(this.label_Chara1);
            this.Controls.Add(this.button_submitPick);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label_Title);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.MaximizeBox = false;
            this.Name = "Form_Pick";
            this.ShowIcon = false;
            this.Text = "選択画面";
            this.Load += new System.EventHandler(this.Form_Pick_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label_Title;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button_submitPick;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.PictureBox pictureBox3;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label_Chara1;
        private System.Windows.Forms.Label label_Chara2;
        private System.Windows.Forms.Label label_Chara3;
        private System.Windows.Forms.Button button_changeName;
    }
}


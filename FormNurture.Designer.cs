namespace nurturing
{
    partial class FormNurture
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.panel_gameArea = new System.Windows.Forms.Panel();
            this.progressBar_defense = new System.Windows.Forms.ProgressBar();
            this.progressBar_attack = new System.Windows.Forms.ProgressBar();
            this.progressBar_hp = new System.Windows.Forms.ProgressBar();
            this.pictureBox_extract = new System.Windows.Forms.PictureBox();
            this.pictureBox_character = new System.Windows.Forms.PictureBox();
            this.panel_stats = new System.Windows.Forms.Panel();
            this.progressBar_exp2 = new System.Windows.Forms.ProgressBar();
            this.button_back = new System.Windows.Forms.Button();
            this.button_save = new System.Windows.Forms.Button();
            this.label_extractCount = new System.Windows.Forms.Label();
            this.label_defense = new System.Windows.Forms.Label();
            this.label_attack = new System.Windows.Forms.Label();
            this.label_hp = new System.Windows.Forms.Label();
            this.progressBar_exp = new System.Windows.Forms.Label();
            this.label_level = new System.Windows.Forms.Label();
            this.label_characterName = new System.Windows.Forms.Label();
            this.panel_gameArea.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_extract)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_character)).BeginInit();
            this.panel_stats.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel_gameArea
            // 
            this.panel_gameArea.BackColor = System.Drawing.Color.LightGreen;
            this.panel_gameArea.Controls.Add(this.progressBar_defense);
            this.panel_gameArea.Controls.Add(this.progressBar_attack);
            this.panel_gameArea.Controls.Add(this.progressBar_hp);
            this.panel_gameArea.Controls.Add(this.pictureBox_extract);
            this.panel_gameArea.Controls.Add(this.pictureBox_character);
            this.panel_gameArea.Location = new System.Drawing.Point(12, 12);
            this.panel_gameArea.Name = "panel_gameArea";
            this.panel_gameArea.Size = new System.Drawing.Size(600, 500);
            this.panel_gameArea.TabIndex = 0;
            // 
            // progressBar_defense
            // 
            this.progressBar_defense.Location = new System.Drawing.Point(15, 230);
            this.progressBar_defense.Name = "progressBar_defense";
            this.progressBar_defense.Size = new System.Drawing.Size(200, 20);
            this.progressBar_defense.TabIndex = 4;
            // 
            // progressBar_attack
            // 
            this.progressBar_attack.Location = new System.Drawing.Point(15, 200);
            this.progressBar_attack.Name = "progressBar_attack";
            this.progressBar_attack.Size = new System.Drawing.Size(200, 20);
            this.progressBar_attack.TabIndex = 3;
            // 
            // progressBar_hp
            // 
            this.progressBar_hp.Location = new System.Drawing.Point(15, 170);
            this.progressBar_hp.Name = "progressBar_hp";
            this.progressBar_hp.Size = new System.Drawing.Size(200, 20);
            this.progressBar_hp.TabIndex = 2;
            // 
            // pictureBox_extract
            // 
            this.pictureBox_extract.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox_extract.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pictureBox_extract.Location = new System.Drawing.Point(275, 400);
            this.pictureBox_extract.Name = "pictureBox_extract";
            this.pictureBox_extract.Size = new System.Drawing.Size(50, 50);
            this.pictureBox_extract.TabIndex = 1;
            this.pictureBox_extract.TabStop = false;
            // 
            // pictureBox_character
            // 
            this.pictureBox_character.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox_character.Location = new System.Drawing.Point(250, 150);
            this.pictureBox_character.Name = "pictureBox_character";
            this.pictureBox_character.Size = new System.Drawing.Size(100, 150);
            this.pictureBox_character.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox_character.TabIndex = 0;
            this.pictureBox_character.TabStop = false;
            // 
            // panel_stats
            // 
            this.panel_stats.BackColor = System.Drawing.Color.White;
            this.panel_stats.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel_stats.Controls.Add(this.progressBar_exp2);
            this.panel_stats.Controls.Add(this.button_back);
            this.panel_stats.Controls.Add(this.button_save);
            this.panel_stats.Controls.Add(this.label_extractCount);
            this.panel_stats.Controls.Add(this.label_defense);
            this.panel_stats.Controls.Add(this.label_attack);
            this.panel_stats.Controls.Add(this.label_hp);
            this.panel_stats.Controls.Add(this.progressBar_exp);
            this.panel_stats.Controls.Add(this.label_level);
            this.panel_stats.Controls.Add(this.label_characterName);
            this.panel_stats.Location = new System.Drawing.Point(630, 12);
            this.panel_stats.Name = "panel_stats";
            this.panel_stats.Size = new System.Drawing.Size(250, 500);
            this.panel_stats.TabIndex = 1;
            // 
            // progressBar_exp2
            // 
            this.progressBar_exp2.Location = new System.Drawing.Point(15, 110);
            this.progressBar_exp2.Name = "progressBar_exp2";
            this.progressBar_exp2.Size = new System.Drawing.Size(200, 23);
            this.progressBar_exp2.TabIndex = 10;
            // 
            // button_back
            // 
            this.button_back.Font = new System.Drawing.Font("MS UI Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.button_back.Location = new System.Drawing.Point(130, 340);
            this.button_back.Name = "button_back";
            this.button_back.Size = new System.Drawing.Size(100, 40);
            this.button_back.TabIndex = 9;
            this.button_back.Text = "戻る";
            this.button_back.UseVisualStyleBackColor = true;
            // 
            // button_save
            // 
            this.button_save.Font = new System.Drawing.Font("MS UI Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.button_save.Location = new System.Drawing.Point(15, 340);
            this.button_save.Name = "button_save";
            this.button_save.Size = new System.Drawing.Size(100, 40);
            this.button_save.TabIndex = 8;
            this.button_save.Text = "保存";
            this.button_save.UseVisualStyleBackColor = true;
            // 
            // label_extractCount
            // 
            this.label_extractCount.Font = new System.Drawing.Font("MS UI Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label_extractCount.ForeColor = System.Drawing.Color.Black;
            this.label_extractCount.Location = new System.Drawing.Point(15, 260);
            this.label_extractCount.Name = "label_extractCount";
            this.label_extractCount.Size = new System.Drawing.Size(200, 20);
            this.label_extractCount.TabIndex = 7;
            this.label_extractCount.Text = "エキス：999";
            // 
            // label_defense
            // 
            this.label_defense.Font = new System.Drawing.Font("MS UI Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label_defense.Location = new System.Drawing.Point(15, 210);
            this.label_defense.Name = "label_defense";
            this.label_defense.Size = new System.Drawing.Size(200, 20);
            this.label_defense.TabIndex = 6;
            this.label_defense.Text = "防御力：60";
            // 
            // label_attack
            // 
            this.label_attack.Font = new System.Drawing.Font("MS UI Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label_attack.Location = new System.Drawing.Point(15, 180);
            this.label_attack.Name = "label_attack";
            this.label_attack.Size = new System.Drawing.Size(200, 20);
            this.label_attack.TabIndex = 5;
            this.label_attack.Text = "攻撃力：80";
            // 
            // label_hp
            // 
            this.label_hp.Font = new System.Drawing.Font("MS UI Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label_hp.Location = new System.Drawing.Point(15, 150);
            this.label_hp.Name = "label_hp";
            this.label_hp.Size = new System.Drawing.Size(200, 20);
            this.label_hp.TabIndex = 4;
            this.label_hp.Text = "体力：100";
            // 
            // progressBar_exp
            // 
            this.progressBar_exp.AutoSize = true;
            this.progressBar_exp.Location = new System.Drawing.Point(15, 110);
            this.progressBar_exp.Name = "progressBar_exp";
            this.progressBar_exp.Size = new System.Drawing.Size(0, 15);
            this.progressBar_exp.TabIndex = 3;
            // 
            // label_level
            // 
            this.label_level.Font = new System.Drawing.Font("MS UI Gothic", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label_level.Location = new System.Drawing.Point(15, 55);
            this.label_level.Name = "label_level";
            this.label_level.Size = new System.Drawing.Size(200, 25);
            this.label_level.TabIndex = 1;
            this.label_level.Text = "レベル：1";
            // 
            // label_characterName
            // 
            this.label_characterName.Font = new System.Drawing.Font("MS UI Gothic", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label_characterName.Location = new System.Drawing.Point(15, 15);
            this.label_characterName.Name = "label_characterName";
            this.label_characterName.Size = new System.Drawing.Size(200, 300);
            this.label_characterName.TabIndex = 0;
            this.label_characterName.Text = "キャラクター名";
            // 
            // FormNurture
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(892, 523);
            this.Controls.Add(this.panel_stats);
            this.Controls.Add(this.panel_gameArea);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "FormNurture";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "育成画面";
            this.panel_gameArea.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_extract)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_character)).EndInit();
            this.panel_stats.ResumeLayout(false);
            this.panel_stats.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel_gameArea;
        private System.Windows.Forms.PictureBox pictureBox_character;
        private System.Windows.Forms.PictureBox pictureBox_extract;
        private System.Windows.Forms.Panel panel_stats;
        private System.Windows.Forms.Label label_characterName;
        private System.Windows.Forms.Label label_level;
        private System.Windows.Forms.Label label_attack;
        private System.Windows.Forms.Label label_hp;
        private System.Windows.Forms.Label progressBar_exp;
        private System.Windows.Forms.Label label_extractCount;
        private System.Windows.Forms.Label label_defense;
        private System.Windows.Forms.Button button_back;
        private System.Windows.Forms.Button button_save;
        private System.Windows.Forms.ProgressBar progressBar_defense;
        private System.Windows.Forms.ProgressBar progressBar_attack;
        private System.Windows.Forms.ProgressBar progressBar_hp;
        private System.Windows.Forms.ProgressBar progressBar_exp2;
    }
}
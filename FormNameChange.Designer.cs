namespace nurturing
{
    partial class FormNameChange
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
            this.textBox_changeName = new System.Windows.Forms.TextBox();
            this.button_cancelName = new System.Windows.Forms.Button();
            this.button_submitName = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // textBox_changeName
            // 
            this.textBox_changeName.Font = new System.Drawing.Font("MS UI Gothic", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.textBox_changeName.Location = new System.Drawing.Point(37, 21);
            this.textBox_changeName.Name = "textBox_changeName";
            this.textBox_changeName.Size = new System.Drawing.Size(207, 28);
            this.textBox_changeName.TabIndex = 0;
            // 
            // button_cancelName
            // 
            this.button_cancelName.Font = new System.Drawing.Font("MS UI Gothic", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.button_cancelName.Location = new System.Drawing.Point(37, 55);
            this.button_cancelName.Name = "button_cancelName";
            this.button_cancelName.Size = new System.Drawing.Size(75, 23);
            this.button_cancelName.TabIndex = 1;
            this.button_cancelName.Text = "キャンセル";
            this.button_cancelName.UseVisualStyleBackColor = true;
            // 
            // button_submitName
            // 
            this.button_submitName.Font = new System.Drawing.Font("MS UI Gothic", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.button_submitName.Location = new System.Drawing.Point(169, 55);
            this.button_submitName.Name = "button_submitName";
            this.button_submitName.Size = new System.Drawing.Size(75, 23);
            this.button_submitName.TabIndex = 2;
            this.button_submitName.Text = "確定";
            this.button_submitName.UseVisualStyleBackColor = true;
            // 
            // FormNameChange
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(308, 97);
            this.Controls.Add(this.button_submitName);
            this.Controls.Add(this.button_cancelName);
            this.Controls.Add(this.textBox_changeName);
            this.Name = "FormNameChange";
            this.Text = "FormNameChange";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox_changeName;
        private System.Windows.Forms.Button button_cancelName;
        private System.Windows.Forms.Button button_submitName;
    }
}
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
    public partial class FormNameChange : Form
    {
        // 名前を変更するためのダイアログ
        // プロパティ
        public string CurrentName { get; set; }
        public string NewName { get; private set; }

        public FormNameChange()
        {
            InitializeComponent();

            // フォームの設定
            this.Text = "名前変更";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            // ボタンのイベントハンドラを設定
            button_cancelName.Click += Button_cancelName_Click;
            button_submitName.Click += Button_submitName_Click;

            // Enterキーで確定、Escapeキーでキャンセル
            this.AcceptButton = button_submitName;
            this.CancelButton = button_cancelName;
        }

        // フォームが表示される時
        protected override void OnShown(EventArgs e)
        {
            // 表示時に現在の名前を設定する
            base.OnShown(e);

            // 現在の名前を表示
            textBox_changeName.Text = CurrentName;
            textBox_changeName.SelectAll();  // 全選択状態にする
            textBox_changeName.Focus();      // フォーカスを設定
        }

        // キャンセルボタン
        private void Button_cancelName_Click(object sender, EventArgs e)
        {
            // キャンセルで閉じる
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        // 確定ボタン
        private void Button_submitName_Click(object sender, EventArgs e)
        {
            // 入力した名前で決定
            // 名前が空でないかチェック
            if (string.IsNullOrWhiteSpace(textBox_changeName.Text))
            {
                MessageBox.Show("名前を入力してください。", "入力エラー",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBox_changeName.Focus();
                return;
            }

            // 新しい名前を設定
            NewName = textBox_changeName.Text.Trim();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
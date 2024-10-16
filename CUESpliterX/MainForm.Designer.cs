namespace CUESpliterX
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            button1 = new Button();
            textBox1 = new TextBox();
            label1 = new Label();
            label2 = new Label();
            textBox2 = new TextBox();
            button2 = new Button();
            button3 = new Button();
            textBox3 = new TextBox();
            label3 = new Label();
            textBox4 = new TextBox();
            label4 = new Label();
            textBox5 = new TextBox();
            label5 = new Label();
            checkBox1 = new CheckBox();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Location = new Point(12, 150);
            button1.Name = "button1";
            button1.Size = new Size(75, 23);
            button1.TabIndex = 0;
            button1.Text = "分割";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // textBox1
            // 
            textBox1.AllowDrop = true;
            textBox1.Location = new Point(12, 29);
            textBox1.Name = "textBox1";
            textBox1.ReadOnly = true;
            textBox1.Size = new Size(679, 23);
            textBox1.TabIndex = 1;
            textBox1.DragDrop += textBox1_DragDrop;
            textBox1.DragEnter += textBox1_DragEnter;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(68, 17);
            label1.TabIndex = 2;
            label1.Text = "CUE文件：";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 55);
            label2.Name = "label2";
            label2.Size = new Size(68, 17);
            label2.TabIndex = 4;
            label2.Text = "输出路径：";
            // 
            // textBox2
            // 
            textBox2.AllowDrop = true;
            textBox2.Location = new Point(12, 75);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(622, 23);
            textBox2.TabIndex = 3;
            // 
            // button2
            // 
            button2.Location = new Point(697, 75);
            button2.Name = "button2";
            button2.Size = new Size(75, 23);
            button2.TabIndex = 5;
            button2.Text = "选择";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // button3
            // 
            button3.Location = new Point(697, 29);
            button3.Name = "button3";
            button3.Size = new Size(75, 23);
            button3.TabIndex = 6;
            button3.Text = "选择";
            button3.UseVisualStyleBackColor = true;
            button3.Click += button3_Click;
            // 
            // textBox3
            // 
            textBox3.AllowDrop = true;
            textBox3.Location = new Point(12, 121);
            textBox3.Name = "textBox3";
            textBox3.Size = new Size(174, 23);
            textBox3.TabIndex = 3;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(12, 101);
            label3.Name = "label3";
            label3.Size = new Size(56, 17);
            label3.TabIndex = 4;
            label3.Text = "艺术家：";
            // 
            // textBox4
            // 
            textBox4.AllowDrop = true;
            textBox4.Location = new Point(192, 121);
            textBox4.Name = "textBox4";
            textBox4.Size = new Size(442, 23);
            textBox4.TabIndex = 3;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(192, 101);
            label4.Name = "label4";
            label4.Size = new Size(44, 17);
            label4.TabIndex = 4;
            label4.Text = "专辑：";
            // 
            // textBox5
            // 
            textBox5.Location = new Point(12, 221);
            textBox5.Multiline = true;
            textBox5.Name = "textBox5";
            textBox5.ScrollBars = ScrollBars.Vertical;
            textBox5.Size = new Size(760, 228);
            textBox5.TabIndex = 7;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(12, 201);
            label5.Name = "label5";
            label5.Size = new Size(68, 17);
            label5.TabIndex = 4;
            label5.Text = "输出日志：";
            // 
            // checkBox1
            // 
            checkBox1.AutoSize = true;
            checkBox1.Location = new Point(640, 77);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new Size(51, 21);
            checkBox1.TabIndex = 8;
            checkBox1.Text = "覆盖";
            checkBox1.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(784, 461);
            Controls.Add(checkBox1);
            Controls.Add(textBox5);
            Controls.Add(button3);
            Controls.Add(button2);
            Controls.Add(label5);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(textBox4);
            Controls.Add(textBox3);
            Controls.Add(textBox2);
            Controls.Add(label1);
            Controls.Add(textBox1);
            Controls.Add(button1);
            Name = "MainForm";
            Text = "CUESpliterX";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button button1;
        private TextBox textBox1;
        private Label label1;
        private Label label2;
        private TextBox textBox2;
        private Button button2;
        private Button button3;
        private TextBox textBox3;
        private Label label3;
        private TextBox textBox4;
        private Label label4;
        private TextBox textBox5;
        private Label label5;
        private CheckBox checkBox1;
    }
}

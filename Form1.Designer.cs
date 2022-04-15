
namespace AsteraFarmer
{
    partial class Form1
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
            this.delayTime = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.startBtn = new System.Windows.Forms.Button();
            this.stopBtn = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.timerBox = new System.Windows.Forms.TextBox();
            this.resetBtn = new System.Windows.Forms.Button();
            this.initBtn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // delayTime
            // 
            this.delayTime.Location = new System.Drawing.Point(67, 28);
            this.delayTime.Name = "delayTime";
            this.delayTime.Size = new System.Drawing.Size(156, 20);
            this.delayTime.TabIndex = 0;
            this.delayTime.Text = "0";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 31);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(40, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Delay: ";
            // 
            // startBtn
            // 
            this.startBtn.Location = new System.Drawing.Point(12, 98);
            this.startBtn.Name = "startBtn";
            this.startBtn.Size = new System.Drawing.Size(75, 23);
            this.startBtn.TabIndex = 2;
            this.startBtn.Text = "Start";
            this.startBtn.UseVisualStyleBackColor = true;
            this.startBtn.Click += new System.EventHandler(this.startBtn_Click);
            // 
            // stopBtn
            // 
            this.stopBtn.Location = new System.Drawing.Point(93, 98);
            this.stopBtn.Name = "stopBtn";
            this.stopBtn.Size = new System.Drawing.Size(75, 23);
            this.stopBtn.TabIndex = 3;
            this.stopBtn.Text = "Stop";
            this.stopBtn.UseVisualStyleBackColor = true;
            this.stopBtn.Click += new System.EventHandler(this.stopBtn_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 62);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(36, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Timer:";
            // 
            // timerBox
            // 
            this.timerBox.Location = new System.Drawing.Point(67, 62);
            this.timerBox.Name = "timerBox";
            this.timerBox.Size = new System.Drawing.Size(156, 20);
            this.timerBox.TabIndex = 5;
            this.timerBox.Text = "10000";
            this.timerBox.TextChanged += new System.EventHandler(this.timerBox_TextChanged);
            // 
            // resetBtn
            // 
            this.resetBtn.Location = new System.Drawing.Point(174, 98);
            this.resetBtn.Name = "resetBtn";
            this.resetBtn.Size = new System.Drawing.Size(75, 23);
            this.resetBtn.TabIndex = 6;
            this.resetBtn.Text = "Reset Time";
            this.resetBtn.UseVisualStyleBackColor = true;
            this.resetBtn.Click += new System.EventHandler(this.resetBtn_Click);
            // 
            // initBtn
            // 
            this.initBtn.Location = new System.Drawing.Point(67, 132);
            this.initBtn.Name = "initBtn";
            this.initBtn.Size = new System.Drawing.Size(132, 23);
            this.initBtn.TabIndex = 7;
            this.initBtn.Text = "Initialize Clients";
            this.initBtn.UseVisualStyleBackColor = true;
            this.initBtn.Click += new System.EventHandler(this.initBtn_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(278, 167);
            this.Controls.Add(this.initBtn);
            this.Controls.Add(this.resetBtn);
            this.Controls.Add(this.timerBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.stopBtn);
            this.Controls.Add(this.startBtn);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.delayTime);
            this.Name = "Form1";
            this.Text = "Roths Macro Farmer";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox delayTime;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button startBtn;
        private System.Windows.Forms.Button stopBtn;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox timerBox;
        private System.Windows.Forms.Button resetBtn;
        private System.Windows.Forms.Button initBtn;
    }
}


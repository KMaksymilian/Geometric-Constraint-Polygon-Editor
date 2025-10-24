namespace GK1_25Z_01189143_Zadanie1
{
    partial class Form1
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
            menuStrip1 = new MenuStrip();
            restartToolStripMenuItem = new ToolStripMenuItem();
            panel1 = new Panel();
            self = new RadioButton();
            lib = new RadioButton();
            menuStrip1.SuspendLayout();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new ToolStripItem[] { restartToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(800, 24);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            // 
            // restartToolStripMenuItem
            // 
            restartToolStripMenuItem.Name = "restartToolStripMenuItem";
            restartToolStripMenuItem.Size = new Size(55, 20);
            restartToolStripMenuItem.Text = "Restart";
            restartToolStripMenuItem.Click += restartToolStripMenuItem_Click_1;
            // 
            // panel1
            // 
            panel1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            panel1.Controls.Add(self);
            panel1.Controls.Add(lib);
            panel1.Location = new Point(714, 27);
            panel1.Name = "panel1";
            panel1.Size = new Size(86, 71);
            panel1.TabIndex = 1;
            // 
            // self
            // 
            self.AutoSize = true;
            self.Location = new Point(12, 38);
            self.Name = "self";
            self.Size = new Size(43, 19);
            self.TabIndex = 3;
            self.TabStop = true;
            self.Text = "self";
            self.UseVisualStyleBackColor = true;
            self.CheckedChanged += self_CheckedChanged;
            // 
            // lib
            // 
            lib.AutoSize = true;
            lib.Checked = true;
            lib.Location = new Point(12, 13);
            lib.Name = "lib";
            lib.Size = new Size(38, 19);
            lib.TabIndex = 2;
            lib.TabStop = true;
            lib.Text = "lib";
            lib.UseVisualStyleBackColor = true;
            lib.CheckedChanged += lib_CheckedChanged;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(panel1);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Name = "Form1";
            Text = "Form1";
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip menuStrip1;
        private ToolStripMenuItem restartToolStripMenuItem;
        private Panel panel1;
        private RadioButton self;
        private RadioButton lib;
    }
}

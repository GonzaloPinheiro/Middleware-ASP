namespace TFCiclo.Forms.Demo
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
            btnVerTiempo = new Button();
            lbxConsola = new ListBox();
            lblEstadoVertiempo = new Label();
            tbxResultadoVertiempo = new TextBox();
            SuspendLayout();
            // 
            // btnVerTiempo
            // 
            btnVerTiempo.Location = new Point(12, 415);
            btnVerTiempo.Name = "btnVerTiempo";
            btnVerTiempo.Size = new Size(75, 23);
            btnVerTiempo.TabIndex = 0;
            btnVerTiempo.Text = "Ver Tiempo";
            btnVerTiempo.UseVisualStyleBackColor = true;
            btnVerTiempo.Click += btnVerTiempo_Click_1;
            // 
            // lbxConsola
            // 
            lbxConsola.FormattingEnabled = true;
            lbxConsola.ItemHeight = 15;
            lbxConsola.Location = new Point(668, 12);
            lbxConsola.Name = "lbxConsola";
            lbxConsola.Size = new Size(120, 424);
            lbxConsola.TabIndex = 1;
            // 
            // lblEstadoVertiempo
            // 
            lblEstadoVertiempo.AutoSize = true;
            lblEstadoVertiempo.Location = new Point(18, 385);
            lblEstadoVertiempo.Name = "lblEstadoVertiempo";
            lblEstadoVertiempo.Size = new Size(38, 15);
            lblEstadoVertiempo.TabIndex = 2;
            lblEstadoVertiempo.Text = "label1";
            // 
            // tbxResultadoVertiempo
            // 
            tbxResultadoVertiempo.Location = new Point(18, 359);
            tbxResultadoVertiempo.Name = "tbxResultadoVertiempo";
            tbxResultadoVertiempo.Size = new Size(100, 23);
            tbxResultadoVertiempo.TabIndex = 3;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(tbxResultadoVertiempo);
            Controls.Add(lblEstadoVertiempo);
            Controls.Add(lbxConsola);
            Controls.Add(btnVerTiempo);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnVerTiempo;
        private ListBox lbxConsola;
        private Label lblEstadoVertiempo;
        private TextBox tbxResultadoVertiempo;
    }
}

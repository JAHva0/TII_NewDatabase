﻿// <auto-generated/>

namespace TII_NewDatabase.AddNewForms
{
    partial class FormAddInspection
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
            this.gbx_Building = new System.Windows.Forms.GroupBox();
            this.lbx_BuildingList = new System.Windows.Forms.ListBox();
            this.txt_BuildingFilter = new System.Windows.Forms.TextBox();
            this.gbx_Building.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbx_Building
            // 
            this.gbx_Building.Controls.Add(this.lbx_BuildingList);
            this.gbx_Building.Controls.Add(this.txt_BuildingFilter);
            this.gbx_Building.Location = new System.Drawing.Point(12, 12);
            this.gbx_Building.Name = "gbx_Building";
            this.gbx_Building.Size = new System.Drawing.Size(147, 320);
            this.gbx_Building.TabIndex = 0;
            this.gbx_Building.TabStop = false;
            this.gbx_Building.Text = "Building";
            // 
            // lbx_BuildingList
            // 
            this.lbx_BuildingList.FormattingEnabled = true;
            this.lbx_BuildingList.Location = new System.Drawing.Point(6, 45);
            this.lbx_BuildingList.Name = "lbx_BuildingList";
            this.lbx_BuildingList.Size = new System.Drawing.Size(135, 264);
            this.lbx_BuildingList.TabIndex = 1;
            // 
            // txt_BuildingFilter
            // 
            this.txt_BuildingFilter.Location = new System.Drawing.Point(6, 19);
            this.txt_BuildingFilter.Name = "txt_BuildingFilter";
            this.txt_BuildingFilter.Size = new System.Drawing.Size(135, 20);
            this.txt_BuildingFilter.TabIndex = 0;
            // 
            // FormAddInspection
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(498, 344);
            this.Controls.Add(this.gbx_Building);
            this.Name = "FormAddInspection";
            this.Text = "Add Inspection";
            this.Load += new System.EventHandler(this.OnLoad);
            this.gbx_Building.ResumeLayout(false);
            this.gbx_Building.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gbx_Building;
        private System.Windows.Forms.ListBox lbx_BuildingList;
        private System.Windows.Forms.TextBox txt_BuildingFilter;
    }
}
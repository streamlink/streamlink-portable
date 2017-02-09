<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class InputBox_General_Custom
    Inherits System.Windows.Forms.Form

    'Form reemplaza a Dispose para limpiar la lista de componentes.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Requerido por el Diseñador de Windows Forms
    Private components As System.ComponentModel.IContainer

    'NOTA: el Diseñador de Windows Forms necesita el siguiente procedimiento
    'Se puede modificar usando el Diseñador de Windows Forms.  
    'No lo modifique con el editor de código.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.aceptar = New System.Windows.Forms.Button()
        Me.Panel2 = New System.Windows.Forms.Panel()
        Me.contenido = New System.Windows.Forms.Label()
        Me.valor = New System.Windows.Forms.TextBox()
        Me.Panel2.SuspendLayout()
        Me.SuspendLayout()
        '
        'aceptar
        '
        Me.aceptar.BackColor = System.Drawing.Color.FromArgb(CType(CType(180, Byte), Integer), CType(CType(20, Byte), Integer), CType(CType(60, Byte), Integer))
        Me.aceptar.FlatAppearance.BorderSize = 0
        Me.aceptar.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.aceptar.Font = New System.Drawing.Font("Segoe UI", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.aceptar.ForeColor = System.Drawing.Color.White
        Me.aceptar.Location = New System.Drawing.Point(12, 154)
        Me.aceptar.Name = "aceptar"
        Me.aceptar.Size = New System.Drawing.Size(388, 56)
        Me.aceptar.TabIndex = 39
        Me.aceptar.Text = "OK"
        Me.aceptar.UseVisualStyleBackColor = False
        '
        'Panel2
        '
        Me.Panel2.Controls.Add(Me.contenido)
        Me.Panel2.Location = New System.Drawing.Point(12, 12)
        Me.Panel2.Name = "Panel2"
        Me.Panel2.Size = New System.Drawing.Size(388, 100)
        Me.Panel2.TabIndex = 38
        '
        'contenido
        '
        Me.contenido.AutoEllipsis = True
        Me.contenido.BackColor = System.Drawing.Color.Transparent
        Me.contenido.Dock = System.Windows.Forms.DockStyle.Fill
        Me.contenido.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Bold)
        Me.contenido.ForeColor = System.Drawing.Color.White
        Me.contenido.Location = New System.Drawing.Point(0, 0)
        Me.contenido.Name = "contenido"
        Me.contenido.Size = New System.Drawing.Size(388, 100)
        Me.contenido.TabIndex = 39
        Me.contenido.Text = "Contenido"
        Me.contenido.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'valor
        '
        Me.valor.BackColor = System.Drawing.Color.FromArgb(CType(CType(115, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(55, Byte), Integer))
        Me.valor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.valor.Font = New System.Drawing.Font("Segoe UI", 10.2!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.valor.ForeColor = System.Drawing.Color.White
        Me.valor.Location = New System.Drawing.Point(12, 118)
        Me.valor.Name = "valor"
        Me.valor.Size = New System.Drawing.Size(388, 30)
        Me.valor.TabIndex = 40
        Me.valor.Text = "Valor"
        Me.valor.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'InputBox_General_Custom
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.BackColor = System.Drawing.Color.FromArgb(CType(CType(115, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(55, Byte), Integer))
        Me.ClientSize = New System.Drawing.Size(412, 221)
        Me.Controls.Add(Me.valor)
        Me.Controls.Add(Me.aceptar)
        Me.Controls.Add(Me.Panel2)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "InputBox_General_Custom"
        Me.ShowIcon = False
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Titulo"
        Me.Panel2.ResumeLayout(False)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents aceptar As System.Windows.Forms.Button
    Friend WithEvents Panel2 As System.Windows.Forms.Panel
    Friend WithEvents contenido As System.Windows.Forms.Label
    Friend WithEvents valor As System.Windows.Forms.TextBox
End Class

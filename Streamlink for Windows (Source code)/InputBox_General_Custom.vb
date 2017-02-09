Public Class InputBox_General_Custom
    Dim cierre_fake As String = "no"
    Private Sub InputBox_General_Custom_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        If cierre_fake = "no" Then 'Cierre legitimo (Con cruz o alt+f4)
            valor.Text = ""
        End If

        valor.TabIndex = 0
        valor.Focus()
    End Sub

    Private Sub InputBox_General_Custom_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        valor.TabIndex = 0
        valor.Focus()

    End Sub

    Public Function AbrirInputBox(ByVal titulo_x As String, ByVal contenido_x As String, ByVal valor_x As String, ByVal Form_Origen As IWin32Window) As String
        'Establecer valores iniciales
        cierre_fake = "no"
        Me.Text = titulo_x
        contenido.Text = contenido_x
        valor.Text = valor_x
        valor.TabIndex = 0
        valor.Focus()
        valor.SelectionStart = valor.Text.Length
        valor.SelectionLength = 0
        '
        Me.ShowDialog(Form_Origen)
        Return (valor.Text)
    End Function

    Private Sub aceptar_Click(sender As Object, e As EventArgs) Handles aceptar.Click
        cierre_fake = "si"
        Me.Close()
    End Sub

    Private Sub valor_KeyPress(sender As Object, e As KeyPressEventArgs) Handles valor.KeyPress
        If e.KeyChar = Convert.ToChar(Keys.Enter) Then
            e.Handled = True
            cierre_fake = "si"
            Me.Close()
        End If
    End Sub

End Class

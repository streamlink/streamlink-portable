Public Class InputBoxDialog
    Dim ReturnValue As String = ""

    Private Sub ReturnButton_MouseUp(sender As Object, e As MouseButtonEventArgs) Handles ReturnButton.MouseUp
        ReturnValue = ValueTB.Text
        Me.Close()
    End Sub

    Private Sub ValueTB_KeyUp(sender As Object, e As KeyEventArgs) Handles ValueTB.KeyUp
        If e.Key = Key.Return Then
            ReturnButton_MouseUp(Nothing, Nothing)
        End If
    End Sub

    Public Shared Function OpenDialog(Title As String, Description As String, Owner As Window, Optional DefaultValue As String = "") As String
        Dim FinalValue As String = ""
        With New InputBoxDialog()
            'Dim i = BitmapImage.Create(1, 1, 1, 1, PixelFormats.Indexed1, New BitmapPalette(New List(Of Color) From {Colors.Transparent}), New Byte() {0, 0, 0, 0}, 1)
            '.Icon = i
            .WindowStyle = WindowStyle.ToolWindow
            .Owner = Owner
            .Title = Title
            .DescriptionTB.Text = Description
            .ValueTB.Text = DefaultValue
            .ValueTB.Focus()
            .ShowDialog()
            FinalValue = .ReturnValue
        End With
        Return FinalValue
    End Function
End Class

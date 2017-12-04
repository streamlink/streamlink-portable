Public Class MessageBoxDialog
    Dim ReturnAccepted As Boolean = False

    Private Sub OKButton_MouseUp(sender As Object, e As MouseButtonEventArgs) Handles OKButton.MouseUp
        ReturnAccepted = True
        Me.Close()
    End Sub

    Private Sub YesButton_MouseUp(sender As Object, e As MouseButtonEventArgs) Handles YesButton.MouseUp
        ReturnAccepted = True
        Me.Close()
    End Sub

    Private Sub NoButton_MouseUp(sender As Object, e As MouseButtonEventArgs) Handles NoButton.MouseUp
        ReturnAccepted = False
        Me.Close()
    End Sub

    Private Sub YesNoDialog_KeyUp(sender As Object, e As KeyEventArgs) Handles Me.KeyUp
        If e.Key = Key.Return Then
            Me.Close()
        End If
    End Sub

    Public Shared Function OpenDialog(Title As String, Description As String, Owner As Window, Optional Buttons As MessageBoxButton = MessageBoxButton.OK, Optional Icon As String = "info", Optional DefaultValue As Boolean = False) As Boolean
        Dim FinalValue As String = ""
        With New MessageBoxDialog
            .ReturnAccepted = DefaultValue
            .WindowStyle = WindowStyle.ToolWindow
            .Owner = Owner
            .Title = Title
            .DescriptionTB.Text = Description
            If Buttons = MessageBoxButton.YesNo Or Buttons = MessageBoxButton.YesNoCancel Then
                .SingleReturnGrid.Visibility = Visibility.Collapsed
                .DoubleReturnGrid.Visibility = Visibility.Visible
            End If
            If Buttons = MessageBoxButton.OKCancel Then
                .SingleReturnGrid.Visibility = Visibility.Collapsed
                .DoubleReturnGrid.Visibility = Visibility.Visible
                .YesButton.Text = "OK"
                .NoButton.Text = "Cancel"
            End If
            .IconTB.Text = Icon
            .ShowDialog()
            FinalValue = .ReturnAccepted
        End With
        Return FinalValue
    End Function
End Class

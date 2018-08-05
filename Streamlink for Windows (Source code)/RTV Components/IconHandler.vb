Public Class IconHandler
    Public Function HandleIcon(ByVal Destination As Object, ByVal NewIcon As String) As String
        If NewIcon.StartsWith("icofont-") Then
            Destination.FontFamily = New FontFamily(New Uri("pack://application:,,,/"), "./RTV Components/#IcoFont")
            Return New IcoFontTranslator().TranslateIcon(NewIcon)
        End If
        Destination.FontFamily = New FontFamily(New Uri("pack://application:,,,/"), "./RTV Components/#Material Icons")
        Return NewIcon
    End Function
End Class
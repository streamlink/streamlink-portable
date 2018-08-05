Public Class IconTextBlock
    'Icon Property
    Public Shared IconHandler As New IconHandler()
    Public Shared ReadOnly IconProperty As DependencyProperty = DependencyProperty.Register("Icon", GetType(String), GetType(IconTextBlock), New PropertyMetadata("", AddressOf IconChanged))
    Public Property Icon() As String
        Get
            Return DirectCast(GetValue(IconProperty), String)
        End Get
        Set
            SetValue(IconProperty, Value)
        End Set
    End Property
    Private Shared Sub IconChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
        Dim MeInvoke = DirectCast(d, IconTextBlock)
        MeInvoke.IconTextBlock.Text = IconHandler.HandleIcon(MeInvoke.IconTextBlock, e.NewValue)
    End Sub
    '

    Sub New()
        InitializeComponent()
        'Handle default properties values
        IconChanged(Me, New DependencyPropertyChangedEventArgs(IconProperty, Nothing, IconProperty.DefaultMetadata.DefaultValue))
        '
    End Sub

End Class

Public Class SimpleHintTextBox

    'TextChanged Event
    Public Event TextChanged As EventHandler
    Private Sub TextBox_TextChanged(sender As Object, e As TextChangedEventArgs)
        If sender.Text = "" Then
            HintText.Visibility = Visibility.Visible
        Else
            HintText.Visibility = Visibility.Hidden
        End If
        RaiseEvent TextChanged(sender, e)
    End Sub
    '

    'Text Property
    Public Shared ReadOnly TextProperty As DependencyProperty = DependencyProperty.Register("Text", GetType(String), GetType(SimpleHintTextBox), New PropertyMetadata("", AddressOf TextChangedFromProperty))
    Public Property Text() As String
        Get
            Return DirectCast(GetValue(TextProperty), String)
        End Get
        Set
            SetValue(TextProperty, Value)
        End Set
    End Property
    Private Shared Sub TextChangedFromProperty(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
        Dim MeInvoke = DirectCast(d, SimpleHintTextBox)
        MeInvoke.TextBox.Text = e.NewValue
    End Sub
    '

    'Hint Property
    Public Shared ReadOnly HintProperty As DependencyProperty = DependencyProperty.Register("Hint", GetType(String), GetType(SimpleHintTextBox), New PropertyMetadata("", AddressOf HintChanged))
    Public Property Hint() As String
        Get
            Return DirectCast(GetValue(HintProperty), String)
        End Get
        Set
            SetValue(HintProperty, Value)
        End Set
    End Property
    Private Shared Sub HintChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
        Dim MeInvoke = DirectCast(d, SimpleHintTextBox)
        MeInvoke.HintText.Text = e.NewValue
    End Sub
    '

    'MultiLine Property
    Public Shared ReadOnly MultiLineProperty As DependencyProperty = DependencyProperty.Register("MultiLine", GetType(Boolean), GetType(SimpleHintTextBox), New PropertyMetadata(False, AddressOf MultiLineChanged))
    Public Property MultiLine() As Boolean
        Get
            Return DirectCast(GetValue(MultiLineProperty), Boolean)
        End Get
        Set
            SetValue(MultiLineProperty, Value)
        End Set
    End Property
    Private Shared Sub MultiLineChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
        Dim MeInvoke = DirectCast(d, SimpleHintTextBox)
        If e.NewValue = True Then
            MeInvoke.TextBox.TextWrapping = TextWrapping.Wrap
            MeInvoke.TextBox.AcceptsReturn = True
            MeInvoke.TextBox.VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        Else
            MeInvoke.TextBox.TextWrapping = TextWrapping.NoWrap
            MeInvoke.TextBox.AcceptsReturn = False
            MeInvoke.TextBox.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled
        End If
    End Sub
    '

    'HintForeground Property
    Public Shared ReadOnly HintForegroundProperty As DependencyProperty = DependencyProperty.Register("HintForeground", GetType(Brush), GetType(SimpleHintTextBox), New PropertyMetadata(Brushes.Gray, AddressOf HintForegroundChanged))
    Public Property HintForeground() As Brush
        Get
            Return DirectCast(GetValue(HintForegroundProperty), Brush)
        End Get
        Set
            SetValue(HintForegroundProperty, Value)
        End Set
    End Property
    Private Shared Sub HintForegroundChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
        Dim MeInvoke = DirectCast(d, SimpleHintTextBox)
        MeInvoke.HintText.Foreground = e.NewValue
    End Sub
    '

    Public Sub New()
        InitializeComponent()
        'Handle events
        AddHandler TextBox.TextChanged, AddressOf TextBox_TextChanged
        '
        'Handle default properties values
        HintChanged(Me, New DependencyPropertyChangedEventArgs(HintProperty, Nothing, HintProperty.DefaultMetadata.DefaultValue))
        TextChangedFromProperty(Me, New DependencyPropertyChangedEventArgs(TextProperty, Nothing, TextProperty.DefaultMetadata.DefaultValue))
        MultiLineChanged(Me, New DependencyPropertyChangedEventArgs(MultiLineProperty, Nothing, MultiLineProperty.DefaultMetadata.DefaultValue))
        HintForegroundChanged(Me, New DependencyPropertyChangedEventArgs(HintForegroundProperty, Nothing, HintForegroundProperty.DefaultMetadata.DefaultValue))
        '
    End Sub

End Class

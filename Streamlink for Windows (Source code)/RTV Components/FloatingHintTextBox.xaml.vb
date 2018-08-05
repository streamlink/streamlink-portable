Public Class FloatingHintTextBox

    'TextChanged Event
    Public Event TextChanged As EventHandler
    Private Sub SimpleHintTextBox_TextChanged(sender As Object, e As TextChangedEventArgs)
        If sender.Text = "" Then
            HintText.Text = ""
            HintText.Visibility = Visibility.Collapsed
        Else
            HintText.Text = SimpleHintTextBox.Hint
            HintText.Visibility = Visibility.Visible
        End If
        RaiseEvent TextChanged(sender, e)
    End Sub
    '

    'Text Property
    Public Shared ReadOnly TextProperty As DependencyProperty = DependencyProperty.Register("Text", GetType(String), GetType(FloatingHintTextBox), New PropertyMetadata("", AddressOf TextChangedFromProperty))
    Public Property Text() As String
        Get
            Return DirectCast(GetValue(TextProperty), String)
        End Get
        Set
            SetValue(TextProperty, Value)
        End Set
    End Property
    Private Shared Sub TextChangedFromProperty(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
        Dim MeInvoke = DirectCast(d, FloatingHintTextBox)
        MeInvoke.SimpleHintTextBox.Text = e.NewValue
    End Sub
    '

    'Hint Property
    Public Shared ReadOnly HintProperty As DependencyProperty = DependencyProperty.Register("Hint", GetType(String), GetType(FloatingHintTextBox), New PropertyMetadata("", AddressOf HintChanged))
    Public Property Hint() As String
        Get
            Return DirectCast(GetValue(HintProperty), String)
        End Get
        Set
            SetValue(HintProperty, Value)
        End Set
    End Property
    Private Shared Sub HintChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
        Dim MeInvoke = DirectCast(d, FloatingHintTextBox)
        MeInvoke.SimpleHintTextBox.Hint = e.NewValue
    End Sub
    '

    'HintBackground Property
    Public Shared ReadOnly HintBackgroundProperty As DependencyProperty = DependencyProperty.Register("HintBackground", GetType(Brush), GetType(FloatingHintTextBox), New PropertyMetadata(Brushes.Transparent, AddressOf HintBackgroundChanged))
    Public Property HintBackground() As Brush
        Get
            Return DirectCast(GetValue(HintBackgroundProperty), Brush)
        End Get
        Set
            SetValue(HintBackgroundProperty, Value)
        End Set
    End Property
    Private Shared Sub HintBackgroundChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
        Dim MeInvoke = DirectCast(d, FloatingHintTextBox)
        MeInvoke.HintText.Background = e.NewValue
    End Sub
    '

    'HintForeground Property
    Public Shared ReadOnly HintForegroundProperty As DependencyProperty = DependencyProperty.Register("HintForeground", GetType(Brush), GetType(FloatingHintTextBox), New PropertyMetadata(Brushes.Gray, AddressOf HintForegroundChanged))
    Public Property HintForeground() As Brush
        Get
            Return DirectCast(GetValue(HintForegroundProperty), Brush)
        End Get
        Set
            SetValue(HintForegroundProperty, Value)
        End Set
    End Property
    Private Shared Sub HintForegroundChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
        Dim MeInvoke = DirectCast(d, FloatingHintTextBox)
        MeInvoke.SimpleHintTextBox.HintForeground = e.NewValue
        If MeInvoke.FloatingHintForeground Is Nothing Then
            MeInvoke.HintText.Foreground = MeInvoke.SimpleHintTextBox.HintForeground
        End If
    End Sub
    '

    'FloatingHintForeground Property
    Public Shared ReadOnly FloatingHintForegroundProperty As DependencyProperty = DependencyProperty.Register("FloatingHintForeground", GetType(Brush), GetType(FloatingHintTextBox), New PropertyMetadata(Brushes.Black, AddressOf FloatingHintForegroundChanged))
    Public Property FloatingHintForeground() As Brush
        Get
            Return DirectCast(GetValue(FloatingHintForegroundProperty), Brush)
        End Get
        Set
            SetValue(FloatingHintForegroundProperty, Value)
        End Set
    End Property
    Private Shared Sub FloatingHintForegroundChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
        Dim MeInvoke = DirectCast(d, FloatingHintTextBox)
        If e.NewValue Is Nothing Then
            MeInvoke.HintText.Foreground = MeInvoke.SimpleHintTextBox.HintForeground
        Else
            MeInvoke.HintText.Foreground = e.NewValue
        End If
    End Sub
    '

    'TextBoxBackground Property
    Public Shared ReadOnly TextBoxBackgroundProperty As DependencyProperty = DependencyProperty.Register("TextBoxBackground", GetType(Brush), GetType(FloatingHintTextBox), New PropertyMetadata(Brushes.Transparent, AddressOf TextBoxBackgroundChanged))
    Public Property TextBoxBackground() As Brush
        Get
            Return DirectCast(GetValue(TextBoxBackgroundProperty), Brush)
        End Get
        Set
            SetValue(TextBoxBackgroundProperty, Value)
        End Set
    End Property
    Private Shared Sub TextBoxBackgroundChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
        Dim MeInvoke = DirectCast(d, FloatingHintTextBox)
        MeInvoke.SimpleHintTextBox.Background = e.NewValue
    End Sub
    '

    'MultiLine Property
    Public Shared ReadOnly MultiLineProperty As DependencyProperty = DependencyProperty.Register("MultiLine", GetType(Boolean), GetType(FloatingHintTextBox), New PropertyMetadata(False, AddressOf MultiLineChanged))
    Public Property MultiLine() As Boolean
        Get
            Return DirectCast(GetValue(MultiLineProperty), Boolean)
        End Get
        Set
            SetValue(MultiLineProperty, Value)
        End Set
    End Property
    Private Shared Sub MultiLineChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
        Dim MeInvoke = DirectCast(d, FloatingHintTextBox)
        MeInvoke.SimpleHintTextBox.MultiLine = e.NewValue
    End Sub
    '

    Public Sub New()
        InitializeComponent()
        'Handle events
        AddHandler SimpleHintTextBox.TextChanged, AddressOf SimpleHintTextBox_TextChanged
        '

        'Handle default properties values
        HintChanged(Me, New DependencyPropertyChangedEventArgs(HintProperty, Nothing, HintProperty.DefaultMetadata.DefaultValue))
        HintBackgroundChanged(Me, New DependencyPropertyChangedEventArgs(HintBackgroundProperty, Nothing, HintBackgroundProperty.DefaultMetadata.DefaultValue))
        HintForegroundChanged(Me, New DependencyPropertyChangedEventArgs(HintForegroundProperty, Nothing, HintForegroundProperty.DefaultMetadata.DefaultValue))
        FloatingHintForegroundChanged(Me, New DependencyPropertyChangedEventArgs(FloatingHintForegroundProperty, Nothing, FloatingHintForegroundProperty.DefaultMetadata.DefaultValue))
        TextBoxBackgroundChanged(Me, New DependencyPropertyChangedEventArgs(TextBoxBackgroundProperty, Nothing, TextBoxBackgroundProperty.DefaultMetadata.DefaultValue))
        TextChangedFromProperty(Me, New DependencyPropertyChangedEventArgs(TextProperty, Nothing, TextProperty.DefaultMetadata.DefaultValue))
        MultiLineChanged(Me, New DependencyPropertyChangedEventArgs(MultiLineProperty, Nothing, MultiLineProperty.DefaultMetadata.DefaultValue))
        '

    End Sub

End Class

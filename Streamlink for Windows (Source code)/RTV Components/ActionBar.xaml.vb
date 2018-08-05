Public Class ActionBar
    'TextChanged Event
    Public Event TextChanged As EventHandler
    Private Sub TextBox_TextChanged(sender As Object, e As TextChangedEventArgs)
        RaiseEvent TextChanged(sender, e)
    End Sub
    '

    'Text Property
    Public Shared ReadOnly TextProperty As DependencyProperty = DependencyProperty.Register("Text", GetType(String), GetType(ActionBar), New PropertyMetadata("", AddressOf TextChangedFromProperty))
    Public Property Text() As String
        Get
            Return DirectCast(GetValue(TextProperty), String)
        End Get
        Set
            SetValue(TextProperty, Value)
        End Set
    End Property
    Private Shared Sub TextChangedFromProperty(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
        Dim MeInvoke = DirectCast(d, ActionBar)
        MeInvoke.SearchTextBox.Text = e.NewValue
    End Sub
    '

    'Hint Property
    Public Shared ReadOnly HintProperty As DependencyProperty = DependencyProperty.Register("Hint", GetType(String), GetType(ActionBar), New PropertyMetadata("", AddressOf HintChanged))
    Public Property Hint() As String
        Get
            Return DirectCast(GetValue(HintProperty), String)
        End Get
        Set
            SetValue(HintProperty, Value)
        End Set
    End Property
    Private Shared Sub HintChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
        Dim MeInvoke = DirectCast(d, ActionBar)
        MeInvoke.SearchTextBox.Hint = e.NewValue
    End Sub
    '

    Public Sub New()
        InitializeComponent()

        'Handle events
        AddHandler SearchButton.MouseDown, AddressOf HandleSearchButtonPressed
        AddHandler SearchTextBox.TextChanged, AddressOf TextBox_TextChanged
        '
        'Handle default properties values
        HintChanged(Me, New DependencyPropertyChangedEventArgs(HintProperty, Nothing, HintProperty.DefaultMetadata.DefaultValue))
        TextChangedFromProperty(Me, New DependencyPropertyChangedEventArgs(TextProperty, Nothing, TextProperty.DefaultMetadata.DefaultValue))
        '

    End Sub

    Public Event SearchButtonPressed As EventHandler

    Private Sub HandleSearchButtonPressed(sender As Object, e As EventArgs)
        If SearchTextBox.TextBox.IsFocused Then
            ControlGrid.FocusVisualStyle = Nothing
            ControlGrid.Focusable = True
            ControlGrid.Focus()
        End If
        RaiseEvent SearchButtonPressed(sender, e)
    End Sub

    Private Sub SearchTextBox_KeyUp(sender As Object, e As KeyEventArgs) Handles SearchTextBox.KeyUp
        If e.Key = Key.Return Then
            HandleSearchButtonPressed(Nothing, Nothing)
        End If
    End Sub

End Class
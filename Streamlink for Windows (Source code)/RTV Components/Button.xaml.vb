
Public Class Button
    'MouseInteractionStyle Property
    Public Shared ReadOnly MouseInteractionStyleHandler As New MouseInteractionStyle
    Public Shared ReadOnly MouseInteractionStyleProperty As DependencyProperty = DependencyProperty.Register("MouseInteractionStyle", GetType(Boolean), GetType(Button), New PropertyMetadata(False, AddressOf MouseInteractionStyleChanged))
    Public Property MouseInteractionStyle() As Boolean
        Get
            Return DirectCast(GetValue(MouseInteractionStyleProperty), Boolean)
        End Get
        Set
            SetValue(MouseInteractionStyleProperty, Value)
        End Set
    End Property
    Private Shared Sub MouseInteractionStyleChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
        Dim MeInvoke = DirectCast(d, Button)
        If e.NewValue = True Then
            MouseInteractionStyleHandler.AddStyle(MeInvoke.MainGrid)
        Else
            MouseInteractionStyleHandler.RemoveStyle(MeInvoke.MainGrid)
        End If
    End Sub
    '
    'Text Property
    Public Shared ReadOnly TextProperty As DependencyProperty = DependencyProperty.Register("Text", GetType(String), GetType(Button), New PropertyMetadata("", AddressOf TextChanged))
    Public Property Text() As String
        Get
            Return DirectCast(GetValue(TextProperty), String)
        End Get
        Set
            SetValue(TextProperty, Value)
        End Set
    End Property
    Private Shared Sub TextChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
        Dim MeInvoke = DirectCast(d, Button)
        MeInvoke.LabelTextBlock.Text = e.NewValue
        If String.IsNullOrWhiteSpace(e.NewValue) Then
            MeInvoke.LabelTextBlock.Visibility = Visibility.Collapsed
        Else
            MeInvoke.LabelTextBlock.Visibility = Visibility.Visible
        End If
    End Sub
    '
    'Icon Property
    Public Shared IconHandler As New IconHandler()
    Public Shared ReadOnly IconProperty As DependencyProperty = DependencyProperty.Register("Icon", GetType(String), GetType(Button), New PropertyMetadata("", AddressOf IconChanged))
    Public Property Icon() As String
        Get
            Return DirectCast(GetValue(IconProperty), String)
        End Get
        Set
            SetValue(IconProperty, Value)
        End Set
    End Property
    Private Shared Sub IconChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
        Dim MeInvoke = DirectCast(d, Button)
        MeInvoke.IconTextBlock.Icon = e.NewValue
        If String.IsNullOrWhiteSpace(e.NewValue) Then
            MeInvoke.IconTextBlock.Visibility = Visibility.Collapsed
        Else
            MeInvoke.IconTextBlock.Visibility = Visibility.Visible
        End If
    End Sub
    '
    'CenterContent Property
    Public Shared ReadOnly CenterContentProperty As DependencyProperty = DependencyProperty.Register("CenterContent", GetType(Boolean), GetType(Button), New PropertyMetadata(False, AddressOf CenterContentChanged))
    Public Property CenterContent() As Boolean
        Get
            Return DirectCast(GetValue(CenterContentProperty), Boolean)
        End Get
        Set
            SetValue(CenterContentProperty, Value)
        End Set
    End Property
    Private Shared Sub CenterContentChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
        Dim MeInvoke = DirectCast(d, Button)
        If e.NewValue = True Then
            MeInvoke.ChildGrid.HorizontalAlignment = HorizontalAlignment.Center
        Else
            MeInvoke.ChildGrid.HorizontalAlignment = HorizontalAlignment.Stretch
        End If
    End Sub
    '

    Sub New()
        InitializeComponent()
        'Handle default properties values
        MouseInteractionStyleChanged(Me, New DependencyPropertyChangedEventArgs(MouseInteractionStyleProperty, Nothing, MouseInteractionStyleProperty.DefaultMetadata.DefaultValue))
        TextChanged(Me, New DependencyPropertyChangedEventArgs(TextProperty, Nothing, TextProperty.DefaultMetadata.DefaultValue))
        IconChanged(Me, New DependencyPropertyChangedEventArgs(IconProperty, Nothing, IconProperty.DefaultMetadata.DefaultValue))
        CenterContentChanged(Me, New DependencyPropertyChangedEventArgs(CenterContentProperty, Nothing, CenterContentProperty.DefaultMetadata.DefaultValue))
        '
    End Sub

End Class
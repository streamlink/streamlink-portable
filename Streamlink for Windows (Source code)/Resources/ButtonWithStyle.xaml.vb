Public Class ButtonWithStyle
    'MouseInteractionStyle Property
    Public Shared ReadOnly MouseInteractionStyleHandler As New MouseInteractionStyle
    Public Shared ReadOnly MouseInteractionStyleProperty As DependencyProperty = DependencyProperty.Register("MouseInteractionStyle", GetType(Boolean), GetType(ButtonWithStyle), New PropertyMetadata(False, AddressOf MouseInteractionStyleChanged))
    Public Property MouseInteractionStyle() As Boolean
        Get
            Return DirectCast(GetValue(MouseInteractionStyleProperty), Boolean)
        End Get
        Set
            SetValue(MouseInteractionStyleProperty, Value)
        End Set
    End Property
    Private Shared Sub MouseInteractionStyleChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
        Dim MeInvoke = DirectCast(d, ButtonWithStyle)
        If e.NewValue = True Then
            MouseInteractionStyleHandler.AddStyle(MeInvoke)
        Else
            MouseInteractionStyleHandler.RemoveStyle(MeInvoke)
        End If
    End Sub
    '
    'Text Property
    Public Shared ReadOnly TextProperty As DependencyProperty = DependencyProperty.Register("Text", GetType(String), GetType(ButtonWithStyle), New PropertyMetadata("", AddressOf TextChanged))
    Public Property Text() As String
        Get
            Return DirectCast(GetValue(TextProperty), String)
        End Get
        Set
            SetValue(TextProperty, Value)
        End Set
    End Property
    Private Shared Sub TextChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
        Dim MeInvoke = DirectCast(d, ButtonWithStyle)
        MeInvoke.LabelTextBlock.Text = e.NewValue
        If String.IsNullOrWhiteSpace(e.NewValue) Then
            MeInvoke.TextColumn.Width = New GridLength(0)
        Else
            MeInvoke.TextColumn.Width = New GridLength(1, GridUnitType.Star)
        End If
    End Sub
    '
    'Icon Property
    Public Shared ReadOnly IconProperty As DependencyProperty = DependencyProperty.Register("Icon", GetType(String), GetType(ButtonWithStyle), New PropertyMetadata("", AddressOf IconChanged))
    Public Property Icon() As String
        Get
            Return DirectCast(GetValue(IconProperty), String)
        End Get
        Set
            SetValue(IconProperty, Value)
        End Set
    End Property
    Private Shared Sub IconChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
        Dim MeInvoke = DirectCast(d, ButtonWithStyle)
        MeInvoke.IconTextBlock.Text = e.NewValue
        If String.IsNullOrWhiteSpace(e.NewValue) Then
            MeInvoke.IconColumn.Width = New GridLength(0)
        Else
            MeInvoke.IconColumn.Width = New GridLength(50)
        End If
    End Sub
    '
End Class

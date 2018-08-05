Imports System.Windows.Media.Animation

Public Class SpinnerIcon
    'MouseInteractionStyle Property
    Public Shared ReadOnly MouseInteractionStyleHandler As New MouseInteractionStyle
    Public Shared ReadOnly MouseInteractionStyleProperty As DependencyProperty = DependencyProperty.Register("MouseInteractionStyle", GetType(Boolean), GetType(SpinnerIcon), New PropertyMetadata(False, AddressOf MouseInteractionStyleChanged))
    Public Property MouseInteractionStyle() As Boolean
        Get
            Return DirectCast(GetValue(MouseInteractionStyleProperty), Boolean)
        End Get
        Set
            SetValue(MouseInteractionStyleProperty, Value)
        End Set
    End Property
    Private Shared Sub MouseInteractionStyleChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
        Dim MeInvoke = DirectCast(d, SpinnerIcon)
        If e.NewValue = True Then
            MouseInteractionStyleHandler.AddStyle(MeInvoke)
        Else
            MouseInteractionStyleHandler.RemoveStyle(MeInvoke)
        End If
    End Sub
    '
    'Icon Property
    Public Shared ReadOnly IconProperty As DependencyProperty = DependencyProperty.Register("Icon", GetType(String), GetType(SpinnerIcon), New PropertyMetadata("", AddressOf IconChanged))
    Public Property Icon() As String
        Get
            Return DirectCast(GetValue(IconProperty), String)
        End Get
        Set
            SetValue(IconProperty, Value)
        End Set
    End Property
    Private Shared Sub IconChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
        Dim MeInvoke = DirectCast(d, SpinnerIcon)
        MeInvoke.SpinnerTextBlock.Icon = e.NewValue
    End Sub
    '
    'Spin Property
    Public Shared ReadOnly SpinProperty As DependencyProperty = DependencyProperty.Register("Spin", GetType(Boolean), GetType(SpinnerIcon), New PropertyMetadata(False, AddressOf SpinChanged))
    Public Property Spin() As Boolean
        Get
            Return DirectCast(GetValue(SpinProperty), Boolean)
        End Get
        Set
            SetValue(SpinProperty, Value)
        End Set
    End Property
    Private Shared Sub SpinChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
        Dim MeInvoke = DirectCast(d, SpinnerIcon)
        Dim SpinnerStoryBoard As Storyboard = DirectCast(MeInvoke.Resources("SpinnerStoryBoard"), Storyboard)
        If e.NewValue = True Then
            SpinnerStoryBoard.Begin()
        Else
            SpinnerStoryBoard.[Stop]()
        End If
    End Sub
    '

    Sub New()
        InitializeComponent()
        'Handle default properties values
        MouseInteractionStyleChanged(Me, New DependencyPropertyChangedEventArgs(MouseInteractionStyleProperty, Nothing, MouseInteractionStyleProperty.DefaultMetadata.DefaultValue))
        IconChanged(Me, New DependencyPropertyChangedEventArgs(IconProperty, Nothing, IconProperty.DefaultMetadata.DefaultValue))
        SpinChanged(Me, New DependencyPropertyChangedEventArgs(SpinProperty, Nothing, SpinProperty.DefaultMetadata.DefaultValue))
        '
    End Sub

End Class
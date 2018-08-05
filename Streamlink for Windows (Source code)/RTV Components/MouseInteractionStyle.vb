Imports System.Windows.Threading

Public Class MouseInteractionStyle

    Dim GlowEffects As Boolean = True
    Dim ChildWindowPartialFix As Boolean = True 'Partial fix for MouseLeave event in child window (WPF bug)
    Dim WithEvents MouseLeaveTimer As New DispatcherTimer
    Dim LatestMouseEnterObject As Object
    '

    Function AddStyle(ByVal Destination As UIElement)
        RemoveStyle(Destination)
        AddHandler Destination.MouseEnter, AddressOf MouseInteractionStyle_MouseEnter
        AddHandler Destination.MouseLeave, AddressOf MouseInteractionStyle_MouseLeave
        AddHandler Destination.MouseDown, AddressOf MouseInteractionStyle_MouseDown
        AddHandler Destination.MouseUp, AddressOf MouseInteractionStyle_MouseUp
        AddHandler Destination.MouseMove, AddressOf MouseInteractionStyle_MouseMove
    End Function

    Function RemoveStyle(ByVal Destination As UIElement)
        RemoveHandler Destination.MouseEnter, AddressOf MouseInteractionStyle_MouseEnter
        RemoveHandler Destination.MouseLeave, AddressOf MouseInteractionStyle_MouseLeave
        RemoveHandler Destination.MouseDown, AddressOf MouseInteractionStyle_MouseDown
        RemoveHandler Destination.MouseUp, AddressOf MouseInteractionStyle_MouseUp
        RemoveHandler Destination.MouseMove, AddressOf MouseInteractionStyle_MouseMove
        MouseInteractionStyle_MouseLeave(Destination, Nothing)
    End Function

    Sub MouseInteractionStyle_MouseMove(sender As Object, e As MouseEventArgs)
        If ChildWindowPartialFix = True Then
            If LatestMouseEnterObject Is Nothing Then
                StartMouseLeaveTimer(sender)
            End If
        End If
        If GlowEffects = False Then
            MouseInteractionStyle_MouseEnter(sender, e)
            RemoveHandler DirectCast(sender, UIElement).MouseMove, AddressOf MouseInteractionStyle_MouseMove
            Return
        End If
        Dim RelativeMousePosition As Point = e.GetPosition(sender)
        Dim GlowEffectsGradientBrush As New RadialGradientBrush()
        GlowEffectsGradientBrush.GradientOrigin = New Point((RelativeMousePosition.X / sender.ActualWidth), (RelativeMousePosition.Y / sender.ActualHeight))
        GlowEffectsGradientBrush.Center = New Point((RelativeMousePosition.X / sender.ActualWidth), (RelativeMousePosition.Y / sender.ActualHeight))
        GlowEffectsGradientBrush.RadiusX = 0.5 'Change horizontal glow (0.1 to 1)
        GlowEffectsGradientBrush.RadiusY = 1 'Change vertical glow (0.1 to 1)
        GlowEffectsGradientBrush.GradientStops.Add(New GradientStop(Colors.Black, 1))
        If e.LeftButton = MouseButtonState.Pressed Then
            GlowEffectsGradientBrush.Opacity = 0.24
            GlowEffectsGradientBrush.GradientStops.Add(New GradientStop(Color.FromArgb(25, 0, 0, 0), 0.2))
        Else
            GlowEffectsGradientBrush.Opacity = 0.12
            GlowEffectsGradientBrush.GradientStops.Add(New GradientStop(Color.FromArgb(50, 0, 0, 0), 0.2))
        End If
        sender.Background = GlowEffectsGradientBrush
    End Sub

    Sub MouseInteractionStyle_MouseEnter(sender As Object, e As MouseEventArgs)
        If ChildWindowPartialFix = True Then
            StartMouseLeaveTimer(sender)
        End If

        If e.LeftButton = MouseButtonState.Pressed Then
            MouseInteractionStyle_MouseDown(sender, e)
        Else
            sender.Background = New SolidColorBrush(Color.FromArgb(30, 0, 0, 0))
        End If
    End Sub
    Sub MouseInteractionStyle_MouseLeave(sender As Object, e As MouseEventArgs)
        LatestMouseEnterObject = Nothing
        MouseLeaveTimer.Stop()
        sender.Background = Brushes.Transparent
    End Sub
    Sub MouseInteractionStyle_MouseDown(sender As Object, e As MouseEventArgs)
        If GlowEffects = True Then
            MouseInteractionStyle_MouseMove(sender, e)
        Else
            sender.Background = New SolidColorBrush(Color.FromArgb(60, 0, 0, 0))
        End If
    End Sub
    Sub MouseInteractionStyle_MouseUp(sender As Object, e As MouseEventArgs)
        If sender.Background Is Brushes.Transparent = False Then
            MouseInteractionStyle_MouseEnter(sender, e)
        End If
    End Sub

    Sub StartMouseLeaveTimer(sender As Object)
        If Window.GetWindow(sender).Owner IsNot Nothing Then
            LatestMouseEnterObject = sender
            MouseLeaveTimer.Interval = New TimeSpan(0, 0, 1)
            MouseLeaveTimer.Start()
        End If
    End Sub

    Private Sub MouseLeaveTimer_Tick(sender As Object, e As EventArgs) Handles MouseLeaveTimer.Tick
        Try
            Dim CurrentMousePosition = GetMousePosition()
            Dim RelativeMousePosition = LatestMouseEnterObject.PointFromScreen(CurrentMousePosition)
            If (RelativeMousePosition.X >= 0) And (RelativeMousePosition.Y >= 0) Then
                If (RelativeMousePosition.X <= LatestMouseEnterObject.ActualWidth) And (RelativeMousePosition.Y <= LatestMouseEnterObject.ActualHeight) Then
                    Return
                End If
            End If
            MouseInteractionStyle_MouseLeave(LatestMouseEnterObject, New MouseEventArgs(Mouse.PrimaryDevice, 0))
        Catch
            If LatestMouseEnterObject Is Nothing Then
                MouseLeaveTimer.Stop()
            End If
        End Try
    End Sub

    'Get global mouse position
    Friend Declare Function GetCursorPos Lib "user32.dll" (ByRef pt As Win32Point) As Boolean
    Structure Win32Point
        Public X As Int32
        Public Y As Int32
    End Structure
    Public Shared Function GetMousePosition() As Point
        Dim w32Mouse As Win32Point = New Win32Point
        GetCursorPos(w32Mouse)
        Return New Point(w32Mouse.X, w32Mouse.Y)
    End Function
    '

End Class
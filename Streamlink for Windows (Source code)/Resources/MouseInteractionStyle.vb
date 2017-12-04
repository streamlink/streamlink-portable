Public Class MouseInteractionStyle

    Dim GlowEffects As Boolean = True

    Public Function AddStyle(ByVal Destination As UIElement)
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
        If e.LeftButton = MouseButtonState.Pressed Then
            MouseInteractionStyle_MouseDown(sender, e)
        Else
            sender.Background = New SolidColorBrush(Color.FromArgb(30, 0, 0, 0))
        End If
    End Sub
    Sub MouseInteractionStyle_MouseLeave(sender As Object, e As MouseEventArgs)
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
        MouseInteractionStyle_MouseEnter(sender, e)
    End Sub
End Class

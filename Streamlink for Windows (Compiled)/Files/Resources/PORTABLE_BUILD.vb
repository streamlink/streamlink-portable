Imports System.Reflection
Imports System.Deployment
Imports System.Text
Imports System.Threading
Imports System.IO

<Assembly: AssemblyTitle("Streamlink for Windows")>
<Assembly: AssemblyDescription("")>
<Assembly: AssemblyCompany("Streamlink")>
<Assembly: AssemblyProduct("Streamlink for Windows")>
<Assembly: AssemblyCopyright("Streamlink")>
<Assembly: AssemblyTrademark("Streamlink for Windows")>
<Assembly: AssemblyVersion("1.0.0.0")>
<Assembly: AssemblyFileVersion("1.0.0.0")>

Module Module1

    Public Current_EXE_Path As String = System.Reflection.Assembly.GetExecutingAssembly().CodeBase
    Public ENABLE_MAINPROGRAM_EXIT As Boolean = True
    Sub Main()
        On Error Resume Next

        AddHandler Console.CancelKeyPress, AddressOf Console_CancelKeyPress
        Current_EXE_Path = Current_EXE_Path.Replace("file:///", "")
        Current_EXE_Path = Current_EXE_Path.Replace("file:\", "")
        Current_EXE_Path = System.IO.Path.GetDirectoryName(Current_EXE_Path)
        Current_EXE_Path = Current_EXE_Path.Replace("file:///", "")
        Current_EXE_Path = Current_EXE_Path.Replace("file:\", "")

        Dim Path_Detection As String = Current_EXE_Path.Remove(3)
        Path_Detection = Path_Detection.Remove(0, 1)
        Path_Detection = Path_Detection.Replace("/", "\")
        If Path_Detection = ":\" = False And Current_EXE_Path.StartsWith("\\") = False Then
            If Current_EXE_Path.StartsWith("\") Then
                Current_EXE_Path = "\" & Current_EXE_Path
            Else
                Current_EXE_Path = "\\" & Current_EXE_Path
            End If
        End If

        Console.OutputEncoding = Encoding.UTF8
        Console.InputEncoding = Encoding.UTF8
        Dim bufSize As Integer = 4096
        Dim inStream As Stream = Console.OpenStandardInput(bufSize)
        Console.SetIn(New StreamReader(inStream, Console.InputEncoding, False, bufSize))

        Dim Final_Args As String = Environment.CommandLine
        If Final_Args.StartsWith(Chr(34)) Then
            Final_Args = Final_Args.Remove(0, 1)
            Final_Args = Final_Args.Remove(0, Final_Args.IndexOf(Chr(34)))
            If Final_Args.Contains(" ") Then
                Final_Args = Final_Args.Remove(0, Final_Args.IndexOf(" "))
            Else
                Final_Args = ""
            End If
        Else
            If Final_Args.Contains(" ") Then
                Final_Args = Final_Args.Remove(0, Final_Args.IndexOf(" "))
            Else
                Final_Args = ""
            End If
        End If

        Dim CMD_INVOKED As Boolean = True
        If String.IsNullOrWhiteSpace(Final_Args) Then
            CMD_INVOKED = False
            Console.Title = "Streamlink for Windows"
            Console.Clear()
            Console.WriteLine("Welcome to Streamlink for Windows")
            Console.WriteLine("Type a valid commmand:")
            Console.WriteLine("")
            Final_Args = Console.ReadLine()
            Console.Clear()
        End If

        If IO.File.Exists(Current_EXE_Path & "\Streamlink\streamlink_cli\constants.py") Then
            Dim TextBox_Analysis_StreamlinkConstants As New TextBox
            Dim StreamlinkConstants_Final_Modded As String = ""
            TextBox_Analysis_StreamlinkConstants.Text = IO.File.ReadAllText(Current_EXE_Path & "\Streamlink\streamlink_cli\constants.py", Encoding.UTF8)

            Dim WIN32_CHK_READY As Boolean = False

            For i_Line_Analysis_StreamlinkConstants As Integer = 0 To TextBox_Analysis_StreamlinkConstants.Lines.Count - 1
                Dim Line_Analysis_StreamlinkConstants As String = TextBox_Analysis_StreamlinkConstants.Lines(i_Line_Analysis_StreamlinkConstants)
                If String.IsNullOrWhiteSpace(Line_Analysis_StreamlinkConstants) = False Then

                    If Line_Analysis_StreamlinkConstants.Contains("if is_win32:") Then
                        WIN32_CHK_READY = True
                    End If
                    If WIN32_CHK_READY = True And Line_Analysis_StreamlinkConstants.Contains("APPDATA =") Then
                        Line_Analysis_StreamlinkConstants = Line_Analysis_StreamlinkConstants.Remove(Line_Analysis_StreamlinkConstants.IndexOf("APPDATA ="))
                        Line_Analysis_StreamlinkConstants += "APPDATA = os.path.normpath(" & chr(34) & Current_EXE_Path.Replace("\", "/") & chr(34) & ")"
                    End If
                    If WIN32_CHK_READY = True And Line_Analysis_StreamlinkConstants.Contains("CONFIG_FILES =") Then
                        Line_Analysis_StreamlinkConstants = Line_Analysis_StreamlinkConstants.Remove(Line_Analysis_StreamlinkConstants.IndexOf("CONFIG_FILES ="))
                        Line_Analysis_StreamlinkConstants += "CONFIG_FILES = [os.path.join(APPDATA, " & chr(34) & "streamlinkrc" & chr(34) & ")]"
                    End If
                    If WIN32_CHK_READY = True And Line_Analysis_StreamlinkConstants.Contains("PLUGINS_DIR =") Then
                        Line_Analysis_StreamlinkConstants = Line_Analysis_StreamlinkConstants.Remove(Line_Analysis_StreamlinkConstants.IndexOf("PLUGINS_DIR ="))
                        Line_Analysis_StreamlinkConstants += "PLUGINS_DIR = os.path.join(APPDATA, " & chr(34) & "plugins" & chr(34) & ")"
                    End If
                    If WIN32_CHK_READY = True And Line_Analysis_StreamlinkConstants.Contains("else:") Then
                        WIN32_CHK_READY = False
                    End If

                End If
                StreamlinkConstants_Final_Modded += Line_Analysis_StreamlinkConstants & vbNewLine
            Next
            TextBox_Analysis_StreamlinkConstants.Dispose()
            IO.File.WriteAllText(Current_EXE_Path & "\Streamlink\streamlink_cli\constants.py", StreamlinkConstants_Final_Modded, Encoding.UTF8)
        End If

        Dim ACTUAL_VER_DATA As String = Current_EXE_Path & "\VERSION.txt"
        If IO.File.Exists(ACTUAL_VER_DATA) Then
            ACTUAL_VER_DATA = IO.File.ReadAllText(ACTUAL_VER_DATA, Encoding.UTF8)
            Console.WriteLine("[Streamlink for Windows " & ACTUAL_VER_DATA & "]")
        Else
            Console.WriteLine("[Streamlink for Windows]")
        End If
        Dim info = New ProcessStartInfo(Chr(34) & Current_EXE_Path & "\Python 3.5.2\python.exe" & Chr(34), Chr(34) & Current_EXE_Path & "\Streamlink\Streamlink.py" & Chr(34) & " --config " & Chr(34) & Current_EXE_Path & "\streamlinkrc" & Chr(34) & " --rtmp-rtmpdump " & Chr(34) & Current_EXE_Path & "\Streamlink\Dependencies\rtmpdump\rtmpdump.exe" & Chr(34) & " --ffmpeg-ffmpeg " & Chr(34) & Current_EXE_Path & "\Streamlink\Dependencies\ffmpeg\ffmpeg.exe" & Chr(34) & " " & Final_Args)
        info.UseShellExecute = False
        ENABLE_MAINPROGRAM_EXIT = False
        Dim proc = Process.Start(info)
        proc.WaitForExit()
        ENABLE_MAINPROGRAM_EXIT = True
        Console.WriteLine("[End of Streamlink for Windows with ExitCode " & proc.ExitCode & "]")

        If CMD_INVOKED = False Then
            Console.ReadKey()
        End If

        Environment.Exit(proc.ExitCode)

    End Sub

    Sub Console_CancelKeyPress(sender As Object, e As ConsoleCancelEventArgs)
        If ENABLE_MAINPROGRAM_EXIT = False Then
            e.Cancel = True
        Else
            Process.GetCurrentProcess.Kill()
        End If
    End Sub

End Module

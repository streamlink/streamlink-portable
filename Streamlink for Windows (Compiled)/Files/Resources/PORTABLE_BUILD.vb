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
    Dim CMD_INVOKED As Boolean = True
    Public NO_CMD_HINTS As Boolean = False
    Public LAST_EXIT_CODE As Integer = 0

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

        If IO.File.Exists(Current_EXE_Path & "\NO_CMD_HINTS") Then
            NO_CMD_HINTS = True
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

        If NO_CMD_HINTS = False Then
            If String.IsNullOrWhiteSpace(Final_Args) Then
                CMD_INVOKED = False
                Console.Title = "Streamlink for Windows"
                Console.Clear()
                Console.WriteLine("Welcome to Streamlink for Windows")
                Console.WriteLine("Type a valid commmand:")
                Console.WriteLine("")
                Final_Args = Console.ReadLine()
            End If
        End If

        LaunchStreamlink(Final_Args)

        If NO_CMD_HINTS = False And CMD_INVOKED = False Then
            Dim EXTRA_ARGS As String = ""

            Do Until EXTRA_ARGS.ToUpper = "EXIT"
                Console.WriteLine("")
                Console.WriteLine("Type another command or EXIT to end:")
                Console.WriteLine("")
                EXTRA_ARGS = Console.ReadLine()
                If EXTRA_ARGS.ToUpper = "EXIT" Then
                    Environment.Exit(LAST_EXIT_CODE)
                Else
                    LaunchStreamlink(EXTRA_ARGS)
                End If
            Loop
        Else
            Environment.Exit(LAST_EXIT_CODE)
        End If

    End Sub

    Function LaunchStreamlink(ByVal InputCommand As String)

        If CMD_INVOKED = False Then
            Console.Clear()
        End If

        Try
            If NO_CMD_HINTS = False Then
                Dim ACTUAL_VER_DATA As String = Current_EXE_Path & "\VERSION.txt"
                If IO.File.Exists(ACTUAL_VER_DATA) Then
                    ACTUAL_VER_DATA = IO.File.ReadAllText(ACTUAL_VER_DATA, Encoding.UTF8)
                    Console.WriteLine("[Streamlink for Windows " & ACTUAL_VER_DATA & "]")
                Else
                    Console.WriteLine("[Streamlink for Windows]")
                End If
            End If

            Dim STREAMLINK_INVOKE_CHK = {"STREAMLINK ", "STREAMLINK.EXE ", "STREAMLINK.BAT "}
            For Each STREAMLINK_INVOKE_CHK_ As String In STREAMLINK_INVOKE_CHK
                If InputCommand.ToUpper.StartsWith(STREAMLINK_INVOKE_CHK_) Then
                    InputCommand = InputCommand.Remove(0, STREAMLINK_INVOKE_CHK_.Length)
                End If
            Next

            Dim info = New ProcessStartInfo(Chr(34) & Current_EXE_Path & "\Python 3.5.2\python.exe" & Chr(34), Chr(34) & Current_EXE_Path & "\Streamlink\Streamlink.py" & Chr(34) & " --config " & Chr(34) & Current_EXE_Path & "\streamlinkrc" & Chr(34) & " --rtmp-rtmpdump " & Chr(34) & Current_EXE_Path & "\Streamlink\Dependencies\rtmpdump\rtmpdump.exe" & Chr(34) & " --ffmpeg-ffmpeg " & Chr(34) & Current_EXE_Path & "\Streamlink\Dependencies\ffmpeg\ffmpeg.exe" & Chr(34) & " " & InputCommand)
            info.UseShellExecute = False
            ENABLE_MAINPROGRAM_EXIT = False
            Dim proc = Process.Start(info)
            proc.WaitForExit()
            ENABLE_MAINPROGRAM_EXIT = True

            If NO_CMD_HINTS = False Then
                Console.WriteLine("[End of Streamlink for Windows with ExitCode " & proc.ExitCode & "]")
            End If

            LAST_EXIT_CODE = proc.ExitCode
        Catch ex As Exception
            LAST_EXIT_CODE = 1
            Console.WriteLine("[Streamlink] An error occurred")
        End Try

    End Function

    Sub Console_CancelKeyPress(sender As Object, e As ConsoleCancelEventArgs)
        If ENABLE_MAINPROGRAM_EXIT = False Then
            e.Cancel = True
        Else
            Process.GetCurrentProcess.Kill()
        End If
    End Sub

End Module
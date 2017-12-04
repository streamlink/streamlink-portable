
Imports System.Deployment
Imports System.IO
Imports System.IO.Compression
Imports System.Reflection
Imports System.Runtime.InteropServices
Imports System.Text
Imports System.Threading

<Assembly: AssemblyTitle("Streamlink for Windows")>
<Assembly: AssemblyDescription("")>
<Assembly: AssemblyCompany("Streamlink")>
<Assembly: AssemblyProduct("Streamlink for Windows")>
<Assembly: AssemblyCopyright("Streamlink")>
<Assembly: AssemblyTrademark("Streamlink for Windows")>
<Assembly: AssemblyVersion("1.0.0.0")>
<Assembly: AssemblyFileVersion("1.0.0.0")>

Module Module1
    Public Current_EXE_Path As String = My.Application.Info.DirectoryPath
    Public ENABLE_MAINPROGRAM_EXIT As Boolean = True
    Dim CMD_INVOKED As Boolean = True
    Public NO_CMD_HINTS As Boolean = False
    Public LAST_EXIT_CODE As Integer = 0
    Public UTF8WithoutBOM = New UTF8Encoding(False)

    Sub Main()
        On Error Resume Next

        AddHandler Console.CancelKeyPress, AddressOf Console_CancelKeyPress
        Current_EXE_Path = Current_EXE_Path.Replace("file:///", "")
        Current_EXE_Path = Current_EXE_Path.Replace("file:\", "")
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

        Console.OutputEncoding = UTF8WithoutBOM
        Console.InputEncoding = UTF8WithoutBOM
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
            Dim Temp_Path_Streamlink As String = Path.GetTempPath & "\Streamlink Standalone"
            Dim Temp_ZIP_Path_Streamlink As String = Temp_Path_Streamlink & "\STREAMLINK_RELEASE.zip"

            Dim Unzip_Required As Boolean = True
            Dim ACTUAL_RANDOM_ID As String = ""

            If IO.Directory.Exists(Temp_Path_Streamlink) = False Then
                IO.Directory.CreateDirectory(Temp_Path_Streamlink) 'Check if path exists
            End If

            If IO.Directory.Exists(Temp_Path_Streamlink) Then
                Dim Temp_Previous_Ver As String = Temp_Path_Streamlink & "\VERSION.txt"
                Dim Temp_Embed_Ver As String = Temp_Path_Streamlink & "\VERSION_EMBED.txt"
                DeleteFileIfExists(Temp_Embed_Ver)
                Using _fileStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("VERSION.txt")
                    CopyStream(_fileStream, Temp_Embed_Ver)
                End Using
                If IO.File.Exists(Temp_Previous_Ver) Then
                    If IO.File.ReadAllText(Temp_Previous_Ver, UTF8WithoutBOM) = IO.File.ReadAllText(Temp_Embed_Ver, UTF8WithoutBOM) Then
                        Unzip_Required = False
                    End If
                End If
                Dim Temp_Previous_Random_ID As String = Temp_Path_Streamlink & "\RANDOM_ID.txt"
                Dim Temp_Embed_Random_ID As String = Temp_Path_Streamlink & "\RANDOM_ID_EMBED.txt"
                DeleteFileIfExists(Temp_Embed_Random_ID)
                Using _fileStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("RANDOM_ID.txt")
                    CopyStream(_fileStream, Temp_Embed_Random_ID)
                End Using
                ACTUAL_RANDOM_ID = IO.File.ReadAllText(Temp_Embed_Random_ID, UTF8WithoutBOM)
                If IO.File.Exists(Temp_Previous_Random_ID) Then
                    If IO.File.ReadAllText(Temp_Previous_Random_ID, UTF8WithoutBOM) = IO.File.ReadAllText(Temp_Embed_Random_ID, UTF8WithoutBOM) Then
                        Unzip_Required = False
                    Else
                        Unzip_Required = True
                    End If
                Else
                    Unzip_Required = True
                End If
            End If

            If Unzip_Required = True Then
                If NO_CMD_HINTS = False Then
                    Console.WriteLine("[Streamlink] Extracting program ...")
                End If
                Kill_All_EXE(Temp_Path_Streamlink)
                DeleteDirectoryIfExists(Temp_Path_Streamlink)
                IO.Directory.CreateDirectory(Temp_Path_Streamlink)
                Using _fileStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("STREAMLINK_RELEASE.zip")
                    CopyStream(_fileStream, Temp_ZIP_Path_Streamlink)
                End Using
                NativeUnzipFile(Temp_ZIP_Path_Streamlink, Temp_Path_Streamlink)
                IO.File.Delete(Temp_ZIP_Path_Streamlink)
                Unlock_All_EXE(Temp_Path_Streamlink) 'Try to unblock Streamlink TEMP path
                If NO_CMD_HINTS = False Then
                    Console.WriteLine("[Streamlink] Program successfully extracted")
                End If
                IO.File.WriteAllText(Temp_Path_Streamlink & "\RANDOM_ID.txt", ACTUAL_RANDOM_ID, UTF8WithoutBOM)
            End If

            IO.File.WriteAllText(Temp_Path_Streamlink & "\CUSTOM_APPDATA", Chr(34) & Current_EXE_Path.Replace("\", "/") & Chr(34), UTF8WithoutBOM) 'Set current PATH in TEMP

            If NO_CMD_HINTS = False Then
                Dim ACTUAL_VER_DATA As String = Temp_Path_Streamlink & "\VERSION.txt"
                If IO.File.Exists(ACTUAL_VER_DATA) Then
                    ACTUAL_VER_DATA = IO.File.ReadAllText(ACTUAL_VER_DATA, UTF8WithoutBOM)
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

            Dim info = New ProcessStartInfo(Chr(34) & Temp_Path_Streamlink & "\Python 3.6.3\python.exe" & Chr(34), Chr(34) & Temp_Path_Streamlink & "\Streamlink\Streamlink.py" & Chr(34) & " --config " & Chr(34) & Current_EXE_Path & "\streamlinkrc" & Chr(34) & " --rtmp-rtmpdump " & Chr(34) & Temp_Path_Streamlink & "\Streamlink\Dependencies\rtmpdump\rtmpdump.exe" & Chr(34) & " --ffmpeg-ffmpeg " & Chr(34) & Temp_Path_Streamlink & "\Streamlink\Dependencies\ffmpeg\ffmpeg.exe" & Chr(34) & " " & InputCommand)
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

    Public Sub CopyStream(stream As Stream, destPath As String)
        Using fileStream = File.Create(destPath)
            stream.Seek(0, SeekOrigin.Begin)
            stream.CopyTo(fileStream)
        End Using
    End Sub

    Sub DeleteDirectoryIfExists(ByVal URL As String)
        If IO.Directory.Exists(URL) Then
            IO.Directory.Delete(URL, True)
        End If
    End Sub

    Sub DeleteFileIfExists(ByVal URL As String)
        If IO.File.Exists(URL) Then
            IO.File.Delete(URL)
        End If
    End Sub

    Function Kill_All_EXE(ByVal path As String)
        On Error Resume Next
        Dim SourceDir As DirectoryInfo = New DirectoryInfo(path)
        Dim pathIndex As Integer

        If SourceDir.Exists Then
            pathIndex = path.LastIndexOf("\")
            For Each childFile As FileInfo In SourceDir.GetFiles("*", SearchOption.AllDirectories).Where(Function(file) file.Extension.ToLower = ".exe")
                For Each prog As Process In Process.GetProcesses
                    If prog.ProcessName = childFile.Name.Remove(childFile.Name.LastIndexOf(".")) Then
                        prog.Kill() 'Kill current process
                        prog.WaitForExit() 'Wait until the process is gone
                    End If
                Next
            Next
        Else
            'Console.WriteLine("The directory does not exist :(")
        End If

    End Function

    Function Unlock_All_EXE(ByVal path As String)
        On Error Resume Next 'To avoid very common errors
        Dim SourceDir As DirectoryInfo = New DirectoryInfo(path)
        Dim pathIndex As Integer

        If SourceDir.Exists Then
            pathIndex = path.LastIndexOf("\")
            For Each childFile As FileInfo In SourceDir.GetFiles("*", SearchOption.AllDirectories).Where(Function(file) file.Extension.ToLower = ".exe")
                FileUnblocker.UnblockFile(childFile.FullName) 'Unlock current .EXE
            Next
        Else
            'Console.WriteLine("The directory does not exist :(")
        End If
    End Function

    Public Function NativeUnzipFile(ByVal Ruta_ZIP As String, ByVal Carpeta_Salida As String)
        Using archive As ZipArchive = ZipFile.OpenRead(Ruta_ZIP)
            For Each entry As ZipArchiveEntry In archive.Entries
                Dim entryFullname = Path.Combine(Carpeta_Salida, entry.FullName)
                Dim entryPath = Path.GetDirectoryName(entryFullname)
                If (Not (Directory.Exists(entryPath))) Then
                    Directory.CreateDirectory(entryPath)
                End If

                Dim entryFn = Path.GetFileName(entryFullname)
                If (Not String.IsNullOrEmpty(entryFn)) Then
                    entry.ExtractToFile(entryFullname, True)
                End If
            Next
        End Using
    End Function

End Module

Public Class FileUnblocker

    <DllImport("kernel32", CharSet:=CharSet.Unicode, SetLastError:=True)>
    Public Shared Function DeleteFile(name As String) As <MarshalAs(UnmanagedType.Bool)> Boolean
    End Function

    Public Shared Sub UnblockPath(path As String)
        Dim files As String() = System.IO.Directory.GetFiles(path)
        Dim dirs As String() = System.IO.Directory.GetDirectories(path)

        For Each file As String In files
            UnblockFile(file)
        Next

        For Each dir As String In dirs
            UnblockPath(dir)
        Next

    End Sub

    Public Shared Function UnblockFile(fileName As String) As Boolean
        Return DeleteFile(fileName & Convert.ToString(":Zone.Identifier"))
    End Function

End Class

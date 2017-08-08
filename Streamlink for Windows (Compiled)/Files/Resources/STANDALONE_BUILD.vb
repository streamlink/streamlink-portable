
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
    Public Current_EXE_Path As String = System.Reflection.Assembly.GetExecutingAssembly().CodeBase
    Public ENABLE_MAINPROGRAM_EXIT As Boolean = True
    Sub Main()
        Dim CMD_INVOKED As Boolean = True
        Try
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

            Dim Temp_Path_RTV As String = Path.GetTempPath & "\RTV_STREAMLINK_FOR_WINDOWS"
            Dim Temp_ZIP_Path_RTV As String = Temp_Path_RTV & "\STREAMLINK_RELEASE.zip"
            Try
                Console.OutputEncoding = Encoding.UTF8
                Console.InputEncoding = Encoding.UTF8
                Dim bufSize As Integer = 4096
                Dim inStream As Stream = Console.OpenStandardInput(bufSize)
                Console.SetIn(New StreamReader(inStream, Console.InputEncoding, False, bufSize))
            Catch
                'Error defining encoding
            End Try

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

            Dim Unzip_Required As Boolean = True
            Dim ACTUAL_RANDOM_ID As String = ""

            If IO.Directory.Exists(Temp_Path_RTV) = False Then
                IO.Directory.CreateDirectory(Temp_Path_RTV) 'Asegurar existencia del directorio
            End If

            If IO.Directory.Exists(Temp_Path_RTV) Then
                Dim Temp_Previous_Ver As String = Temp_Path_RTV & "\VERSION.txt"
                Dim Temp_Embed_Ver As String = Temp_Path_RTV & "\VERSION_EMBED.txt"
                DeleteFileIfExists(Temp_Embed_Ver)
                Using _fileStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("VERSION.txt")
                    CopyStream(_fileStream, Temp_Embed_Ver)
                End Using
                If IO.file.exists(Temp_Previous_Ver) Then
                    If IO.File.ReadAllText(Temp_Previous_Ver, Encoding.UTF8) = IO.File.ReadAllText(Temp_Embed_Ver, Encoding.UTF8) Then
                        Unzip_Required = False
                    End If
                End If
                Dim Temp_Previous_Random_ID As String = Temp_Path_RTV & "\RANDOM_ID.txt"
                Dim Temp_Embed_Random_ID As String = Temp_Path_RTV & "\RANDOM_ID_EMBED.txt"
                DeleteFileIfExists(Temp_Embed_Random_ID)
                Using _fileStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("RANDOM_ID.txt")
                    CopyStream(_fileStream, Temp_Embed_Random_ID)
                End Using
                ACTUAL_RANDOM_ID = IO.File.ReadAllText(Temp_Embed_Random_ID, Encoding.UTF8)
                If IO.File.Exists(Temp_Previous_Random_ID) Then
                    If IO.File.ReadAllText(Temp_Previous_Random_ID, Encoding.UTF8) = IO.File.ReadAllText(Temp_Embed_Random_ID, Encoding.UTF8) Then
                        Unzip_Required = False
                    Else
                        Unzip_Required = True
                    End If
                Else
                    Unzip_Required = True
                End If

            End If

            If Unzip_Required = True Then
                Console.WriteLine("[Streamlink] Extracting program ...")
                Kill_All_EXE(Temp_Path_RTV)
                DeleteDirectoryIfExists(Temp_Path_RTV)
                IO.Directory.CreateDirectory(Temp_Path_RTV)
                Using _fileStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("STREAMLINK_RELEASE.zip")
                    CopyStream(_fileStream, Temp_ZIP_Path_RTV)
                End Using
                NativeUnzipFile(Temp_ZIP_Path_RTV, Temp_Path_RTV)
                IO.File.Delete(Temp_ZIP_Path_RTV)
                Unlock_All_EXE(Temp_Path_RTV) 'Try to unblock Streamlink temp path
                Console.WriteLine("[Streamlink] Program successfully extracted")
                IO.File.WriteAllText(Temp_Path_RTV & "\RANDOM_ID.txt", ACTUAL_RANDOM_ID, Encoding.UTF8)
            End If

            If IO.File.Exists(Temp_Path_RTV & "\Streamlink\streamlink_cli\constants.py") Then
                Dim TextBox_Analysis_StreamlinkConstants As New TextBox
                Dim StreamlinkConstants_Final_Modded As String = ""
                TextBox_Analysis_StreamlinkConstants.Text = IO.File.ReadAllText(Temp_Path_RTV & "\Streamlink\streamlink_cli\constants.py", Encoding.UTF8)

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
                IO.File.WriteAllText(Temp_Path_RTV & "\Streamlink\streamlink_cli\constants.py", StreamlinkConstants_Final_Modded, Encoding.UTF8)
            End If

            Dim ACTUAL_VER_DATA As String = Temp_Path_RTV & "\VERSION.txt"
            If IO.File.Exists(ACTUAL_VER_DATA) Then
                ACTUAL_VER_DATA = IO.File.ReadAllText(ACTUAL_VER_DATA, Encoding.UTF8)
                Console.WriteLine("[Streamlink for Windows " & ACTUAL_VER_DATA & "]")
            Else
                Console.WriteLine("[Streamlink for Windows]")
            End If
            Dim info = New ProcessStartInfo(Chr(34) & Temp_Path_RTV & "\Python 3.5.2\python.exe" & Chr(34), Chr(34) & Temp_Path_RTV & "\Streamlink\Streamlink.py" & Chr(34) & " --config " & Chr(34) & Current_EXE_Path & "\streamlinkrc" & Chr(34) & " --rtmp-rtmpdump " & Chr(34) & Temp_Path_RTV & "\Streamlink\Dependencies\rtmpdump\rtmpdump.exe" & Chr(34) & " --ffmpeg-ffmpeg " & Chr(34) & Temp_Path_RTV & "\Streamlink\Dependencies\ffmpeg\ffmpeg.exe" & Chr(34) & " " & Final_Args)
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
        Catch ex As Exception
            Console.WriteLine("[Streamlink] An error occurred")

            If CMD_INVOKED = False Then
                Console.ReadKey()
            End If

        End Try
        'Threading.Thread.Sleep(Threading.Timeout.Infinite)
    End Sub

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
            Console.WriteLine("The directory does not exist :(")
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
            Console.WriteLine("The directory does not exist :(")
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

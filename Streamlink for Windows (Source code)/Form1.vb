Imports System.ComponentModel
Imports System.IO
Imports System.Net
Imports System.Reflection
Imports System.Runtime.InteropServices
Imports System.Security.Cryptography
Imports System.CodeDom.Compiler
Imports System.Text
Imports System.Security
Imports System.Text.RegularExpressions

Public Class Form1
    Public Shared CodeDomProvider As CodeDomProvider = CodeDomProvider.CreateProvider("VB")
    Public Shared Current_EXE_Path As String = IO.Path.GetDirectoryName(Application.ExecutablePath)
    Dim WithEvents DATA_Downloader As New WebClient() 'Data downloader
    Dim DATA_Downloader_CurrentProgress As String = "0%"
    Dim UserAgent_1 As String = "Mozilla/5.0 (Android; Mobile; rv:30.0) Gecko/30.0 Firefox/30.0" 'Android (Mobile)
    Dim UserAgent_2 As String = "Dalvik/1.6.0 (Linux; U; Android 4.4.2; TegraNote-P1640 Build/KOT49H)" 'Android (Tablet)
    Dim UserAgent_3 As String = "Mozilla/5.0 (Windows NT 6.3; rv:36.0) Gecko/20100101 Firefox/36.0" 'Windows (Desktop)

    'To work with WebRequest y WebResponse
    Dim myHttpWebRequest As HttpWebRequest
    Dim myHttpWebResponse As HttpWebResponse
    '

    'Current data storage
    Dim URL_ZIP_ARCHIVE_CURRENT As String = "https://github.com/streamlink/streamlink/archive/master.zip"
    Dim DEPENDENCY_CHK_FINAL As String = ""
    Dim CURRENT_VERSION As String = ""
    Dim RELEASE_VER_CURRENT As String = ""
    Dim RELEASE_FILE_EXT_CURRENT As String = ".exe"
    '

    Public WithEvents BW_STEPS As New ExtendedBackgroundWorker
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Me.KeyPreview = True
        Me.Icon = ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location) 'The icon will be the same as the application
        Me.DoubleBuffered = True 'To Avoid UI flickering
        Me.ClientSize = Panel2.ClientSize
        Me.CenterToScreen()
        CheckForIllegalCrossThreadCalls = False 'Enable full management by backgroundworkers
        IO.Directory.SetCurrentDirectory(Current_EXE_Path)

        'Check write permissions
        If Check_Write_Permissions() = False Then
            Msgbox_THREADSAFE("I dont have write permissions :(" & vbNewLine & "Try running me with administrator rights.", MsgBoxStyle.Critical, "Error")
            Application.Exit()
            Application.ExitThread()
            Return
        End If
        '

        Unlock_All_EXE("Files")

        For Each Button_TMP As Control In Panel2.Controls
            If TypeOf (Button_TMP) Is Button Then
                Button_TMP.TabIndex = 0
                Button_TMP.TabStop = False
            End If
        Next

        For Each Button_TMP As Control In Panel1.Controls
            If TypeOf (Button_TMP) Is Button Then
                Button_TMP.TabIndex = 0
                Button_TMP.TabStop = False
            End If
        Next

        AdjustPaddingButtonCustomURL()

    End Sub

    Public Function AdjustPaddingButtonCustomURL()
        If Button4.Visible = True Then
            Dim diff_button_customurl As Integer = Panel1.Width - Button2.Width
            Button2.Padding = New Padding(diff_button_customurl, 0, 0, 0)
        Else
            Button2.Padding = New Padding(0, 0, 0, 0)
        End If
    End Function

    Public Function Check_Write_Permissions() As Boolean
        Try
            IO.File.WriteAllText("TEST.rtv", "", Encoding.UTF8)
            IO.File.Delete("TEST.rtv")
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function GetPageHTMLCustom(ByVal URL As String, ByVal UserAgent_Custom As String, ByVal Referer_Custom As String, Optional ByVal Cookies As String = "") As String
        Dim RESULT As String = ""
        DATA_Downloader.Encoding = Encoding.UTF8
        DATA_Downloader.Headers.Clear()
        DATA_Downloader.Headers(Net.HttpRequestHeader.Referer) = Referer_Custom
        DATA_Downloader.Headers(Net.HttpRequestHeader.UserAgent) = UserAgent_Custom
        DATA_Downloader.Headers(Net.HttpRequestHeader.Cookie) = Cookies
        Try
            RESULT = DATA_Downloader.DownloadString(URL) 'Try to get results directly
        Catch fail As WebException 'Try to get result ignoring errors
            Using sr = New StreamReader(fail.Response.GetResponseStream())
                RESULT = sr.ReadToEnd()
            End Using
        End Try
        Return RESULT
    End Function

    Public Function GetPageHTMLCustom_POST(ByVal POST As String, ByVal URL As String, ByVal UserAgent_Custom As String, ByVal Referer_Custom As String, Optional ByVal Cookies As String = "") As String
        Dim RESULT As String = ""
        DATA_Downloader.Encoding = Encoding.UTF8
        DATA_Downloader.Headers.Clear()
        DATA_Downloader.Headers(Net.HttpRequestHeader.Referer) = Referer_Custom
        DATA_Downloader.Headers(Net.HttpRequestHeader.UserAgent) = UserAgent_Custom
        DATA_Downloader.Headers(Net.HttpRequestHeader.Cookie) = Cookies
        DATA_Downloader.Headers.Add("Content-Type: application/x-www-form-urlencoded; charset=UTF-8")

        Try
            RESULT = DATA_Downloader.UploadString(URL, POST) 'Try to get results directly
        Catch fail As WebException 'Try to get result ignoring errors
            Using sr = New StreamReader(fail.Response.GetResponseStream())
                RESULT = sr.ReadToEnd()
            End Using
        End Try
        Return RESULT
    End Function

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Form1_Deactivate(Nothing, Nothing)
        If Button2.Text = "Start downloading" Then
            If sender.text = "Portable EXE" Then
                Button1.Text = "Standalone EXE"
                RELEASE_FILE_EXT_CURRENT = ".exe"
            ElseIf sender.text = "Standalone EXE" Then
                Button1.Text = "Portable BAT"
                RELEASE_FILE_EXT_CURRENT = ".bat"
            Else
                Button1.Text = "Portable EXE"
                RELEASE_FILE_EXT_CURRENT = ".exe"
            End If
        End If
    End Sub

    Private Sub Button2_MouseUp(sender As Object, e As MouseEventArgs) Handles Button2.MouseUp
        Form1_Deactivate(Nothing, Nothing)
        If sender.Text = "Start downloading" Then

            If e.Button = MouseButtons.Left Then
                If BW_STEPS.IsBusy = False Then
                    BW_STEPS.RunWorkerAsync("STEP1")
                End If
            End If

            If e.Button = MouseButtons.Right Then
                If BW_STEPS.IsBusy = False Then
                    Dim URL_ZIP_ARCHIVE_TEMP As String = InputBox_General_Custom.AbrirInputBox("Custom download", "Enter a ZIP archive URL", URL_ZIP_ARCHIVE_CURRENT, Me)
                    If String.IsNullOrWhiteSpace(URL_ZIP_ARCHIVE_TEMP) = False Then
                        If URL_ZIP_ARCHIVE_TEMP.ToLower.StartsWith("http") Then
                            URL_ZIP_ARCHIVE_CURRENT = URL_ZIP_ARCHIVE_TEMP
                            BW_STEPS.RunWorkerAsync("STEP1")
                        End If
                    End If
                End If
            End If

        End If
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        Form1_Deactivate(Nothing, Nothing)
        If Button2.Text = "Completed" And sender.text = "Start building" Then
            If BW_STEPS.IsBusy = False Then
                BW_STEPS.RunWorkerAsync("STEP2")
            End If
        End If
    End Sub

    Sub DeleteFileIfExists(ByVal URL As String)
        If IO.File.Exists(URL) Then
            IO.File.Delete(URL)
        End If
    End Sub

    Sub DeleteDirectoryIfExists(ByVal URL As String)
        If IO.Directory.Exists(URL) Then
            IO.Directory.Delete(URL, True)
        End If
    End Sub

    Private Sub BW_STEPS_DoWork(sender As Object, e As DoWorkEventArgs) Handles BW_STEPS.DoWork
        Cancel_Internet_Interactions()
        If e.Argument = "STEP1" Then
            Try

                ChangeObjectVisibility_THREADSAFE(Button4, False)
                AdjustPaddingButtonCustomURL()

                Button2.Text = "Loading (1/1)"

                'DeleteDirectoryIfExists("Files\TEMP")
                IO.Directory.CreateDirectory("Files\TEMP")

                Dim New_Version_Available As Boolean = True
                Dim Streamlink_URL As String = URL_ZIP_ARCHIVE_CURRENT
                CURRENT_VERSION = GetETAG_HTTPHEADER(Streamlink_URL)

                If IO.Directory.Exists("Releases") Then
                    If IO.File.Exists("Releases\VERSION.txt") Then
                        Dim vers_check As String = IO.File.ReadAllText("Releases\VERSION.txt")
                        If vers_check.Contains(CURRENT_VERSION.Remove(7)) Then
                            New_Version_Available = False
                            Dim result As Integer = Msgbox_Interactive_THREADSAFE("It looks like you already have the latest version." & vbNewLine & "Continue anyway?", "Notice", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                            If result = DialogResult.Yes Then
                                'The user agreed to update anyway
                            Else
                                Button2.Text = "Start downloading"
                                ChangeObjectVisibility_THREADSAFE(Button4, True)
                                AdjustPaddingButtonCustomURL()
                                Return
                            End If
                        End If
                    End If
                End If

                If IO.File.Exists("Files\TEMP\Streamlink_Latest_GIT.txt") Then
                    Dim vers_check As String = IO.File.ReadAllText("Files\TEMP\Streamlink_Latest_GIT.txt", Encoding.UTF8)
                    If vers_check.Contains(CURRENT_VERSION) Then
                        New_Version_Available = False
                    End If
                End If

                Dim Redownload_Latest_Streamlink_ZIP As Boolean = True
                If New_Version_Available = False And IO.File.Exists("Files\TEMP\Streamlink_Latest_MD5.txt") And IO.File.Exists("Files\TEMP\Streamlink_Latest.zip") Then
                    Dim MD5_previous As String = IO.File.ReadAllText("Files\TEMP\Streamlink_Latest_MD5.txt", Encoding.UTF8)
                    Dim MD5_Current As String = getFileMD5("Files\TEMP\Streamlink_Latest.zip")
                    If MD5_previous = MD5_Current Then
                        Redownload_Latest_Streamlink_ZIP = False
                    End If
                End If
                If Redownload_Latest_Streamlink_ZIP = True Then
                    DeleteDirectoryIfExists("Files\TEMP")
                    IO.Directory.CreateDirectory("Files\TEMP")
                    'DATA_Downloader.DownloadFile(Streamlink_URL, "Files\TEMP\Streamlink_Latest.zip")

                    DATA_Downloader.DownloadFileTaskAsync(Streamlink_URL, "Files\TEMP\Streamlink_Latest.zip")
                    Do Until DATA_Downloader.IsBusy = False
                        Threading.Thread.Sleep(500)
                        Button2.Text = "Downloading (" & DATA_Downloader_CurrentProgress & ")"
                    Loop

                End If

                Button2.Text = "Completed"
                Button3.Text = "Start building"

            Catch
                Button2.Text = "Start downloading"
                ChangeObjectVisibility_THREADSAFE(Button4, True)
                AdjustPaddingButtonCustomURL()
            End Try
        End If

        If e.Argument = "STEP2" Then
            Try
                Button3.Text = "Loading (1/3)"

                Dim compressed_path As String = Chr(34) & Current_EXE_Path & "\Files\TEMP\Streamlink_Latest.zip" & Chr(34)
                Dim compressed_destination As String = Chr(34) & Current_EXE_Path & "\Files\TEMP" & Chr(34)
                DeleteDirectoryIfExists("Files\TEMP\streamlink-master")
                RunAndWait("Files\7zip\7za.exe", "-y x " & compressed_path & " -o" & compressed_destination) 'Unzip content

                'Find or adjust streamlink-master
                If IO.Directory.Exists("Files\TEMP\streamlink-master") = False Then
                    Dim Alternative_Streamlink_Master_DIR As String = ""
                    For Each Possible_Streamlink_Master_DIR As String In IO.Directory.GetDirectories("Files\TEMP", "*", SearchOption.TopDirectoryOnly)
                        Possible_Streamlink_Master_DIR = Possible_Streamlink_Master_DIR.Replace("/", "\")
                        Possible_Streamlink_Master_DIR = Possible_Streamlink_Master_DIR.Remove(0, Possible_Streamlink_Master_DIR.LastIndexOf("\") + 1)
                        If Possible_Streamlink_Master_DIR.StartsWith("streamlink-") Then
                            Alternative_Streamlink_Master_DIR = Possible_Streamlink_Master_DIR
                        End If
                    Next
                    If String.IsNullOrEmpty(Alternative_Streamlink_Master_DIR) = False Then
                        My.Computer.FileSystem.RenameDirectory("Files\TEMP\" & Alternative_Streamlink_Master_DIR, "streamlink-master")
                    End If
                End If
                '

                'Dependency check (BETA)
                If IO.File.Exists("Files\TEMP\streamlink-master\script\makeinstaller.sh") Then
                    Dim Extra_Dependencies_Check As String = GetPageHTMLCustom("Files\TEMP\streamlink-master\script\makeinstaller.sh", UserAgent_1, "https://github.com")
                    Extra_Dependencies_Check = Extra_Dependencies_Check.Remove(0, Extra_Dependencies_Check.IndexOf("[Include]") + 10)
                    Extra_Dependencies_Check = Extra_Dependencies_Check.Remove(0, Extra_Dependencies_Check.IndexOf("packages="))
                    Extra_Dependencies_Check = Extra_Dependencies_Check.Remove(Extra_Dependencies_Check.IndexOf("files="))
                    Extra_Dependencies_Check = Extra_Dependencies_Check.Replace(" ", "")
                    Dim Extra_Dependencies_Check_2 As String = ""
                    For Each ExtDependency_Check_TMP As String In Extra_Dependencies_Check.Split({ControlChars.Cr, ControlChars.Lf})
                        ExtDependency_Check_TMP = ExtDependency_Check_TMP.Replace("packages=", "")
                        ExtDependency_Check_TMP = ExtDependency_Check_TMP.Replace("pypi_wheels=", "")
                        If ExtDependency_Check_TMP.Contains("=") Then
                            ExtDependency_Check_TMP = ExtDependency_Check_TMP.Remove(ExtDependency_Check_TMP.IndexOf("="))
                        End If
                        If ExtDependency_Check_TMP.StartsWith(";") = False And ExtDependency_Check_TMP.StartsWith("-") = False Then
                            Extra_Dependencies_Check_2 += ExtDependency_Check_TMP & vbNewLine
                        End If
                    Next
                    Extra_Dependencies_Check = Extra_Dependencies_Check_2
                    'Adjust bundled packages
                    Extra_Dependencies_Check = Extra_Dependencies_Check.Replace("iso639", "pycountry")
                    Extra_Dependencies_Check = Extra_Dependencies_Check.Replace("iso3166", "pycountry")
                    Extra_Dependencies_Check = Extra_Dependencies_Check.Replace("pycryptodome", "Crypto")
                    Extra_Dependencies_Check = Extra_Dependencies_Check.Replace("pkg_resources", "")
                    '
                    Extra_Dependencies_Check = Regex.Replace(Extra_Dependencies_Check, "^\s+$[\r\n]*", "", RegexOptions.Multiline)
                    DEPENDENCY_CHK_FINAL = Extra_Dependencies_Check
                    'MsgBox(DEPENDENCY_CHK_FINAL)
                End If
                '

                For Each File_TMP As String In IO.Directory.GetFiles("Files\TEMP\streamlink-master", "*.*", SearchOption.TopDirectoryOnly)
                    IO.File.Delete(File_TMP)
                Next

                For Each Folder_TMP As String In IO.Directory.GetDirectories("Files\TEMP\streamlink-master")
                    Folder_TMP = Folder_TMP.Replace("/", "\")
                    If (Folder_TMP.EndsWith("\src") Or Folder_TMP.EndsWith("\win32")) = False Then
                        IO.Directory.Delete(Folder_TMP, True)
                    End If
                Next

                MoveAllItems("Files\TEMP\streamlink-master\src", "Files\TEMP\streamlink-master")
                DeleteDirectoryIfExists("Files\TEMP\streamlink-master\src")
                MoveAllItems("Files\TEMP\streamlink-master\win32", "Files\TEMP\streamlink-master")
                DeleteDirectoryIfExists("Files\TEMP\streamlink-master\win32")

                For Each barrido_post_extract_file As String In Directory.GetFiles("Files\TEMP\streamlink-master", "*.*", SearchOption.TopDirectoryOnly)
                    If barrido_post_extract_file.ToLower.EndsWith(".ico") Then
                        IO.File.Delete(barrido_post_extract_file)
                    End If
                Next

                Dim argparser_py_location As String = "Files\TEMP\streamlink-master\streamlink_cli\argparser.py"
                Dim argparser_py As String = IO.File.ReadAllText(argparser_py_location, Encoding.UTF8)
                Dim argparser_py_replace As String = "%(prog)s"
                Dim argparser_py_replace_end As String = "Streamlink.exe"
                argparser_py = argparser_py.Replace(argparser_py_replace, argparser_py_replace_end)
                IO.File.WriteAllText(argparser_py_location, argparser_py, Encoding.UTF8)

                compressed_path = Chr(34) & Current_EXE_Path & "\Files\Resources\Streamlink_Patches.zip" & Chr(34)
                compressed_destination = Chr(34) & Current_EXE_Path & "\Files\TEMP\streamlink-master" & Chr(34)
                RunAndWait("Files\7zip\7za.exe", "-y x " & compressed_path & " -o" & compressed_destination) 'Si hay que descomprimir

                Button3.Text = "Loading (2/3)"

                Kill_All_EXE("Releases")

                If IO.File.Exists("Releases\streamlinkrc") Then
                    DeleteFileIfExists("Files\TEMP\streamlinkrc_BACKUP")
                    IO.File.Copy("Releases\streamlinkrc", "Files\TEMP\streamlinkrc_BACKUP")
                End If
                DeleteDirectoryIfExists("Releases")
                IO.Directory.CreateDirectory("Releases\Python 3.5.2")
                MoveAllItems("Files\TEMP\streamlink-master", "Releases\Streamlink")
                DeleteDirectoryIfExists("Files\TEMP\streamlink-master")

                compressed_path = Chr(34) & Current_EXE_Path & "\Files\Resources\python-3.5.2-embed-win32.zip" & Chr(34)
                compressed_destination = Chr(34) & Current_EXE_Path & "\Releases\Python 3.5.2" & Chr(34)
                RunAndWait("Files\7zip\7za.exe", "-y x " & compressed_path & " -o" & compressed_destination) 'Si hay que descomprimir

                Button3.Text = "Loading (3/3)"

                Dim README_CONTENT As String = IO.File.ReadAllText("Files\Resources\BUILD_README.txt", Encoding.UTF8)
                README_CONTENT = README_CONTENT.Replace("%STREAMLINK_EXEC_FILE%", "Streamlink" & RELEASE_FILE_EXT_CURRENT)
                IO.File.WriteAllText("Releases\README.txt", README_CONTENT)

                RELEASE_VER_CURRENT = "Releases\Streamlink\streamlink\__init__.py"
                If IO.File.Exists(RELEASE_VER_CURRENT) Then
                    Try
                        RELEASE_VER_CURRENT = IO.File.ReadAllText(RELEASE_VER_CURRENT, Encoding.UTF8)
                        RELEASE_VER_CURRENT = RELEASE_VER_CURRENT.ToLower.Remove(0, RELEASE_VER_CURRENT.IndexOf("__version__ = ") + 15)
                        RELEASE_VER_CURRENT = RELEASE_VER_CURRENT.Remove(RELEASE_VER_CURRENT.IndexOf(Chr(34)))
                    Catch
                        RELEASE_VER_CURRENT = ""
                    End Try
                Else
                    RELEASE_VER_CURRENT = ""
                End If

                Dim version_txt_out As String = ""
                If String.IsNullOrEmpty(RELEASE_VER_CURRENT) Then
                    version_txt_out = "Git " & CURRENT_VERSION.Remove(7)
                Else
                    version_txt_out = "v" & RELEASE_VER_CURRENT & " - Git " & CURRENT_VERSION.Remove(7)
                End If
                IO.File.WriteAllText("Releases\VERSION.txt", version_txt_out)

                If IO.File.Exists("Releases\Streamlink\streamlinkrc") Then
                    Dim textbox_analysis_streamlinkrc As New TextBox
                    Dim streamlinkrc_final_modded As String = ""
                    textbox_analysis_streamlinkrc.Text = IO.File.ReadAllText("Releases\Streamlink\streamlinkrc", Encoding.UTF8)
                    For i_line_analysis_streamlinkrc As Integer = 0 To textbox_analysis_streamlinkrc.Lines.Count - 1
                        Dim line_analysis_streamlinkrc As String = textbox_analysis_streamlinkrc.Lines(i_line_analysis_streamlinkrc)
                        If String.IsNullOrWhiteSpace(line_analysis_streamlinkrc) = False Then
                            If line_analysis_streamlinkrc.Replace(" ", "").StartsWith("#") = False Then
                                line_analysis_streamlinkrc = "#" & line_analysis_streamlinkrc
                            End If
                        End If
                        streamlinkrc_final_modded += line_analysis_streamlinkrc & vbNewLine
                    Next
                    textbox_analysis_streamlinkrc.Dispose()
                    IO.File.WriteAllText("Releases\streamlinkrc", streamlinkrc_final_modded, Encoding.UTF8)
                    IO.File.Delete("Releases\Streamlink\streamlinkrc")
                End If

                'Dependency check (Part 2)
                Dim MISSED_DEPENDENCIES As String = ""
                If String.IsNullOrEmpty(DEPENDENCY_CHK_FINAL) = False Then
                    For Each line_analysis_dep As String In DEPENDENCY_CHK_FINAL.Split({ControlChars.Cr, ControlChars.Lf})
                        If String.IsNullOrEmpty(line_analysis_dep) = False Then
                            Dim missed_dep As Boolean = True
                            If IO.Directory.Exists("Releases\Streamlink\" & line_analysis_dep) Or IO.File.Exists("Releases\Streamlink\" & line_analysis_dep & ".py") Then
                                missed_dep = False
                            End If
                            If IO.Directory.Exists("Releases\Streamlink\Dependencies\" & line_analysis_dep) Or IO.File.Exists("Releases\Streamlink\Dependencies\" & line_analysis_dep & ".py") Then
                                missed_dep = False
                            End If
                            If missed_dep = True Then
                                MISSED_DEPENDENCIES += line_analysis_dep & vbNewLine
                            End If
                        End If
                    Next
                    MISSED_DEPENDENCIES = Regex.Replace(MISSED_DEPENDENCIES, "^\s+$[\r\n]*", "", RegexOptions.Multiline)
                End If
                '

                If IO.Directory.Exists("Releases\Streamlink\ffmpeg") Then
                    IO.Directory.Move("Releases\Streamlink\ffmpeg", "Releases\Streamlink\Dependencies\ffmpeg")
                End If
                If IO.Directory.Exists("Releases\Streamlink\rtmpdump") Then
                    IO.Directory.Move("Releases\Streamlink\rtmpdump", "Releases\Streamlink\Dependencies\rtmpdump")
                End If
                If IO.File.Exists("Releases\Streamlink\LICENSE.txt") Then
                    IO.File.Move("Releases\Streamlink\LICENSE.txt", "Releases\LICENSE.txt")
                End If

                If Button1.Text = "Portable BAT" Then
                    IO.File.Copy("Files\Resources\BAT_BUILD.txt", "Releases\Streamlink.bat", True)
                    DeleteDirectoryIfExists("Releases\TEMP_COMPILE_FILES")
                    DeleteFileIfExists("Releases\TEMP_COMPILE.vb")
                End If

                If Button1.Text = "Portable EXE" Then
                    CompileCode(CodeDomProvider, "Files\Resources\PORTABLE_BUILD.vb", "Releases\Streamlink.exe", "PORTABLE_EXE", "Files\Resources\BUILD_DEPENDENCIES.txt")
                    DeleteDirectoryIfExists("Releases\TEMP_COMPILE_FILES")
                    DeleteFileIfExists("Releases\TEMP_COMPILE.vb")
                End If

                If Button1.Text = "Standalone EXE" Then
                    RunAndWait("Files\7zip\7za.exe", "a " & Chr(34) & Current_EXE_Path & "\Releases\Streamlink_Release.zip" & Chr(34) & " " & Chr(34) & Current_EXE_Path & "\Releases\*" & Chr(34)) 'Si hay que comprimir
                    CompileCode(CodeDomProvider, "Files\Resources\STANDALONE_BUILD.vb", "Releases\Streamlink.exe", "STANDALONE_EXE", "Files\Resources\BUILD_DEPENDENCIES.txt")

                    For Each File_TMP As String In IO.Directory.GetFiles("Releases", "*.*", SearchOption.TopDirectoryOnly)
                        If (File_TMP.EndsWith("Streamlink.exe") Or File_TMP.EndsWith("LICENSE.txt") Or File_TMP.EndsWith("README.txt") Or File_TMP.EndsWith("VERSION.txt") Or File_TMP.EndsWith("streamlinkrc")) = False Then
                            IO.File.Delete(File_TMP)
                        End If
                    Next

                    For Each Folder_TMP As String In IO.Directory.GetDirectories("Releases")
                        IO.Directory.Delete(Folder_TMP, True)
                    Next

                End If

                If IO.File.Exists("Files\TEMP\streamlinkrc_BACKUP") Then
                    FileSystem.Rename("Releases\streamlinkrc", "Releases\streamlinkrc_ORIGINAL")
                    IO.File.Move("Files\TEMP\streamlinkrc_BACKUP", "Releases\streamlinkrc")
                    If getFileMD5("Releases\streamlinkrc") = getFileMD5("Releases\streamlinkrc_ORIGINAL") Then
                        IO.File.Delete("Releases\streamlinkrc_ORIGINAL")
                    End If
                End If

                'DeleteDirectoryIfExists("Files\TEMP")
                Unlock_All_EXE("Releases")

                IO.File.WriteAllText("Files\TEMP\Streamlink_Latest_MD5.txt", getFileMD5("Files\TEMP\Streamlink_Latest.zip"), Encoding.UTF8)
                IO.File.WriteAllText("Files\TEMP\Streamlink_Latest_GIT.txt", CURRENT_VERSION, Encoding.UTF8)

                Button3.Text = "Completed"

                Dim Release_Version_Info As String = RELEASE_VER_CURRENT
                If String.IsNullOrEmpty(Release_Version_Info) Then
                    Release_Version_Info = CURRENT_VERSION.Remove(7)
                Else
                    If URL_ZIP_ARCHIVE_CURRENT = "https://github.com/streamlink/streamlink/archive/master.zip" Then
                        Release_Version_Info = RELEASE_VER_CURRENT & " (with the latest commits)"
                    End If
                End If

                If String.IsNullOrEmpty(MISSED_DEPENDENCIES) Then
                    Msgbox_THREADSAFE("Release " & Release_Version_Info & " was successfully built." & vbNewLine & "You can find it inside the Releases folder.", MsgBoxStyle.Information, "Notice")
                Else
                    IO.File.WriteAllText("Releases\ERRORS.txt", "The following dependencies are probably missing:" & vbNewLine & MISSED_DEPENDENCIES & vbNewLine & "Possible solutions:" & vbNewLine & "-Check if a new version is available at https://github.com/streamlink/streamlink-portable" & vbNewLine & "-Manually add the dependencies to 'Files\Resources\Streamlink_Patches.zip' and build again." & vbNewLine & "-Use an older ZIP archive rather than latest snapshot")
                    Msgbox_THREADSAFE("Release " & Release_Version_Info & " was built with some errors." & vbNewLine & "You can find it inside the Releases folder." & vbNewLine & "Check ERRORS.txt for more info.", MsgBoxStyle.Exclamation, "Notice")
                End If

                'Reset all values (to generate a new version)
                Button2.Text = "Start downloading"
                ChangeObjectVisibility_THREADSAFE(Button4, True)
                AdjustPaddingButtonCustomURL()
                Button3.Text = "Pending"
                '

            Catch
                Button3.Text = "Start building"
            End Try
        End If

    End Sub

    Sub ChangeObjectVisibility_THREADSAFE(ByVal Req_Object As Object, ByVal Visibility As Boolean)
        Me.Invoke(DirectCast(Sub()
                                 Req_Object.visible = Visibility
                             End Sub, MethodInvoker))
    End Sub

    Private Function getFileMD5(ByVal filePath As String) As String
        ' Get all the file contents
        Dim File() As Byte = System.IO.File.ReadAllBytes(filePath)

        ' Create a new MD5 object
        Dim MD5 As New MD5CryptoServiceProvider()

        ' Compute the hash
        Dim byteHash() As Byte = MD5.ComputeHash(File)

        ' Return the value in base 64 
        Return Convert.ToBase64String(byteHash)
    End Function


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

    Sub MoveAllItems(ByVal fromPath As String, ByVal toPath As String)
        My.Computer.FileSystem.MoveDirectory(fromPath, toPath, True)

        ''Create the target directory if necessary
        'Dim toPathInfo = New DirectoryInfo(toPath)
        'If (Not toPathInfo.Exists) Then
        '    toPathInfo.Create()
        'End If
        'Dim fromPathInfo = New DirectoryInfo(fromPath)
        ''Move all files
        'For Each file As FileInfo In fromPathInfo.GetFiles()
        '    file.MoveTo(Path.Combine(toPath, file.Name))
        'Next
        ''Move all folders
        'For Each dir As DirectoryInfo In fromPathInfo.GetDirectories()
        '    dir.MoveTo(Path.Combine(toPath, dir.Name))
        'Next
    End Sub


    Function GetETAG_HTTPHEADER(ByVal URL As String) As String
        ' Creates an HttpWebRequest with the specified URL. 
        myHttpWebRequest = CType(WebRequest.Create(URL), HttpWebRequest)
        'Set request method
        myHttpWebRequest.Method = "HEAD"
        ' Sends the HttpWebRequest and waits for a response.
        myHttpWebResponse = CType(myHttpWebRequest.GetResponse(), HttpWebResponse)
        ' Displays all the Headers present in the response received from the URI.
        Console.WriteLine(ControlChars.Lf + ControlChars.Cr + "The following headers were received in the response")
        'The Headers property is a WebHeaderCollection. Use it's properties to traverse the collection and display each header.
        Dim i As Integer
        Dim ETAG As String = ""
        While i < myHttpWebResponse.Headers.Count
            i = i + 1
            If myHttpWebResponse.Headers.Keys(i) = "ETag" Then
                ETAG = myHttpWebResponse.Headers(i)
                ETAG = ETAG.Replace(Chr(34), "")
                i = myHttpWebResponse.Headers.Count
            End If
        End While
        Return ETAG
        myHttpWebRequest.Abort()
        myHttpWebResponse.Close()
        myHttpWebResponse.Dispose()
    End Function

    Sub THREADSAFE_CALL(ByVal Funcion As MethodInvoker)
        Me.Invoke(DirectCast(Funcion, MethodInvoker))
        'Ejemplo:
        'THREADSAFE_CALL(Sub()
        ' MsgBox("Prueba")
        ' End Sub)
    End Sub

    Sub Cancel_Internet_Interactions()
        On Error Resume Next
        DATA_Downloader.CancelAsync()
        'DATA_Downloader.Dispose()
        myHttpWebRequest.Abort()
        myHttpWebResponse.Close()
        myHttpWebResponse.Dispose()
    End Sub

    Private Sub Form1_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        On Error Resume Next
        DATA_Downloader.CancelAsync()
        DATA_Downloader.Dispose()
        BW_STEPS.CancelAsync()
        BW_STEPS.CancelImmediately()
        BW_STEPS.Dispose()
        Process.GetCurrentProcess.Kill()
    End Sub

    Function Msgbox_THREADSAFE(ByVal Message As String, ByVal Style As MsgBoxStyle, ByVal Title As String)
        THREADSAFE_CALL(Sub()
                            MsgBox(Message, Style, Title)
                        End Sub)
    End Function

    Function Msgbox_Interactive_THREADSAFE(ByVal Message As String, ByVal Title As String, ByVal Buttons As MessageBoxButtons, ByVal Icon As MessageBoxIcon)
        Dim msgbox_result As Integer = MsgBoxResult.No
        THREADSAFE_CALL(Sub()
                            msgbox_result = MessageBox.Show(Message, Title, Buttons, Icon)
                        End Sub)
        Return msgbox_result
    End Function

    Function RunAndWait(ByVal Path As String, ByVal Arguments As String)

        If IO.File.Exists(Path) = False Then
            Msgbox_THREADSAFE("A required file is missing :(", MsgBoxStyle.Critical, "Error")
            Process.GetCurrentProcess.Kill()
        End If

        Dim umaka As New Process
        umaka.StartInfo.FileName = Path
        umaka.StartInfo.Arguments = Arguments
        umaka.StartInfo.WindowStyle = ProcessWindowStyle.Hidden
        umaka.Start()
        umaka.WaitForExit()
    End Function

    Private Sub Form1_KeyUp(sender As Object, e As KeyEventArgs) Handles Me.KeyUp
        If e.KeyCode = Keys.Escape Then
            'If ESC is pressed, focus is removed
            Panel2.Focus()
            Panel2.Select()
            '
        End If
    End Sub

    Public Shared Function CompileCode(ByVal provider As CodeDomProvider, ByVal sourceFile As String, ByVal exeFile As String, ByVal REQUESTED_PROJECT As String, ByVal ProjectDependenciesFile As String) As Boolean

        ' Configure dependencies and imports based on the current project
        Dim DLL_References_List As New List(Of String)
        Dim DLL_Calls_Imports_List As New List(Of String)
        Dim Projectpath As String = Current_EXE_Path
        Projectpath = Projectpath.Replace("/", "\")
        Projectpath = Projectpath.Remove(Projectpath.LastIndexOf("\"))
        Projectpath = Projectpath.Remove(Projectpath.LastIndexOf("\"))
        Dim Found_PJ_Path As String = ProjectDependenciesFile
        If String.IsNullOrEmpty(Found_PJ_Path) Then
            For Each Possible_PJ_File As String In IO.Directory.GetFiles(Projectpath, "*.vbproj", IO.SearchOption.TopDirectoryOnly)
                Found_PJ_Path = Possible_PJ_File
            Next
        End If
        Dim analysis_pj As String = IO.File.ReadAllText(Found_PJ_Path, Encoding.UTF8)
        For Each line_analysis_pj As String In analysis_pj.Split({ControlChars.Cr, ControlChars.Lf})
            If line_analysis_pj.Contains("<Reference Include=") Then
                line_analysis_pj = line_analysis_pj.Remove(0, line_analysis_pj.IndexOf(Chr(34)) + 1)
                line_analysis_pj = line_analysis_pj.Remove(line_analysis_pj.IndexOf(Chr(34)))
                line_analysis_pj += ".dll"
                DLL_References_List.Add(line_analysis_pj)
            End If
            If line_analysis_pj.Contains("<Import Include=") Then
                line_analysis_pj = line_analysis_pj.Remove(0, line_analysis_pj.IndexOf(Chr(34)) + 1)
                line_analysis_pj = line_analysis_pj.Remove(line_analysis_pj.IndexOf(Chr(34)))
                DLL_Calls_Imports_List.Add(line_analysis_pj.ToLower)
            End If
        Next

        Dim analysis_class As String = IO.File.ReadAllText(sourceFile, Encoding.UTF8)
        For Each line_analysis_class As String In analysis_class.Split({ControlChars.Cr, ControlChars.Lf})
            line_analysis_class = line_analysis_class.Replace(" ", "")
            line_analysis_class = line_analysis_class.ToLower
            If String.IsNullOrEmpty(line_analysis_class) = False Then
                If line_analysis_class.StartsWith("imports") Then
                    line_analysis_class = line_analysis_class.Remove(0, line_analysis_class.IndexOf("imports") + 7)
                    If DLL_Calls_Imports_List.Contains(line_analysis_class) Then
                        DLL_Calls_Imports_List.Remove(line_analysis_class)
                    End If
                End If
            End If
        Next

        Dim generic_imports As String = ""
        For Each imports_final As String In DLL_Calls_Imports_List
            generic_imports += "Imports " & imports_final & vbNewLine
        Next

        If String.IsNullOrEmpty(generic_imports) = False Then
            analysis_class = "'Declaraciones genericas" & vbNewLine & generic_imports & "'" & vbNewLine & analysis_class
        End If

        IO.File.WriteAllText("Releases\TEMP_COMPILE.vb", analysis_class, Encoding.UTF8)
        sourceFile = "Releases\TEMP_COMPILE.vb"
        If REQUESTED_PROJECT = "STANDALONE_EXE" Then
            DLL_References_List.Add("System.IO.Compression.dll") 'Add ZIP support (required)
            DLL_References_List.Add("System.IO.Compression.FileSystem.dll") 'Add ZIP support (required)
        End If
        Dim referenceAssemblies As String() = DLL_References_List.ToArray
        '

        Dim cp As New CompilerParameters(referenceAssemblies, exeFile, False)

        ' Generate an executable instead of 
        ' a class library.
        cp.GenerateExecutable = True

        ' Set the assembly file name to generate.
        cp.OutputAssembly = exeFile

        ' Generate debug information.
        cp.IncludeDebugInformation = False

        ' Save the assembly as a physical file.
        cp.GenerateInMemory = False

        ' Set the level at which the compiler 
        ' should start displaying warnings.
        cp.WarningLevel = 3

        ' Set whether to treat all warnings as errors.
        cp.TreatWarningsAsErrors = False

        ' Set compiler argument to optimize output.
        cp.CompilerOptions = "/optimize /win32icon:" & Chr(34) & "Files\Resources\Streamlink Logo.ico" & Chr(34)

        ' Set a temporary files collection.
        ' The TempFileCollection stores the temporary files
        ' generated during a build in the current directory,
        ' and does not delete them after compilation.
        IO.Directory.CreateDirectory("Releases\TEMP_COMPILE_FILES")
        cp.TempFiles = New TempFileCollection("Releases\TEMP_COMPILE_FILES", True)

        'If provider.Supports(GeneratorSupport.EntryPointMethod) Then
        '    ' Specify the class that contains
        '    ' the main method of the executable.
        'cp.MainClass = "Module1"
        'End If

        'Generate needed resources
        If REQUESTED_PROJECT = "STANDALONE_EXE" Then
            cp.EmbeddedResources.Add("Releases\STREAMLINK_RELEASE.zip")
            cp.EmbeddedResources.Add("Releases\VERSION.txt")
            IO.File.WriteAllText("Releases\RANDOM_ID.txt", New RandomPassword().Generate(20))
            cp.EmbeddedResources.Add("Releases\RANDOM_ID.txt")
        End If
        '

        ' Invoke compilation.
        Dim cr As CompilerResults =
        provider.CompileAssemblyFromFile(cp, sourceFile)

        If cr.Errors.Count > 0 Then
            ' Display compilation errors.
            Console.WriteLine("Errors building {0} into {1}",
            sourceFile, cr.PathToAssembly)
            Dim ce As CompilerError
            For Each ce In cr.Errors
                Console.WriteLine("  {0}", ce.ToString())
                Console.WriteLine()
            Next ce
        Else
            Console.WriteLine("Source {0} built into {1} successfully.",
            sourceFile, cr.PathToAssembly)
            Console.WriteLine("{0} temporary files created during the compilation.",
                cp.TempFiles.Count.ToString())
        End If

        ' Return the results of compilation.
        If cr.Errors.Count > 0 Then
            Return False
        Else
            Return True
        End If
    End Function

    Private Sub Form1_Deactivate(sender As Object, e As EventArgs) Handles Me.Deactivate
        Panel2.Focus()
        Panel2.Select()
    End Sub

    Public Shared Function ExtractAssociatedIcon(filePath As [String]) As Icon
        Dim index As Integer = 0

        Dim uri As Uri
        If filePath Is Nothing Then
            Throw New ArgumentException([String].Format("'{0}' is not valid for '{1}'", "null", "filePath"), "filePath")
        End If
        Try
            uri = New Uri(filePath)
        Catch generatedExceptionName As UriFormatException
            filePath = Path.GetFullPath(filePath)
            uri = New Uri(filePath)
        End Try
        'if (uri.IsUnc)
        '{
        '  throw new ArgumentException(String.Format("'{0}' is not valid for '{1}'", filePath, "filePath"), "filePath");
        '}
        If uri.IsFile Then
            If Not File.Exists(filePath) Then
                'IntSecurity.DemandReadFileIO(filePath);
                Throw New FileNotFoundException(filePath)
            End If

            Dim iconPath As New StringBuilder(260)
            iconPath.Append(filePath)

            Dim handle As IntPtr = SafeNativeMethods.ExtractAssociatedIcon(New HandleRef(Nothing, IntPtr.Zero), iconPath, index)
            If handle <> IntPtr.Zero Then
                'IntSecurity.ObjectFromWin32Handle.Demand();
                Return Icon.FromHandle(handle)
            End If
        End If
        Return Nothing
    End Function

    Private Sub DATA_Downloader_DownloadProgressChanged(sender As Object, e As DownloadProgressChangedEventArgs) Handles DATA_Downloader.DownloadProgressChanged
        DATA_Downloader_CurrentProgress = e.ProgressPercentage & "%"
    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        If Button2.Text = "Start downloading" Then
            If BW_STEPS.IsBusy = False Then
                Dim URL_ZIP_ARCHIVE_TEMP As String = InputBox_General_Custom.AbrirInputBox("Custom download", "Enter a ZIP archive URL", URL_ZIP_ARCHIVE_CURRENT, Me)
                If String.IsNullOrWhiteSpace(URL_ZIP_ARCHIVE_TEMP) = False Then
                    If URL_ZIP_ARCHIVE_TEMP.ToLower.StartsWith("http") Then
                        URL_ZIP_ARCHIVE_CURRENT = URL_ZIP_ARCHIVE_TEMP
                    End If
                End If
            End If
        End If
    End Sub

End Class

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

<SuppressUnmanagedCodeSecurity>
Friend NotInheritable Class SafeNativeMethods
    Private Sub New()
    End Sub
    <DllImport("shell32.dll", EntryPoint:="ExtractAssociatedIcon", CharSet:=CharSet.Auto)>
    Friend Shared Function ExtractAssociatedIcon(hInst As HandleRef, iconPath As StringBuilder, ByRef index As Integer) As IntPtr
    End Function
End Class
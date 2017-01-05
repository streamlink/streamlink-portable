Imports System.ComponentModel
Imports System.IO
Imports System.Net
Imports System.Reflection
Imports System.Runtime.InteropServices
Imports System.Security.Cryptography
Imports System.CodeDom.Compiler
Imports System.Text
Imports System.Security

Public Class Form1
    Public Shared CodeDomProvider As CodeDomProvider = CodeDomProvider.CreateProvider("VB")
    Public Shared url_trabajo_app As String = IO.Path.GetDirectoryName(Application.ExecutablePath)
    Dim WithEvents Descargador_HTML As New WebClient() 'Descargador de datos
    Dim UserAgent_1 As String = "Mozilla/5.0 (Android; Mobile; rv:30.0) Gecko/30.0 Firefox/30.0" 'Android (Mobile)
    Dim UserAgent_2 As String = "Dalvik/1.6.0 (Linux; U; Android 4.4.2; TegraNote-P1640 Build/KOT49H)" 'Android (Tablet)
    Dim UserAgent_3 As String = "Mozilla/5.0 (Windows NT 6.3; rv:36.0) Gecko/20100101 Firefox/36.0" 'Windows (Escritorio)

    'Para tareas que requieren WebRequest y WebResponse
    Dim myHttpWebRequest As HttpWebRequest
    Dim myHttpWebResponse As HttpWebResponse
    '

    'Almacen de datos actuales
    Dim VERSION_ACTUAL As String = ""
    Dim RELEASE_VER_ACTUAL As String = ""
    '

    Public WithEvents BW_PASOS As New ExtendedBackgroundWorker
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Me.KeyPreview = True
        Me.Icon = ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location) 'El icono sera el mismo que el de la aplicacion
        Me.DoubleBuffered = True 'Evitar que ocurra flickering en la UI
        Me.ClientSize = Panel2.ClientSize
        Me.CenterToScreen()
        CheckForIllegalCrossThreadCalls = False 'Activar manejo total por parte de backgroundworkers
        IO.Directory.SetCurrentDirectory(url_trabajo_app)

        'Averiguar si hay permisos de escritura
        If Averiguar_Permisos_Escritura() = False Then
            Msgbox_THREADSAFE("I dont have write permissions :(" & vbNewLine & "Try running me with administrator rights.", MsgBoxStyle.Critical, "Error")
            Application.Exit()
            Application.ExitThread()
            Return
        End If
        '

        Desbloquear_Todos_Los_EXE("Files")

        For Each botoncito As Control In Panel2.Controls
            If TypeOf (botoncito) Is Button Then
                botoncito.TabIndex = 0
                botoncito.TabStop = False
            End If
        Next

    End Sub

    Public Function Averiguar_Permisos_Escritura() As Boolean
        Try
            IO.File.WriteAllText("TEST.rtv", "", Encoding.UTF8)
            IO.File.Delete("TEST.rtv")
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function GetPageHTMLCustom(ByVal URL As String, ByVal UserAgent_Custom As String, ByVal Referer_Custom As String, Optional ByVal Cookies As String = "") As String
        Dim RESULTADO As String = ""
        Descargador_HTML.Encoding = Encoding.UTF8
        Descargador_HTML.Headers.Clear()
        Descargador_HTML.Headers(Net.HttpRequestHeader.Referer) = Referer_Custom
        Descargador_HTML.Headers(Net.HttpRequestHeader.UserAgent) = UserAgent_Custom
        Descargador_HTML.Headers(Net.HttpRequestHeader.Cookie) = Cookies
        Try
            RESULTADO = Descargador_HTML.DownloadString(URL) 'Intentar obtener resultado de manera directa
        Catch falla As WebException 'Intentar obtener resultado ignorando errores
            Using sr = New StreamReader(falla.Response.GetResponseStream())
                RESULTADO = sr.ReadToEnd()
            End Using
        End Try
        Return RESULTADO
    End Function

    Public Function GetPageHTMLCustom_POST(ByVal POST As String, ByVal URL As String, ByVal UserAgent_Custom As String, ByVal Referer_Custom As String, Optional ByVal Cookies As String = "") As String
        Dim RESULTADO As String = ""
        Descargador_HTML.Encoding = Encoding.UTF8
        Descargador_HTML.Headers.Clear()
        Descargador_HTML.Headers(Net.HttpRequestHeader.Referer) = Referer_Custom
        Descargador_HTML.Headers(Net.HttpRequestHeader.UserAgent) = UserAgent_Custom
        Descargador_HTML.Headers(Net.HttpRequestHeader.Cookie) = Cookies
        Descargador_HTML.Headers.Add("Content-Type: application/x-www-form-urlencoded; charset=UTF-8")

        Try
            RESULTADO = Descargador_HTML.UploadString(URL, POST) 'Intentar obtener resultado de manera directa
        Catch falla As WebException 'Intentar obtener resultado ignorando errores
            Using sr = New StreamReader(falla.Response.GetResponseStream())
                RESULTADO = sr.ReadToEnd()
            End Using
        End Try
        Return RESULTADO
    End Function

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Form1_Deactivate(Nothing, Nothing)
        If Button2.Text = "Start downloading" Then
            If sender.text = "Portable EXE" Then
                Button1.Text = "Standalone EXE"
            Else
                Button1.Text = "Portable EXE"
            End If
        End If
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Form1_Deactivate(Nothing, Nothing)
        If sender.Text = "Start downloading" Then
            If BW_PASOS.IsBusy = False Then
                BW_PASOS.RunWorkerAsync("PASO1")
            End If
        End If
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        Form1_Deactivate(Nothing, Nothing)
        If Button2.Text = "Completed" And sender.text = "Start building" Then
            If BW_PASOS.IsBusy = False Then
                BW_PASOS.RunWorkerAsync("PASO2")
            End If
        End If
    End Sub

    Sub BorrarArchivoSiExiste(ByVal URL As String)
        If IO.File.Exists(URL) Then
            IO.File.Delete(URL)
        End If
    End Sub

    Sub BorrarDirectorioSiExiste(ByVal URL As String)
        If IO.Directory.Exists(URL) Then
            IO.Directory.Delete(URL, True)
        End If
    End Sub

    Private Sub BW_PASOS_DoWork(sender As Object, e As DoWorkEventArgs) Handles BW_PASOS.DoWork
        CancelarInteraccionesInternet()
        If e.Argument = "PASO1" Then
            Try

                Button2.Text = "Loading (1/1)"

                'BorrarDirectorioSiExiste("Files\TEMP")
                IO.Directory.CreateDirectory("Files\TEMP")

                Dim nueva_version_disponible As Boolean = True
                Dim strlk_url As String = "https://github.com/streamlink/streamlink/archive/master.zip"
                VERSION_ACTUAL = ObtenerETAG_HTTPHEADER(strlk_url)

                If IO.Directory.Exists("Releases") Then
                    If IO.File.Exists("Releases\VERSION.txt") Then
                        Dim vers_check As String = IO.File.ReadAllText("Releases\VERSION.txt")
                        If vers_check.Contains(VERSION_ACTUAL.Remove(7)) Then
                            nueva_version_disponible = False
                            Dim result As Integer = Msgbox_Interactive_THREADSAFE("It looks like you already have the latest version." & vbNewLine & "Continue anyway?", "Notice", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                            If result = DialogResult.Yes Then
                                'El usuario acepto actualizar de todas formas
                            Else
                                Button2.Text = "Start downloading"
                                Return
                            End If
                        End If
                    End If
                End If

                Dim redescargar_latest_streamlink_zip As Boolean = True
                If nueva_version_disponible = False And IO.File.Exists("Files\TEMP\Streamlink_Latest_MD5.txt") And IO.File.Exists("Files\TEMP\Streamlink_Latest.zip") Then
                    Dim md5_anterior As String = IO.File.ReadAllText("Files\TEMP\Streamlink_Latest_MD5.txt", Encoding.UTF8)
                    Dim md5_actual As String = getFileMd5("Files\TEMP\Streamlink_Latest.zip")
                    If md5_anterior = md5_actual Then
                        redescargar_latest_streamlink_zip = False
                    End If
                End If
                If redescargar_latest_streamlink_zip = True Then
                    BorrarDirectorioSiExiste("Files\TEMP")
                    IO.Directory.CreateDirectory("Files\TEMP")
                    Descargador_HTML.DownloadFile(strlk_url, "Files\TEMP\Streamlink_Latest.zip")
                End If

                Button2.Text = "Completed"
                Button3.Text = "Start building"

            Catch
                Button2.Text = "Start downloading"
            End Try
        End If

        If e.Argument = "PASO2" Then
            Try
                Button3.Text = "Loading (1/3)"

                Dim ruta_comprimido As String = Chr(34) & url_trabajo_app & "\Files\TEMP\Streamlink_Latest.zip" & Chr(34)
                Dim destino_comprimido As String = Chr(34) & url_trabajo_app & "\Files\TEMP" & Chr(34)
                BorrarDirectorioSiExiste("Files\TEMP\streamlink-master")
                EjecutarYEsperar("Files\7zip\7za.exe", "-y x " & ruta_comprimido & " -o" & destino_comprimido) 'Si hay que descomprimir


                For Each archivin As String In IO.Directory.GetFiles("Files\TEMP\streamlink-master", "*.*", SearchOption.TopDirectoryOnly)
                    IO.File.Delete(archivin)
                Next

                For Each carpetin As String In IO.Directory.GetDirectories("Files\TEMP\streamlink-master")
                    carpetin = carpetin.Replace("/", "\")
                    If (carpetin.EndsWith("\src") Or carpetin.EndsWith("\win32")) = False Then
                        IO.Directory.Delete(carpetin, True)
                    End If
                Next

                MoveAllItems("Files\TEMP\streamlink-master\src", "Files\TEMP\streamlink-master")
                MoveAllItems("Files\TEMP\streamlink-master\win32", "Files\TEMP\streamlink-master")
                BorrarDirectorioSiExiste("Files\TEMP\streamlink-master\src")
                BorrarDirectorioSiExiste("Files\TEMP\streamlink-master\win32")

                For Each barrido_post_extract_file As String In Directory.GetFiles("Files\TEMP\streamlink-master", "*.*", SearchOption.TopDirectoryOnly).Where(Function(s) s.ToLower.EndsWith(".ico")) 'OrElse s.EndsWith(".png"))
                    IO.File.Delete(barrido_post_extract_file)
                Next

                Dim argparser_py_location As String = "Files\TEMP\streamlink-master\streamlink_cli\argparser.py"
                Dim argparser_py As String = IO.File.ReadAllText(argparser_py_location, Encoding.UTF8)
                Dim argparser_py_replace As String = "%(prog)s"
                Dim argparser_py_replace_end As String = "Streamlink.exe"
                argparser_py = argparser_py.Replace(argparser_py_replace, argparser_py_replace_end)
                IO.File.WriteAllText(argparser_py_location, argparser_py, Encoding.UTF8)

                ruta_comprimido = Chr(34) & url_trabajo_app & "\Files\Resources\Streamlink_Patches.zip" & Chr(34)
                destino_comprimido = Chr(34) & url_trabajo_app & "\Files\TEMP\streamlink-master" & Chr(34)
                EjecutarYEsperar("Files\7zip\7za.exe", "-y x " & ruta_comprimido & " -o" & destino_comprimido) 'Si hay que descomprimir

                Button3.Text = "Loading (2/3)"

                Finalizar_Todos_Los_EXE("Releases")
                If IO.File.Exists("Releases\streamlinkrc") Then
                    IO.File.Copy("Releases\streamlinkrc", "Files\TEMP\streamlinkrc_BACKUP")
                End If
                BorrarDirectorioSiExiste("Releases")
                IO.Directory.CreateDirectory("Releases\Python 3.5.2")

                MoveAllItems("Files\TEMP\streamlink-master", "Releases\Streamlink")
                BorrarDirectorioSiExiste("Files\TEMP\streamlink-master")

                ruta_comprimido = Chr(34) & url_trabajo_app & "\Files\Resources\python-3.5.2-embed-win32.zip" & Chr(34)
                destino_comprimido = Chr(34) & url_trabajo_app & "\Releases\Python 3.5.2" & Chr(34)
                EjecutarYEsperar("Files\7zip\7za.exe", "-y x " & ruta_comprimido & " -o" & destino_comprimido) 'Si hay que descomprimir

                Button3.Text = "Loading (3/3)"

                Dim README_CONTENT As String = "Usage from cmd:" & vbNewLine & "Streamlink.exe ARGUMENTS" & vbNewLine & vbNewLine & "For more info visit https://github.com/streamlink/streamlink or https://streamlink.github.io"
                IO.File.WriteAllText("Releases\README.txt", README_CONTENT)

                RELEASE_VER_ACTUAL = "Releases\Streamlink\streamlink\__init__.py"
                If IO.File.Exists(RELEASE_VER_ACTUAL) Then
                    Try
                        RELEASE_VER_ACTUAL = IO.File.ReadAllText(RELEASE_VER_ACTUAL, Encoding.UTF8)
                        RELEASE_VER_ACTUAL = RELEASE_VER_ACTUAL.ToLower.Remove(0, RELEASE_VER_ACTUAL.IndexOf("__version__ = ") + 15)
                        RELEASE_VER_ACTUAL = RELEASE_VER_ACTUAL.Remove(RELEASE_VER_ACTUAL.IndexOf(Chr(34)))
                    Catch
                        RELEASE_VER_ACTUAL = ""
                    End Try
                Else
                    RELEASE_VER_ACTUAL = ""
                End If

                Dim version_txt_out As String = ""
                If String.IsNullOrEmpty(RELEASE_VER_ACTUAL) Then
                    version_txt_out = "Git " & VERSION_ACTUAL.Remove(7)
                Else
                    version_txt_out = "v" & RELEASE_VER_ACTUAL & " - Git " & VERSION_ACTUAL.Remove(7)
                End If
                IO.File.WriteAllText("Releases\VERSION.txt", version_txt_out)

                If IO.File.Exists("Releases\Streamlink\streamlinkrc") Then
                    Dim textbox_analisis_streamlinkrc As New TextBox
                    Dim streamlinkrc_final_modded As String = ""
                    textbox_analisis_streamlinkrc.Text = IO.File.ReadAllText("Releases\Streamlink\streamlinkrc", Encoding.UTF8)
                    For i_linea_analisis_streamlinkrc As Integer = 0 To textbox_analisis_streamlinkrc.Lines.Count - 1
                        Dim linea_analisis_streamlinkrc As String = textbox_analisis_streamlinkrc.Lines(i_linea_analisis_streamlinkrc)
                        If String.IsNullOrWhiteSpace(linea_analisis_streamlinkrc) = False Then
                            If linea_analisis_streamlinkrc.Replace(" ", "").StartsWith("#") = False Then
                                linea_analisis_streamlinkrc = "#" & linea_analisis_streamlinkrc
                            End If
                        End If
                        streamlinkrc_final_modded += linea_analisis_streamlinkrc & vbNewLine
                    Next
                    textbox_analisis_streamlinkrc.Dispose()
                    IO.File.WriteAllText("Releases\streamlinkrc", streamlinkrc_final_modded, Encoding.UTF8)
                    IO.File.Delete("Releases\Streamlink\streamlinkrc")
                End If

                If Button1.Text = "Portable EXE" Then
                    CompileCode(CodeDomProvider, "Files\Resources\PORTABLE_BUILD.vb", "Releases\Streamlink.exe", "PORTABLE_EXE", "Files\Resources\BUILD_DEPENDENCIES.txt")
                    BorrarDirectorioSiExiste("Releases\TEMP_COMPILE_FILES")
                    BorrarArchivoSiExiste("Releases\TEMP_COMPILE.vb")
                End If

                If Button1.Text = "Standalone EXE" Then

                    EjecutarYEsperar("Files\7zip\7za.exe", "a " & Chr(34) & url_trabajo_app & "\Releases\Streamlink_Release.zip" & Chr(34) & " " & Chr(34) & url_trabajo_app & "\Releases\*" & Chr(34)) 'Si hay que comprimir
                    CompileCode(CodeDomProvider, "Files\Resources\STANDALONE_BUILD.vb", "Releases\Streamlink.exe", "STANDALONE_EXE", "Files\Resources\BUILD_DEPENDENCIES.txt")

                    For Each archivin As String In IO.Directory.GetFiles("Releases", "*.*", SearchOption.TopDirectoryOnly)
                        If (archivin.EndsWith("Streamlink.exe") Or archivin.EndsWith("README.txt") Or archivin.EndsWith("VERSION.txt") Or archivin.EndsWith("streamlinkrc")) = False Then
                            IO.File.Delete(archivin)
                        End If
                    Next

                    For Each carpetin As String In IO.Directory.GetDirectories("Releases")
                        IO.Directory.Delete(carpetin, True)
                    Next

                End If

                If IO.File.Exists("Files\TEMP\streamlinkrc_BACKUP") Then
                    FileSystem.Rename("Releases\streamlinkrc", "Releases\streamlinkrc_ORIGINAL")
                    IO.File.Move("Files\TEMP\streamlinkrc_BACKUP", "Releases\streamlinkrc")
                    If getFileMd5("Releases\streamlinkrc") = getFileMd5("Releases\streamlinkrc_ORIGINAL") Then
                        IO.File.Delete("Releases\streamlinkrc_ORIGINAL")
                    End If
                End If

                'BorrarDirectorioSiExiste("Files\TEMP")
                Desbloquear_Todos_Los_EXE("Releases")

                IO.File.WriteAllText("Files\TEMP\Streamlink_Latest_MD5.txt", getFileMd5("Files\TEMP\Streamlink_Latest.zip"), Encoding.UTF8)

                Button3.Text = "Completed"

                Dim release_version_show As String = RELEASE_VER_ACTUAL
                If String.IsNullOrEmpty(release_version_show) Then
                    release_version_show = VERSION_ACTUAL.Remove(7)
                Else
                    release_version_show = RELEASE_VER_ACTUAL & " (with the latest commits)"
                End If

                Msgbox_THREADSAFE("Release " & release_version_show & " was successfully built." & vbNewLine & "You can find it inside the Releases folder.", MsgBoxStyle.Information, "Notice")

            Catch
                Button3.Text = "Start building"
            End Try
        End If

    End Sub

    Private Function getFileMd5(ByVal filePath As String) As String
        ' get all the file contents
        Dim File() As Byte = System.IO.File.ReadAllBytes(filePath)

        ' create a new md5 object
        Dim Md5 As New MD5CryptoServiceProvider()

        ' compute the hash
        Dim byteHash() As Byte = Md5.ComputeHash(File)

        ' return the value in base 64 
        Return Convert.ToBase64String(byteHash)
    End Function


    Function Finalizar_Todos_Los_EXE(ByVal ruta As String)
        Dim SourceDir As DirectoryInfo = New DirectoryInfo(ruta)
        Dim pathIndex As Integer

        If SourceDir.Exists Then
            pathIndex = ruta.LastIndexOf("\")
            For Each childFile As FileInfo In SourceDir.GetFiles("*", SearchOption.AllDirectories).Where(Function(file) file.Extension.ToLower = ".exe")
                For Each prog As Process In Process.GetProcesses
                    If prog.ProcessName = childFile.Name.Remove(childFile.Name.LastIndexOf(".")) Then
                        prog.Kill() 'Matar proceso encontrado en la ubicacion actual
                        prog.WaitForExit() 'Esperar hasta que el proceso se haya ido
                    End If
                Next
            Next
        Else
            Console.WriteLine("El directorio donde se deben finalizar los procesos no existe")
        End If

    End Function

    Function Desbloquear_Todos_Los_EXE(ByVal ruta As String)
        On Error Resume Next 'Solo usar para este programa, ya que lo normal es disparar un error
        Dim SourceDir As DirectoryInfo = New DirectoryInfo(ruta)
        Dim pathIndex As Integer

        If SourceDir.Exists Then
            pathIndex = ruta.LastIndexOf("\")
            For Each childFile As FileInfo In SourceDir.GetFiles("*", SearchOption.AllDirectories).Where(Function(file) file.Extension.ToLower = ".exe")
                FileUnblocker.UnblockFile(childFile.FullName) 'Desbloquear el .EXE actual
            Next
        Else
            Console.WriteLine("El directorio donde necesito desbloquear archivos no existe :(")
        End If
    End Function

    Sub MoveAllItems(ByVal fromPath As String, ByVal toPath As String)
        ''Create the target directory if necessary
        Dim toPathInfo = New DirectoryInfo(toPath)
        If (Not toPathInfo.Exists) Then
            toPathInfo.Create()
        End If
        Dim fromPathInfo = New DirectoryInfo(fromPath)
        ''move all files
        For Each file As FileInfo In fromPathInfo.GetFiles()
            file.MoveTo(Path.Combine(toPath, file.Name))
        Next
        ''move all folders
        For Each dir As DirectoryInfo In fromPathInfo.GetDirectories()
            dir.MoveTo(Path.Combine(toPath, dir.Name))
        Next
    End Sub


    Function ObtenerETAG_HTTPHEADER(ByVal URL As String) As String
        ' Creates an HttpWebRequest with the specified URL. 
        myHttpWebRequest = CType(WebRequest.Create(URL), HttpWebRequest)
        'Establecer el metodo de entrega
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

    Sub CancelarInteraccionesInternet()
        On Error Resume Next
        Descargador_HTML.CancelAsync()
        'Descargador_HTML.Dispose()
        myHttpWebRequest.Abort()
        myHttpWebResponse.Close()
        myHttpWebResponse.Dispose()
    End Sub

    Private Sub Form1_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        On Error Resume Next
        Descargador_HTML.CancelAsync()
        Descargador_HTML.Dispose()
        BW_PASOS.CancelAsync()
        BW_PASOS.CancelImmediately()
        BW_PASOS.Dispose()
        Process.GetCurrentProcess.Kill()
    End Sub

    Function Msgbox_THREADSAFE(ByVal Mensaje As String, ByVal Estilo As MsgBoxStyle, ByVal Titulo As String)
        THREADSAFE_CALL(Sub()
                            MsgBox(Mensaje, Estilo, Titulo)
                        End Sub)
    End Function

    Function Msgbox_Interactive_THREADSAFE(ByVal Mensaje As String, ByVal Titulo As String, ByVal Botones As MessageBoxButtons, ByVal Icono As MessageBoxIcon)
        Dim msgbox_result As Integer = MsgBoxResult.No
        THREADSAFE_CALL(Sub()
                            msgbox_result = MessageBox.Show(Mensaje, Titulo, Botones, Icono)
                        End Sub)
        Return msgbox_result
    End Function

    Function EjecutarYEsperar(ByVal Ruta As String, ByVal Argumentos As String)

        If IO.File.Exists(Ruta) = False Then
            Msgbox_THREADSAFE("A required file is missing :(", MsgBoxStyle.Critical, "Error")
            Process.GetCurrentProcess.Kill()
        End If

        Dim umaka As New Process
        umaka.StartInfo.FileName = Ruta
        umaka.StartInfo.Arguments = Argumentos
        umaka.StartInfo.WindowStyle = ProcessWindowStyle.Hidden
        umaka.Start()
        umaka.WaitForExit()
    End Function

    Private Sub Form1_KeyUp(sender As Object, e As KeyEventArgs) Handles Me.KeyUp
        If e.KeyCode = Keys.Escape Then
            'Si se presiona ESC se quita el foco
            Panel2.Focus()
            Panel2.Select()
            '
        End If
    End Sub

    Public Shared Function CompileCode(ByVal provider As CodeDomProvider, ByVal sourceFile As String, ByVal exeFile As String, ByVal PROYECTO_SOLICITADO As String, ByVal PROYECTO_DEPENDENCIAS_ARCHIVO As String) As Boolean

        ' Configurar dependencias e importaciones basadas en el proyecto actual
        Dim DLL_References_List As New List(Of String)
        Dim DLL_Calls_Imports_List As New List(Of String)
        Dim proyect_route As String = url_trabajo_app
        proyect_route = proyect_route.Replace("/", "\")
        proyect_route = proyect_route.Remove(proyect_route.LastIndexOf("\"))
        proyect_route = proyect_route.Remove(proyect_route.LastIndexOf("\"))
        Dim pj_route_encontrada As String = PROYECTO_DEPENDENCIAS_ARCHIVO
        If String.IsNullOrEmpty(pj_route_encontrada) Then
            For Each archivin_posible_pj As String In IO.Directory.GetFiles(proyect_route, "*.vbproj", IO.SearchOption.TopDirectoryOnly)
                pj_route_encontrada = archivin_posible_pj
            Next
        End If
        Dim analisis_pj As String = IO.File.ReadAllText(pj_route_encontrada, Encoding.UTF8)
        For Each linea_analisis_pj As String In analisis_pj.Split({ControlChars.Cr, ControlChars.Lf})
            If linea_analisis_pj.Contains("<Reference Include=") Then
                linea_analisis_pj = linea_analisis_pj.Remove(0, linea_analisis_pj.IndexOf(Chr(34)) + 1)
                linea_analisis_pj = linea_analisis_pj.Remove(linea_analisis_pj.IndexOf(Chr(34)))
                linea_analisis_pj += ".dll"
                DLL_References_List.Add(linea_analisis_pj)
            End If
            If linea_analisis_pj.Contains("<Import Include=") Then
                linea_analisis_pj = linea_analisis_pj.Remove(0, linea_analisis_pj.IndexOf(Chr(34)) + 1)
                linea_analisis_pj = linea_analisis_pj.Remove(linea_analisis_pj.IndexOf(Chr(34)))
                DLL_Calls_Imports_List.Add(linea_analisis_pj.ToLower)
            End If
        Next

        Dim analisis_class As String = IO.File.ReadAllText(sourceFile, Encoding.UTF8)
        For Each linea_analisis_class As String In analisis_class.Split({ControlChars.Cr, ControlChars.Lf})
            linea_analisis_class = linea_analisis_class.Replace(" ", "")
            linea_analisis_class = linea_analisis_class.ToLower
            If String.IsNullOrEmpty(linea_analisis_class) = False Then
                If linea_analisis_class.StartsWith("imports") Then
                    linea_analisis_class = linea_analisis_class.Remove(0, linea_analisis_class.IndexOf("imports") + 7)
                    If DLL_Calls_Imports_List.Contains(linea_analisis_class) Then
                        DLL_Calls_Imports_List.Remove(linea_analisis_class)
                    End If
                End If
            End If
        Next

        Dim importaciones_genericas As String = ""
        For Each imports_final As String In DLL_Calls_Imports_List
            importaciones_genericas += "Imports " & imports_final & vbNewLine
        Next

        If String.IsNullOrEmpty(importaciones_genericas) = False Then
            analisis_class = "'Declaraciones genericas" & vbNewLine & importaciones_genericas & "'" & vbNewLine & analisis_class
        End If

        IO.File.WriteAllText("Releases\TEMP_COMPILE.vb", analisis_class, Encoding.UTF8)
        sourceFile = "Releases\TEMP_COMPILE.vb"
        If PROYECTO_SOLICITADO = "STANDALONE_EXE" Then
            DLL_References_List.Add("System.IO.Compression.dll") 'Agregar soporte ZIP (obligatorio)
            DLL_References_List.Add("System.IO.Compression.FileSystem.dll") 'Agregar soporte ZIP (obligatorio)
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
        'cp.MainClass = "Samples.Class1"
        'End If

        'Generar recursos necesarios
        If PROYECTO_SOLICITADO = "STANDALONE_EXE" Then
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

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
    Public url_trabajo_app As String = System.Reflection.Assembly.GetExecutingAssembly().CodeBase
    Sub Main()
        Try
            SetConsoleCtrlHandler(New HandlerRoutine(AddressOf ControlHandler), True)
            url_trabajo_app = url_trabajo_app.Replace("file:///", "")
            url_trabajo_app = url_trabajo_app.Replace("file:\", "")
            url_trabajo_app = System.IO.Path.GetDirectoryName(url_trabajo_app)
            url_trabajo_app = url_trabajo_app.Replace("file:///", "")
            url_trabajo_app = url_trabajo_app.Replace("file:\", "")

            Dim deteccion_path As String = url_trabajo_app.Remove(3)
            deteccion_path = deteccion_path.Remove(0, 1)
            deteccion_path = deteccion_path.Replace("/", "\")
            If deteccion_path = ":\" = False And url_trabajo_app.StartsWith("\\") = False Then
                If url_trabajo_app.StartsWith("\") Then
                    url_trabajo_app = "\" & url_trabajo_app
                Else
                    url_trabajo_app = "\\" & url_trabajo_app
                End If
            End If

            Dim temp_patch_rtv As String = Path.GetTempPath & "\RTV_STREAMLINK_FOR_WINDOWS"
            Dim temp_zip_patch_rtv As String = temp_patch_rtv & "\STREAMLINK_RELEASE.zip"
            Try
                Console.OutputEncoding = Encoding.UTF8
                Console.InputEncoding = Encoding.UTF8
            Catch
                'Error al definir encoding
            End Try

            Dim argumentos_finales As String = Environment.CommandLine
            If argumentos_finales.StartsWith(Chr(34)) Then
                argumentos_finales = argumentos_finales.Remove(0, 1)
                argumentos_finales = argumentos_finales.Remove(0, argumentos_finales.IndexOf(Chr(34)))
                If argumentos_finales.Contains(" ") Then
                    argumentos_finales = argumentos_finales.Remove(0, argumentos_finales.IndexOf(" "))
                Else
                    argumentos_finales = ""
                End If
            Else
                If argumentos_finales.Contains(" ") Then
                    argumentos_finales = argumentos_finales.Remove(0, argumentos_finales.IndexOf(" "))
                Else
                    argumentos_finales = ""
                End If
            End If

            Dim requiere_extraccion As Boolean = True
            Dim RANDOM_ID_ACTUAL As String = ""

            If IO.Directory.Exists(temp_patch_rtv) = False Then
                IO.Directory.CreateDirectory(temp_patch_rtv) 'Asegurar existencia del directorio
            End If

            If IO.Directory.Exists(temp_patch_rtv) Then
                Dim temp_ver_anterior As String = temp_patch_rtv & "\VERSION.txt"
                Dim temp_ver_embed As String = temp_patch_rtv & "\VERSION_EMBED.txt"
                BorrarArchivoSiExiste(temp_ver_embed)
                Dim _assembly As [Assembly] = [Assembly].GetExecutingAssembly()
                Dim _filestream As Stream = _assembly.GetManifestResourceStream("VERSION.txt")
                CopyStream(_filestream, temp_ver_embed)
                If IO.file.exists(temp_ver_anterior) Then
                    If IO.File.ReadAllText(temp_ver_anterior, Encoding.UTF8) = IO.File.ReadAllText(temp_ver_embed, Encoding.UTF8) Then
                        requiere_extraccion = False
                    End If
                End If
                Dim temp_random_id_anterior As String = temp_patch_rtv & "\RANDOM_ID.txt"
                Dim temp_random_id_embed As String = temp_patch_rtv & "\RANDOM_ID_EMBED.txt"
                    BorrarArchivoSiExiste(temp_random_id_embed)
                    Dim _assembly2 As [Assembly] = [Assembly].GetExecutingAssembly()
                Dim _filestream2 As Stream = _assembly2.GetManifestResourceStream("RANDOM_ID.txt")
                CopyStream(_filestream2, temp_random_id_embed)
                RANDOM_ID_ACTUAL = IO.File.ReadAllText(temp_random_id_embed, Encoding.UTF8)
                    If IO.File.Exists(temp_random_id_anterior) Then
                        If IO.File.ReadAllText(temp_random_id_anterior, Encoding.UTF8) = IO.File.ReadAllText(temp_random_id_embed, Encoding.UTF8) Then
                            requiere_extraccion = False
                        Else
                            requiere_extraccion = True
                        End If
                    Else
                        requiere_extraccion = True
                    End If

                End If

                If requiere_extraccion = True Then
                Console.WriteLine("[Streamlink] Extracting program ...")
                Finalizar_Todos_Los_EXE(temp_patch_rtv)
                BorrarDirectorioSiExiste(temp_patch_rtv)
                IO.Directory.CreateDirectory(temp_patch_rtv)
                Dim _assembly As [Assembly] = [Assembly].GetExecutingAssembly()
                Dim _filestream As Stream = _assembly.GetManifestResourceStream("STREAMLINK_RELEASE.zip")
                CopyStream(_filestream, temp_zip_patch_rtv)
                DescomprimirArchivoZIP(temp_zip_patch_rtv, temp_patch_rtv)
                IO.File.Delete(temp_zip_patch_rtv)
                Desbloquear_Todos_Los_EXE(temp_patch_rtv) 'Intentar desbloquear ruta temporal de Streamlink
                Console.WriteLine("[Streamlink] Program successfully extracted")
                IO.File.WriteAllText(temp_patch_rtv & "\RANDOM_ID.txt", RANDOM_ID_ACTUAL, Encoding.UTF8)
            End If

            Dim DATOS_VER_ACTUAL As String = temp_patch_rtv & "\VERSION.txt"
            If IO.File.Exists(DATOS_VER_ACTUAL) Then
                DATOS_VER_ACTUAL = IO.File.ReadAllText(DATOS_VER_ACTUAL, Encoding.UTF8)
                Console.WriteLine("[Streamlink for Windows " & DATOS_VER_ACTUAL & "]")
            Else
                Console.WriteLine("[Streamlink for Windows]")
            End If
            Dim info = New ProcessStartInfo(Chr(34) & temp_patch_rtv & "\Python 3.5.2\python.exe" & Chr(34), Chr(34) & temp_patch_rtv & "\Streamlink\Streamlink.py" & Chr(34) & " --config " & Chr(34) & url_trabajo_app & "\streamlinkrc" & Chr(34) & " --rtmp-rtmpdump " & Chr(34) & temp_patch_rtv & "\Streamlink\rtmpdump\rtmpdump.exe" & Chr(34) & " " & argumentos_finales)
            info.UseShellExecute = False
            Dim proc = Process.Start(info)
            proc.WaitForExit()
            Console.WriteLine("[End of Streamlink for Windows]")
            Environment.Exit(Proc.ExitCode)
        Catch ex As Exception
            Console.WriteLine("[Streamlink] An error occurred")
        End Try
        'Threading.Thread.Sleep(Threading.Timeout.Infinite)
    End Sub

    Public Function ControlHandler(ByVal ctrlType As CtrlTypes) As Boolean
        Threading.Thread.Sleep(Timeout.Infinite)
    End Function

    Public Declare Auto Function SetConsoleCtrlHandler Lib "kernel32.dll" (ByVal Handler As HandlerRoutine, ByVal Add As Boolean) As Boolean

    Public Delegate Function HandlerRoutine(ByVal CtrlType As CtrlTypes) As Boolean

    Public Enum CtrlTypes
        CTRL_C_EVENT = 0
        CTRL_BREAK_EVENT
        CTRL_CLOSE_EVENT
        CTRL_LOGOFF_EVENT = 5
        CTRL_SHUTDOWN_EVENT
    End Enum

    Public Sub CopyStream(stream As Stream, destPath As String)
        Using fileStream = New FileStream(destPath, FileMode.Create, FileAccess.Write)
            stream.CopyTo(fileStream)
        End Using
    End Sub

    Sub BorrarDirectorioSiExiste(ByVal URL As String)
        If IO.Directory.Exists(URL) Then
            IO.Directory.Delete(URL, True)
        End If
    End Sub

    Sub BorrarArchivoSiExiste(ByVal URL As String)
        If IO.File.Exists(URL) Then
            IO.File.Delete(URL)
        End If
    End Sub

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
            'Console.WriteLine("El directorio donde se deben finalizar los procesos no existe")
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
            'Console.WriteLine("El directorio donde necesito desbloquear archivos no existe :(")
        End If
    End Function

    Public Function DescomprimirArchivoZIP(ByVal Ruta_ZIP As String, ByVal Carpeta_Salida As String)
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

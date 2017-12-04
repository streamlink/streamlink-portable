Imports System.ComponentModel
Imports System.Threading

Public Class ExtendedBackgroundWorker
    Inherits BackgroundWorker

    Private mThread As Thread
    Private ReadOnly Property BackgroundThread() As Thread
        Get
            Return mThread
        End Get
    End Property

    Private ReadOnly Property HasBackgroundThread() As Boolean
        Get
            Return Not mThread Is Nothing And Me.IsBusy
        End Get
    End Property

    Private Function GetBaseFieldValue(Of T)(ByVal privateFieldName As String) As T

        Dim objValue As Object =
            Me.GetType().BaseType.GetField(privateFieldName,
                                    Reflection.BindingFlags.Instance Or
                                    Reflection.BindingFlags.NonPublic).GetValue(Me)


        Return SafeCast(Of T)(objValue)
    End Function

    Function SafeCast(Of T)(ByVal value As Object) As T
        If value Is Nothing Then Return Nothing
        If TypeOf (value) Is T Then Return value
        Return Nothing
    End Function

    Public Sub CancelImmediately()
        If Me.HasBackgroundThread Then
            Try
                BackgroundThread.Abort()

                Dim op As AsyncOperation = GetBaseFieldValue(Of AsyncOperation)("asyncOperation")
                Dim callback As SendOrPostCallback = GetBaseFieldValue(Of SendOrPostCallback)("operationCompleted")
                Dim completedArgs As New RunWorkerCompletedEventArgs(Nothing, Nothing, True)

                op.PostOperationCompleted(callback, completedArgs)

            Catch ex As Exception
            End Try
        End If
    End Sub

    Protected Overrides Sub OnDoWork(ByVal args As DoWorkEventArgs)
        mThread = Thread.CurrentThread

        Try
            MyBase.OnDoWork(args)
        Catch ex As ThreadAbortException
            Thread.ResetAbort()
        End Try

    End Sub
End Class

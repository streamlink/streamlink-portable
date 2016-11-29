'''Created by Roy Osherove, Team Agile
'''Blog: www.ISerializable.com
'''Roy@TeamAgile.com
Imports System.ComponentModel
Imports System.Threading
''' <summary>
''' Extends the standard BackgroundWorker Component in .NET 2.0 Winforms
''' To support the ability of aborting the thread the worker is using.
''' </summary>
''' <remarks></remarks>
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

    ''' <summary>
    ''' returns the private fields value of the base class using reflection.
    ''' </summary>
    ''' <typeparam name="T">The expected type of the field to be retrieved</typeparam>
    ''' <param name="privateFieldName">The name of the field to look for</param>
    ''' <returns>The value of the private field requested in the base class of the caller</returns>
    ''' <remarks></remarks>
    Private Function GetBaseFieldValue(Of T)(ByVal privateFieldName As String) As T

        Dim objValue As Object =
            Me.GetType().BaseType.GetField(privateFieldName,
                                    Reflection.BindingFlags.Instance Or
                                    Reflection.BindingFlags.NonPublic).GetValue(Me)


        Return SafeCast(Of T)(objValue)
    End Function


    ''' <summary>
    ''' Works like the 'as' operator in C#. If the cast is not successfull
    ''' no exception is thrown, and the default value for the requested type is returned.
    ''' </summary>
    ''' <typeparam name="T">The Type to cast the object into</typeparam>
    ''' <param name="value">The object to be casted</param>
    ''' <returns>Either the object casted into the requested type or the default value for the type (null, zero etc..)</returns>
    ''' <remarks></remarks>
    Function SafeCast(Of T)(ByVal value As Object) As T
        If value Is Nothing Then Return Nothing
        If TypeOf (value) Is T Then Return value
        Return Nothing
    End Function




    ''' <summary>
    ''' This method kills the running task.
    ''' If you have any exception handling in your 'DoWork' method, 
    ''' be sure to also catch and ignore ThreadAbortException which will be raised when this method is called.
    ''' If you don't have exception handling inside the DoWork method, you do not need to do anything. The exception will be caught by the component.
    ''' </summary>
    ''' <remarks></remarks>
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

    ''' <summary>
    ''' We override DoWork so we can get a hold of the currently running thread from the threadpool
    ''' </summary>
    ''' <param name="args"></param>
    ''' <remarks></remarks>
    Protected Overrides Sub OnDoWork(ByVal args As DoWorkEventArgs)
        mThread = Thread.CurrentThread

        Try
            MyBase.OnDoWork(args)
        Catch ex As ThreadAbortException
            'don't let the thread excpetion propogate any further.
            Thread.ResetAbort()
        End Try

    End Sub
End Class

﻿Imports System.Net
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.IO
Imports System.Web
Imports System.Threading
Public Class Form1
#Region "Declarations"
    Public C As New CookieContainer
    Public Money As Double = 0
    Public Session As New SessionInfo
    Public PagesInSubscribers As Integer = 0
    Public PagesGoneThrough As Integer = 0
    Public NumberSubsGrabbed As Integer = 0
#End Region
#Region "Events"
    Public Event Stats(Status As String)
#End Region
#Region "Structures"
    Public Structure SessionInfo
        Dim DispatchVar
        Dim ContextVar
        Dim AuthVar
    End Structure
    Public Structure CustomerInformation
        Dim Name
        Dim Email
        Dim CustomerID
        Dim StartDate
        Dim Price
        Dim Status
    End Structure
    Public Structure RecentPayment
        Dim Name
        Dim Amount
        Dim Email
        Dim RecievedDate
        Dim Time
    End Structure
    Public Structure SessionData 'Structure containing the current Session Data
        Dim session As String
        Dim dispatch As String
        Dim context As String
    End Structure
#End Region

    Private trd As Thread

    Sub loadClassesPage(Optional ByVal ClassCode As String = "CSE+")
        RaiseEvent Stats("Loading default classes page to grab cookies.")
        Dim request As HttpWebRequest = HttpWebRequest.Create("https://act.ucsd.edu/scheduleOfClasses/scheduleOfClassesStudent.htm")
        With request
            .Referer = "http://www.google.com"
            .UserAgent = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/535.7 (KHTML, like Gecko) Chrome/16.0.912.75 Safari/535.7"
            .KeepAlive = True
            .CookieContainer = C
            .Method = "GET"

            Dim response As System.Net.HttpWebResponse = .GetResponse
            Dim sr As System.IO.StreamReader = New System.IO.StreamReader(response.GetResponseStream())
            Dim dataresponse As String = sr.ReadToEnd
        End With
        RaiseEvent Stats("Finished grabbing cookies.")
        checkClasses(ClassCode)



    End Sub
    Sub checkClasses(ClassCodes As String)
        'Try
        RaiseEvent Stats("Checking first classes page")
        Dim url = "https://act.ucsd.edu/scheduleOfClasses/scheduleOfClassesStudentResult.htm?selectedTerm=WI15&xsoc_term=&loggedIn=false&tabNum=&selectedSubjects=" + ClassCodes + "&_selectedSubjects=1&schedOption1=true&_schedOption1=on&_schedOption11=on&_schedOption12=on&schedOption2=true&_schedOption2=on&_schedOption4=on&_schedOption5=on&_schedOption3=on&_schedOption7=on&_schedOption8=on&_schedOption13=on&_schedOption10=on&_schedOption9=on&schDay=M&_schDay=on&schDay=T&_schDay=on&schDay=W&_schDay=on&schDay=R&_schDay=on&schDay=F&_schDay=on&schDay=S&_schDay=on&schStartTime=12%3A00&schStartAmPm=0&schEndTime=12%3A00&schEndAmPm=0&_selectedDepartments=1&schedOption1Dept=true&_schedOption1Dept=on&_schedOption11Dept=on&_schedOption12Dept=on&schedOption2Dept=true&_schedOption2Dept=on&_schedOption4Dept=on&_schedOption5Dept=on&_schedOption3Dept=on&_schedOption7Dept=on&_schedOption8Dept=on&_schedOption13Dept=on&_schedOption10Dept=on&_schedOption9Dept=on&schDayDept=M&_schDayDept=on&schDayDept=T&_schDayDept=on&schDayDept=W&_schDayDept=on&schDayDept=R&_schDayDept=on&schDayDept=F&_schDayDept=on&schDayDept=S&_schDayDept=on&schStartTimeDept=12%3A00&schStartAmPmDept=0&schEndTimeDept=12%3A00&schEndAmPmDept=0&courses=&sections=&instructorType=begin&instructor=&titleType=contain&title=&_hideFullSec=on&_showPopup=on"
        Dim request As HttpWebRequest = HttpWebRequest.Create(url)
        With request
            .Referer = "https://act.ucsd.edu/scheduleOfClasses/scheduleOfClassesStudent.htm"
            .UserAgent = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/535.7 (KHTML, like Gecko) Chrome/16.0.912.75 Safari/535.7"
            .KeepAlive = True
            .CookieContainer = C
            .Method = "POST"
            .Timeout = 2000
            Dim sb As New StringBuilder

            Dim byteArray As Byte() = Encoding.UTF8.GetBytes(sb.ToString)
            .ContentLength = byteArray.Length
            Dim dataStream As Stream = .GetRequestStream()
            dataStream.Write(byteArray, 0, byteArray.Length)
            dataStream.Close() : dataStream.Dispose() : dataStream = Nothing

            Dim response As System.Net.HttpWebResponse = .GetResponse

            Dim sr As System.IO.StreamReader = New System.IO.StreamReader(response.GetResponseStream())
            Dim dataresponse As String = sr.ReadToEnd
            RaiseEvent Stats("Grabbed a page of classes.")
            displayMatches(dataresponse)
        End With

        Pageinate(2)
        'Catch ex As Exception
        'TextBox1.AppendText(ex.Message)
        'End Try

    End Sub
    Sub displayMatches(data As String)
        RaiseEvent Stats("Displaying matches:")
        Dim delim As String() = New String(0) {"title=""Final"">FI</span>"}
        'Split = temp_string.Split(delim, StringSplitOptions.None)
        Dim splitByClass = data.Split(delim, StringSplitOptions.RemoveEmptyEntries)
        'TextBox1.AppendText(splitByClass(0))
        For i = 0 To Convert.ToInt32(splitByClass.Length) - 2
            'RaiseEvent Stats(i & " " & Convert.ToInt32(splitByClass.Length) - 2)
            Dim classNameRegex As New Regex("<[a-zA-Z0-9\s]*=""javascript:[a-zA-Z]*\('http:[\/a-zA-Z\.]*.ucsd.edu\/catalog\/courses\/[a-zA-Z0-9]*.html\#([a-zA-Z\.0-9]*)'\)""><[a-zA-Z0-9\s=""]*>([0-9a-zA-Z\/\s-:\(\)\.&;]*)") '<[a-zA-Z0-9]*\s[a-zA-Z0-9=""]*javascript:openNewWindow[a-zA-Z\(\)\':/\.\#0-9""]*><[a-zA-Z\(\)\':/\.\#0-9""\s=]*>([a-zA-Z\(\)\':/\.\#0-9\""\s=\.&;-]*)<\/span>") '
            Dim classCountRegex As New Regex("<td\sclass=[""a-zA-Z]*>([0-9]*)<\/td>\s*<td\sclass=[""a-zA-Z]*>([0-9]*)<\/td>\s*[0-9a-zA-Z<\s="">:;]*\('https:\/\/ucsdbkst.ucsd.edu\/wrtx\/TextSearch\?[a-zA-Z]*=([0-9]*)") ''<[a-zA-Z0-9\s=]*"[a-zA-Z0-9]*">([0-9a-zA-Z\(\)]*)<\/[0-9a-zA-Z>\s]*<[a-zA-Z0-9\s=]*"[a-zA-Z0-9]*">([0-9a-zA-Z\(\)]*)<\/[0-9a-zA-Z>\s]*<[a-zA-Z0-9\s=]*"[a-zA-Z0-9]*"><[a-zA-Z\s=":]*;"\s*onclick=[0-9a-zA-Z":]*\('https:\/\/ucsdbkst.ucsd.edu\/wrtx\/TextSearch\?section=

            Dim classNameMatches As MatchCollection = Regex.Matches(splitByClass(i), classNameRegex.ToString, RegexOptions.Multiline)
            Dim classCountMatches As MatchCollection = Regex.Matches(splitByClass(i), classCountRegex.ToString, RegexOptions.Multiline)
            Dim seatsAvailable As Integer = 0
            For Each Match As Match In classCountMatches
                seatsAvailable += Match.Groups(1).ToString()
            Next Match
            TextBox1.AppendText(classNameMatches(0).Groups(1).ToString().Replace("&amp;", " ") & " " & classNameMatches(0).Groups(2).ToString().Replace("&amp;", " ") & " has " & seatsAvailable & " seats available." & vbNewLine)


        Next i

    End Sub
     Sub Pageinate(x As Integer)
        RaiseEvent Stats("Loading next pageinated page, " & x & ".")
        Dim url = "https://act.ucsd.edu/scheduleOfClasses/scheduleOfClassesStudentResult.htm?page=" & x
        Dim request As HttpWebRequest = HttpWebRequest.Create(url)
        Dim finalPageinate As Integer
        With request
            .Referer = "https://act.ucsd.edu/scheduleOfClasses/scheduleOfClassesStudent.htm"
            .UserAgent = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/535.7 (KHTML, like Gecko) Chrome/16.0.912.75 Safari/535.7"
            .KeepAlive = True
            .CookieContainer = C
            .Method = "POST"
            Dim response As System.Net.HttpWebResponse = .GetResponse
            Dim sr As System.IO.StreamReader = New System.IO.StreamReader(response.GetResponseStream())
            Dim dataresponse As String = sr.ReadToEnd
            displayMatches(dataresponse)

            Dim pageinateRegex As New Regex("<[a-zA-Z\s0-9=""/.]*\?page=([0-9]*)"">[a-zA-Z&0-9;<\/>\s]*\(url,\sname\)\s{")
            Dim finalPageinateMatches As MatchCollection = Regex.Matches(dataresponse, pageinateRegex.ToString, RegexOptions.Multiline)
            finalPageinate = Convert.ToInt32(finalPageinateMatches(0).Groups(1).Value)
        End With
        If x < finalPageinate Then
            Pageinate(x + 1)
        Else
            trd.Abort()
        End If
    End Sub



    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        trd = New Thread(AddressOf loadClassesPage)
        trd.IsBackground = True
        trd.Start()

    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Form1.CheckForIllegalCrossThreadCalls = False
    End Sub


    Private Sub Form1_Stats(Status As String) Handles Me.Stats
        TextBox1.AppendText(Status & vbNewLine)
    End Sub
End Class
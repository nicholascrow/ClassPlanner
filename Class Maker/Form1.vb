Imports System.Net
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.IO
Imports System.Threading
Public Class Form1
#Region "Declarations"
    Private C As New CookieContainer
    Private trd As Thread
#End Region
#Region "Events"
    Public Event Stats(Status As String)
#End Region

    ''' <summary>
    ''' This method loads the main page of classes, and grabs cookies.
    ''' </summary>
    ''' <param name="ClassCode">The type of classes to search through.</param>
    ''' <remarks>Default class code is CSE.</remarks>
    Sub loadClassesPage(Optional ByVal ClassCode As String = "CSE+")

        'Post new status
        RaiseEvent Stats("Loading default classes page to grab cookies.")

        'New web request at the beginning url
        Dim request As HttpWebRequest = HttpWebRequest.Create("https://act.ucsd.edu/scheduleOfClasses/scheduleOfClassesStudent.htm")
        With request
            .Referer = "http://www.google.com"
            .UserAgent = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/535.7 (KHTML, like Gecko) Chrome/16.0.912.75 Safari/535.7"
            .KeepAlive = True
            .CookieContainer = C  'Main cookie container
            .Method = "GET"

            Dim response As System.Net.HttpWebResponse = .GetResponse
            Dim sr As System.IO.StreamReader = New System.IO.StreamReader(response.GetResponseStream())

            'HTML code of response
            Dim dataresponse As String = sr.ReadToEnd
        End With

        'new status after grabbing cookies
        RaiseEvent Stats("Finished grabbing cookies.")

        'loads next page to start checking clases.
        checkClasses(ClassCode)

    End Sub

    ''' <summary>
    ''' This checks the first page of classes.
    ''' </summary>
    ''' <param name="ClassCodes">Class codes to insert.</param>
    ''' <remarks></remarks>
    Sub checkClasses(ClassCodes As String)
        Try
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

                Dim response As System.Net.HttpWebResponse = .GetResponse
                Dim sr As System.IO.StreamReader = New System.IO.StreamReader(response.GetResponseStream())
                Dim dataresponse As String = sr.ReadToEnd
                RaiseEvent Stats("Grabbed a page of classes.")
                displayMatches(dataresponse)
            End With

            Pageinate(2)
        Catch ex As Exception
            RaiseEvent Stats(ex.Message)
        End Try
    End Sub


    ''' <summary>
    ''' Uses regular expressions to check 
    ''' </summary>
    ''' <param name="data">Data to regex.</param>
    ''' <remarks></remarks>
    Sub displayMatches(data As String)
        Dim delim As String() = New String(0) {"title=""Final"">FI</span>"}

        Dim splitByClass = data.Split(delim, StringSplitOptions.RemoveEmptyEntries)
        For i = 0 To Convert.ToInt32(splitByClass.Length) - 2

            Dim classNameRegex As New Regex("<[a-zA-Z0-9\s]*=""javascript:[a-zA-Z]*\('http:[\/a-zA-Z\.]*.ucsd.edu\/catalog\/courses\/[a-zA-Z0-9]*.html\#([a-zA-Z\.0-9]*)'\)""><[a-zA-Z0-9\s=""]*>([0-9a-zA-Z\/\s-:\(\)\.&;]*)") '<[a-zA-Z0-9]*\s[a-zA-Z0-9=""]*javascript:openNewWindow[a-zA-Z\(\)\':/\.\#0-9""]*><[a-zA-Z\(\)\':/\.\#0-9""\s=]*>([a-zA-Z\(\)\':/\.\#0-9\""\s=\.&;-]*)<\/span>") '
            Dim classCountRegex As New Regex("<td\sclass=[""a-zA-Z]*>([0-9]*)<\/td>\s*<td\sclass=[""a-zA-Z]*>([0-9]*)<\/td>\s*[0-9a-zA-Z<\s="">:;]*\('https:\/\/ucsdbkst.ucsd.edu\/wrtx\/TextSearch\?[a-zA-Z]*=([0-9]*)") ''<[a-zA-Z0-9\s=]*"[a-zA-Z0-9]*">([0-9a-zA-Z\(\)]*)<\/[0-9a-zA-Z>\s]*<[a-zA-Z0-9\s=]*"[a-zA-Z0-9]*">([0-9a-zA-Z\(\)]*)<\/[0-9a-zA-Z>\s]*<[a-zA-Z0-9\s=]*"[a-zA-Z0-9]*"><[a-zA-Z\s=":]*;"\s*onclick=[0-9a-zA-Z":]*\('https:\/\/ucsdbkst.ucsd.edu\/wrtx\/TextSearch\?section=

            Dim classNameMatches As MatchCollection = Regex.Matches(splitByClass(i), classNameRegex.ToString, RegexOptions.Multiline)
            Dim classCountMatches As MatchCollection = Regex.Matches(splitByClass(i), classCountRegex.ToString, RegexOptions.Multiline)
            Dim seatsAvailable As Integer = 0
            For Each Match As Match In classCountMatches
                seatsAvailable += Match.Groups(1).ToString()
            Next Match
            Dim item As ListViewItem = ListView1.Items.Add(classNameMatches(0).Groups(1).ToString().Replace("&amp;", " & "))
            item.SubItems.Add(classNameMatches(0).Groups(2).ToString().Replace("&amp;", " & "))
            item.SubItems.Add(seatsAvailable)
            ListView1.Sort()

        Next i

    End Sub


    ''' <summary>
    ''' Pageinate, go to the next page and grab data, recursively.
    ''' </summary>
    ''' <param name="x">Page number</param>
    ''' <remarks></remarks>
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


    ''' <summary>
    ''' Button to start running the program
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        trd = New Thread(AddressOf loadClassesPage)
        trd.IsBackground = True
        trd.Start()
    End Sub


    ''' <summary>
    ''' Load of form, set things.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Form1.CheckForIllegalCrossThreadCalls = False
        ListView1.Columns(0).Width = 204
        ListView1.Columns(1).Width = 545
        ListView1.Columns(2).Width = 179
    End Sub


    ''' <summary>
    ''' Post stats to text box.
    ''' </summary>
    ''' <param name="Status"></param>
    ''' <remarks></remarks>
    Private Sub Form1_Stats(Status As String) Handles Me.Stats
        TextBox2.AppendText(Status & vbNewLine)
    End Sub
End Class

Public Class Game
    Private Player1Name As String
    Private Player2Name As String
    Private Player1Score As Integer = 0
    Private Player2Score As Integer = 0
    Private CurrentPlayer As Integer = 1
    Private CurrentRound As Integer = 1
    Private TotalRounds As Integer = 3

    Private DiceTimer As New Timer()
    Private Dice1Value As Integer = 1
    Private Dice2Value As Integer = 1
    Private Rnd As New Random()
    Private IsRolling As Boolean = False
    Private IsFullscreen As Boolean = False

    Private lblPlayer1Name, lblPlayer2Name As Label
    Private lblPlayer1Score, lblPlayer2Score As Label
    Private lblPlayer1LastRoll, lblPlayer2LastRoll As Label
    Private lblCurrentTurn, lblRoundInfo As Label
    Private picDice1, picDice2 As PictureBox
    Private btnRoll, btnStop, btnQuit, btnPlayAgain, btnRetry As Button
    Private pnlGame As Panel
    Private pnlPlayer1, pnlPlayer2 As Panel
    Private lblTitle, lblGameOver As Label

    Private DiceImages(6) As Image

    Private GameFont As Font
    Private TitleFont As Font
    Private ScoreFont As Font

    Public Sub New(p1Name As String, p2Name As String)
        InitializeComponent()

        ' CRITICAL: Enable double buffering BEFORE creating controls
        Me.SetStyle(ControlStyles.OptimizedDoubleBuffer Or
                    ControlStyles.AllPaintingInWmPaint Or
                    ControlStyles.UserPaint, True)
        Me.UpdateStyles()

        Player1Name = p1Name
        Player2Name = p2Name
    End Sub

    Private Sub Game_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.WindowState = FormWindowState.Maximized
        Me.FormBorderStyle = FormBorderStyle.None
        Me.Text = "Dice Game"
        Me.KeyPreview = True
        IsFullscreen = True

        LoadCustomFont()
        LoadDiceImages()

        ' Increase timer interval to reduce flickering
        DiceTimer.Interval = 80
        AddHandler DiceTimer.Tick, AddressOf DiceTimer_Tick

        CreateGameUI()
        UpdateTurnDisplay()
    End Sub

    Private Sub LoadCustomFont()
        Try
            Dim fontPath As String = System.IO.Path.Combine(Application.StartupPath, "Assets", "PressStart2P-Regular.ttf")
            If System.IO.File.Exists(fontPath) Then
                Dim pfc As New System.Drawing.Text.PrivateFontCollection()
                pfc.AddFontFile(fontPath)
                TitleFont = New Font(pfc.Families(0), 24, FontStyle.Regular)
                GameFont = New Font(pfc.Families(0), 12, FontStyle.Regular)
                ScoreFont = New Font(pfc.Families(0), 32, FontStyle.Regular)
            Else
                TitleFont = New Font("Arial", 24, FontStyle.Bold)
                GameFont = New Font("Arial", 12, FontStyle.Bold)
                ScoreFont = New Font("Arial", 32, FontStyle.Bold)
            End If
        Catch ex As Exception
            TitleFont = New Font("Arial", 24, FontStyle.Bold)
            GameFont = New Font("Arial", 12, FontStyle.Bold)
            ScoreFont = New Font("Arial", 32, FontStyle.Bold)
        End Try
    End Sub

    Private Sub LoadDiceImages()
        Try
            For i As Integer = 1 To 6
                Dim imagePath As String = System.IO.Path.Combine(Application.StartupPath, "Assets", $"Dice{i}.png")
                If System.IO.File.Exists(imagePath) Then
                    ' Load and cache images
                    DiceImages(i) = Image.FromFile(imagePath)
                Else
                    MessageBox.Show($"Warning: Dice image not found: {imagePath}", "Missing Image", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                End If
            Next

            Dim bgPath As String = System.IO.Path.Combine(Application.StartupPath, "Assets", "background.png")
            If System.IO.File.Exists(bgPath) Then
                Me.BackgroundImage = Image.FromFile(bgPath)
                Me.BackgroundImageLayout = ImageLayout.Stretch
            End If
        Catch ex As Exception
            MessageBox.Show($"Error loading images: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub CreateGameUI()
        pnlGame = New Panel With {
            .Dock = DockStyle.Fill,
            .BackColor = Color.Transparent
        }
        ' Enable double buffering for panel
        EnableDoubleBuffering(pnlGame)
        Me.Controls.Add(pnlGame)

        lblTitle = New Label With {
            .Text = "DICE GAME",
            .Font = TitleFont,
            .ForeColor = Color.White,
            .AutoSize = True,
            .BackColor = Color.Transparent
        }
        pnlGame.Controls.Add(lblTitle)

        lblRoundInfo = New Label With {
            .Font = GameFont,
            .ForeColor = Color.Yellow,
            .AutoSize = True,
            .BackColor = Color.Transparent
        }
        pnlGame.Controls.Add(lblRoundInfo)

        pnlPlayer1 = New Panel With {
            .Size = New Size(350, 120),
            .BackColor = Color.FromArgb(100, 135, 206)
        }
        EnableDoubleBuffering(pnlPlayer1)
        CreateRoundedPanel(pnlPlayer1)

        lblPlayer1Name = New Label With {
            .Text = Player1Name,
            .Font = GameFont,
            .ForeColor = Color.White,
            .AutoSize = True,
            .Location = New Point(15, 15),
            .BackColor = Color.Transparent
        }

        lblPlayer1LastRoll = New Label With {
            .Text = "",
            .Font = GameFont,
            .ForeColor = Color.LightGreen,
            .AutoSize = True,
            .BackColor = Color.Transparent
        }

        lblPlayer1Score = New Label With {
            .Text = "0",
            .Font = ScoreFont,
            .ForeColor = Color.White,
            .AutoSize = True,
            .Location = New Point(15, 50),
            .BackColor = Color.Transparent
        }

        pnlPlayer1.Controls.AddRange({lblPlayer1Name, lblPlayer1LastRoll, lblPlayer1Score})
        pnlGame.Controls.Add(pnlPlayer1)

        pnlPlayer2 = New Panel With {
            .Size = New Size(350, 120),
            .BackColor = Color.FromArgb(220, 20, 60)
        }
        EnableDoubleBuffering(pnlPlayer2)
        CreateRoundedPanel(pnlPlayer2)

        lblPlayer2Name = New Label With {
            .Text = Player2Name,
            .Font = GameFont,
            .ForeColor = Color.White,
            .AutoSize = True,
            .Location = New Point(15, 15),
            .BackColor = Color.Transparent
        }

        lblPlayer2LastRoll = New Label With {
            .Text = "",
            .Font = GameFont,
            .ForeColor = Color.LightGreen,
            .AutoSize = True,
            .BackColor = Color.Transparent
        }

        lblPlayer2Score = New Label With {
            .Text = "0",
            .Font = ScoreFont,
            .ForeColor = Color.White,
            .AutoSize = True,
            .Location = New Point(15, 50),
            .BackColor = Color.Transparent
        }

        pnlPlayer2.Controls.AddRange({lblPlayer2Name, lblPlayer2LastRoll, lblPlayer2Score})
        pnlGame.Controls.Add(pnlPlayer2)

        lblCurrentTurn = New Label With {
            .Font = GameFont,
            .ForeColor = Color.Yellow,
            .AutoSize = True,
            .BackColor = Color.Transparent
        }
        pnlGame.Controls.Add(lblCurrentTurn)

        ' PictureBoxes with double buffering enabled
        picDice1 = New PictureBox With {
            .Size = New Size(300, 300),
            .BackColor = Color.Transparent,
            .SizeMode = PictureBoxSizeMode.Zoom
        }
        EnableDoubleBuffering(picDice1)

        picDice2 = New PictureBox With {
            .Size = New Size(300, 300),
            .BackColor = Color.Transparent,
            .SizeMode = PictureBoxSizeMode.Zoom
        }
        EnableDoubleBuffering(picDice2)

        If DiceImages(1) IsNot Nothing Then
            picDice1.Image = DiceImages(1)
            picDice2.Image = DiceImages(1)
        End If

        pnlGame.Controls.AddRange({picDice1, picDice2})

        btnRoll = New Button With {
            .Text = "Roll",
            .Font = GameFont,
            .Size = New Size(180, 60),
            .BackColor = Color.FromArgb(34, 139, 34),
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Cursor = Cursors.Hand
        }
        btnRoll.FlatAppearance.BorderSize = 0
        AddHandler btnRoll.Click, AddressOf RollDice
        AddHandler btnRoll.Paint, AddressOf DrawRoundedButton

        btnStop = New Button With {
            .Text = "Stop",
            .Font = GameFont,
            .Size = New Size(180, 60),
            .BackColor = Color.FromArgb(178, 34, 34),
            .ForeColor = Color.White,
            .Enabled = False,
            .FlatStyle = FlatStyle.Flat,
            .Cursor = Cursors.Hand
        }
        btnStop.FlatAppearance.BorderSize = 0
        AddHandler btnStop.Click, AddressOf StopRolling
        AddHandler btnStop.Paint, AddressOf DrawRoundedButton

        btnQuit = New Button With {
            .Text = "Quit",
            .Font = GameFont,
            .Size = New Size(180, 60),
            .BackColor = Color.FromArgb(128, 128, 128),
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Cursor = Cursors.Hand
        }
        btnQuit.FlatAppearance.BorderSize = 0
        AddHandler btnQuit.Click, AddressOf QuitGame
        AddHandler btnQuit.Paint, AddressOf DrawRoundedButton

        btnPlayAgain = New Button With {
            .Text = "Play Again",
            .Font = GameFont,
            .Size = New Size(200, 60),
            .BackColor = Color.FromArgb(255, 165, 0),
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Cursor = Cursors.Hand,
            .Visible = False
        }
        btnPlayAgain.FlatAppearance.BorderSize = 0
        AddHandler btnPlayAgain.Click, AddressOf PlayAgain
        AddHandler btnPlayAgain.Paint, AddressOf DrawRoundedButton

        btnRetry = New Button With {
            .Text = "↻",
            .Font = New Font(GameFont.FontFamily, 20, FontStyle.Bold),
            .Size = New Size(60, 60),
            .BackColor = Color.FromArgb(255, 193, 7),
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Cursor = Cursors.Hand
        }
        btnRetry.FlatAppearance.BorderSize = 0
        AddHandler btnRetry.Click, AddressOf PlayAgain
        AddHandler btnRetry.Paint, AddressOf DrawRoundedButton

        pnlGame.Controls.AddRange({btnRoll, btnStop, btnQuit, btnPlayAgain, btnRetry})

        AddHandler Me.Resize, AddressOf RepositionElements
        RepositionElements(Nothing, Nothing)
    End Sub

    ' Helper method to enable double buffering on any control
    Private Sub EnableDoubleBuffering(ctrl As Control)
        Dim prop As System.Reflection.PropertyInfo = ctrl.GetType().GetProperty("DoubleBuffered",
            System.Reflection.BindingFlags.Instance Or System.Reflection.BindingFlags.NonPublic)
        If prop IsNot Nothing Then
            prop.SetValue(ctrl, True, Nothing)
        End If
    End Sub

    Private Sub CreateRoundedPanel(panel As Panel)
        Dim radius As Integer = 20
        Dim rect As New Rectangle(0, 0, panel.Width, panel.Height)

        AddHandler panel.Paint, Sub(sender As Object, e As PaintEventArgs)
                                    e.Graphics.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
                                    Dim gp As New Drawing2D.GraphicsPath()
                                    gp.AddArc(rect.X, rect.Y, radius, radius, 180, 90)
                                    gp.AddArc(rect.X + rect.Width - radius, rect.Y, radius, radius, 270, 90)
                                    gp.AddArc(rect.X + rect.Width - radius, rect.Y + rect.Height - radius, radius, radius, 0, 90)
                                    gp.AddArc(rect.X, rect.Y + rect.Height - radius, radius, radius, 90, 90)
                                    gp.CloseFigure()
                                    panel.Region = New Region(gp)
                                End Sub
    End Sub

    Private Sub DrawRoundedButton(sender As Object, e As PaintEventArgs)
        Dim btn As Button = CType(sender, Button)
        Dim rect As New Rectangle(0, 0, btn.Width, btn.Height)
        Dim radius As Integer = 15

        e.Graphics.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
        Dim gp As New Drawing2D.GraphicsPath()
        gp.AddArc(rect.X, rect.Y, radius, radius, 180, 90)
        gp.AddArc(rect.X + rect.Width - radius, rect.Y, radius, radius, 270, 90)
        gp.AddArc(rect.X + rect.Width - radius, rect.Y + rect.Height - radius, radius, radius, 0, 90)
        gp.AddArc(rect.X, rect.Y + rect.Height - radius, radius, radius, 90, 90)
        gp.CloseFigure()
        btn.Region = New Region(gp)
    End Sub

    Private Sub RepositionElements(sender As Object, e As EventArgs)
        ' Suspend layout to prevent flickering during repositioning
        Me.SuspendLayout()
        pnlGame.SuspendLayout()

        Dim centerX As Integer = Me.ClientSize.Width \ 2
        Dim centerY As Integer = Me.ClientSize.Height \ 2

        lblTitle.Location = New Point(centerX - lblTitle.Width \ 2, 50)
        lblRoundInfo.Location = New Point(centerX - lblRoundInfo.Width \ 2, 120)

        pnlPlayer1.Location = New Point(centerX - 380, 170)
        pnlPlayer2.Location = New Point(centerX + 30, 170)

        lblPlayer1LastRoll.Location = New Point(lblPlayer1Name.Right + 10, 15)
        lblPlayer2LastRoll.Location = New Point(lblPlayer2Name.Right + 10, 15)

        lblCurrentTurn.Location = New Point(centerX - lblCurrentTurn.Width \ 2, 310)

        picDice1.Location = New Point(centerX - 330, centerY - 150)
        picDice2.Location = New Point(centerX + 30, centerY - 150)

        Dim buttonY As Integer = centerY + 250

        If btnRoll.Visible Then
            btnRoll.Location = New Point(centerX - 300, buttonY)
            btnStop.Location = New Point(centerX - 90, buttonY)
            btnQuit.Location = New Point(centerX + 120, buttonY)
        End If

        If btnPlayAgain.Visible Then
            btnPlayAgain.Location = New Point(centerX - 220, buttonY)
            btnQuit.Location = New Point(centerX + 20, buttonY)
        End If

        btnRetry.Location = New Point(Me.ClientSize.Width - btnRetry.Width - 30, 30)

        If lblGameOver IsNot Nothing AndAlso lblGameOver.Visible Then
            lblGameOver.Location = New Point(centerX - lblGameOver.Width \ 2, centerY - 50)
        End If

        ' Resume layout
        pnlGame.ResumeLayout(False)
        Me.ResumeLayout(False)
    End Sub

    Private Sub UpdateTurnDisplay()
        If CurrentRound > TotalRounds Then
            EndGame()
            Return
        End If

        lblRoundInfo.Text = $"Round {CurrentRound}/{TotalRounds} - {If(CurrentPlayer = 1, Player1Name, Player2Name)}'s Turn"
        lblRoundInfo.ForeColor = Color.Yellow
        lblCurrentTurn.Visible = False

        RepositionElements(Nothing, Nothing)
    End Sub

    Private Sub RollDice(sender As Object, e As EventArgs)
        If Not IsRolling Then
            IsRolling = True
            btnRoll.Enabled = False
            btnStop.Enabled = True
            DiceTimer.Start()
        End If
    End Sub

    Private Sub StopRolling(sender As Object, e As EventArgs)
        If IsRolling Then
            DiceTimer.Stop()
            IsRolling = False
            btnRoll.Enabled = True
            btnStop.Enabled = False

            Dim total As Integer = Dice1Value + Dice2Value

            If CurrentPlayer = 1 Then
                Player1Score += total
                lblPlayer1Score.Text = Player1Score.ToString()
                lblPlayer1LastRoll.Text = $"+{total}"
                CurrentPlayer = 2
            Else
                Player2Score += total
                lblPlayer2Score.Text = Player2Score.ToString()
                lblPlayer2LastRoll.Text = $"+{total}"
                CurrentPlayer = 1
                CurrentRound += 1
            End If

            UpdateTurnDisplay()
        End If
    End Sub

    Private Sub DiceTimer_Tick(sender As Object, e As EventArgs)
        Dice1Value = Rnd.Next(1, 7)
        Dice2Value = Rnd.Next(1, 7)

        ' Only update if images exist - reduces unnecessary redraws
        If DiceImages(Dice1Value) IsNot Nothing Then
            picDice1.Image = DiceImages(Dice1Value)
        End If

        If DiceImages(Dice2Value) IsNot Nothing Then
            picDice2.Image = DiceImages(Dice2Value)
        End If
    End Sub

    Private Sub EndGame()
        DiceTimer.Stop()

        btnRoll.Visible = False
        btnStop.Visible = False
        picDice1.Visible = False
        picDice2.Visible = False
        lblRoundInfo.Visible = False
        lblCurrentTurn.Visible = False

        Dim winner As String = ""

        If Player1Score > Player2Score Then
            winner = $"{Player1Name} Wins!"
        ElseIf Player2Score > Player1Score Then
            winner = $"{Player2Name} Wins!"
        Else
            winner = "It's a Tie!"
        End If

        If lblGameOver Is Nothing Then
            lblGameOver = New Label With {
                .Font = New Font(TitleFont.FontFamily, 20, FontStyle.Regular),
                .ForeColor = Color.Yellow,
                .AutoSize = True,
                .BackColor = Color.Transparent
            }
            pnlGame.Controls.Add(lblGameOver)
        End If

        lblGameOver.Text = $"Game Over - {winner}"
        lblGameOver.Visible = True

        btnPlayAgain.Visible = True
        btnQuit.Visible = True

        RepositionElements(Nothing, Nothing)
    End Sub

    Private Sub PlayAgain(sender As Object, e As EventArgs)
        If IsRolling Then
            DiceTimer.Stop()
            IsRolling = False
        End If

        Player1Score = 0
        Player2Score = 0
        CurrentPlayer = 1
        CurrentRound = 1

        lblPlayer1Score.Text = "0"
        lblPlayer2Score.Text = "0"
        lblPlayer1LastRoll.Text = ""
        lblPlayer2LastRoll.Text = ""

        btnRoll.Visible = True
        btnRoll.Enabled = True
        btnStop.Visible = True
        btnStop.Enabled = False
        picDice1.Visible = True
        picDice2.Visible = True
        lblRoundInfo.Visible = True
        pnlPlayer1.Visible = True
        pnlPlayer2.Visible = True

        If lblGameOver IsNot Nothing Then
            lblGameOver.Visible = False
        End If
        btnPlayAgain.Visible = False

        If DiceImages(1) IsNot Nothing Then
            picDice1.Image = DiceImages(1)
            picDice2.Image = DiceImages(1)
        End If

        UpdateTurnDisplay()
        RepositionElements(Nothing, Nothing)
    End Sub

    Private Sub QuitGame(sender As Object, e As EventArgs)
        Dim result = MessageBox.Show("Are you sure you want to quit?", "Quit Game", MessageBoxButtons.YesNo, MessageBoxIcon.Question)

        If result = DialogResult.Yes Then
            Application.Exit()
        End If
    End Sub

    Private Sub Game_KeyDown(sender As Object, e As KeyEventArgs) Handles MyBase.KeyDown
        If e.KeyCode = Keys.F11 Then
            ToggleFullscreen()
        ElseIf e.KeyCode = Keys.Escape AndAlso IsFullscreen Then
            ToggleFullscreen()
        End If
    End Sub

    Private Sub ToggleFullscreen()
        If IsFullscreen Then
            Me.FormBorderStyle = FormBorderStyle.Sizable
            Me.WindowState = FormWindowState.Maximized
            IsFullscreen = False
        Else
            Me.FormBorderStyle = FormBorderStyle.None
            Me.WindowState = FormWindowState.Normal
            Me.Bounds = Screen.PrimaryScreen.Bounds
            Me.TopMost = True
            IsFullscreen = True
        End If
    End Sub

    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        For i As Integer = 1 To 6
            If DiceImages(i) IsNot Nothing Then
                DiceImages(i).Dispose()
            End If
        Next

        If GameFont IsNot Nothing Then GameFont.Dispose()
        If TitleFont IsNot Nothing Then TitleFont.Dispose()
        If ScoreFont IsNot Nothing Then ScoreFont.Dispose()

        MyBase.OnFormClosing(e)
    End Sub
End Class
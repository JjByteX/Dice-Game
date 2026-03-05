Public Class LoginWindow
    Private Sub LoginWindow_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' Set window properties
        Me.FormBorderStyle = FormBorderStyle.FixedSingle
        Me.MaximizeBox = False
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.Size = New Size(400, 300)
        Me.Text = "Dice Game - Player Login"

        ' Create and add controls
        Dim lblTitle As New Label With {
            .Text = "🎲 Dice Game Login 🎲",
            .Font = New Font("Arial", 16, FontStyle.Bold),
            .AutoSize = True,
            .Location = New Point(80, 20)
        }

        Dim lblPlayer1 As New Label With {
            .Text = "Player 1 Name:",
            .Location = New Point(50, 80),
            .AutoSize = True
        }

        Dim txtPlayer1 As New TextBox With {
            .Name = "txtPlayer1",
            .Location = New Point(170, 77),
            .Width = 150
        }

        Dim lblPlayer2 As New Label With {
            .Text = "Player 2 Name:",
            .Location = New Point(50, 120),
            .AutoSize = True
        }

        Dim txtPlayer2 As New TextBox With {
            .Name = "txtPlayer2",
            .Location = New Point(170, 117),
            .Width = 150
        }

        Dim btnStart As New Button With {
            .Text = "Start Game",
            .Location = New Point(140, 180),
            .Width = 120,
            .Height = 40,
            .Font = New Font("Arial", 10, FontStyle.Bold)
        }

        ' Add click event for start button
        AddHandler btnStart.Click, Sub()
                                       Dim player1Name As String = Me.Controls.Find("txtPlayer1", True)(0).Text.Trim()
                                       Dim player2Name As String = Me.Controls.Find("txtPlayer2", True)(0).Text.Trim()

                                       If String.IsNullOrEmpty(player1Name) OrElse String.IsNullOrEmpty(player2Name) Then
                                           MessageBox.Show("Please enter names for both players!", "Missing Information", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                                           Return
                                       End If

                                       If player1Name = player2Name Then
                                           MessageBox.Show("Players must have different names!", "Duplicate Names", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                                           Return
                                       End If

                                       ' Open game window
                                       Dim gameForm As New Game(player1Name, player2Name)
                                       gameForm.Show()
                                       Me.Hide()
                                   End Sub

        ' Add controls to form
        Me.Controls.AddRange({lblTitle, lblPlayer1, txtPlayer1, lblPlayer2, txtPlayer2, btnStart})
    End Sub
End Class
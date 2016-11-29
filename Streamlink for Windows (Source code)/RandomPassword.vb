Imports System.Security.Cryptography

Public Class RandomPassword

    ' Define default min and max password lengths.
    Private Shared DEFAULT_MIN_PASSWORD_LENGTH As Integer = 8
    Private Shared DEFAULT_MAX_PASSWORD_LENGTH As Integer = 10

    ' Define supported password characters divided into groups.
    ' You can add (or remove) characters to (from) these groups.
    Private Shared PASSWORD_CHARS_LCASE As String = "abcdefghijklmnopqrstuvwxyz"
    Private Shared PASSWORD_CHARS_UCASE As String = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"
    Private Shared PASSWORD_CHARS_NUMERIC As String = "0123456789"
    Private Shared PASSWORD_CHARS_SPECIAL As String = "*$-+?_&=!%{}/"

    ' <summary>
    ' Generates a random password.
    ' </summary>
    ' <returns>
    ' Randomly generated password.
    ' </returns>
    ' <remarks>
    ' The length of the generated password will be determined at
    ' random. It will be no shorter than the minimum default and
    ' no longer than maximum default.
    ' </remarks>
    Public Shared Function Generate() As String
        Generate = Generate(DEFAULT_MIN_PASSWORD_LENGTH,
                            DEFAULT_MAX_PASSWORD_LENGTH)
    End Function

    ' <summary>
    ' Generates a random password of the exact length.
    ' </summary>
    ' <param name="length">
    ' Exact password length.
    ' </param>
    ' <returns>
    ' Randomly generated password.
    ' </returns>
    Public Shared Function Generate(length As Integer) As String
        Generate = Generate(length, length)
    End Function

    ' <summary>
    ' Generates a random password.
    ' </summary>
    ' <param name="minLength">
    ' Minimum password length.
    ' </param>
    ' <param name="maxLength">
    ' Maximum password length.
    ' </param>
    ' <returns>
    ' Randomly generated password.
    ' </returns>
    ' <remarks>
    ' The length of the generated password will be determined at
    ' random and it will fall with the range determined by the
    ' function parameters.
    ' </remarks>
    Public Shared Function Generate(minLength As Integer,
                                    maxLength As Integer) _
        As String

        ' Make sure that input parameters are valid.
        If (minLength <= 0 Or maxLength <= 0 Or minLength > maxLength) Then
            Generate = Nothing
        End If

        ' Create a local array containing supported password characters
        ' grouped by types. You can remove character groups from this
        ' array, but doing so will weaken the password strength.
        Dim charGroups As Char()() = New Char()() _
        {
            PASSWORD_CHARS_LCASE.ToCharArray(),
            PASSWORD_CHARS_UCASE.ToCharArray(),
            PASSWORD_CHARS_NUMERIC.ToCharArray(),
            PASSWORD_CHARS_SPECIAL.ToCharArray()
        }

        ' Use this array to track the number of unused characters in each
        ' character group.
        Dim charsLeftInGroup As Integer() = New Integer(charGroups.Length - 1) {}

        ' Initially, all characters in each group are not used.
        Dim I As Integer
        For I = 0 To charsLeftInGroup.Length - 1
            charsLeftInGroup(I) = charGroups(I).Length
        Next

        ' Use this array to track (iterate through) unused character groups.
        Dim leftGroupsOrder As Integer() = New Integer(charGroups.Length - 1) {}

        ' Initially, all character groups are not used.
        For I = 0 To leftGroupsOrder.Length - 1
            leftGroupsOrder(I) = I
        Next

        ' Because we cannot use the default randomizer, which is based on the
        ' current time (it will produce the same "random" number within a
        ' second), we will use a random number generator to seed the
        ' randomizer.

        ' Use a 4-byte array to fill it with random bytes and convert it then
        ' to an integer value.
        Dim randomBytes As Byte() = New Byte(3) {}

        ' Generate 4 random bytes.
        Dim rng As RNGCryptoServiceProvider = New RNGCryptoServiceProvider()

        rng.GetBytes(randomBytes)

        ' Convert 4 bytes into a 32-bit integer value.
        Dim seed As Integer = BitConverter.ToInt32(randomBytes, 0)

        ' Now, this is real randomization.
        Dim random As Random = New Random(seed)

        ' This array will hold password characters.
        Dim password As Char() = Nothing

        ' Allocate appropriate memory for the password.
        If (minLength < maxLength) Then
            password = New Char(random.Next(minLength - 1, maxLength)) {}
        Else
            password = New Char(minLength - 1) {}
        End If

        ' Index of the next character to be added to password.
        Dim nextCharIdx As Integer

        ' Index of the next character group to be processed.
        Dim nextGroupIdx As Integer

        ' Index which will be used to track not processed character groups.
        Dim nextLeftGroupsOrderIdx As Integer

        ' Index of the last non-processed character in a group.
        Dim lastCharIdx As Integer

        ' Index of the last non-processed group.
        Dim lastLeftGroupsOrderIdx As Integer = leftGroupsOrder.Length - 1

        ' Generate password characters one at a time.
        For I = 0 To password.Length - 1

            ' If only one character group remained unprocessed, process it;
            ' otherwise, pick a random character group from the unprocessed
            ' group list. To allow a special character to appear in the
            ' first position, increment the second parameter of the Next
            ' function call by one, i.e. lastLeftGroupsOrderIdx + 1.
            If (lastLeftGroupsOrderIdx = 0) Then
                nextLeftGroupsOrderIdx = 0
            Else
                nextLeftGroupsOrderIdx = random.Next(0, lastLeftGroupsOrderIdx)
            End If

            ' Get the actual index of the character group, from which we will
            ' pick the next character.
            nextGroupIdx = leftGroupsOrder(nextLeftGroupsOrderIdx)

            ' Get the index of the last unprocessed characters in this group.
            lastCharIdx = charsLeftInGroup(nextGroupIdx) - 1

            ' If only one unprocessed character is left, pick it; otherwise,
            ' get a random character from the unused character list.
            If (lastCharIdx = 0) Then
                nextCharIdx = 0
            Else
                nextCharIdx = random.Next(0, lastCharIdx + 1)
            End If

            ' Add this character to the password.
            password(I) = charGroups(nextGroupIdx)(nextCharIdx)

            ' If we processed the last character in this group, start over.
            If (lastCharIdx = 0) Then
                charsLeftInGroup(nextGroupIdx) =
                                charGroups(nextGroupIdx).Length
                ' There are more unprocessed characters left.
            Else
                ' Swap processed character with the last unprocessed character
                ' so that we don't pick it until we process all characters in
                ' this group.
                If (lastCharIdx <> nextCharIdx) Then
                    Dim temp As Char = charGroups(nextGroupIdx)(lastCharIdx)
                    charGroups(nextGroupIdx)(lastCharIdx) =
                                charGroups(nextGroupIdx)(nextCharIdx)
                    charGroups(nextGroupIdx)(nextCharIdx) = temp
                End If

                ' Decrement the number of unprocessed characters in
                ' this group.
                charsLeftInGroup(nextGroupIdx) =
                           charsLeftInGroup(nextGroupIdx) - 1
            End If

            ' If we processed the last group, start all over.
            If (lastLeftGroupsOrderIdx = 0) Then
                lastLeftGroupsOrderIdx = leftGroupsOrder.Length - 1
                ' There are more unprocessed groups left.
            Else
                ' Swap processed group with the last unprocessed group
                ' so that we don't pick it until we process all groups.
                If (lastLeftGroupsOrderIdx <> nextLeftGroupsOrderIdx) Then
                    Dim temp As Integer =
                                leftGroupsOrder(lastLeftGroupsOrderIdx)
                    leftGroupsOrder(lastLeftGroupsOrderIdx) =
                                leftGroupsOrder(nextLeftGroupsOrderIdx)
                    leftGroupsOrder(nextLeftGroupsOrderIdx) = temp
                End If

                ' Decrement the number of unprocessed groups.
                lastLeftGroupsOrderIdx = lastLeftGroupsOrderIdx - 1
            End If
        Next

        ' Convert password characters into a string and return the result.
        Generate = New String(password)
    End Function

End Class

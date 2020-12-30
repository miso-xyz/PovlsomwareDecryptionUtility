Imports dnlib
Imports System.IO
Imports System.Security.Cryptography
Imports Microsoft.Win32
Imports System.Windows.Forms
Module Module1

    Function getStrPath() As String
        Dim registryKey As RegistryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\Microsoft\Windows\CurrentVersion\Run", True)
        Return RegistryKey.GetValue("Povlsomware")
    End Function
    Sub Main(ByVal args() As String)
        Console.Title = "Povlsomware Decryption Utility 1.0 by misonothx"
        If args.Count = 0 Then
            Console.WriteLine("Password in """ & getStrPath() & """: " & getPassword())
            Console.WriteLine()
            Console.WriteLine("Copied to clipboard!")
            Clipboard.SetText(getPassword())
            Console.ReadKey()
        End If
        For x = 0 To args.Count - 1
            If System.IO.File.Exists(args(x)) Then
                Try
a_:
                    Console.Write("What do you want to do with """ & Path.GetFileName(args(x)) & """ | (E)ncrypt or (D)ecrypt?: ")
                    Dim a_ = Console.ReadKey()
                    If a_.Key = ConsoleKey.E Then
                        EncryptFile(args(x))
                    ElseIf a_.Key = ConsoleKey.D Then
                        decryptFile(args(x))
                    Else
                        GoTo a_
                    End If
                Catch ex As Exception
                    Throw
                End Try
            End If
        Next

    End Sub

    Public Function AES_Encrypt(ByVal bytesToBeEncrypted As Byte(), ByVal passwordBytes As Byte()) As Byte()
        Dim result As Byte() = Nothing
        Dim salt As Byte() = New Byte() {1, 2, 3, 4, 5, 6, 7, 8}
        Using memoryStream As MemoryStream = New MemoryStream()
            Using rijndaelManaged As RijndaelManaged = New RijndaelManaged()
                rijndaelManaged.KeySize = 256
                rijndaelManaged.BlockSize = 128
                Dim rfc2898DeriveBytes As Rfc2898DeriveBytes = New Rfc2898DeriveBytes(passwordBytes, salt, 1000)
                rijndaelManaged.Key = rfc2898DeriveBytes.GetBytes(rijndaelManaged.KeySize / 8)
                rijndaelManaged.IV = rfc2898DeriveBytes.GetBytes(rijndaelManaged.BlockSize / 8)
                rijndaelManaged.Mode = CipherMode.CBC
                Using cryptoStream As CryptoStream = New CryptoStream(memoryStream, rijndaelManaged.CreateEncryptor(), CryptoStreamMode.Write)
                    cryptoStream.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length)
                    cryptoStream.Close()
                End Using
                result = memoryStream.ToArray()
            End Using
        End Using
        Return result
    End Function

    Public Sub EncryptFile(ByVal fileUnencrypted As String)
        Dim array As Byte() = SHA256.Create().ComputeHash(System.Text.Encoding.UTF8.GetBytes(getPassword()))

        Dim array2 As Byte() = AES_Encrypt(File.ReadAllBytes(fileUnencrypted), array)
        Using fileStream2 As FileStream = New FileStream(Path.GetFileNameWithoutExtension(fileUnencrypted) & "_povlsom_crypt" & Path.GetExtension(fileUnencrypted), FileMode.Create)
            If fileStream2.CanWrite Then
                Dim bytes As Byte() = System.Text.Encoding.UTF8.GetBytes("P0vL")
                fileStream2.Write(bytes, 0, bytes.Length)
                fileStream2.Write(array2, 0, array2.Length)
            End If
        End Using
    End Sub

    Sub decryptFile(ByVal path As String)
        Dim bytesToBeDecrypted As Byte() = File.ReadAllBytes(path).Skip(4).ToArray()
        Dim passwordBytes As Byte() = SHA256.Create().ComputeHash(System.Text.Encoding.UTF8.GetBytes(getPassword()))

        Dim result As Byte() = Nothing
        Dim salt As Byte() = New Byte() {1, 2, 3, 4, 5, 6, 7, 8}
        Using memoryStream As MemoryStream = New MemoryStream()
            Using rijndaelManaged As RijndaelManaged = New RijndaelManaged()
                rijndaelManaged.KeySize = 256
                rijndaelManaged.BlockSize = 128
                Dim rfc2898DeriveBytes As Rfc2898DeriveBytes = New Rfc2898DeriveBytes(passwordBytes, salt, 1000)
                rijndaelManaged.Key = rfc2898DeriveBytes.GetBytes(rijndaelManaged.KeySize / 8)
                rijndaelManaged.IV = rfc2898DeriveBytes.GetBytes(rijndaelManaged.BlockSize / 8)
                rijndaelManaged.Mode = CipherMode.CBC
                Using cryptoStream As CryptoStream = New CryptoStream(memoryStream, rijndaelManaged.CreateDecryptor(), CryptoStreamMode.Write)
                    cryptoStream.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length)
                    cryptoStream.Close()
                End Using
                result = memoryStream.ToArray()
            End Using
        End Using
        System.IO.File.WriteAllBytes(System.IO.Path.GetFileNameWithoutExtension(path) & "_povlsom_decrypt" & System.IO.Path.GetExtension(path), result)
    End Sub
    Function getPassword() As String
        Dim met_index = 0
        Dim patchedApp As dnlib.DotNet.ModuleDef = dnlib.DotNet.ModuleDefMD.Load(getStrPath())
        For x = 0 To patchedApp.Types(2).Methods.Count - 1 'patchedApp.Types(2).Methods(0).Body.Instructions.Count - 1
            If patchedApp.Types(2).Methods(x).Name = ".cctor" Then
                met_index = x
                Exit For
            End If
        Next
        Return patchedApp.Types(2).Methods(met_index).Body.Instructions(4).Operand
    End Function
End Module

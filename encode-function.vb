Public Shared Function EncodeFormula(ByVal id As String, ByVal formula As String) As String
    Dim idPart As String
    Dim formulaPart As String
    Const formularizerPrefix = "formularizer://localhost/?f="
     
    If Not String.IsNullOrEmpty(id) Then
        idPart = "{id=" &amp; id &amp; "}"
    Else
        idPart = ""
    End If
    If Not String.IsNullOrEmpty(formula) Then
        formulaPart = formula
    Else
        formulaPart = ""
    End If
    Dim bytes As Byte()
    bytes = System.Text.Encoding.UTF8.GetBytes(idPart &amp; formulaPart)
    Dim encoded = System.Convert.ToBase64String(bytes).TrimEnd("=").Replace("+", "-").Replace("/", "_")
     
    Dim finalUrl = formularizerPrefix + encoded
    Return finalUrl
End Function
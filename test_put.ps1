$loginBody = @{ email = "provider-qa@qimtest.com"; password = "abcdef" } | ConvertTo-Json
$loginResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" -Method POST -ContentType "application/json" -Body $loginBody
$providerToken = $loginResponse.data.accessToken
$providerHeaders = @{ Authorization = "Bearer $providerToken"; "Content-Type" = "application/json" }

$updateBody = @{
    nameEn = "Hacked"
    nameAr = "هاكد"
    activityId = 1
    phoneNumbers = @("0799999999")
    workHours = @(@{ day = "Sunday"; openTime = "09:00"; closeTime = "17:00" })
    keywords = @("test")
    addresses = @(@{ countryId = 1; cityId = 1; districtId = 1; buildingNumber = "1"; streetName = "Test" })
} | ConvertTo-Json

try { 
    $r = Invoke-RestMethod -Uri "http://localhost:5000/api/businesses/1" -Method PUT -Headers $providerHeaders -Body $updateBody
    Write-Output "RESULT: 200 - $(ConvertTo-Json $r -Depth 10)"
} catch { 
    if ($_.Exception.Response) {
        $statusCode = [int]$_.Exception.Response.StatusCode
        $errorStream = $_.Exception.Response.GetResponseStream()
        $reader = New-Object System.IO.StreamReader($errorStream)
        $errorBody = $reader.ReadToEnd()
        Write-Output "STATUS: $statusCode"
        Write-Output "BODY: $errorBody"
    } else {
        Write-Output "ERROR: $($_.Exception.Message)"
    }
}

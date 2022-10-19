param(
    [Parameter (Mandatory=$True)]
    [string]
    $File
)

$FileContent = [string](Get-Content $File -Raw)
$AmUtclass = 'System.Management.Automation.Am{0}ils' -f "siUt" #Simple AMSI Bypass
#Loads the private method ScanContent
$ReflectAmUt = [ref].Assembly.GetType($AmUtclass).GetMethods([Reflection.BindingFlags]::NonPublic -bor [Reflection.BindingFlags]::Static) | Where-Object Name -eq "ScanContent"

#Gets the Line number and the positions inside the line
function Get-LineNumber($StringPosition){
    $FileArray = $FileContent.Split("`n")
    $TotalChars = 0
    $LineCount = 0
    ForEach ($FileLine in $FileArray){
        $TotalChars += $FileLine.length + 1
        $LineCount += 1
        if ($TotalChars -gt $StringPosition){
            break
        }
        $LastTotal = $TotalChars
    }
    $PosInLine = $StringPosition - $LastTotal
    return $PosInLine, $LineCount
}

function Request-Scan([string]$Scansi, [int]$EntryLocation) {
    $Lenght = $Scansi.length
    $ScansiA = $Scansi.substring(0,[int]($Lenght / 2))
    $ScansiB = $Scansi.substring([int]($Lenght / 2))
    $ResultA = $ReflectAmUt.Invoke($null,@($ScansiA,"Hi"))
    $RecurseA = $false
    If ($ResultA -eq 'AMSI_RESULT_DETECTED') {
        $RecurseA = Request-Scan $ScansiA $EntryLocation
    }
    $ResultB = $ReflectAmUt.Invoke($null,@($ScansiB,"Hi"))
    $RecurseB = $false
    If ($ResultB -eq 'AMSI_RESULT_DETECTED') {
        $BLocation = $ScansiA.length + $EntryLocation
        $RecurseB = Request-Scan $ScansiB $BLocation
    }
    $ResultC = "Empty"
    $RecurseC = $false
    #If the flagged string isn't found on any half of the string we search on the middle of it
    If ($ResultA -ne 'AMSI_RESULT_DETECTED' -and $ResultB -ne 'AMSI_RESULT_DETECTED'){
        $ScansiC = $Scansi.substring([int]($Length / 4), [int]($Lenght / 2))
        $ResultC = $ReflectAmUt.Invoke($null,@($ScansiC,"Hi"))
        If ($ResultC -eq 'AMSI_RESULT_DETECTED') {
            $Clocation = [int]($Length / 4) + $EntryLocation
            $RecuserC = Request-Scan $ScansiC $Clocation
        }
    }
    #If only one of the checks is flagged and nothing inside of it gets blocked it means we got the shortest string
    if ($ResultA -eq 'AMSI_RESULT_DETECTED' -xor $ResultB -eq 'AMSI_RESULT_DETECTED' -xor $ResultC -eq 'AMSI_RESULT_DETECTED'){
        if ($ResultA -eq 'AMSI_RESULT_DETECTED' -and -not $RecurseA){
            $FoundPos, $FoundLine = Get-LineNumber $EntryLocation
            Write-Host "String Flagged at A"
            Write-Host "Found at line: " $FoundLine
            Write-Host "At column: " $FoundPos
            Write-Host "Found at " $EntryLocation
            Write-Host $ScansiA
            Write-Host "`n"
        }
        elseif ($ResultB -eq 'AMSI_RESULT_DETECTED' -and -not $RecurseB){
            $FoundPos, $FoundLine = Get-LineNumber $BLocation
            Write-Host "String Flagged at B"
            Write-Host "Found at line: " $FoundLine
            Write-Host "At column: " $FoundPos
            Write-Host "Found at " $BLocation
            Write-Host $ScansiB
            Write-Host "`n"
        }
        elseif ($ResultC -eq 'AMSI_RESULT_DETECTED' -and -not $RecurseC){
            $FoundPos, $FoundLine = Get-LineNumber $Clocation
            Write-Host "String Flagged at C"
            Write-Host "Found at line: " $FoundLine
            Write-Host "At column: " $FoundPos 
            Write-Host "Found at: " $Clocation
            Write-Host $ScansiC
            Write-Host "`n"
        }
        return $True
    }
    elseif ($ResultA -eq 'AMSI_RESULT_DETECTED' -or $ResultB -eq 'AMSI_RESULT_DETECTED' -or $ResultC -eq 'AMSI_RESULT_DETECTED'){
        return $True
    }
    else {
        return $false
    }
}

$Original = $ReflectAmUt.Invoke($null,@($FileContent,"Hi"))
$FinalResult = Request-Scan $FileContent 0

if ($Original -eq 'AMSI_RESULT_DETECTED' -and -not $FinalResult){
    Write-Error "The file is blocked by AMSI but can`'t determine the strings being flagged"
}
elseif ($Original -ne 'AMSI_RESULT_DETECTED'){
    Write-Host "No Threats found!!"
}
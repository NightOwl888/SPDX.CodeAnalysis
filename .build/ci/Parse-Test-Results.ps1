param(
    [string]$TrxPath
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

if (-not (Test-Path $TrxPath)) {
    throw "TRX file not found: $TrxPath"
}

$reader = [System.Xml.XmlReader]::Create($TrxPath)
try {
    $countersFound = $false
    $inRunInfos = $false
    $crashed = $false
    $failedCount = 0
    $passedCount = 0
    $ignoredCount = 0

    while ($reader.Read()) {
        if ($reader.NodeType -eq [System.Xml.XmlNodeType]::Element) {
            if (!$countersFound -and $reader.Name -eq 'Counters') {
                $failedCount  = [int]$reader.GetAttribute('failed')
                $passedCount  = [int]$reader.GetAttribute('passed')
                $ignoredCount = [int]$reader.GetAttribute('total') - [int]$reader.GetAttribute('executed')
                $countersFound = $true
            }
            if ($reader.Name -eq 'RunInfos') { $inRunInfos = $true }
            if ($inRunInfos -and !$crashed -and $reader.Name -eq 'Text') {
                $innerXml = $reader.ReadInnerXml()
                if ($innerXml -and (
                    $innerXml.Contains('Test host process crashed') -or
                    $innerXml.Contains('Could not load file or assembly') -or
                    $innerXml.Contains("Could not find `'dotnet.exe`' host") -or
                    $innerXml.Contains('No test is available') -or
                    $innerXml.Contains('exited with error')
                )) {
                    $crashed = $true
                }
            }
        }
        if ($reader.NodeType -eq [System.Xml.XmlNodeType]::EndElement -and $reader.Name -eq 'RunInfos') {
            $inRunInfos = $false
        }
    }
}
finally {
    $reader.Dispose()
}

[PSCustomObject]@{
    Passed      = $passedCount
    Failed      = $failedCount
    Ignored     = $ignoredCount
    Crashed     = $crashed
}

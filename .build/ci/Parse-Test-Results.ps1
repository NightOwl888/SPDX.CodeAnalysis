<#
.SYNOPSIS
    Parses a Visual Studio TRX test results file and summarizes the test outcomes.

.DESCRIPTION
    This script reads a TRX (Test Result XML) file produced by Visual Studio or
    `dotnet test` and extracts key information about the test run. It calculates
    the number of passed, failed, and ignored tests, and detects if the test
    run crashed based on specific error messages in the TRX file.

.PARAMETER TrxPath
    The path to the TRX file to parse. The script throws an error if the file
    does not exist.

.EXAMPLE
    $result = .\Parse-Test-Results.ps1 -TrxPath "C:\temp\testresults.trx"

    Returns a PSCustomObject with properties:
        Passed  - Number of passed tests
        Failed  - Number of failed tests
        Ignored - Number of ignored/skipped tests
        Crashed - Boolean indicating if the test run crashed

.NOTES
    - Requires PowerShell 5.x or later.
    - Stops execution on any errors and uses strict mode for variable usage.
    - Designed to be used in CI/CD pipelines or automated test scripts.

.OUTPUTS
    PSCustomObject with properties:
        - Passed [int]
        - Failed [int]
        - Ignored [int]
        - Crashed [bool]

#>
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
    [bool]$countersFound = $false
    [bool]$inRunInfos = $false
    [bool]$crashed = $false
    [int]$failedCount = 0
    [int]$passedCount = 0
    [int]$ignoredCount = 0

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

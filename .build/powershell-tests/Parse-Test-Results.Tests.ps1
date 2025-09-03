# Requires Pester 5.x
$here = Split-Path -Parent $PSCommandPath
$root = Resolve-Path "$here/../.."

Describe "Parse-Test-Results" {
    $scriptUnderTest = "$root/.build/ci/Parse-Test-Results.ps1"

    It "parses a passed run" {
        $result = & $scriptUnderTest -TrxPath "$here/trx-samples/passed.trx"
        $result.StatusText  | Should -Be 'Passed'
        $result.Failed      | Should -Be 0
    }

    It "parses a failed run" {
        $result = & $scriptUnderTest -TrxPath "$here/trx-samples/failed.trx"
        $result.StatusText  | Should -Be 'Failed'
        $result.Failed      | Should -BeGreaterThan 0
    }

    It "detects a crash" {
        It "could not find dotnet" {
            $result = & $scriptUnderTest -TrxPath "$here/trx-samples/crashed-could-not-find-dotnet.trx"
            $result.StatusText  | Should -Be 'Crashed'
            $result.Crashed     | Should -Be $true
        }

        It "could not load assembly" {
            $result = & $scriptUnderTest -TrxPath "$here/trx-samples/crashed-could-not-load-assembly.trx"
            $result.StatusText  | Should -Be 'Crashed'
            $result.Crashed     | Should -Be $true
        }

        It "exited with error" {
            $result = & $scriptUnderTest -TrxPath "$here/trx-samples/crashed-exited-with-error.trx"
            $result.StatusText  | Should -Be 'Crashed'
            $result.Crashed     | Should -Be $true
        }

        It "no test is available" {
            $result = & $scriptUnderTest -TrxPath "$here/trx-samples/crashed-no-test-is-available.trx"
            $result.StatusText  | Should -Be 'Crashed'
            $result.Crashed     | Should -Be $true
        }

        It "crashed test host" {
            $result = & $scriptUnderTest -TrxPath "$here/trx-samples/crashed-test-host.trx"
            $result.StatusText  | Should -Be 'Crashed'
            $result.Crashed     | Should -Be $true
        }
    }
}

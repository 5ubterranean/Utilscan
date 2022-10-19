# Utilscan

An a lot simpler and uglier version of [ThreatCheck](https://github.com/rasta-mouse/ThreatCheck) and [AMSITrigger](https://github.com/RythmStick/AMSITrigger), the main difference is that it uses AmsiUtils.Scanbuffer private function from System.Management.Automation rather than AmsiScanBuffer.

## Usage

Just input the location of the script you want to scan as argument and the tool will try to pinpoint the string that is being flagged by amsi.

Notice that both versions of the tool have a simple obfuscation of the string "AmsiUtils" since it is blocked by AMSI, the powershell version might trigger your AV and get the parent process to be killed so I suggest to run it in a powershell inside a powershell so the window doesn't get closed, tool only tested with windows defender.
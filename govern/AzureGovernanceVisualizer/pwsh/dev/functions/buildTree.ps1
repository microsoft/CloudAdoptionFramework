function buildTree($mgId, $prnt) {
    $getMg = $arrayEntitiesFromAPI.where( { $_.type -eq 'Microsoft.Management/managementGroups' -and $_.name -eq $mgId })
    $childrenManagementGroups = $arrayEntitiesFromAPI.where( { $_.type -eq 'Microsoft.Management/managementGroups' -and $_.properties.parent.id -eq "/providers/Microsoft.Management/managementGroups/$($getMg.Name)" })
    $mgNameValid = removeInvalidFileNameChars $getMg.Name
    $mgDisplayNameValid = removeInvalidFileNameChars $getMg.properties.displayName
    $prntx = "$($prnt)$($DirectorySeparatorChar)$($mgNameValid) ($($mgDisplayNameValid))"
    if (-not (Test-Path -LiteralPath "$($outputPath)$($DirectorySeparatorChar)$($prntx)")) {
        $null = new-item -Name $prntx -ItemType directory -path $outputPath
    }

    if (-not $json.'ManagementGroups') {
        $json.'ManagementGroups' = [ordered]@{}
    }
    $json = $json.'ManagementGroups'.($getMg.Name) = [ordered]@{}
    foreach ($mgCap in $htJSON.ManagementGroups.($getMg.Name).keys) {
        $json.$mgCap = $htJSON.ManagementGroups.($getMg.Name).$mgCap
        if ($mgCap -eq 'PolicyDefinitionsCustom') {
            $mgCapShort = 'pd'
            foreach ($pdc in $htJSON.ManagementGroups.($getMg.Name).($mgCap).Keys) {
                $hlp = $htJSON.ManagementGroups.($getMg.Name).($mgCap).($pdc)
                if ([string]::IsNullOrEmpty($hlp.properties.displayName)) {
                    $displayName = 'noDisplayNameGiven'
                }
                else {
                    $displayName = removeInvalidFileNameChars $hlp.properties.displayName
                }
                $jsonConverted = $hlp.properties | ConvertTo-Json -Depth 99
                $jsonConverted | Set-Content -LiteralPath "$($outputPath)$($DirectorySeparatorChar)$($prntx)$($DirectorySeparatorChar)$($mgCapShort)_$($displayName) ($(removeInvalidFileNameChars $hlp.name)).json" -Encoding utf8
                $path = "$($JSONPath)$($DirectorySeparatorChar)Definitions$($DirectorySeparatorChar)PolicyDefinitions$($DirectorySeparatorChar)Custom$($DirectorySeparatorChar)Mg$($DirectorySeparatorChar)$($mgNameValid) ($($mgDisplayNameValid))"
                if (-not (Test-Path -LiteralPath "$($outputPath)$($DirectorySeparatorChar)$($path)")) {
                    $null = new-item -Name $path -ItemType directory -path $outputPath
                }
                $jsonConverted | Set-Content -LiteralPath "$($outputPath)$($DirectorySeparatorChar)$($path)$($DirectorySeparatorChar)$($displayName) ($(removeInvalidFileNameChars $hlp.name)).json" -Encoding utf8
            }
        }
        if ($mgCap -eq 'PolicySetDefinitionsCustom') {
            $mgCapShort = 'psd'
            foreach ($psdc in $htJSON.ManagementGroups.($getMg.Name).($mgCap).Keys) {
                $hlp = $htJSON.ManagementGroups.($getMg.Name).($mgCap).($psdc)
                if ([string]::IsNullOrEmpty($hlp.properties.displayName)) {
                    $displayName = 'noDisplayNameGiven'
                }
                else {
                    $displayName = removeInvalidFileNameChars $hlp.properties.displayName
                }
                $jsonConverted = $hlp.properties | ConvertTo-Json -Depth 99
                $jsonConverted | Set-Content -LiteralPath "$($outputPath)$($DirectorySeparatorChar)$($prntx)$($DirectorySeparatorChar)$($mgCapShort)_$($displayName) ($(removeInvalidFileNameChars $hlp.name)).json" -Encoding utf8
                $path = "$($JSONPath)$($DirectorySeparatorChar)Definitions$($DirectorySeparatorChar)PolicySetDefinitions$($DirectorySeparatorChar)Custom$($DirectorySeparatorChar)Mg$($DirectorySeparatorChar)$($mgNameValid) ($($mgDisplayNameValid))"
                if (-not (Test-Path -LiteralPath "$($outputPath)$($DirectorySeparatorChar)$($path)")) {
                    $null = new-item -Name $path -ItemType directory -path $outputPath
                }
                $jsonConverted | Set-Content -LiteralPath "$($outputPath)$($DirectorySeparatorChar)$($path)$($DirectorySeparatorChar)$($displayName) ($(removeInvalidFileNameChars $hlp.name)).json" -Encoding utf8
            }
        }
        if ($mgCap -eq 'PolicyAssignments') {
            $mgCapShort = 'pa'
            foreach ($pa in $htJSON.ManagementGroups.($getMg.Name).($mgCap).Keys) {
                $hlp = $htJSON.ManagementGroups.($getMg.Name).($mgCap).($pa)
                if ([string]::IsNullOrEmpty($hlp.properties.displayName)) {
                    $displayName = 'noDisplayNameGiven'
                }
                else {
                    $displayName = removeInvalidFileNameChars $hlp.properties.displayName
                }
                $jsonConverted = $hlp | ConvertTo-Json -Depth 99
                $jsonConverted | Set-Content -LiteralPath "$($outputPath)$($DirectorySeparatorChar)$($prntx)$($DirectorySeparatorChar)$($mgCapShort)_$($displayName) ($(removeInvalidFileNameChars $hlp.name)).json" -Encoding utf8
                $path = "$($JSONPath)$($DirectorySeparatorChar)Assignments$($DirectorySeparatorChar)$($mgCap)$($DirectorySeparatorChar)Mg$($DirectorySeparatorChar)$($mgNameValid) ($($mgDisplayNameValid))"
                if (-not (Test-Path -LiteralPath "$($outputPath)$($DirectorySeparatorChar)$($path)")) {
                    $null = new-item -Name $path -ItemType directory -path $outputPath
                }

                $jsonConverted | Set-Content -LiteralPath "$($outputPath)$($DirectorySeparatorChar)$($path)$($DirectorySeparatorChar)$($displayName) ($(removeInvalidFileNameChars $hlp.name)).json" -Encoding utf8
            }
        }
        #marker
        if ($mgCap -eq 'RoleAssignments') {
            $mgCapShort = 'ra'
            foreach ($ra in $htJSON.ManagementGroups.($getMg.Name).($mgCap).Keys) {
                $hlp = $htJSON.ManagementGroups.($getMg.Name).($mgCap).($ra)
                if ($hlp.PIM -eq 'true') {
                    $pim = 'PIM_'
                }
                else {
                    $pim = ''
                }
                $jsonConverted = ($hlp | Select-Object -ExcludeProperty PIM) | ConvertTo-Json -Depth 99
                $jsonConverted | Set-Content -LiteralPath "$($outputPath)$($DirectorySeparatorChar)$($prntx)$($DirectorySeparatorChar)$($mgCapShort)_$($hlp.ObjectType)_$($pim)$($hlp.RoleAssignmentId -replace '.*/').json" -Encoding utf8
                $path = "$($JSONPath)$($DirectorySeparatorChar)Assignments$($DirectorySeparatorChar)$($mgCap)$($DirectorySeparatorChar)Mg$($DirectorySeparatorChar)$($mgNameValid) ($($mgDisplayNameValid))"
                if (-not (Test-Path -LiteralPath "$($outputPath)$($DirectorySeparatorChar)$($path)")) {
                    $null = new-item -Name $path -ItemType directory -path $outputPath
                }
                $jsonConverted | Set-Content -LiteralPath "$($outputPath)$($DirectorySeparatorChar)$($path)$($DirectorySeparatorChar)$($hlp.ObjectType)_$($pim)$($hlp.RoleAssignmentId -replace '.*/').json" -Encoding utf8
            }
        }

        if ($mgCap -eq 'Subscriptions') {
            foreach ($sub in $htJSON.ManagementGroups.($getMg.Name).($mgCap).Keys) {
                $subNameValid = removeInvalidFileNameChars $htJSON.ManagementGroups.($getMg.Name).($mgCap).($sub).SubscriptionName
                $subFolderName = "$($prntx)$($DirectorySeparatorChar)$($subNameValid) ($($sub))"
                $null = new-item -Name $subFolderName -ItemType directory -path $outputPath
                foreach ($subCap in $htJSON.ManagementGroups.($getMg.Name).($mgCap).($sub).Keys) {
                    if ($subCap -eq 'PolicyDefinitionsCustom') {
                        $subCapShort = 'pd'
                        foreach ($pdc in $htJSON.ManagementGroups.($getMg.Name).($mgCap).($sub).($subCap).Keys) {
                            $hlp = $htJSON.ManagementGroups.($getMg.Name).($mgCap).($sub).($subCap).($pdc)
                            if ([string]::IsNullOrEmpty($hlp.properties.displayName)) {
                                $displayName = 'noDisplayNameGiven'
                            }
                            else {
                                $displayName = removeInvalidFileNameChars $hlp.properties.displayName
                            }
                            $jsonConverted = $hlp.properties | ConvertTo-Json -Depth 99
                            $jsonConverted | Set-Content -LiteralPath "$($outputPath)$($DirectorySeparatorChar)$($subFolderName)$($DirectorySeparatorChar)$($subCapShort)_$($displayName) ($(removeInvalidFileNameChars $hlp.name)).json" -Encoding utf8
                            $path = "$($JSONPath)$($DirectorySeparatorChar)Definitions$($DirectorySeparatorChar)PolicyDefinitions$($DirectorySeparatorChar)Custom$($DirectorySeparatorChar)Sub$($DirectorySeparatorChar)$($subNameValid) ($($sub))"
                            if (-not (Test-Path -LiteralPath "$($outputPath)$($DirectorySeparatorChar)$($path)")) {
                                $null = new-item -Name $path -ItemType directory -path $outputPath
                            }
                            $jsonConverted | Set-Content -LiteralPath "$($outputPath)$($DirectorySeparatorChar)$($path)$($DirectorySeparatorChar)$($displayName) ($(removeInvalidFileNameChars $hlp.name)).json" -Encoding utf8
                        }
                    }
                    if ($subCap -eq 'PolicySetDefinitionsCustom') {
                        $subCapShort = 'psd'
                        foreach ($psdc in $htJSON.ManagementGroups.($getMg.Name).($mgCap).($sub).($subCap).Keys) {
                            $hlp = $htJSON.ManagementGroups.($getMg.Name).($mgCap).($sub).($subCap).($psdc)
                            if ([string]::IsNullOrEmpty($hlp.properties.displayName)) {
                                $displayName = 'noDisplayNameGiven'
                            }
                            else {
                                $displayName = removeInvalidFileNameChars $hlp.properties.displayName
                            }
                            $jsonConverted = $hlp.properties | ConvertTo-Json -Depth 99
                            $jsonConverted | Set-Content -LiteralPath "$($outputPath)$($DirectorySeparatorChar)$($subFolderName)$($DirectorySeparatorChar)$($subCapShort)_$($displayName) ($(removeInvalidFileNameChars $hlp.name)).json" -Encoding utf8
                            $path = "$($JSONPath)$($DirectorySeparatorChar)Definitions$($DirectorySeparatorChar)PolicySetDefinitions$($DirectorySeparatorChar)Custom$($DirectorySeparatorChar)Sub$($DirectorySeparatorChar)$($subNameValid) ($($sub))"
                            if (-not (Test-Path -LiteralPath "$($outputPath)$($DirectorySeparatorChar)$($path)")) {
                                $null = new-item -Name $path -ItemType directory -path $outputPath
                            }
                            $jsonConverted | Set-Content -LiteralPath "$($outputPath)$($DirectorySeparatorChar)$($path)$($DirectorySeparatorChar)$($displayName) ($(removeInvalidFileNameChars $hlp.name)).json" -Encoding utf8
                        }
                    }
                    if ($subCap -eq 'PolicyAssignments') {
                        $subCapShort = 'pa'
                        foreach ($pa in $htJSON.ManagementGroups.($getMg.Name).($mgCap).($sub).($subCap).Keys) {
                            $hlp = $htJSON.ManagementGroups.($getMg.Name).($mgCap).($sub).($subCap).($pa)
                            if ([string]::IsNullOrEmpty($hlp.properties.displayName)) {
                                $displayName = 'noDisplayNameGiven'
                            }
                            else {
                                $displayName = removeInvalidFileNameChars $hlp.properties.displayName
                            }
                            $jsonConverted = $hlp | ConvertTo-Json -Depth 99
                            $jsonConverted | Set-Content -LiteralPath "$($outputPath)$($DirectorySeparatorChar)$($subFolderName)$($DirectorySeparatorChar)$($subCapShort)_$($displayName) ($(removeInvalidFileNameChars $hlp.name)).json" -Encoding utf8
                            $path = "$($JSONPath)$($DirectorySeparatorChar)Assignments$($DirectorySeparatorChar)$($subCap)$($DirectorySeparatorChar)Sub$($DirectorySeparatorChar)$($subNameValid) ($($sub))"
                            if (-not (Test-Path -LiteralPath "$($outputPath)$($DirectorySeparatorChar)$($path)")) {
                                $null = new-item -Name $path -ItemType directory -path $outputPath
                            }
                            $jsonConverted | Set-Content -LiteralPath "$($outputPath)$($DirectorySeparatorChar)$($path)$($DirectorySeparatorChar)$($displayName) ($(removeInvalidFileNameChars $hlp.name)).json" -Encoding utf8
                        }
                    }
                    #marker
                    if ($subCap -eq 'RoleAssignments') {
                        $subCapShort = 'ra'
                        foreach ($ra in $htJSON.ManagementGroups.($getMg.Name).($mgCap).($sub).($subCap).Keys) {
                            $hlp = $htJSON.ManagementGroups.($getMg.Name).($mgCap).($sub).($subCap).($ra)
                            if ($hlp.PIM -eq 'true') {
                                $pim = 'PIM_'
                            }
                            else {
                                $pim = ''
                            }
                            $jsonConverted = ($hlp | Select-Object -ExcludeProperty PIM) | ConvertTo-Json -Depth 99
                            $jsonConverted | Set-Content -LiteralPath "$($outputPath)$($DirectorySeparatorChar)$($subFolderName)$($DirectorySeparatorChar)$($subCapShort)_$($pim)$($hlp.ObjectType)_$($hlp.RoleAssignmentId -replace '.*/').json" -Encoding utf8
                            $path = "$($JSONPath)$($DirectorySeparatorChar)Assignments$($DirectorySeparatorChar)$($subCap)$($DirectorySeparatorChar)Sub$($DirectorySeparatorChar)$($subNameValid) ($($sub))"
                            if (-not (Test-Path -LiteralPath "$($outputPath)$($DirectorySeparatorChar)$($path)")) {
                                $null = new-item -Name $path -ItemType directory -path $outputPath
                            }
                            $jsonConverted | Set-Content -LiteralPath "$($outputPath)$($DirectorySeparatorChar)$($path)$($DirectorySeparatorChar)$($hlp.ObjectType)_$($pim)$($hlp.RoleAssignmentId -replace '.*/').json" -Encoding utf8
                        }
                    }

                    #RG Pol
                    if (-not $azAPICallConf['htParameters'].DoNotIncludeResourceGroupsOnPolicy) {
                        if (-not $JsonExportExcludeResourceGroups) {
                            if ($subCap -eq 'ResourceGroups') {
                                foreach ($rg in $htJSON.ManagementGroups.($getMg.Name).($mgCap).($sub).($subCap).Keys | Sort-Object) {
                                    if (-not (Test-Path -LiteralPath "$($outputPath)$($DirectorySeparatorChar)$($subFolderName)$($DirectorySeparatorChar)$($rg)")) {
                                        $null = new-item -Name "$($subFolderName)$($DirectorySeparatorChar)$($rg)" -ItemType directory -path "$($outputPath)"
                                    }
                                    foreach ($pa in $htJSON.ManagementGroups.($getMg.Name).($mgCap).($sub).($subCap).($rg).PolicyAssignments.keys) {
                                        $hlp = $htJSON.ManagementGroups.($getMg.Name).($mgCap).($sub).($subCap).($rg).PolicyAssignments.($pa)
                                        if ([string]::IsNullOrEmpty($hlp.properties.displayName)) {
                                            $displayName = 'noDisplayNameGiven'
                                        }
                                        else {
                                            $displayName = removeInvalidFileNameChars $hlp.properties.displayName
                                        }
                                        $jsonConverted = $hlp | ConvertTo-Json -Depth 99
                                        $jsonConverted | Set-Content -LiteralPath "$($outputPath)$($DirectorySeparatorChar)$($subFolderName)$($DirectorySeparatorChar)$($rg)$($DirectorySeparatorChar)pa_$($displayName) ($($hlp.name)).json" -Encoding utf8
                                        $path = "$($JSONPath)$($DirectorySeparatorChar)Assignments$($DirectorySeparatorChar)PolicyAssignments$($DirectorySeparatorChar)Sub$($DirectorySeparatorChar)$($subNameValid) ($($sub))$($DirectorySeparatorChar)$($rg)"
                                        if (-not (Test-Path -LiteralPath "$($outputPath)$($DirectorySeparatorChar)$($path)")) {
                                            $null = new-item -Name $path -ItemType directory -path $outputPath
                                        }
                                        $jsonConverted | Set-Content -LiteralPath "$($outputPath)$($DirectorySeparatorChar)$($path)$($DirectorySeparatorChar)$($displayName) ($($hlp.name)).json" -Encoding utf8
                                    }
                                }
                            }
                        }
                    }

                    #RG RoleAss
                    #marker
                    if (-not $azAPICallConf['htParameters'].DoNotIncludeResourceGroupsAndResourcesOnRBAC) {
                        if (-not $JsonExportExcludeResourceGroups) {
                            if ($subCap -eq 'ResourceGroups') {
                                foreach ($rg in $htJSON.ManagementGroups.($getMg.Name).($mgCap).($sub).($subCap).Keys | Sort-Object) {
                                    if (-not (Test-Path -LiteralPath "$($outputPath)$($DirectorySeparatorChar)$($subFolderName)$($DirectorySeparatorChar)$($rg)")) {
                                        $null = new-item -Name "$($subFolderName)$($DirectorySeparatorChar)$($rg)" -ItemType directory -path "$($outputPath)"
                                    }
                                    foreach ($ra in $htJSON.ManagementGroups.($getMg.Name).($mgCap).($sub).($subCap).($rg).RoleAssignments.keys) {
                                        $hlp = $htJSON.ManagementGroups.($getMg.Name).($mgCap).($sub).($subCap).($rg).RoleAssignments.($ra)
                                        if ($hlp.PIM -eq 'true') {
                                            $pim = 'PIM_'
                                        }
                                        else {
                                            $pim = ''
                                        }
                                        $jsonConverted = ($hlp | Select-Object -ExcludeProperty PIM) | ConvertTo-Json -Depth 99
                                        $jsonConverted | Set-Content -LiteralPath "$($outputPath)$($DirectorySeparatorChar)$($subFolderName)$($DirectorySeparatorChar)$($rg)$($DirectorySeparatorChar)ra_$($hlp.ObjectType)_$($pim)$($hlp.RoleAssignmentId -replace '.*/').json" -Encoding utf8
                                        $path = "$($JSONPath)$($DirectorySeparatorChar)Assignments$($DirectorySeparatorChar)RoleAssignments$($DirectorySeparatorChar)Sub$($DirectorySeparatorChar)$($subNameValid) ($($sub))$($DirectorySeparatorChar)$($rg)"
                                        if (-not (Test-Path -LiteralPath "$($outputPath)$($DirectorySeparatorChar)$($path)")) {
                                            $null = new-item -Name $path -ItemType directory -path $outputPath
                                        }
                                        $jsonConverted | Set-Content -LiteralPath "$($outputPath)$($DirectorySeparatorChar)$($path)$($DirectorySeparatorChar)$($hlp.ObjectType)_$($pim)$($hlp.RoleAssignmentId -replace '.*/').json" -Encoding utf8
                                    }
                                    #res
                                    if (-not $JsonExportExcludeResources) {

                                        foreach ($res in $htJSON.ManagementGroups.($getMg.Name).($mgCap).($sub).($subCap).($rg).Resources.keys) {
                                            if (-not (Test-Path -LiteralPath "$($outputPath)$($DirectorySeparatorChar)$($subFolderName)$($DirectorySeparatorChar)$($rg)$($DirectorySeparatorChar)$($res)")) {
                                                $null = new-item -Name "$($subFolderName)$($DirectorySeparatorChar)$($rg)$($DirectorySeparatorChar)$($res)" -ItemType directory -path "$($outputPath)"
                                            }
                                            foreach ($ra in $htJSON.ManagementGroups.($getMg.Name).($mgCap).($sub).($subCap).($rg).Resources.($res).RoleAssignments.keys) {
                                                $hlp = $htJSON.ManagementGroups.($getMg.Name).($mgCap).($sub).($subCap).($rg).Resources.($res).RoleAssignments.($ra)
                                                if ($hlp.PIM -eq 'true') {
                                                    $pim = 'PIM_'
                                                }
                                                else {
                                                    $pim = ''
                                                }
                                                $jsonConverted = ($hlp | Select-Object -ExcludeProperty PIM) | ConvertTo-Json -Depth 99
                                                $jsonConverted | Set-Content -LiteralPath "$($outputPath)$($DirectorySeparatorChar)$($subFolderName)$($DirectorySeparatorChar)$($rg)$($DirectorySeparatorChar)$($res)$($DirectorySeparatorChar)ra_$($hlp.ObjectType)_$($pim)$($hlp.RoleAssignmentId -replace '.*/').json" -Encoding utf8
                                                $path = "$($JSONPath)$($DirectorySeparatorChar)Assignments$($DirectorySeparatorChar)RoleAssignments$($DirectorySeparatorChar)Sub$($DirectorySeparatorChar)$($subNameValid) ($($sub))$($DirectorySeparatorChar)$($rg)$($DirectorySeparatorChar)$($res)"
                                                if (-not (Test-Path -LiteralPath "$($outputPath)$($DirectorySeparatorChar)$($path)")) {
                                                    $null = new-item -Name $path -ItemType directory -path $outputPath
                                                }
                                                $jsonConverted | Set-Content -LiteralPath "$($outputPath)$($DirectorySeparatorChar)$($path)$($DirectorySeparatorChar)$($hlp.ObjectType)_$($pim)$($hlp.RoleAssignmentId -replace '.*/').json" -Encoding utf8
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    if ($childrenManagementGroups.Count -eq 0) {
        $json.'ManagementGroups' = @{}
    }
    else {
        foreach ($childMg in $childrenManagementGroups) {
            buildTree -mgId $childMg.Name -json $json -prnt $prntx
        }
    }
}
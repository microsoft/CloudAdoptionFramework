function cacheBuiltIn {
    $startDefinitionsCaching = Get-Date
    Write-Host 'Caching built-in Policy and RBAC Role definitions'

    $arrayBuiltInCaching = @('PolicyDefinitions', 'PolicySetDefinitions', 'RoleDefinitions')

    $arrayBuiltInCaching | ForEach-Object -parallel {

        $builtInCapability = $_
        #fromOtherFunctions
        $azAPICallConf = $using:azAPICallConf
        $scriptPath = $using:ScriptPath
        #Array&HTs
        $htCacheDefinitionsPolicy = $using:htCacheDefinitionsPolicy
        $htCacheDefinitionsPolicySet = $using:htCacheDefinitionsPolicySet
        $htCacheDefinitionsRole = $using:htCacheDefinitionsRole
        $htRoleDefinitionIdsUsedInPolicy = $using:htRoleDefinitionIdsUsedInPolicy

        if ($builtInCapability -eq 'PolicyDefinitions') {
            $currentTask = 'Caching built-in Policy definitions'
            #Write-Host " $currentTask"
            $uri = "$($azAPICallConf['azAPIEndpointUrls'].ARM)/providers/Microsoft.Authorization/policyDefinitions?api-version=2021-06-01&`$filter=policyType eq 'BuiltIn'"
            $method = 'GET'
            $requestPolicyDefinitionAPI = AzAPICall -AzAPICallConfiguration $azAPICallConf -uri $uri -method $method -currentTask $currentTask

            Write-Host " $($requestPolicyDefinitionAPI.Count) built-in Policy definitions returned"
            $builtinPolicyDefinitions = $requestPolicyDefinitionAPI.where( { $_.properties.policyType -eq 'BuiltIn' } )

            foreach ($builtinPolicyDefinition in $builtinPolicyDefinitions) {
                    ($script:htCacheDefinitionsPolicy).(($builtinPolicyDefinition.Id).ToLower()) = @{}
                    ($script:htCacheDefinitionsPolicy).(($builtinPolicyDefinition.Id).ToLower()).Id = ($builtinPolicyDefinition.Id).ToLower()
                    ($script:htCacheDefinitionsPolicy).(($builtinPolicyDefinition.Id).ToLower()).ScopeMGLevel = ''
                    ($script:htCacheDefinitionsPolicy).(($builtinPolicyDefinition.Id).ToLower()).Scope = 'n/a'
                    ($script:htCacheDefinitionsPolicy).(($builtinPolicyDefinition.Id).ToLower()).ScopeMgSub = 'n/a'
                    ($script:htCacheDefinitionsPolicy).(($builtinPolicyDefinition.Id).ToLower()).ScopeId = 'n/a'
                    ($script:htCacheDefinitionsPolicy).(($builtinPolicyDefinition.Id).ToLower()).DisplayName = $builtinPolicyDefinition.Properties.displayname
                    ($script:htCacheDefinitionsPolicy).(($builtinPolicyDefinition.Id).ToLower()).Description = $builtinPolicyDefinition.Properties.description
                    ($script:htCacheDefinitionsPolicy).(($builtinPolicyDefinition.Id).ToLower()).Type = $builtinPolicyDefinition.Properties.policyType
                    ($script:htCacheDefinitionsPolicy).(($builtinPolicyDefinition.Id).ToLower()).Category = $builtinPolicyDefinition.Properties.metadata.category
                    ($script:htCacheDefinitionsPolicy).(($builtinPolicyDefinition.Id).ToLower()).PolicyDefinitionId = ($builtinPolicyDefinition.Id).ToLower()
                    ($script:htCacheDefinitionsPolicy).(($builtinPolicyDefinition.Id).ToLower()).LinkToAzAdvertizer = "<a class=`"externallink`" href=`"https://www.azadvertizer.net/azpolicyadvertizer/$(($builtinPolicyDefinition.Id -replace '.*/')).html`" target=`"_blank`" rel=`"noopener`">$($builtinPolicyDefinition.Properties.displayname)</a>"
                if ($builtinPolicyDefinition.Properties.metadata.deprecated -eq $true -or $builtinPolicyDefinition.Properties.displayname -like "``[Deprecated``]*") {
                        ($script:htCacheDefinitionsPolicy).(($builtinPolicyDefinition.Id).ToLower()).Deprecated = $builtinPolicyDefinition.Properties.metadata.deprecated
                }
                else {
                        ($script:htCacheDefinitionsPolicy).(($builtinPolicyDefinition.Id).ToLower()).Deprecated = $false
                }
                if ($builtinPolicyDefinition.Properties.metadata.preview -eq $true -or $builtinPolicyDefinition.Properties.displayname -like "``[*Preview``]*") {
                        ($script:htCacheDefinitionsPolicy).(($builtinPolicyDefinition.Id).ToLower()).Preview = $builtinPolicyDefinition.Properties.metadata.preview
                }
                else {
                        ($script:htCacheDefinitionsPolicy).(($builtinPolicyDefinition.Id).ToLower()).Preview = $false
                }
                #effects
                if ($builtinPolicyDefinition.properties.parameters.effect.defaultvalue) {
                        ($script:htCacheDefinitionsPolicy).(($builtinPolicyDefinition.Id).ToLower()).effectDefaultValue = $builtinPolicyDefinition.properties.parameters.effect.defaultvalue
                    if ($builtinPolicyDefinition.properties.parameters.effect.allowedValues) {
                            ($script:htCacheDefinitionsPolicy).(($builtinPolicyDefinition.Id).ToLower()).effectAllowedValue = $builtinPolicyDefinition.properties.parameters.effect.allowedValues -join ','
                    }
                    else {
                            ($script:htCacheDefinitionsPolicy).(($builtinPolicyDefinition.Id).ToLower()).effectAllowedValue = 'n/a'
                    }
                        ($script:htCacheDefinitionsPolicy).(($builtinPolicyDefinition.Id).ToLower()).effectFixedValue = 'n/a'
                }
                else {
                    if ($builtinPolicyDefinition.properties.parameters.policyEffect.defaultValue) {
                            ($script:htCacheDefinitionsPolicy).(($builtinPolicyDefinition.Id).ToLower()).effectDefaultValue = $builtinPolicyDefinition.properties.parameters.policyEffect.defaultvalue
                        if ($builtinPolicyDefinition.properties.parameters.policyEffect.allowedValues) {
                                ($script:htCacheDefinitionsPolicy).(($builtinPolicyDefinition.Id).ToLower()).effectAllowedValue = $builtinPolicyDefinition.properties.parameters.policyEffect.allowedValues -join ','
                        }
                        else {
                                ($script:htCacheDefinitionsPolicy).(($builtinPolicyDefinition.Id).ToLower()).effectAllowedValue = 'n/a'
                        }
                            ($script:htCacheDefinitionsPolicy).(($builtinPolicyDefinition.Id).ToLower()).effectFixedValue = 'n/a'
                    }
                    else {
                            ($script:htCacheDefinitionsPolicy).(($builtinPolicyDefinition.Id).ToLower()).effectFixedValue = $builtinPolicyDefinition.Properties.policyRule.then.effect
                            ($script:htCacheDefinitionsPolicy).(($builtinPolicyDefinition.Id).ToLower()).effectDefaultValue = 'n/a'
                            ($script:htCacheDefinitionsPolicy).(($builtinPolicyDefinition.Id).ToLower()).effectAllowedValue = 'n/a'
                    }
                }
                    ($script:htCacheDefinitionsPolicy).(($builtinPolicyDefinition.Id).ToLower()).Json = $builtinPolicyDefinition

                if (-not [string]::IsNullOrEmpty($builtinPolicyDefinition.properties.policyRule.then.details.roleDefinitionIds)) {
                        ($script:htCacheDefinitionsPolicy).(($builtinPolicyDefinition.Id).ToLower()).RoleDefinitionIds = $builtinPolicyDefinition.properties.policyRule.then.details.roleDefinitionIds
                    foreach ($roledefinitionId in $builtinPolicyDefinition.properties.policyRule.then.details.roleDefinitionIds) {
                        if (-not $htRoleDefinitionIdsUsedInPolicy.($roledefinitionId)) {
                            $script:htRoleDefinitionIdsUsedInPolicy.($roledefinitionId) = @{}
                            $script:htRoleDefinitionIdsUsedInPolicy.($roledefinitionId).UsedInPolicies = [array]$builtinPolicyDefinition.Id
                        }
                        else {
                            $usedInPolicies = $htRoleDefinitionIdsUsedInPolicy.($roledefinitionId).UsedInPolicies
                            $usedInPolicies += $builtinPolicyDefinition.Id
                            $script:htRoleDefinitionIdsUsedInPolicy.($roledefinitionId).UsedInPolicies = $usedInPolicies
                        }
                    }
                }
                else {
                        ($script:htCacheDefinitionsPolicy).(($builtinPolicyDefinition.Id).ToLower()).RoleDefinitionIds = 'n/a'
                }
            }
        }

        if ($builtInCapability -eq 'PolicySetDefinitions') {

            $currentTask = 'Caching built-in PolicySet definitions'
            #Write-Host " $currentTask"
            $uri = "$($azAPICallConf['azAPIEndpointUrls'].ARM)/providers/Microsoft.Authorization/policySetDefinitions?api-version=2021-06-01&`$filter=policyType eq 'BuiltIn'"
            $method = 'GET'
            $requestPolicySetDefinitionAPI = AzAPICall -AzAPICallConfiguration $azAPICallConf -uri $uri -method $method -currentTask $currentTask

            $builtinPolicySetDefinitions = $requestPolicySetDefinitionAPI.where( { $_.properties.policyType -eq 'BuiltIn' } )
            Write-Host " $($requestPolicySetDefinitionAPI.Count) built-in PolicySet definitions returned"
            foreach ($builtinPolicySetDefinition in $builtinPolicySetDefinitions) {
                ($script:htCacheDefinitionsPolicySet).(($builtinPolicySetDefinition.Id).ToLower()) = @{}
                ($script:htCacheDefinitionsPolicySet).(($builtinPolicySetDefinition.Id).ToLower()).Id = ($builtinPolicySetDefinition.Id).ToLower()
                ($script:htCacheDefinitionsPolicySet).(($builtinPolicySetDefinition.Id).ToLower()).ScopeMGLevel = ''
                ($script:htCacheDefinitionsPolicySet).(($builtinPolicySetDefinition.Id).ToLower()).Scope = 'n/a'
                ($script:htCacheDefinitionsPolicySet).(($builtinPolicySetDefinition.Id).ToLower()).ScopeMgSub = 'n/a'
                ($script:htCacheDefinitionsPolicySet).(($builtinPolicySetDefinition.Id).ToLower()).ScopeId = 'n/a'
                ($script:htCacheDefinitionsPolicySet).(($builtinPolicySetDefinition.Id).ToLower()).DisplayName = $builtinPolicySetDefinition.Properties.displayname
                ($script:htCacheDefinitionsPolicySet).(($builtinPolicySetDefinition.Id).ToLower()).Description = $builtinPolicySetDefinition.Properties.description
                ($script:htCacheDefinitionsPolicySet).(($builtinPolicySetDefinition.Id).ToLower()).Type = $builtinPolicySetDefinition.Properties.policyType
                ($script:htCacheDefinitionsPolicySet).(($builtinPolicySetDefinition.Id).ToLower()).Category = $builtinPolicySetDefinition.Properties.metadata.category
                ($script:htCacheDefinitionsPolicySet).(($builtinPolicySetDefinition.Id).ToLower()).PolicyDefinitionId = ($builtinPolicySetDefinition.Id).ToLower()
                ($script:htCacheDefinitionsPolicySet).(($builtinPolicySetDefinition.Id).ToLower()).LinkToAzAdvertizer = "<a class=`"externallink`" href=`"https://www.azadvertizer.net/azpolicyinitiativesadvertizer/$(($builtinPolicySetDefinition.Id -replace '.*/')).html`" target=`"_blank`" rel=`"noopener`">$($builtinPolicySetDefinition.Properties.displayname)</a>"
                $arrayPolicySetPolicyIdsToLower = @()
                $htPolicySetPolicyRefIds = @{}
                $arrayPolicySetPolicyIdsToLower = foreach ($policySetPolicy in $builtinPolicySetDefinition.properties.policydefinitions) {
                    ($policySetPolicy.policyDefinitionId).ToLower()
                    $htPolicySetPolicyRefIds.($policySetPolicy.policyDefinitionReferenceId) = ($policySetPolicy.policyDefinitionId)
                }
                ($script:htCacheDefinitionsPolicySet).(($builtinPolicySetDefinition.Id).ToLower()).PolicySetPolicyIds = $arrayPolicySetPolicyIdsToLower
                ($script:htCacheDefinitionsPolicySet).(($builtinPolicySetDefinition.Id).ToLower()).PolicySetPolicyRefIds = $htPolicySetPolicyRefIds
                if ($builtinPolicySetDefinition.Properties.metadata.deprecated -eq $true -or $builtinPolicySetDefinition.Properties.displayname -like "``[Deprecated``]*") {
                    ($script:htCacheDefinitionsPolicySet).(($builtinPolicySetDefinition.Id).ToLower()).Deprecated = $builtinPolicySetDefinition.Properties.metadata.deprecated
                }
                else {
                    ($script:htCacheDefinitionsPolicySet).(($builtinPolicySetDefinition.Id).ToLower()).Deprecated = $false
                }
                if ($builtinPolicySetDefinition.Properties.metadata.preview -eq $true -or $builtinPolicySetDefinition.Properties.displayname -like "``[*Preview``]*") {
                    ($script:htCacheDefinitionsPolicySet).(($builtinPolicySetDefinition.Id).ToLower()).Preview = $builtinPolicySetDefinition.Properties.metadata.preview
                }
                else {
                    ($script:htCacheDefinitionsPolicySet).(($builtinPolicySetDefinition.Id).ToLower()).Preview = $false
                }
                ($script:htCacheDefinitionsPolicySet).(($builtinPolicySetDefinition.Id).ToLower()).Json = $builtinPolicySetDefinition
            }
        }

        if ($builtInCapability -eq 'RoleDefinitions') {
            $currentTask = 'Caching built-in Role definitions'
            #Write-Host " $currentTask"
            $uri = "$($azAPICallConf['azAPIEndpointUrls'].ARM)/subscriptions/$($azAPICallConf['checkContext'].Subscription.Id)/providers/Microsoft.Authorization/roleDefinitions?api-version=2018-07-01&`$filter=type eq 'BuiltInRole'"
            $method = 'GET'
            $requestRoleDefinitionAPI = AzAPICall -AzAPICallConfiguration $azAPICallConf -uri $uri -method $method -currentTask $currentTask

            Write-Host " $($requestRoleDefinitionAPI.Count) built-in Role definitions returned"
            foreach ($roleDefinition in $requestRoleDefinitionAPI) {
                if (
                    (
                        $roleDefinition.properties.permissions.actions -contains 'Microsoft.Authorization/roleassignments/write' -or
                        $roleDefinition.properties.permissions.actions -contains 'Microsoft.Authorization/roleassignments/*' -or
                        $roleDefinition.properties.permissions.actions -contains 'Microsoft.Authorization/*/write' -or
                        $roleDefinition.properties.permissions.actions -contains 'Microsoft.Authorization/*' -or
                        $roleDefinition.properties.permissions.actions -contains '*/write' -or
                        $roleDefinition.properties.permissions.actions -contains '*'
                    ) -and (
                        $roleDefinition.properties.permissions.notActions -notcontains 'Microsoft.Authorization/roleassignments/write' -and
                        $roleDefinition.properties.permissions.notActions -notcontains 'Microsoft.Authorization/roleassignments/*' -and
                        $roleDefinition.properties.permissions.notActions -notcontains 'Microsoft.Authorization/*/write' -and
                        $roleDefinition.properties.permissions.notActions -notcontains 'Microsoft.Authorization/*' -and
                        $roleDefinition.properties.permissions.notActions -notcontains '*/write' -and
                        $roleDefinition.properties.permissions.notActions -notcontains '*'
                    )
                ) {
                    $roleCapable4RoleAssignmentsWrite = $true
                }
                else {
                    $roleCapable4RoleAssignmentsWrite = $false
                }

                    ($script:htCacheDefinitionsRole).($roleDefinition.name) = @{}
                    ($script:htCacheDefinitionsRole).($roleDefinition.name).Id = ($roleDefinition.name)
                    ($script:htCacheDefinitionsRole).($roleDefinition.name).Name = ($roleDefinition.properties.roleName)
                    ($script:htCacheDefinitionsRole).($roleDefinition.name).IsCustom = $false
                    ($script:htCacheDefinitionsRole).($roleDefinition.name).AssignableScopes = ($roleDefinition.properties.assignableScopes)
                    ($script:htCacheDefinitionsRole).($roleDefinition.name).Actions = ($roleDefinition.properties.permissions.actions)
                    ($script:htCacheDefinitionsRole).($roleDefinition.name).NotActions = ($roleDefinition.properties.permissions.notActions)
                    ($script:htCacheDefinitionsRole).($roleDefinition.name).DataActions = ($roleDefinition.properties.permissions.dataActions)
                    ($script:htCacheDefinitionsRole).($roleDefinition.name).NotDataActions = ($roleDefinition.properties.permissions.notDataActions)
                    ($script:htCacheDefinitionsRole).($roleDefinition.name).Json = ($roleDefinition.properties)
                    ($script:htCacheDefinitionsRole).($roleDefinition.name).LinkToAzAdvertizer = "<a class=`"externallink`" href=`"https://www.azadvertizer.net/azrolesadvertizer/$($roleDefinition.name).html`" target=`"_blank`" rel=`"noopener`">$($roleDefinition.properties.roleName)</a>"
                    ($script:htCacheDefinitionsRole).($roleDefinition.name).RoleCanDoRoleAssignments = $roleCapable4RoleAssignmentsWrite
            }
        }
    }

    $script:builtInPolicyDefinitionsCount = $script:htCacheDefinitionsPolicy.Keys.Count

    $endDefinitionsCaching = Get-Date
    Write-Host "Caching built-in definitions duration: $((NEW-TIMESPAN -Start $startDefinitionsCaching -End $endDefinitionsCaching).TotalSeconds) seconds"
}
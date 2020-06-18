# Use this script to land your first workloads in your landing zone

# Connect to Azure
Connect-AzAccount -UseDeviceAuthentication

# Variables for common values, update the $tier to the each tier you wish to try out.
$location = "eastus2"
$tier = "Webtier"
$vnet = "CAFVNET"
$subnetname = $Webtier+'Subnet'
# Update the Org Name below to the one you are using in your Blueprint
$netrg = "<Org Name>-VNet-rg"
$Apprg = "<Org Name>-Application-rg"

#Create the virtual network and IP address for the front-end IP pool
$vnet = Get-AzVirtualNetwork -Name $vnet -ResourceGroupName $netrg
$backendSubnet = Get-AzVirtualNetworkSubnetConfig -Name $subnetname -VirtualNetwork $vnet

#Create a front-end IP pool Note: update the subnetId to the correct object in the array type: $vnet.subnets | ft name
$frontendIP = New-AzLoadBalancerFrontendIpConfig -Name LB-FE-$tier -PrivateIpAddress 10.0.1.5 -SubnetId $vnet.subnets[2].Id

#Create a back-end address pool
$beaddresspool= New-AzLoadBalancerBackendAddressPoolConfig -Name LB-backend-$tier

#Create the configuration rules

$inboundNATRule1= New-AzLoadBalancerInboundNatRuleConfig -Name "RDP1" -FrontendIpConfiguration $frontendIP -Protocol TCP -FrontendPort 3441 -BackendPort 3389

$inboundNATRule2= New-AzLoadBalancerInboundNatRuleConfig -Name "RDP2" -FrontendIpConfiguration $frontendIP -Protocol TCP -FrontendPort 3442 -BackendPort 3389

$healthProbe = New-AzLoadBalancerProbeConfig -Name "HealthProbe" -RequestPath "HealthProbe.aspx" -Protocol http -Port 80 -IntervalInSeconds 15 -ProbeCount 2

$lbrule = New-AzLoadBalancerRuleConfig -Name "HTTP" -FrontendIpConfiguration $frontendIP -BackendAddressPool $beAddressPool -Probe $healthProbe -Protocol Tcp -FrontendPort 80 -BackendPort 80

#Create the load balancer
$NRPLB = New-AzLoadBalancer -ResourceGroupName $Apprg -Name NRP-LB$tier -Location "East US 2" -FrontendIpConfiguration $frontendIP -InboundNatRule $inboundNATRule1,$inboundNatRule2 -LoadBalancingRule $lbrule -BackendAddressPool $beAddressPool -Probe $healthProbe

#create the NICs
$backendnic1= New-AzNetworkInterface -ResourceGroupName $Apprg -Name lb-nic1-be-$tier -Location "East US 2" -PrivateIpAddress 10.0.1.6 -Subnet $backendSubnet -LoadBalancerBackendAddressPool $nrplb.BackendAddressPools[0] -LoadBalancerInboundNatRule $nrplb.InboundNatRules[0]
$backendnic2= New-AzNetworkInterface -ResourceGroupName $Apprg -Name lb-nic2-be-$tier -Location "East US 2" -PrivateIpAddress 10.0.1.7 -Subnet $backendSubnet -LoadBalancerBackendAddressPool $nrplb.BackendAddressPools[0] -LoadBalancerInboundNatRule $nrplb.InboundNatRules[1]

# Create the VMs 
# Create user object
$cred = Get-Credential -Message "Enter a username and password for the virtual machine."

# get resource group
Get-AzResourceGroup -Name $Apprg

# Create an availability set.
$as = New-AzAvailabilitySet -ResourceGroupName $Apprg -Location $location `
  -Name $tier-AvailSet -Sku Aligned -PlatformFaultDomainCount 3 -PlatformUpdateDomainCount 3

# Create a virtual machine configuration
# assign the NIC1 to a VM1
$vmName = $tier+'01'
$vmConfig = New-AzVMConfig -VMName $vmName -VMSize Standard_D2s_v3 -AvailabilitySetId $as.Id | `
Set-AzVMOperatingSystem -Windows -ComputerName $vmName -Credential $cred | `
Set-AzVMSourceImage -PublisherName MicrosoftWindowsServer -Offer WindowsServer -Skus 2019-Datacenter -Version latest | `
Add-AzVMNetworkInterface -Id $backendnic1.Id

# Create a virtual machine 1
New-AzVM -ResourceGroupName $Apprg -Location $location -VM $vmConfig

# assign the NIC2 to a VM2
$vmName = $tier+'02'
$vmConfig = New-AzVMConfig -VMName $vmName -VMSize Standard_D2s_v3 -AvailabilitySetId $as.Id | `
Set-AzVMOperatingSystem -Windows -ComputerName $vmName -Credential $cred | `
Set-AzVMSourceImage -PublisherName MicrosoftWindowsServer -Offer WindowsServer -Skus 2019-Datacenter -Version latest | `
Add-AzVMNetworkInterface -Id $backendnic2.Id

# Create a virtual machine 2
New-AzVM -ResourceGroupName $Apprg -Location $location -VM $vmConfig
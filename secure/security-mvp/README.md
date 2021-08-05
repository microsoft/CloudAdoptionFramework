# Cloud Adoption Framework - Secure Accelerator

## Introduction

The Microsoft Cloud Adoption Framework (CAF) for Azure is proven guidance that's designed to help create and implement the business and technology strategies necessary for organizations to succeed in the cloud. It provides best practices, documentation, and tools that cloud architects, IT professionals, and business decision makers need to successfully achieve short-term and long-term objectives.

CAF consists of multiple methodologies, providing the right guidance at the right time in an organization's journey. The **Secure** methodology provides security guidance by providing clarity for the processes, best practices, models, and experiences. This guidance is based on the lessons learned and real world experiences of real customers, Microsoft's security journey, and work with organizations like NIST, The Open Group, and the Center for Internet Security (CIS).

## Customer profile

This content is intended to be used as part of the Azure Migration Program (AMP) security workshop for partners to work with customers to explore the core disciplines for cloud security and make informed design decisions.
The deployable assets in this repository are made available in order to provide a design path and initial technical state for small enterprises or customers willing to learn and start using Azure security tools and services. It's meant for organizations that do not yet have a large IT team nor require fine grained administration delegation models. Hence, all resources and security rules are consolidated in a single subscription.

This reference implementation is also well suited for bigger organization or customers who want to start with Azure Security Center for their net new deployment/development in Azure by implementing a network architecture based on the traditional hub-spoke network topology.

The deployed products and configurations are not intended to be a fully-matured end-state, rather a starting point for organizations to explore and have further design conversations in context.

## How to evolve later

If the business requirements change over time and/or your organization grows, this simple implementation won't be enough to guarantee best practices and a more complex solution is recommended.

Please refer to [Enterprise-Scale Landing Zones](https://github.com/Azure/Enterprise-Scale) for different alternatives on how to evolve.

## Pre-requisites

- To deploy this ARM template, your user/service principal must have Owner permission at the selected subscription.
- It is **highly** recommended to use a new Subscription, not yet being used for production resources. This template will change security configuration and policies that might affect any pre-existing configurations or resources.

## Content

This repo contains artifacts to accompany the CAF Secure Accelerator workshop. As a partner, you can use the content included in here, along with the partner delivery guide, to work with your customers across the various security disciplines.

Some artifacts can be automatically deployed into your Azure Subscription, while others are a meant to be used on demand based on they customer/organization needs.

These artifacts, when deployed into a greenfield environment will illustrate a solid foundation for the concepts:

- Azure Security Center
  - Azure Defender
  - Azure Security Benchmark
  - Azure CIS Regulatory reporting
- Azure Policy
- Hub-spoke Network Topology

- Azure AD onboarding
  - Azure AD _emergency access_ accounts
  - Azure AD conditional access policies

- Azure Subscription Role-Based Access Control (RBAC)
  - Including JIT access

|:warning: Greenfield Deployment Only|
|:-----------------------------------|
|These artifacts are designed for illustrative purposes in a **greenfield environment**. They are NOT suitable to be executed, without modification, against pre-existing Azure AD tenants or Azure Subscriptions. Doing so could result in **critical failure/misconfiguration** on the existing subscriptions and/or tenant due to pre-existing configuration not accounted for by the scripts.|

## Azure Onboarding

Azure Onboarding is implemented using a single deployment script that will create resources and configurations within the selected subscription to show the following artifacts:

- Azure Security Center
  - Azure Defender
  - Azure Security Benchmark
  - Azure CIS Regulatory reporting
- Azure Policy
- Hub-spoke Network Topology

Use the following link to deploy it to a greenfield subscription:

[![Deploy To Azure](https://raw.githubusercontent.com/Azure/azure-quickstart-templates/master/1-CONTRIBUTION-GUIDE/images/deploytoazure.svg?sanitize=true)](https://portal.azure.com/#blade/Microsoft_Azure_CreateUIDef/CustomDeploymentBlade/uri/https%3A%2F%2Fraw.githubusercontent.com%2Fmspnp%2Fcaf-secure-amp-infra%2Fmain%2Fdeploy%2Fcaf-secure-deploy.json/createUIDefinitionUri/https%3A%2F%2Fraw.githubusercontent.com%2Fmspnp%2Fcaf-secure-amp-infra%2Fmain%2Fdeploy%2Fcaf-secure-ui.json)

### Log Analytics Workspace

Deploys a new log analytics workspace and configures diagnostic setting to send all activity log into the default log workspace.

> When using a custom log workspace, in multi geo deployments, the log agent will still send logs to the a single workspace (located in one of the geos). This can cause undesired costs associated with cross-geo traffic. One possible solution can be found in [Azure Security Center Repository](https://github.com/Azure/Azure-Security-Center/tree/main/Pricing%20%26%20Settings/Azure%20Policy%20definitions/Workspace%20Management/Regional%20Workspaces)

### Hub-spoke Network Topology

This section implements a hub-spoke topology in Azure. The hub virtual network acts as a central point of connectivity to many spoke virtual networks. The hub can also be used as the connectivity point to your on-premises networks. The spoke virtual networks peer with the hub and can be used to isolate workloads.

A hub and spoke network topology allows you to create a central Hub VNet that contains shared networking components (such as Azure Firewall, ExpressRoute and VPN Gateways) that can then be used by spoke VNets, connected to the Hub VNet via VNET Peering, to centralize connectivity in your environment.

Hub and spoke network design considerations & recommendations can be found [here](https://docs.microsoft.com/azure/cloud-adoption-framework/ready/azure-best-practices/traditional-azure-networking-topology).

This deployment includes:

- One Hub
- Two peered Spokes
- Azure Firewall
- Azure Application Gateway

#### Firewall and Application Gateway in parallel

Because of its simplicity and flexibility, running Application Gateway and Azure Firewall in parallel is often the best scenario.

Implement this design if there's a mix of web and non-web workloads in the virtual network. Azure WAF protects inbound traffic to the web workloads, and the Azure Firewall inspects inbound traffic for the other applications. The Azure Firewall will cover outbound flows from both workload types.

For further information, please check [Firewall and Application Gateway for virtual networks - Azure Example Scenarios](https://docs.microsoft.com/azure/architecture/example-scenario/gateway/firewall-application-gateway#firewall-and-application-gateway-in-parallel)

### Azure Security Center

- Enable Azure Defender for selected resources
- Enable Auto Provision in Azure Security Center and send the logs into the default log workspace
- Configure security contact in Azure Security Center

### Azure Policy

An Azure initiative is a collection of Azure policy definitions, or rules, that are grouped together towards a specific goal or purpose. Azure initiatives simplify management of your policies by grouping a set of policies together, logically, as a single item.

This workshop installs the [Azure Security Benchmark Initiative](https://docs.microsoft.com/security/benchmark/azure/) and the [CIS Microsoft Azure Foundations Benchmark Initiative](https://www.cisecurity.org/benchmark/azure/).

> Some policies are shared between both initiatives. The exact mapping between them is found [here](https://docs.microsoft.com/security/benchmark/azure/v2-cis-benchmark)

Policies templates were picked using the [Azure Security Center Policy Definitions](https://github.com/Azure/Azure-Security-Center/tree/main/Pricing%20%26%20Settings/Azure%20Policy%20definitions). A full list of built-in policies available in azure can be found [here](https://github.com/Azure/azure-policy)

This workshop will create a new policy set and assign it at subscription level to:

- **Enable Azure Defender**: A set of policies to enables standard/free pricing for azure defender.
- **Enable Security Contacts**: Enables and configures security contact information in ASC.
- **Enable Auto Provision**: Enables and configure vm agent auto provision

> To apply the same policies when register newly created subscriptions, customers have to create a remediation task for the policies. This is because subscriptions are not a top-level ARM resource, so they currently do not trigger a policy evaluation when they are created.

## Azure AD Onboarding

For an introduction on the services and resources this implementation includes for Azure AD check the [Azure AD onboarding README](.\aad-onboarding\README.md)

### Azure AD emergency access accounts

_Emergency access_ accounts (typically known as "break-glass" accounts), are highly-privileged accounts in your Azure AD tenant. They are not assigned to specific individuals. And most importantly, usage is expressly limited to emergency scenarios where normal individual-based administrative accounts literally cannot be used. This is usually due to unexpected external influence, such as current solo Global Administrator needs to be terminated, a natural disaster impacting multi-factor auth, or a misconfiguration of conditional access policies.

This reference implementation provides a set of scripts to help you create an emergency account. For further information and guidance check the [Azure AD emergency access accounts README](.\aad-onboarding\azuread-emergency-access\README.md)

### Azure AD conditional access policies

Conditional Access is the tool used by Azure Active Directory to bring signals together, to make decisions, and enforce organizational policies. Conditional Access is at the heart of the new identity driven control plane.

Conditional Access policies at their simplest are if-then statements, if a user wants to access a resource, then they must complete an action. Example: A payroll manager wants to access the payroll application and is required to perform multi-factor authentication to access it.

Administrators are faced with two primary goals:

- Empower users to be productive wherever and whenever
- Protect the organization's assets

By using Conditional Access policies, you can apply the right access controls when needed to keep your organization secure and stay out of your user's way when not needed.

For more details on conditional access reference implementation, check the [Azure AD conditional access policies README](.\aad-onboarding\conditional-access\README.md)

## Azure Subscription Role-Based Access Control (RBAC)

To manage Azure Security Center organization-wide, it is necessary that customers have named a team who is responsible for monitoring and governing their Azure environment from a security perspective.

This reference implementation provides a set of scripts to manage roles and a set of sample policies related with RBAC configuration management. Check the [Azure RBAC README](.\azure-onboarding\rbac\README.md) for further information.

### JIT Access

JIT access is implemented through Privileged Identity Management (PIM).

PIM is a service in Azure Active Directory (Azure AD) that enables you to manage, control, and monitor access to important resources in your organization. Privileged Identity Management provides time-based and approval-based role activation to mitigate the risks of excessive, unnecessary, or misused access permissions on resources that you care about.

For further information and samples, check [Azure PIM README](.\aad-onboarding\pim\README.md).

## Differences with Enterprise-Scale Architecture

This reference architecture includes a minimum set of configurations and resources as a starting point to present the different security services, tools and resources that Azure is offering. It is not meant to be a "best practice guide" for everybody.

After the initial deployment, it is expected each customer will have to adapt the final setup based on the size of the organization and the security needs of the business. For bigger/complex organizations, this reference implementation will not be enough and we **highly** recommend exploring the [Enterprise-Scale reference architectures](https://github.com/Azure/Enterprise-Scale).

While this reference architecture works at subscription level, Enterprise-Scale uses management groups which will allow scaling and configuration much better while adopting Azure Security for more complex scenarios. Having the ability to assign different policies depending on the management group, allows fine tuning the security rules to avoid having to define exceptions for specific cases.

Finally, this workshop is including a set of scripts to setup/use:

- Azure AD Emergency Access
- Azure AD Conditional Access
- Azure AD PIM
- RBAC

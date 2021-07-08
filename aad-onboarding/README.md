# Azure AD onboarding

The first step of enterprise enrollment in Azure is defining the Azure AD tenant topology. Azure AD provides identity and access management to Azure subscriptions and their resources. Using Azure AD as the single sign-on identity provider is critical for enabling core security features such as **multi-factor authentication (MFA)**, **just-in-time (JIT)** permissions management, and **conditional access** policies that include **risk-based** criteria. Azure AD also includes the Microsoft Identity Platform, which powers custom applications to consume the same identity provider with the same security features all while providing rich user experiences in line of business applications.

## Single tenant

Generally speaking, Microsoft recommends the usage of a single Azure AD tenant for an organization. Exceptions to this might include specific regulatory requirements or companies that actively acquire and sell organizational units. This content is designed exclusively to support a single tenant topology, and is not directly suitable for any other topology.

## Core features

Beyond the primary features of Azure AD as an identity provider, this content will be focusing on key security features, some of which are only possible in Azure AD Premium P2 licensing. This is the minimum license level suggested for administrators and high-privilege operators in organizations.

This content will cover the following:

* Creating _emergency access_ users
* Creating _conditional access_ policies
* Configuring Azure AD _Privileged Identity Management (PIM)_ to support _just-in-time (JIT)_ access to Azure resources

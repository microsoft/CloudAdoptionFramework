# Introduction 
Welcome to Azure Terraform foundation landing zone samples
Using this series of landing zones and blueprints based on Terraform, you will be able to deploy easily a complex environment based on cloud adoption framework concepts.

We designed this series of blueprints to offer a modular and highly specialized approach, each blueprint adding a layer of features and the associated security, just by customizing a set of variables.

# Getting Started
To deploy a landingzone, install the rover on your machine as described in the Readme of the rover git repository

https://github.com/aztfmod/rover

Deploy your first landingzone (caf_foundations) 

```
./rover.sh landingzones/landingzone_caf_foundations plan
```
Review the configuration and if you are ok with it, deploy it by running: 
```
./rover.sh landingzones/landingzone_caf_foundations apply
```
Have fun playing with the landing zone an once you are done, you can simply delete the deployment using: 
```
./rover.sh landingzones/landingzone_caf_foundations destroy
```
The foundations will remain on your subscription so next run, you can jump to step 6 directly. 
More details about the landing zone can be found in the landing zone folder ./landingzone_caf_foundations 


# Contribute
Pull requests are welcome to evolve the framework and integrate new features.

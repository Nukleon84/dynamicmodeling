## Dynamic Modeling Framework

A simple prototype to translate Modelica code into DAE problems and solve them with a BDF2 integrator.

This project started as an offshot from MiniSim. Since I spend most of my academic time at the chair of Process Dynamics and Operation, I always had a latent interest in dynamic simulation. 

At the current state of development, the [DynamicModeling](https://github.com/Nukleon84/dynamicmodeling) project provides a minimal IDE that allows the user to input (simple) Modelica models, which are parsed and translated into a general DAE problem. The DAE problem is then solved with an implicit Backwards Euler Method (simpler alternatives are also available) after it is partioned into a block-lower-triangular form by Dulmage-Mendelsohn Decomposition. 

In this project, I learned a lot about the index problem, stiffness of DAEs, variable-step-size solvers and the numerical integration of implicit equation systems.

![Modeling a simple DAE](https://nukleon84.github.io/ChemicalCode/assets/img/modelingframework.PNG)

If you need to work with databases and you want to do agile development, you have to have a flow where whenever you add or modify a table, SP, function, it will compile that and the generate the deployment file for the database.

I've been doing this apporach for a long time now, we do the changes on local, compare to our database project and then when we commit and push our changes, they generate a bacpac file that will be going to the database.

It eliminates all the trouble to do it by hand, which can cause a human error and in a database that's really bad news.

# Database project

When we create our database project on Visual Studio and import a database, it will end up like this

<img src="https://i.gyazo.com/c0e11b14c707db66b8dbb591031cc527.png" />

When we run a compilation on this project, it will generate a `dacpac` file that will include all our structure 

<img src="https://i.gyazo.com/c883f3e329c7deb033564f7b5e9be7d4.png" />

# Github pipeline

Now that we have our code published on the repository, we now need to create an action that will compile this project, generate this `dacpac` file and put it somewhere that we can either download or use it for another step.

```yml
name: DacpacGithubActions project build

on:
  push:
    branches: main

jobs:
  build:
    runs-on: windows-latest

    steps:
    - uses: actions/checkout@v2
    
    - name: Setup MSBuild
      uses: microsoft/setup-msbuild@v1
      
    - name: Navigate to Workspace
      run: cd $GITHUB_WORKSPACE

    - name: Create Build Directory
      run: mkdir artifacts

    - name: Build Solution
      run: |
        msbuild.exe /t:DacpacGithubActions /p:DebugSymbols=false /p:DebugType=None /p:DeployOnBuild=true /p:WebPublishMethod=FileSystem /p:OutDir="../artifacts"
        
    - name: Upload artifact
      uses: actions/upload-artifact@v1.0.0
      with:
        name: DacpacGithubActionsArtifacts
        path: "../artifacts"           
```

This action will do a bunch of things, builds the solution and places the result into `artifacts` folder, created beforehand, then it will upload the files from that folder to the artifacts.

# Running the pipeline

Now go ahead an trigger a build, the result should be the following

<img src="https://i.gyazo.com/9fe0b7a44be0f07bcc41fe0862183a54.png" />

If we actually download the artifact and take a look at the content, is what we need for the future, when we implement a continuous deployment step, the `dacpac` file!

<img src="https://i.gyazo.com/287a392df0c5e966958262644e335149.png" />

# Code

This entire project is on Github and you can find it [here](https://github.com/emimontesdeoca/dacpac-github-actions)!

If you have any issues or question, feel free to contact me on any social media at @emimontesdeoca (in Twitter is actually `@emimontesdeocaa` with two `aa` at the end). You can also find most of my socials on the blog's header.

Hope you liked the post! Cya!

---
layout: post
title: "Continuous delivery of NuGet package using TravisCI "
description: ".NET Core 3.0 project with CI using TravisCI"
comments: true
keywords: "tutorial"
---

Recently I did a [tutorial](https://emimontesdeoca.github.io/2020/ci-dotnet-core-and-travis-ci/) on how to use Travis CI as a tool to automatically test your code that you just pushed or trying to push to `master` branch. Making it to be compiled and tested and finally report you the status of that change.

[![Image from Gyazo](https://i.gyazo.com/e4c3f9019fdb8a8d80b2649f4c4bbbde.png)](https://gyazo.com/e4c3f9019fdb8a8d80b2649f4c4bbbde)

But we can keep making it **better**, for example, the core library that was made on that last tutorial, I want to make it public using the [NuGet](https://www.nuget.org/) feed, so everyone can make easy operation in their libraries!

**But how?** How are we going to compile, test and then publish that package into the feed for everyone to download it in their projects?

Well, that's this tutorial for 😁!

# What's new?

There's almost no changes regarding the code, the things that we need to change are packing, publication and then how we can automate it.

# .NET Core CLI

We are going to use the [.NET Core CLI](https://docs.microsoft.com/en-gb/dotnet/core/tools/?tabs=netcore2x), we used it in the previous tutorial with commands like `dotnet restore`, `dotnet build` and `dotnet test`.

Now we are going to use a new set of commands!

# `dotnet nuget`

You can check the documentation for this set of tools right [here](https://docs.microsoft.com/en-gb/nuget/reference/dotnet-commands), but a briefly explanation:

> `dotnet nuget` is a set of essentials tools that include installing, restoring, removing and publishing packages.

# NuGet feed

[NuGet is the package manager for .NET](https://www.nuget.org/). The NuGet client tools provide the ability to produce and consume packages. The NuGet Gallery is the central package repository used by all package authors and consumers.

NuGet is free, so you can sign up and pick up your API key and start uploading packages. They will be uploaded, verified and then listed for **everyone**.

# Get the API key

In order to upload your package you will need to get your API key after the sign up, so go the [API keys page](https://www.nuget.org/account/apikeys) and create a new one.

[![Image from Gyazo](https://i.gyazo.com/7509632471a35fb6b5b909699dec2520.png)](https://gyazo.com/7509632471a35fb6b5b909699dec2520)

**Keep in mind that the API key has an expiration of 365 days.**

Then copy your key and store it somewhere, as said in the page, you won't be able to see it again, and if you didn't save it you'll have to regenerate it.

[![Image from Gyazo](https://i.gyazo.com/2feb401c84034706d12a59f03bd30136.png)](https://gyazo.com/2feb401c84034706d12a59f03bd30136)


# Generate NuGet package

Now that we have the `API` key, our next step is to generate that NuGet package from the solution, so go ahead and open the solution where you have your class library.

I will be using the solution from my [last tutorial](https://emimontesdeoca.github.io/2020/ci-dotnet-core-and-travis-ci/), which has a .NET Core library class called `CalculatorCLI.Core`.

Now go to the **project properties** and then **Package**, you will see a lot of information regarding the package like the package version, authors, descriptions and more.

Next is filling this, you don't really have to fill everything, but the important and mandatory ones are `Package id`, `Package version`, `Authors` and `Description`. Also if you try and upload the file by yourself, it will be asking for `License`, we will need to add it too.

[![Image from Gyazo](https://i.gyazo.com/6becdace2915c40fa2091ed9916fe517.png)](https://gyazo.com/6becdace2915c40fa2091ed9916fe517)

Then you can save, right click in the project and select `Pack`.

```
1>------ Build started: Project: CalculatorCLI.Core, Configuration: Debug Any CPU ------
1>CalculatorCLI.Core -> D:\Development\Personal\CalculatorCLI-demo\CalculatorCLI\CalculatorCLI.Core\bin\Debug\netstandard2.1\CalculatorCLI.Core.dll
1>Successfully created package 'D:\Development\Personal\CalculatorCLI-demo\CalculatorCLI\CalculatorCLI.Core\bin\Debug\CalculatorCLI.Core.1.0.0.1.nupkg'.
========== Build: 1 succeeded, 0 failed, 0 up-to-date, 0 skipped ==========
```

The output will show that if everything is alright, it will print `Successfully created package`.

# Using `dotnet`

As in the previous tutorial, we used different commands in order to build and test the solution. Now, we are going to add more commands to pack and publish the packages.

Those commands are:

1. `dotnet pack -c <configuration>`
2. `dotnet nuget push <package> <k <apikey> -s <source>`

# Deployment scripts

As we are going to change how the build is going to be, I belive that having different files, each of them containing a build definition depending on the enviroment, would be the best idea.

So let's create a folder called `scripts` with three `bash` scripts in the root of the repository.

`scripts\compile.sh`
```
#!/bin/sh

echo "Restoring..."
dotnet restore ".\CalculatorCLI\CalculatorCLI.sln"   

echo "Building..."
dotnet build ".\CalculatorCLI\CalculatorCLI.sln" -c Release
```

`scripts\test.sh`
```
#!/bin/sh

echo "Testing..."
dotnet test ".\CalculatorCLI\CalculatorCLI.sln" -c Release -v n
```

`scripts\push.sh`

```
#!/bin/sh

echo "Packing..."
dotnet pack ./CalculatorCLI/CalculatorCLI.Core/CalculatorCLI.Core.csproj -c Release

echo "Pushing..."
dotnet nuget push ./CalculatorCLI/CalculatorCLI.Core/bin/Release/*.nupkg -s "https://nuget.org" -k $NUGET_API_KEY

```


# Improving the `.travis.yml` file

Now that we have the source code updated, we know the commands that we need to use, we need to integrate our continuous delivery with out continuous integration, so we don't really have to do anything but code, test, review and push.

What we are going to do now is the following:

1. Add different `stages`
2. Each `stage` depends on the branch
3. If your build is on master, it means that the package must be updated, therefore we are going to push our package to the NuGet feeds
4. If you build is a pull request, we still are going to check if the build compiles and test, but we are not going to publish it.

## Stages

From their documentation:

>You can filter out and reject builds, stages and jobs by specifying conditions in your build configuration (your .travis.yml file).

You can find more information regarding TravisCI `stages` in [their documentation page](https://docs.travis-ci.com/user/conditional-builds-stages-jobs).

So we are going to change the file and add the stages, which ends up like this:

```yml
language:
    csharp
sudo: required
mono: none 
dotnet: 3.0

os:
  - linux
    
jobs:
  include:
    - stage: compile
      script: bash scripts/compile.sh

    - stage: test
      script: bash scripts/test.sh

    - stage: deploy-prod
      if: branch = master AND type = push
      name: "Deploy to NuGet"
      script: bash scripts/push.sh
```

As you can see we have three different stages `compile`, `test` and `push`, which the last one only is going to run when the `branch` is `master` and it's not a `pull request` to it, only a `push`.

If you don't undestand about this, just go into the documentation and it's explained quite easy.

With that in mind, all the `pull request` won't be pushing anything to the NuGet feed.

# Setting the API key as an environment variable

Go to your repository build's settings and add a new environment variable, called `NUGET_API_KEY` with the value being the copied api key from the NuGet page.

[![Image from Gyazo](https://i.gyazo.com/68a4bcc4ce57eccd6bb25b7d62901c84.png)](https://gyazo.com/68a4bcc4ce57eccd6bb25b7d62901c84)

## Create a `pull request`

Now that we have everything set up, it's time to make a `pull request` and check if the compilation for that pull request is ignoring out last `stage`.

## Check the builds

Right as you make the pull request, a build will be queued in the TravisCI dashboard, and as you can see that we have 2 different builds there and not three.

[![Image from Gyazo](https://i.gyazo.com/11a209e72a121aef82a5b5a80d77a2f0.png)](https://gyazo.com/11a209e72a121aef82a5b5a80d77a2f0)

Now let's accept the `pull request` and check the build again to see that we now have three jobs instead of two.

[![Image from Gyazo](https://i.gyazo.com/46dad20d00defb8c14279332a946e73e.png)](https://gyazo.com/46dad20d00defb8c14279332a946e73e)

And as soon as you can see the build being queued, you can see the three jobs including the `Deploy-prod` at the end.

[![Image from Gyazo](https://i.gyazo.com/d24344e42f2b45225acb618ac030fd8f.png)](https://gyazo.com/d24344e42f2b45225acb618ac030fd8f)

As soon as this entire build complete, you can see in the logs that the package has been pushed to the sources.

[![Image from Gyazo](https://i.gyazo.com/77b1e44b4f9059cb7266fb18fbace8a7.png)](https://gyazo.com/77b1e44b4f9059cb7266fb18fbace8a7)


And obviously you can see it in the NuGet page

[![Image from Gyazo](https://i.gyazo.com/09421094730ea2e087cc5da639069ec4.png)](https://gyazo.com/09421094730ea2e087cc5da639069ec4)

[![Image from Gyazo](https://i.gyazo.com/a7905ddfd6665bb6e8e62e160f21630d.png)](https://gyazo.com/a7905ddfd6665bb6e8e62e160f21630d)



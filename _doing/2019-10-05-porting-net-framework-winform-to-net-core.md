---
layout: post
title: "Porting a Winforms application to .NET Core in 10 minutes"
description: "Porting a Winforms application to .NET Core in 10 minutes"
comments: true
keywords: "tutorial"
---

A few days ago was the .NET Conf, and Microsoft released the .NET Core 3.0 with a lof of new things, you can check all of them reading this [post](https://devblogs.microsoft.com/dotnet/announcing-net-core-3-0) from Microsoft's devblog.

I was also lucky to give a presentation here in Tenerife about [Blazor](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor), which was also released with .NET Core 3. A teammate of mine, [@Eickhel](https://twitter.com/Eickhel) gave presentation about Windows Desktop Apps, where he showed us about the current state of desktop applications with the new released .NET Core version. You can check about this presentacion which was also made by [Olia Gavrysh](https://twitter.com/oliagavrysh?lang=es) at the [keynote](https://youtu.be/W8yL8vRnUnA?t=2929).

Eickhel told us that even we have a desktop application that is using .NET Framework, we could easily port it with an application created a few days ago.

## Porting?

Yeah, porting. The dotnet team released an console application that could change your .csproj file from .NET Framework to .NET Core.

This tool is called [try-convert](https://github.com/dotnet/try-convert), and as you can see, it can try to convert your project from .NET Framework to .NET Core.

**Basically this tool helps you to make sure that the project does not fail to load in Visual Studio.**

## What does it do?

Well, you can check the [readme](https://github.com/dotnet/try-convert/blob/master/README.md) from the repository, but I'll copy it for you:

> It loads a given project and evaluates it to get a list of all properties and items. It then replaces the project in memory with a simple .NET SDK based template and then re-evaluates it.
>
>It does the second evaluation in the same project folder so that items that are automatically picked up by globbing will be known as well. It then applies rules about well-known properties and items, finally producing a diff of the two states to identify the following:
>
>* Properties that can now be removed from the project because they are already implicitly defined by the SDK and the project had the default value
>* Properties that need to be kept in the project either because they override the default or are not defined in the SDK.
>* Items that can be removed because they are implicitly brought in by globs in the SDK
> * Items that need to be changed to the Update syntax because although they're brought in by the SDK, there is extra metadata being added.
> * Items that need to be kept because they are are not implicit in the SDK.
>
> This diff is used to convert a given project file.

## Installation

In order to install this try-convert application, you can follow the instructions in the repository's releases [here](https://github.com/dotnet/try-convert/releases).

## Testing it

So I've loved winforms applications since the first one, and I keep doing them because I love to automate things.

So a few weeks ago I created [sql-script-generator-gui](https://github.com/emimontesdeoca/sql-script-generator-gui) which is a simple tool that generates an script with all the selected functions/stored procedures with an drop query at first. This is being used when you want to update an existing function/sp in another environment.

I built it because I'm lazy and I don't want to right-click the database and generate a script :)

### 
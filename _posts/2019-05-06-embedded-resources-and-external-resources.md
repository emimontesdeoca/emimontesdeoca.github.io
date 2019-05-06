---
layout: post
title: "Custom languages using embedded and external resources in .NET Framework"
description: "Working with embedded resources and external resrouces in the same project"
comments: true
keywords: "csharp"
---
# Introduction

At work we came up with a normal problem that I'm pretty sure a lot of developers have to deal with, which is languages and custom languages from the clients that are giving our service to their clients. 

For example, let's say that the base language is using informal language and a client want formal languague because their clients are older people.

Also note that this solutions is for several languages, we will be using English and Spanish.

*My mother language is not english si I apologize for any mistakes in the tutorial. If you find mistakes and want to fix them, you can open a pull request at [this repo](https://github.com/emimontesdeoca/emimontesdeoca.github.io) and I'll gladly aprove it!*

# Resources

Resources are XML files with extension `.resx` which have a key/value structure and they look like the following:

```xml
<?xml version="1.0" encoding="utf-8"?>
<root>
  <xsd:schema id="root" xmlns="" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:msdata="urn:schemas-microsoft-com:xml-msdata">
    <xsd:import namespace="http://www.w3.org/XML/1998/namespace" />
    <xsd:element name="root" msdata:IsDataSet="true">
      <xsd:complexType>
        <xsd:choice maxOccurs="unbounded">
          <xsd:element name="metadata">
            <xsd:complexType>
              <xsd:sequence>
                <xsd:element name="value" type="xsd:string" minOccurs="0" />
              </xsd:sequence>
              <xsd:attribute name="name" use="required" type="xsd:string" />
              <xsd:attribute name="type" type="xsd:string" />
              <xsd:attribute name="mimetype" type="xsd:string" />
              <xsd:attribute ref="xml:space" />
            </xsd:complexType>
          </xsd:element>
          <xsd:element name="assembly">
            <xsd:complexType>
              <xsd:attribute name="alias" type="xsd:string" />
              <xsd:attribute name="name" type="xsd:string" />
            </xsd:complexType>
          </xsd:element>
          <xsd:element name="data">
            <xsd:complexType>
              <xsd:sequence>
                <xsd:element name="value" type="xsd:string" minOccurs="0" msdata:Ordinal="1" />
                <xsd:element name="comment" type="xsd:string" minOccurs="0" msdata:Ordinal="2" />
              </xsd:sequence>
              <xsd:attribute name="name" type="xsd:string" use="required" msdata:Ordinal="1" />
              <xsd:attribute name="type" type="xsd:string" msdata:Ordinal="3" />
              <xsd:attribute name="mimetype" type="xsd:string" msdata:Ordinal="4" />
              <xsd:attribute ref="xml:space" />
            </xsd:complexType>
          </xsd:element>
          <xsd:element name="resheader">
            <xsd:complexType>
              <xsd:sequence>
                <xsd:element name="value" type="xsd:string" minOccurs="0" msdata:Ordinal="1" />
              </xsd:sequence>
              <xsd:attribute name="name" type="xsd:string" use="required" />
            </xsd:complexType>
          </xsd:element>
        </xsd:choice>
      </xsd:complexType>
    </xsd:element>
  </xsd:schema>
  <resheader name="resmimetype">
    <value>text/microsoft-resx</value>
  </resheader>
  <resheader name="version">
    <value>2.0</value>
  </resheader>
  <resheader name="reader">
    <value>System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
  <resheader name="writer">
    <value>System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
  <data name="Action_cancel" xml:space="preserve">
    <value>Finish</value>
  </data>
  <data name="Action_greeting" xml:space="preserve">
    <value>Hello</value>
  </data>
</root>
```

We will be creating resources for two differente languages: **english** and **spanish**, so the nomenclature for the files will be: `resources.ISOLANGUAGECODE.resx`, for example: `resource.es.resx` for Spanish and `resource.resx` for english, in case we want to later add german, the file will be named `resource.de.resx`.

## Embedded resource

Embedded resources, when the project is compiled, they will be added inside the `dll`.

Here is an image with a embedded resource, as you can see, the file `Resource.resx` can't be seen in the compilation.

[![Image from Gyazo](https://i.gyazo.com/b696d4ff1129634477c0fe3d570a05e8.png)](https://gyazo.com/b696d4ff1129634477c0fe3d570a05e8)

## External resource

On the other side the external resources or not embedded are resources that will be added to the folder after compilation.

The resource files (`.resx`) will be inside the `Properties` folder

[![Image from Gyazo](https://i.gyazo.com/db83dc7bb9e3d3fb521cb321eaa8e74a.png)](https://gyazo.com/db83dc7bb9e3d3fb521cb321eaa8e74a)

# Building the project

Let's start with a console project in .NET Framework.

## Creating the embedded resource file

Add a resource file with a few keys and be sure that it is embedded resource, which later will we using to update with external resources.

[![Image from Gyazo](https://i.gyazo.com/79af1981a3ee3214089791a3511b94ba.png)](https://gyazo.com/79af1981a3ee3214089791a3511b94ba)

## Creating the external resource file

In order to separate the external resources from the embedded ones, we will be adding the external resources inside a folder with a name so we can access them easily later.

*Tip: to add a folder inside the Properties folder, create it outside and move it inside, Visual Studio doesn't let you create it*

Do the same that we did for embedded resource, then go to `properties` of the file and inside `Advanced`, `Build action` and change it to: `Content` and change `Copy to Output Dictionary` to `Copy if newer`.

[![Image from Gyazo](https://i.gyazo.com/6f7d53c82760b6989d74dc1864bbc2d4.png)](https://gyazo.com/6f7d53c82760b6989d74dc1864bbc2d4)

This is how it should look like:

[![Image from Gyazo](https://i.gyazo.com/75b2f06b11fa99b01bca7b5ac64b4e35.png)](https://gyazo.com/75b2f06b11fa99b01bca7b5ac64b4e35)

## Adding a key to app.config

Because we are cool developers and like to have everything well done and **NOT** changing the code for each client, let's add a key to the ``appconfig`` that will have the name of the folder that we will be looking the resource files

```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
    <startup> 
        <supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.7.2" />
    </startup>
  <appSettings>
    <add key="CustomResources.Folder" value="John" />
  </appSettings>
</configuration>
```

So later, when we will be looking for the resources, it will look in `Properties.John` instead of `Properties.Doe`. 

Also this makes it easier to change when the application is already deployed since you can change the app.config easily.

## Accessing embedded resource files

Using .NET Framework is fairly easy, so in order to access the Embedded properties we just have to use the `Properties` object, which will have the resources files as a property, and inside that object there will be all the key/value that we have in the `resx` file.

```csharp
static void Main(string[] args)
{
    var hello = Properties.Resource.Action_greeting;
    var bye = Properties.Resource.Action_cancel;

    Console.WriteLine($"Action_greeting value: {hello}, Action_cancel value: {bye}");

    Console.Read();
}
```

Running that it should show us something like this:

`Action_greeting value: Hello, Action_cancel value: Cancel`

## Accessing external resource files

This part it's pretty much the same, you can access the resource files by the `Properties` object.

```csharp
var hello = Properties.Resource.Action_greeting;
var bye = Properties.Resource.Action_cancel;

var johnhello = Properties.John.Resource.Action_greeting;
var johnbye = Properties.John.Resource.Action_cancel;

var doehello = Properties.Doe.Resource.Action_greeting;
var doebye = Properties.Doe.Resource.Action_cancel;
```

# Problem

The main problem using this method is that every key is a property inside the object, so we have to call it like we saw before. If you want to call the key `Action_greeting` of the resources file of `John` we have to use the following `Properties.John.Resource.Action_greeting`. 

**Right there is the problem.**

Thats because if we are developing an applicaiton for a a lot of clients, it's a bad idea to be change how we call the resource files for each of them. 

*Could you imagine that?* Compiling the application for each client and change `John` to `Doe` and then to something else. **That is insane!**

# Solution

Our teamleader thought of a pretty good method, something like a fallback system, we must have a base model of resources, and then for each of the clients have a a resource file that will be updating the file with their resources, and we end up with a single list of resources. 

If the client doesn't want custom resources we use the base resources, and if they want them, we use theirs.

To put this in a checklist, we have to do:
- [ ] Find a way to map all the key/values to a dictionary for the embedded resources.
- [ ] Find a way to map all the key/values to a dictionary for the external resources
- [ ] Mix both files and have a single dictionary for each language
- [ ] Create a method that access the dictionary and returns the value

## Diagram

[![Image from Gyazo](https://i.gyazo.com/c7e9ae6c7792c3bace10ff9b3b2b08ee.png)](https://gyazo.com/c7e9ae6c7792c3bace10ff9b3b2b08ee)

## Let's code

First of all, let's create a separated class where we will have all our logic, from getting the resource files, to mixing them and returning the value. That class will be called `CustomResources`.

This is what is looks like:

```csharp
    class CustomResources
    {

        private static Dictionary<string, string> _ResourcesEnglish;
        private static Dictionary<string, string> ResourcesEnglish;

        private static Dictionary<string, string> _ResourcesSpanish;
        private static Dictionary<string, string> ResourcesSpanish;


        private static Dictionary<string, string> OverwriteDictionary(Dictionary<string, string> currentDictionary, Dictionary<string, string> newDictionary, bool addIfDoesntExist = false)
        {
            ...
        }

        private static Dictionary<string, string> GetDictionaryFromEmbedded(string embedded, string cultureInfoCode)
        {
            ...
        }

        private static Dictionary<string, string> GetDictionaryFromFile(string file)
        {
            ...
        }

        public static string GetText(string key)
        {
            ...
        }

        public static string GetText(string key, string language)
        {
            ...
        }
    }
```

**Note that we are implementing Lazy loading for properties, which helps to increase the performance and makes it load the dictionary once.**

- `GetDictionaryFromEmbedded`: returns a dictionary from embedded resources.
- `GetDictionaryFromFile`: returns a dictionaryu from external resources.
- `OverwriteDictionary`: mix two dictionaries and returns a single one.
- `GetText`: returns a value given a key

## From embedded resource to dictionary

We have to get all the properties from the xml file and return a dictionary:

```csharp
private static Dictionary<string, string> GetDictionaryFromEmbedded(string embedded, string cultureInfoCode)
{
    Dictionary<string, string> res = new Dictionary<string, string>();

    try
    {
        ResourceManager rm = new ResourceManager(embedded, Assembly.GetExecutingAssembly());
        var resourceSet = rm.GetResourceSet(new CultureInfo(cultureInfoCode), true, true);

        var resourceDictionary = resourceSet.Cast<DictionaryEntry>()
                            .ToDictionary(r => r.Key.ToString(),
                                          r => r.Value.ToString());
        res = resourceDictionary;
    }
    catch (Exception e)
    {
        string a = e.Message;
        // Error getting resource file
    }

    return res;
}
```

Two things:

- Note that it needs a parameter called `embedded`, that parementers is the name of the file that you can see in the designer, in our case is: `resources-demo.Properties.Resource`. 

- Also we have a parameter called cultreInfoCode, which is the code for the language to be selected. Luckily for us, .NET Framework does the job for us and we dont have to do anything, just set that we want either english or spanish, and it will select between `resource.es.resx` or `resource.resx`

## From external resource to dictionary

Getting from file a bit hacky but not hard, we have to get the current location of the executable, concat the location of the resource file and then parse it to a dictionary.

But first you have to add the reference to `System.Windows.Forms`, in order to access `ResXResourceReader`.

[![Image from Gyazo](https://i.gyazo.com/5bdf8353acc57b1d95b72acc14af41cd.png)](https://gyazo.com/5bdf8353acc57b1d95b72acc14af41cd)

Now to our `GetDictionaryFromFile` method:

```csharp
private static Dictionary<string, string> GetDictionaryFromFile(string file)
{
    Dictionary<string, string> res = new Dictionary<string, string>();
    string currentPath = (System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase) + file).Replace("file:\\", "");

    try
    {
        using (ResXResourceReader resxReader = new ResXResourceReader(currentPath))
        {
            foreach (DictionaryEntry entry in resxReader)
            {
                res.Add((string)entry.Key, (string)entry.Value);
            }
        }
    }
    catch (Exception e)
    {
        string a = e.Message;
    }

    return res;
}
```

The file parameters needs to be filled with the location from the executable to the resource file, in our case is: `"\\Properties\\John\\Resource.resx"`.

## Mixing dictionaries

We are pretty much done, first, remember to add `System.Configuration` to the references so you can access `app.settings`.

```csharp
 private static Dictionary<string, string> OverwriteDictionary(Dictionary<string, string> currentDictionary, Dictionary<string, string> newDictionary, bool addIfDoesntExist = false)
{
    var identifier = ConfigurationManager.AppSettings["CustomResources.Folder"];
    if (String.IsNullOrEmpty(identifier))
        return currentDictionary;

    foreach (var item in newDictionary)
    {
        try
        {
            currentDictionary[item.Key] = item.Value;
        }
        catch (Exception)
        {
            if(addIfDoesntExist)
            currentDictionary.Add(item.Key, item.Value);
        }
    }

    return currentDictionary;
}
```

## Getting text from the dictionary

Create a public method that calls a private method which selects a language:

```csharp
private static string GetText(string key, string language)
{
    try
    {
        switch (language)
        {
            case "es":
                return ResourceSpanish[key];
            case "en":
            default:
                return ResourceEnglish[key];
        }
    }
    catch (Exception)
    {
        return $"No value with key: {key} and language: {language}";
    }
}
```

```csharp
public static string GetText(string key)
{
    try
    {
        return GetText(key, Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName);
        }
    catch (Exception)
    {
        return $"No value with key: {key}";
    }
}
```

**Note that you have to modify the switch if you are adding more languages.**

## Adding code to the properties getter

Since we have all the methods right now, we can modify the getter of the public property to get the values.

```csharp
private static Dictionary<string, string> _ResourcesEnglish;
private static Dictionary<string, string> ResourcesEnglish
{
    get
    {
        if (_ResourcesEnglish == null)
        {
            var folderIndentifier = ConfigurationManager.AppSettings["CustomResources.Folder"]; ;
            var baseResources = GetDictionaryFromEmbedded("resources-demo.Properties.Resource");
            var customResources = GetDictionaryFromFile($"\\Properties\\{folderIndentifier}\\Resource.resx", "en");
            _ResourcesEnglish = OverwriteDictionary(baseResources, customResources);
        }

        return _ResourcesEnglish;
    }
}

private static Dictionary<string, string> _ResourcesSpanish;
private static Dictionary<string, string> ResourcesSpanish
{
    get
    {
        if (_ResourcesSpanish == null)
        {
            var folderIndentifier = ConfigurationManager.AppSettings["CustomResources.Folder"]; ;
            var baseResources = GetDictionaryFromEmbedded("resources-demo.Properties.Resources");
            var customResources = GetDictionaryFromFile($"\\Properties\\{folderIndentifier}\\Resources.es.resx", "es");
            _ResourcesSpanish = OverwriteDictionary(baseResources, customResources);
        }

        return _ResourcesSpanish;
    }
}
```


1. First we get the indentifier from the app.settings.
2. Then we get the base resources, the embedded ones. 
3. After that we get the custom resources and for that we need the folder name(which is the identifier).
4. Then we mix them and return the value.

All of this will be done once, ence the lazy loading.

# Testing

Everything related to code is finished, so now let's test it, in order to get a value from the dictionary we have to call the method `CustomResources.GetText(string key)` which returns the value.

## Updating entire resource files

This testing is a case when we want to update the entire key/value of the resource files, as you can see in the pictures we have the same keys but different values.

We will we testing `John`, and in order to set that we will have the app.config set to `<add key="CustomResources.Folder" value="John" />`.

Now let's check our base resource file (`Properties/Resource.es.resx`):

[![Image from Gyazo](https://i.gyazo.com/26d6cef147ca3cdd93a1d6cc4c60c212.png)](https://gyazo.com/26d6cef147ca3cdd93a1d6cc4c60c212)

And then our external resource file (`Properties/John/Resource.es.resx`):

[![Image from Gyazo](https://i.gyazo.com/fd6582da0d6a427c2b21cc8b1999f9e0.png)](https://gyazo.com/fd6582da0d6a427c2b21cc8b1999f9e0)

Okey with everything set, we run the console application, let's stop to the `get` part of the properites and check everything

`folderIdentifier` has the value of the appseting:

[![Image from Gyazo](https://i.gyazo.com/ac30d3deb7abe29a877b368d9300b990.png)](https://gyazo.com/ac30d3deb7abe29a877b368d9300b990)

`baseResources` has the value of the base resources, the embedded ones:

[![Image from Gyazo](https://i.gyazo.com/a61410b365b55d4735f2b2622a18b966.png)](https://gyazo.com/a61410b365b55d4735f2b2622a18b966)

`customResources` has the values of the external resources inside the folder `John`:

[![Image from Gyazo](https://i.gyazo.com/0e93733fbd616b274a69cdffbc16afb1.png)](https://gyazo.com/0e93733fbd616b274a69cdffbc16afb1)

And finally, `_ResourcesSpanish` has the value mixed from the base resources to to the external resources.

[![Image from Gyazo](https://i.gyazo.com/0e25a79023364ea1d5be2109dbf6492c.png)](https://gyazo.com/0e25a79023364ea1d5be2109dbf6492c)


## Updating only one

Now let's test the same but having different scenario, just update one key and let the other be the same.

[![Image from Gyazo](https://i.gyazo.com/9c2a05fb976f1f671b13c9cf77220336.png)](https://gyazo.com/9c2a05fb976f1f671b13c9cf77220336)

As you can see, the files has the same value meaning for `Action_greeting` but different value for `Action_cancel`, so it should update only `Action_cancel`.

[![Image from Gyazo](https://i.gyazo.com/16938859b05bb1f0fe08751a08b1fcad.png)](https://gyazo.com/16938859b05bb1f0fe08751a08b1fcad)

## Missing the resource file

If you don't provide the external resource file, it doesn't matter at all because as we are expecting to have a file, if it fails it will return an empty dictionary and when mixing both dictionaries it will end up with the base one.

## Missing pair in embedded resource

If you have a pair in the external resource file, by default it will not add it to the final dictionary, you can change that by calling the method `OverwriteDictionary(Dictionary<string, string> currentDictionary, Dictionary<string, string> newDictionary, bool addIfDoesntExist = false)` when mixing both dictionaries and set the parameter `addIfDoesntExist` to `true`. 

## Different languages

As you can see we didn't specified any language, because all of that is being done by the function `GetText(string key)` which is calling `GetText(string key, string language)`, and the parameter `language` is filled by `Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName` which returns the current language of our thread.

In my case I have spanish as the default language and that's why it always shows it in spanish, but we can try using english too.

Let's code a bit and build something to check both spanish and english.

```csharp
static void Main(string[] args)
{

    // System.Threading.Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
    System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
    WriteText("Action_greeting");
    WriteText("Action_cancel");

    // System.Threading.Thread.CurrentThread.CurrentUICulture = new CultureInfo("es-ES");
    System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("es-ES");
    WriteText("Action_greeting");
    WriteText("Action_cancel");

    Console.Read();
}

private static void WriteText(string key) {
    Console.WriteLine($"[Culture: {System.Threading.Thread.CurrentThread.CurrentCulture}] Key: {key} -> value: {CustomResources.GetText(key)}");
}
```

Running this, the output should be something like this:

```
[Culture: en-US] Key: Action_greeting -> value: Hello
[Culture: en-US] Key: Action_cancel -> value: Finish
[Culture: es-ES] Key: Action_greeting -> value: Hola
[Culture: es-ES] Key: Action_cancel -> value: Terminar
```

[![Image from Gyazo](https://i.gyazo.com/de7e5efcc2ff5e82529749b1188706ff.png)](https://gyazo.com/de7e5efcc2ff5e82529749b1188706ff)

First two values are from `resources.resx` which are in english and the last both are in spanish and the values are retrieved from `resources.es.resx`.

# That's it

In this tutorial we found a way to mix both embedded and external resources for laguages, this was a solution for a problem we had in the team and it's been running since then without any problems.

You can check the source code [here](https://github.com/emimontesdeoca/resources-demo).

If you have any question feel free to tweet me at [@emimontesdeocaa](https://twitter.com/emimontesdeocaa) and I'll get back to you when I have time.


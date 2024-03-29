# Custom everything

As you have probably seen on all my post I really try to keep everything as clean as possible, as I've already wrote posts regarding custom attributes, custom exception handling, service collection injection, etc. 

I've realized with time that this kind of way of coding gives me and my team a way to improve overtime, find issues easier and separate the code as much as possible.

Yeah after this cool story I have you just now, I've been working on Blazor lately as usual and I found that, after years and years of developing, you can create custom validation attributes.

Yeah it's funny, after all this years...

# Custom validation attributes

The idea came from work actually, we always do validation on all places but I had some fields that required kind of the same validation process, so I thought that there might be something there.... like custom validation attributes!

So I fired up Microsoft documentation for that and found out that yeah, you can create custom validation attributes and assign them to the properties, just like this:

```csharp
public class StringLengthRangeAttribute : ValidationAttribute
{
    public int Minimum { get; set; }
    public int Maximum { get; set; }

    public StringLengthRangeAttribute()
    {
        this.Minimum = 0;
        this.Maximum = int.MaxValue;
    }

    public override bool IsValid(object value)
    {
        string strValue = value as string;
        if (!string.IsNullOrEmpty(strValue))
        {
            int len = strValue.Length;
            return len >= this.Minimum && len <= this.Maximum;
        }
        return true;
    }
}
```

and use it in a simple class like this:

```csharp
[Required]
[StringLengthRange(Minimum = 10, ErrorMessage = "Must be >10 characters.")]

[StringLengthRange(Maximum = 20)]

[Required]
[StringLengthRange(Minimum = 10, Maximum = 20)]
```

# Custom validator

So I have this validator that I need to have for some specific business case that will countain 20 first characters that will bye 9 numbers and a hyphen, and end with 2 chars that will usually be the country code, so something like this: **123456789-123456789-ES**

I ended up coming with something like this, it's really simple but it works:

```csharp
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

public class SpecialStringValidatorAttribute : ValidationAttribute
{
    private const int TotalLength = 22;
    private const string Pattern = @"^(\d{10})-(\d{10})-([A-Za-z]{2})$";

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        string strValue = value as string;

        if (!string.IsNullOrEmpty(strValue))
        {
            if (strValue.Length != TotalLength)
            {
                return new ValidationResult($"The string must be {TotalLength} characters long.");
            }

            if (!Regex.IsMatch(strValue, Pattern))
            {
                return new ValidationResult("The string must follow the pattern: 1234567890-1234567890-AB");
            }

            return ValidationResult.Success;
        }

        return new ValidationResult("The string cannot be null or empty.");
    }
}
```

## Tests

I wrote some test for them too, just in case:

```csharp
public class SpecialStringValidatorTests
{
    [Theory]
    [InlineData("1234567890-1234567890-AB", true)]
    [InlineData("1234567890-1234567890-XY", true)]
    [InlineData("1234567890-1234567890-A1", false)]
    [InlineData("1234567890-1234567890-A", false)]
    [InlineData("1234567890-123456789-AB", false)]
    [InlineData("1234567890-1234567890", false)]
    [InlineData("1234567890-1234567890-ABCDE", false)]
    public void SpecialStringValidatorTest(string input, bool expectedResult)
    {
        // Arrange
        var validator = new SpecialStringValidatorAttribute();

        // Act
        var result = validator.IsValid(input);

        // Assert
        Assert.Equal(expectedResult, result);
    }
}
```

And when ran I had this results:

```bash
Microsoft (R) Test Execution Command Line Tool Version 16.9.1
Copyright (c) Microsoft Corporation.  All rights reserved.

Starting test execution, please wait...

A total of 1 test files matched the specified pattern.

Test run in progress...

Passed!  - SpecialStringValidatorTests.SpecialStringValidatorTest(input: "1234567890-1234567890-AB", expectedResult: True)
Passed!  - SpecialStringValidatorTests.SpecialStringValidatorTest(input: "1234567890-1234567890-XY", expectedResult: True)
Passed!  - SpecialStringValidatorTests.SpecialStringValidatorTest(input: "1234567890-1234567890-A1", expectedResult: False)
Passed!  - SpecialStringValidatorTests.SpecialStringValidatorTest(input: "1234567890-1234567890-A", expectedResult: False)
Passed!  - SpecialStringValidatorTests.SpecialStringValidatorTest(input: "1234567890-123456789-AB", expectedResult: False)
Passed!  - SpecialStringValidatorTests.SpecialStringValidatorTest(input: "1234567890-1234567890", expectedResult: False)
Passed!  - SpecialStringValidatorTests.SpecialStringValidatorTest(input: "1234567890-1234567890-ABCDE", expectedResult: False)

Test Run Successful.
Total tests: 7
     Passed: 7
 Total time: 1.7296 Seconds
```

# Custom validation and Blazor

So now that I know that his can be used, it's clearly a good idea to implement it into Blazor right?

Let's assume I have this form, that will use the model `Person` that I showed before:

```csharp
@using Models
@page "/"

<PageTitle>Home</PageTitle>

<h1>Hello, world!</h1>

<EditForm Model=@Person FormName="PersonForm">
  <DataAnnotationsValidator/>
  <ValidationSummary/>
  <div class="form-group">
    <label for="Name">Name</label>
    <InputText @bind-Value=Person.Name class="form-control" id="Name" />
    <ValidationMessage For="() => Person.Name"/>
  </div>
  <div class="form-group">
    <label for="MySpecialString">My special string</label>
    <InputText @bind-Value=Person.MySpecialString class="form-control" id="Name" />
    <ValidationMessage For="() => Person.MySpecialString"/>
  </div>
  <div class="form-group">
    <label for="Age">Age</label>
    <InputNumber @bind-Value=Person.Age class="form-control" id="Age" />
    <ValidationMessage For=@(() => Person.Age) />
  </div>
  <input type="submit" class="btn btn-primary" value="Save"/>
</EditForm>

@code {
  Person Person = new Person();
}
```

As soon as we run this, we get the following errors:

<img src="https://i.imgur.com/psfpzdr.png">


And if we just put what we want, we get the following thing, all clear:

<img src="https://i.imgur.com/RPoUq02.png">

To be honest for me it's quite clear that we should move the logic at least to validate those forms into custom validation attribues, it gives us freedom to store the code for that login in a single place, and we can use it later for an API or another application.

Hope you liked it, if you have any questions or you want to get in touch, don't hestiate and contact me!